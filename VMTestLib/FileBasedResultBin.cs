using System;
using System.IO;
using System.Collections.Generic;

namespace Bizarrefish.VMTestLib
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
		
		/// <summary>
		/// Gets the artifact paths.
		/// </summary>
		/// <returns>
		/// A mapping from artifact names to paths.
		/// </returns>
		public IDictionary<string, string> GetArtifactPaths()
		{
			var result = new Dictionary<string, string>();
			
			foreach(var fileName in Directory.GetFileSystemEntries(baseDir + "/artifacts"))
			{
				result[Path.GetFileName(fileName)] = fileName;
			}
			return result;
		}
		
		public void PutDetail (string name, string detail)
		{
			using(var fs = File.Create(baseDir + "/results/" + name + ".result"))
			{
				using(var writer = new StreamWriter(fs))
				{
					writer.Write (detail);
				}
			}
		}

		public void PutArtifact (string name, System.IO.Stream stream)
		{
			using(var fs = File.Create (baseDir + "/artifacts/" + name))
			{
				Utils.CopyStream(stream, fs);
			}
		}
	}
}

