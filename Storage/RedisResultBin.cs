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

	public class RedisResultBin2 : ITestResultBin
	{
		IRedisClient client;

		// List of indexes.
		string artifactListKey;

		// Counter to give our artifact files names
		string fileNameCounterKey;

		// Prefix for storing artifact files
		string filePrefix;

		public RedisResultBin2 (Uri dbUri, string dbPrefix, string filePrefix)
		{
			this.client = new RedisClient(dbUri);
			artifactListKey = dbPrefix + "/Artifacts";
			fileNameCounterKey = dbPrefix + "/ArtifactCounter";
			this.filePrefix = filePrefix;
		}

		public IEnumerable<ArtifactObject> GetArtifacts()
		{
			var jss = new JsonSerializer<ArtifactObject>();

			return client.GetAllItemsFromList(artifactListKey)
				.Select (jss.DeserializeFromString);
		}

		public void PutArtifact (ArtifactInfo info, System.IO.Stream stream)
		{
			var jss = new JsonSerializer<ArtifactObject>();

			long ctr = client.IncrementValue(fileNameCounterKey);

			string fileName = filePrefix + "/" + ctr + ".artifact";

			client.AddItemToList(artifactListKey, jss.Serialize(info));

			using(var fs = File.Create(fileName))
			{
				stream.CopyTo(fs);
			}
		}

	}
}

