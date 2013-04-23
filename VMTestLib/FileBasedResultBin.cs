using System;
using System.IO;

namespace Bizarrefish.VMTestLib
{
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
		
		public void PutResult (string testName, bool success, string detail)
		{
			using(var fs = File.Create(baseDir + "/results/" + (success ? "SUCCESS-" : "FAIL-") + testName + ".result"))
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

