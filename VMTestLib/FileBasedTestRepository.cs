using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Linq;
using System.Web.Script.Serialization;

namespace Bizarrefish.VMTestLib
{
	
	/*public static class Program
	{
		class BlobTest
		{
			public string TestData;
			public IEnumerable<string> TestList;
		}
		
		public static void Main(string[] args)
		{
			if(!Directory.Exists("/home/lee/repo"))
				Directory.CreateDirectory("/home/lee/repo");
			
			var repo = new FileBasedTestRepository("/home/lee/repo");
			
			var res = repo.GetResource("Hats");
			
			var ms = new MemoryStream();
			byte[] stringBytes = Encoding.UTF8.GetBytes ("Hello");
			ms.Write (stringBytes, 0, stringBytes.Length);
			
			ms.Seek (0,SeekOrigin.Begin);
			res.Write (ms);
			ms.Close ();
			
			var blob = new BlobTest();
			blob.TestData = "Heloooo";
			blob.TestList = repo.Resources;
			
			repo.Store(blob);
			
			
			var newBlob = new BlobTest();
			
			newBlob = repo.Load<BlobTest>();
			
			
			Console.WriteLine (repo.Resources);
		}
	}*/
	
	class FileBasedTestResource : ITestResource
	{
		public string Path;
		public FileBasedTestResource(string path)
		{
			this.Path = path;
			if(!File.Exists(path))
				throw new Exception("Resource backing file: " + path + " doesn't exist!");
		}
		
		public Stream Read ()
		{
			return File.OpenRead(Path);
		}

		public void Write (Stream s)
		{
			using(Stream outStream = File.OpenWrite(Path))
			{
				Utils.CopyStream(s, outStream);
			}
		}
		
		public Stream Write()
		{
			return File.OpenWrite(Path);
		}
	}
	
	public class FileBasedTestRepository : ITestRepository
	{
		IDictionary<string, FileBasedTestResource> index =
			new Dictionary<string, FileBasedTestResource>();
		
		public static JavaScriptSerializer jss = new JavaScriptSerializer();
		
		public IEnumerable<string> Resources { get { return index.Keys; } }
		
		const string IndexFileName = "resourceIndex";
		const string BlobFileName = "blobData";
		
		string blobPath;
		
		Action saveIndex;
		
		string baseDir;
		
		public FileBasedTestRepository (string baseDir)
		{
			this.baseDir = baseDir;
			string indexFile = baseDir + "/" + IndexFileName;
			this.blobPath = baseDir + "/" + BlobFileName;
			
			if(!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);
			
			if(File.Exists(indexFile))
			{
				foreach(var line in File.ReadAllLines(indexFile))
				{
					string[] parts = line.Split ('&').Select(HttpUtility.UrlDecode).ToArray();
					index[parts[0]] = new FileBasedTestResource(parts[1]);
				}
			}
			else
			{
				File.Create(indexFile);
			}
			
			saveIndex = delegate() {
				var lines = index.Select (kvp =>
					HttpUtility.UrlEncode(kvp.Key) + '&' +
					HttpUtility.UrlEncode (kvp.Value.Path));
				
				File.WriteAllLines(indexFile, lines);
			};
		}

		public void Store<TBlob> (TBlob blob)
		{
			File.WriteAllText(blobPath,jss.Serialize(blob));
		}

		public TBlob Load<TBlob> () where TBlob : new()
		{
			if(File.Exists (blobPath))
				return jss.Deserialize<TBlob>(File.ReadAllText(blobPath));
			else
				return new TBlob();
		}

		public ITestResource CreateResource (string name)
		{
			string realPath = baseDir + '/' + HttpUtility.UrlEncode(name);
			File.Create(realPath).Close ();
			index[name] = new FileBasedTestResource(realPath);
			saveIndex();
			return GetResource (name);
		}

		public ITestResource GetResource (string name)
		{
			if(!index.ContainsKey(name))
				throw new Exception("Resource: " + name + " not present in index");
			
			return index[name];
		}

		public void DeleteResource (string name)
		{
			if(!index.ContainsKey(name))
				throw new Exception("Resource: " + name + " not present in index");
			
			string realPath = index[name].Path;
			index.Remove (name);
			File.Delete(realPath);
			saveIndex();
		}
	}
}

