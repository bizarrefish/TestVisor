using System;

using System.Collections.Generic;
using Bizarrefish.VMLib;
using System.IO;

namespace Bizarrefish.VMTestLib.TestDrivers.WindowsBatch
{
	public class BatchFileDriver : ITestDriver
	{
		const string TestPathPrefix = "C:\\WindowsBatchFileTests\\";
		
		class BatchFileDriverData
		{
			public IList<string> Tests = new List<string>();
		}
		
		public ITestRepository Repo { get; set; }
		public IEnumerable<string> Tests {
			get
			{
				var data = Repo.Load<BatchFileDriverData>();
				return data.Tests;
			}
		}
		
		
		public void InstallTest (string name, Stream source)
		{
			var data = Repo.Load<BatchFileDriverData>();
			var res = Repo.CreateResource(name);
			res.Write(source);
			data.Tests.Add(name);
			Repo.Store (data);
		}
		 
		public void RemoveTest (string name)
		{
			Repo.DeleteResource(name);
			var data = Repo.Load <BatchFileDriverData>();
			data.Tests.Remove (name);
			Repo.Store (data);
		}

		public void RunTest (string name, IMachine machine, ITestResultBin bin, IDictionary<string, string> env)
		{
			ITestResource res = Repo.GetResource(name);
			string targetFileName = TestPathPrefix + name + "\\" + name + ".bat";
			using (Stream s = res.Read())
			{
				machine.PutFile(targetFileName, s);
			}
			
			ProgramResult result = machine.RunProgram(targetFileName, "", TestPathPrefix + name + "\\", env);
			
			// Hoover up artifacts  *SLUURRRRRP*
			var artifacts = machine.ListFiles(TestPathPrefix + name + "\\");
			foreach(var fileName in artifacts)
			{
				string tempFile = Path.GetTempFileName();
				using(FileStream fs = File.Create (tempFile))
				{
					machine.GetFile (fileName, fs);
					fs.Seek (0, SeekOrigin.Begin);
					bin.PutArtifact(fileName, fs);
				}
				File.Delete(tempFile);
			}
			
			// Get RESULTS
			if(result.ExitCode == 0)
			{
				bin.PutResult(name, true, result.StandardOutput);
			}
			else
			{
				bin.PutResult(name, false, result.StandardError);
			}
		}		
	}
}

