using System;
using ServiceStack.Redis;
using System.IO;
using Bizarrefish.VMTestLib;
using System.Web.Script.Serialization;
using System.Text;

namespace Bizarrefish.TestVisorStorage
{
	internal class RedisTestResource : ITestResource
	{
		IRedisClient client;
		string hashKey, name;

		public RedisTestResource(IRedisClient client, string name, string hashKey)
		{
			this.client = client;
			this.hashKey = hashKey;
			this.name = name;
		}

		Stream OpenWithFunction(bool deleteFirst, Func<string, Stream> func)
		{
			string fileName = client.GetValueFromHash(hashKey, name);
			if(fileName == null)
			{
				throw new Exception("Resource: " + name + " has been removed from the index");
			}

			if (deleteFirst) File.Delete(fileName);

			return func(fileName);
		}

		public System.IO.Stream Read ()
		{
			return OpenWithFunction(false, File.OpenRead);
		}

		public void Write (System.IO.Stream s)
		{
			using(Stream fileStream = OpenWithFunction(true, File.OpenWrite))
			{
				s.CopyTo(fileStream);
			}
		}

		public System.IO.Stream Write ()
		{
			return OpenWithFunction (true, File.OpenWrite);
		}
	}

	public class RedisTestRepository : ITestRepository
	{
		IRedisClient client;

		// The blob
		string blobKey;

		// A hash mapping resource names to file names
		string resourceIndexKey;

		// To generate filenames for our resources
		string resourceCounterKey;

		// A lock, for when we're modifying the index
		string lockKey;

		// The directory resources are kept in.
		string resourceDirectory;

		static JavaScriptSerializer jss = new JavaScriptSerializer();

		public RedisTestRepository (RedisClient client, string filePrefix, string dbPrefix)
		{
			this.client = client;
			blobKey = dbPrefix + "/Blob";
			resourceIndexKey = dbPrefix + "/ResourceFileIndex";
			resourceCounterKey = dbPrefix + "/ResourceCounter";
			lockKey = dbPrefix + "/IndexLock";
			resourceDirectory = filePrefix;

			if(!Directory.Exists(resourceDirectory))
				Directory.CreateDirectory(resourceDirectory);

			CheckIndex();
		}

		
		public System.Collections.Generic.IEnumerable<string> Resources {
			get
			{
				return client.GetHashKeys(resourceIndexKey);
			}
		}

		void CheckIndex()
		{
			WithIndexLock (() => 
			{
				var map = client.GetAllEntriesFromHash(resourceIndexKey);

				foreach(var entry in map)
				{
					if(!File.Exists(entry.Value))
					{
						Console.Error.WriteLine("Resource: " + entry.Key + " doesn't exist; Deleting from index.");
						client.RemoveEntryFromHash(resourceIndexKey, entry.Key);
					}
				}
			});
		}


		public void Store<TBlob> (TBlob blob)
		{
			client.SetEntry(blobKey, jss.Serialize(blob));
		}

		public TBlob Load<TBlob> () where TBlob : new ()
		{
			var blobBytes = client.Get<byte[]>(blobKey);
			if(blobBytes != null)
			{
				return jss.Deserialize<TBlob>(Encoding.UTF8.GetString(blobBytes));
			}
			else
			{
				return new TBlob();
			}
		}

		void WithIndexLock(Action act)
		{
			using(var l = client.AcquireLock(lockKey))
			{
				act();
			}
		}

		public ITestResource CreateResource (string name)
		{
			string fileName = null;

			WithIndexLock (() => 
			{
				long ctr = client.IncrementValue(resourceCounterKey);
				fileName = resourceDirectory + "/" + ctr + ".resource";

				client.SetEntryInHash(resourceIndexKey, name, fileName);

			});

			File.Create(fileName).Close ();

			return new RedisTestResource(client, name, resourceIndexKey);
		}

		public ITestResource GetResource (string name)
		{
			var fileName = client.GetValueFromHash(resourceIndexKey, name);
			if(fileName != null)
			{
				return new RedisTestResource(client, name, resourceIndexKey);
			}
			else
			{
				throw new Exception("Resource: " + name + " doesn't exist");
			}
		}

		public void DeleteResource (string name)
		{
			string fileName = null;

			WithIndexLock (() => 
			{
				fileName = client.GetValueFromHash(resourceIndexKey, name);
				client.RemoveEntryFromHash(resourceIndexKey, name);
			});

			File.Delete (fileName);
		}
	}
}

