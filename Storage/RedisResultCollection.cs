using System;
using ServiceStack.Redis;
using System.Collections.Generic;
using Bizarrefish.VMTestLib;
using System.Linq;
using System.IO;
using System.Web.Script.Serialization;

namespace Bizarrefish.TestVisorStorage
{
	internal class RedisResultBin : ITestResultBin
	{
		static JavaScriptSerializer jss = new JavaScriptSerializer();

		string dbPrefix, filePrefix;

		string artifactCounterKey;
		string lockKey;

		IRedisClient client;

		string MakeArtifactInfoKey(long artifactNumber)
		{
			return dbPrefix + "/Artifact:" + artifactNumber + "/Info";
		}

		string MakeArtifactFilename(long artifactNumber)
		{
			return filePrefix + "/" + artifactNumber;
		}

		public void ForceUnlock()
		{
			client.Remove (lockKey);
		}

		public RedisResultBin(IRedisClient client, string artifactDbPrefix, string artifactFilePrefix)
		{
			this.client = client;
			this.dbPrefix = artifactDbPrefix;
			this.filePrefix = artifactFilePrefix;

			this.artifactCounterKey = artifactDbPrefix + "/ArtifactCounter";
			this.lockKey = artifactDbPrefix + "/Lock";
		}

		public void PutArtifact (ArtifactInfo info, Stream stream)
		{
			using(var myLock = client.AcquireLock(lockKey))
			{
				long number = client.IncrementValue(artifactCounterKey);

				string infoString = jss.Serialize(info);
				client.SetEntry(MakeArtifactInfoKey(number), infoString);

				string fileName = MakeArtifactFilename(number);

				using(var fs = File.OpenWrite(fileName))
				{
					stream.CopyTo(fs);
				}
			}
		}

		public ArtifactInfo[] GetArtifactInfos()
		{
			using(var myLock = client.AcquireLock(lockKey))
			{
				int counter = client.Get<int>(artifactCounterKey);

				var infos = new List<ArtifactInfo>();

				for(int i = 0; i < counter; i++)
				{
					string infoString = client.GetValue(MakeArtifactInfoKey(i));
					if(infoString != null)
					{
						infos.Add (jss.Deserialize<ArtifactInfo>(infoString));
					}
				}

				return infos.ToArray();
			}
		}

		public Stream ReadArtifact(long artifactNumber)
		{
			using(var myLock = client.AcquireLock(lockKey))
			{
				return File.OpenRead(MakeArtifactFilename(artifactNumber));
			}
		}

		public void Delete()
		{
			using(var myLock = client.AcquireLock(lockKey))
			{
				int nArtifacts = client.Get<int>(artifactCounterKey);

				client.RemoveAll(Enumerable.Range (0, nArtifacts)
				                 .Select (num => MakeArtifactInfoKey(num)));
				client.Remove (artifactCounterKey);
				for(int i = 0; i < nArtifacts; i++)
				{
					string fileName = MakeArtifactFilename(i);
					if(File.Exists(fileName))
					{
						File.Delete(fileName);
					}
				}
			}
		}
	}

	/// <summary>
	/// Manages test results with a redis database.
	/// 
	/// Each test run has a number of test keys associated.
	/// Each test key has a TestResult and a number of artifacts associated.
	/// </summary>
	public class RedisResultCollection
	{
		IRedisClient client;
		string filePrefix;

		const string runIdSet = "/TestRuns";

		static string MakeTestKeySetKey(string runId)
		{
			return "/TestRun:" + runId + "/TestKeys";
		}

		static string MakeRunLockKey(string runId)
		{
			return "/TestRun:" + runId + "/Lock";
		}

		static string MakeResultBinKey(string runId, string testKey)
		{
			return "/TestRun:" + runId + "/TestKey:" + testKey + "/Bin";
		}

		string MakeResultBinPath(string runId, string testKey)
		{
			return filePrefix + "/" + runId + "/" + testKey;
		}

		string MakeResultKey(string runId, string testKey)
		{
			return "/TestRun:" + runId + "/TestKey:" + testKey + "/TestResult";
		}

	 	RedisResultBin MakeResultBin(string runId, string testKey)
		{
			return new RedisResultBin(client, MakeResultBinKey(runId, testKey), MakeResultBinPath(runId, testKey));
		}

		static JavaScriptSerializer jss = new JavaScriptSerializer();

		void ForceUnlock()
		{
			var runIds = client.GetAllItemsFromSet(runIdSet);
			foreach(var runId in runIds)
			{
				client.Remove (MakeRunLockKey(runId));
				var testKeys = client.GetAllItemsFromSet(MakeTestKeySetKey(runId));

				foreach(var testKey in testKeys)
				{
					MakeResultBin(runId, testKey).ForceUnlock();
				}
			}
		}

		public RedisResultCollection (IRedisClient client, string filePrefix)
		{
			this.client = client;
			this.filePrefix = filePrefix;

			if(!Directory.Exists(filePrefix))
				Directory.CreateDirectory(filePrefix);

			ForceUnlock();
		}

		public ITestResultBin CreateResultBin(string runId, string testKey)
		{
			// Add the result id to the set if not already there
			client.AddItemToSet(runIdSet, runId);

			// Add the test key
			client.AddItemToSet(MakeTestKeySetKey(runId), testKey);

			// Give it somewhere to live
			Directory.CreateDirectory (MakeResultBinPath(runId, testKey));

			// BIN!
			return MakeResultBin(runId, testKey);
		}

		/// <summary>
		/// Get the ids of all test runs.
		/// </summary>
		public IEnumerable<string> GetRuns()
		{
			return client.GetAllItemsFromSet(runIdSet);
		}

		TestResult GetResult(string runId, string testKey)
		{
			var result = client.GetValue(MakeResultKey(runId, testKey));
			if(result == null) return new TestResult();
			else return jss.Deserialize<TestResult>(result);
		}

		/// <summary>
		/// Get a map from testkey -> test result for a run.
		/// </summary>
		public IDictionary<string, TestResult> GetResults(string runId)
		{
			return client.GetAllItemsFromSet(MakeTestKeySetKey(runId))
				.ToDictionary(tk => tk, tk => GetResult(runId, tk));

		}

		/// <summary>
		/// Gets the artifact infos associated with a test key in a run.
		/// With each artifactinfo is also a function to open a stream to read the artifact.
		/// </summary>
		public ArtifactInfo[] GetArtifacts(string runId, string testKey)
		{
			var resultBin = MakeResultBin(runId, testKey);

			return resultBin.GetArtifactInfos();
		}

		/// <summary>
		/// Sets the TestResult for a testkey in a run.
		/// </summary>
		public void SetResult(string runId, string testKey, TestResult result)
		{
			string testKeyListKey = MakeTestKeySetKey(runId);
			string resultKey = MakeResultKey(runId, testKey);


			client.AddItemToSet(runIdSet, runId);
			client.AddItemToSet(testKeyListKey, testKey);

			client.SetEntry (resultKey, jss.Serialize(result));
		}

		/// <summary>
		/// Delete a run and everything associated.
		/// </summary>
		public void DeleteRun(string runId)
		{
			using(var l = client.AcquireLock(MakeRunLockKey(runId)))
			{
				var testKeys = client.GetAllItemsFromSet(MakeTestKeySetKey(runId));
				foreach(var tk in testKeys)
				{
					MakeResultBin (runId, tk).Delete();
					Directory.Delete(MakeResultBinPath(runId, tk), true);
				}
				client.RemoveAll (testKeys.Select(tk => MakeResultKey(runId, tk)));
				client.Remove (MakeTestKeySetKey(runId));
				client.RemoveItemFromSet(runIdSet, runId);
   			}
		}

	}
}

