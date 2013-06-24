using System;

using System.Collections.Generic;
using Bizarrefish.VMLib;
using System.IO;
using System.Linq;

namespace Bizarrefish.VMTestLib.TestDrivers.WindowsBatch
{
	public class BatchFileDriver : ITestDriver
	{
		const string TestPathPrefix = "C:\\WindowsBatchFileTests\\";

		class BatchFileTest
		{
			public string Name;
			public IDictionary<string, string> Parameters = new Dictionary<string, string>();
		}

		class BatchFileDriverData
		{
			public IList<BatchFileTest> Tests = new List<BatchFileTest>();
		}
		
		public ITestRepository Repo { get; set; }
		public IEnumerable<string> Tests {
			get
			{
				var data = Repo.Load<BatchFileDriverData>();
				return data.Tests.Select(t => t.Name);
			}
		}
		
		
		public void InstallTest (string name, Stream source)
		{
			var res = Repo.CreateResource(name);
			res.Write(source);

			BatchFileTest bft = new BatchFileTest();

			using(Stream s = res.Read ())
			{
				using(StreamReader rd = new StreamReader(s))
				{
					while(!rd.EndOfStream)
					{
						string line = rd.ReadLine ();
						if(!line.StartsWith ("REM TESTPARAM"))
							break;

						string[] parts = line.Split (' ');
						string paramName = parts[2];
						string paramDesc = "";
						for(int i = 3; i < parts.Length; i++)
							paramDesc = paramDesc + parts[i] + " ";

						bft.Parameters[paramName] = paramDesc;
					}
				}
			}
			bft.Name = name;

			var data = Repo.Load<BatchFileDriverData>();
			data.Tests.Add(bft);
			Repo.Store (data);
		}
		 
		public void RemoveTest (string name)
		{
			Repo.DeleteResource(name);
			var data = Repo.Load <BatchFileDriverData>();
			data.Tests.Remove(data.Tests.Where(t => t.Name == name).First ());
			Repo.Store (data);
		}

		public TestResult RunTest (string name, IMachine machine, ITestResultBin bin, IDictionary<string, string> env)
		{
			ITestResource res = Repo.GetResource(name);

			TestResult tr = new TestResult();
			try
			{
				string winDirectory = TestPathPrefix + name + "\\" + DateTime.Now.Ticks + "\\";
				
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
						bin.PutArtifact(new ArtifactInfo()
						{
							Name = fileName,
							Description = fileName,
							FileName = fileName
						}, fs);
					}
					File.Delete(tempFile);
				}
				tr.Success = result.ExitCode == 0;
			} catch (Exception e)
			{
				tr.Success = false;
				tr.StandardError = e.Message;
			}


			return tr;
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

		public IDictionary<string, string> GetTestParamters (string testName)
		{
			var data = Repo.Load<BatchFileDriverData>();
			return data.Tests.Where(t => t.Name == testName).First ().Parameters;
		}

	}
}

