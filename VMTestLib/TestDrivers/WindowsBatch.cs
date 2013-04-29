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

		public TestResult RunTest (string name, string testKey, IMachine machine, ITestResultBin bin, IDictionary<string, string> env)
		{
			ITestResource res = Repo.GetResource(name);
			
			string winDirectory = TestPathPrefix + name + "\\" + testKey + "\\";
			
			string targetFileName = winDirectory + name + ".bat";
			using (Stream s = res.Read())
			{
				machine.PutFile(targetFileName, s);
			}
			
			ProgramResult result = machine.RunProgram(targetFileName, "", winDirectory, env);
			
			// Hoover up artifacts  *SLUURRRRRP*
			var artifacts = machine.ListFiles(winDirectory);
			foreach(var fileName in artifacts)
			{
				string tempFile = Path.GetTempFileName();
				using(FileStream fs = File.Create (tempFile))
				{
					machine.GetFile (fileName, fs);
					fs.Seek (0, SeekOrigin.Begin);
					bin.PutArtifact(testKey, fileName, fs);
				}
				File.Delete(tempFile);
			}
			
			bin.PutDetail("Standard Output", result.StandardOutput);
			bin.PutDetail("Standard Error", result.StandardError);
			
			// Get RESULTS
			if(result.ExitCode == 0)
			{
				return TestResult.PASSED;
			}
			else
			{
				return TestResult.FAILED;
			}
		}		

		public string[] FileExtensions {
			get {
				return new[] { ".bat" };
			}
		}

		public string Name {
			get {
				return "Windows Batch File";
			}
		}

		public string Description {
			get {
				return Name;
			}
		}

		public string Id {
			get {
				return "BatchFile";
			}
		}
	}
}

