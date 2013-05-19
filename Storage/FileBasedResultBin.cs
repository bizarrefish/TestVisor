using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Text;
using Bizarrefish.VMTestLib;

namespace Bizarrefish.TestVisorStorage
{
	/// <summary>
	/// Filesystem-backed result bin.
	/// </summary>
	public class FileBasedResultBin : ITestResultBin
	{
		string baseDir;
		public FileBasedResultBin (string baseDir)
		{
			this.baseDir = baseDir;
			if(!Directory.Exists(baseDir))
			{
				Directory.CreateDirectory(baseDir);
				Directory.CreateDirectory(baseDir + "/results");
				Directory.CreateDirectory(baseDir + "/artifacts");
			}
		}
		
		public IDictionary<string, string> GetDetails(string name)
		{
			var result = new Dictionary<string, string>();
			
			foreach(var fileName in Directory.GetFileSystemEntries(baseDir + "/results"))
			{
				string detail = File.ReadAllText(fileName);
				result[fileName.Replace (".result","")] = detail;
			}
			
			return result;
		}
		
		public IEnumerable<string> TestKeys
		{
			get { return Directory.GetFileSystemEntries(baseDir + "/artifacts").Select(Path.GetFileName); }
		}
		
		/// <summary>
		/// Gets the artifact paths.
		/// </summary>
		/// <returns>
		/// A mapping from artifact names to paths.
		/// </returns>
		public IDictionary<string, string> GetArtifactPaths(string testKey)
		{
			string artifactDir = baseDir + "/artifacts/" + testKey;
			
			if(!Directory.Exists(artifactDir)) Directory.CreateDirectory(artifactDir);
			
			var result = new Dictionary<string, string>();
			
			foreach(var fileName in Directory.GetFileSystemEntries(artifactDir))
			{
				result[Path.GetFileName(fileName)] = fileName;
			}
			return result;
		}
		
		public void PutDetail<TDetail>(string testKey, TestDetail type, TDetail detail)
		{
			using(var fs = File.Create(baseDir + "/results/" + type.ToString () + ".result"))
			{
				using(var writer = new StreamWriter(fs))
				{
					writer.Write (new JsonSerializer<TDetail>().SerializeToString(detail));
				}
			}
		}

		public void PutArtifact (string testKey, string name, System.IO.Stream stream)
		{
			string artifactDir = baseDir + "/artifacts/" + testKey;
			if(!Directory.Exists(artifactDir)) Directory.CreateDirectory(artifactDir);

			
			using(var fs = File.Create (baseDir + "/artifacts/" + testKey + "/" + name))
			{
				Utils.CopyStream(stream, fs);
			}
		}
	}
}

