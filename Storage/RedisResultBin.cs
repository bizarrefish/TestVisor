using System;
using ServiceStack.Redis;
using Bizarrefish.VMTestLib;
using ServiceStack.Text;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace Bizarrefish.TestVisorStorage
{
	public class ArtifactObject
	{
		public string TestKey { get; set; }
		public string ArtifactName { get; set; }
		public string FileName { get; set; }
	}

	public class TestDetailObject
	{
		public string TestKey { get; set; }
		public TestDetail DetailType { get; set; }
		public object Content { get; set; }
	}

	public class RedisResultBin : ITestResultBin
	{
		IRedisClient client;

		// List of indexes.
		string artifactListKey;

		// List of artifacts.
		string detailListKey;

		// Counter to give our artifact files names
		string fileNameCounterKey;

		// Prefix for storing artifact files
		string filePrefix;

		public RedisResultBin (Uri dbUri, string dbPrefix, string filePrefix)
		{
			this.client = new RedisClient(dbUri);
			artifactListKey = dbPrefix + "/Artifacts";
			detailListKey = dbPrefix + "/Detail";
			fileNameCounterKey = dbPrefix + "/ArtifactCounter";
			this.filePrefix = filePrefix;
		}


		public IEnumerable<TestDetailObject> GetDetails(string name)
		{
			var jss = new JsonSerializer<TestDetailObject>();

			return client.GetAllItemsFromList(detailListKey)
				.Select(jss.DeserializeFromString);
		}

		public IEnumerable<ArtifactObject> GetArtifacts(string name)
		{
			var jss = new JsonSerializer<ArtifactObject>();

			return client.GetAllItemsFromList(artifactListKey)
				.Select (jss.DeserializeFromString);
		}

		public void PutDetail<TDetail>(string testKey, TestDetail type, TDetail detail)
		{
			var jss = new JsonSerializer<TestDetailObject>();

			client.AddItemToList(detailListKey, jss.SerializeToString(
				new TestDetailObject()
			{
				TestKey = testKey,
				DetailType = type,
				Content = detail
			}));
		}

		public void PutArtifact (string testKey, string name, System.IO.Stream stream)
		{
			var jss = new JsonSerializer<ArtifactObject>();

			long ctr = client.IncrementValue(fileNameCounterKey);

			string fileName = filePrefix + "/" + ctr + ".artifact";

			client.AddItemToList(artifactListKey, jss.SerializeToString(
				new ArtifactObject()
			{
				TestKey = testKey,
				ArtifactName = name,
				FileName = fileName
			}));

			using(var fs = File.Create(fileName))
			{
				stream.CopyTo(fs);
			}
		}

	}
}

