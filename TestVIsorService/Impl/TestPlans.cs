using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bizarrefish.VMTestLib;
using Bizarrefish.TestVisorService.Interface;
using System.Threading;

namespace Bizarrefish.TestVisorService.Impl
{
	public partial class TestVisorService : ITestVisorService
	{
		TestPlanRepository tpr;
		
		string ResultsDirectory;
		
		void InitTestPlans()
		{
			tpr = new TestPlanRepository(Directory.GetCurrentDirectory() + "/TestPlans");
			ResultsDirectory = Directory.GetCurrentDirectory() + "/TestResults";
		}
		
		/// <summary>
		/// List of test plans.
		/// </summary>
		public IEnumerable<TestPlanInfo> TestPlans
		{
			get { return tpr.TestPlans; }
		}
		
		/// <summary>
		/// Creates a new, empty test plan.
		/// </summary>
		public TestPlanInfo CreateTestPlan(string name)
		{
			tpr.CreateTestPlan(name);
			return tpr.TestPlans.Where(tp => tp.Id == name).First ();
		}
		
		/// <summary>
		/// Sets the test plan's info.
		/// </summary>
		public void SetInfo(TestPlanInfo info)
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Deletes a test plan.
		/// </summary>
		/// <param name='id'>
		/// Test plan id.
		/// </param>
		public void DeleteTestPlan(string id)
		{
			tpr.DeleteTestPlan(id);
		}
		
		/// <summary>
		/// Gets a stream to read a test plan's contents.
		/// </summary>
		public Stream ReadTestPlan(string id)
		{
			return tpr.ReadTestPlan(id);
		}
		
		/// <summary>
		/// Gets a stream to write a test plan's contents.
		/// </summary>
		public Stream WriteTestPlan(string id)
		{
			return tpr.WriteTestPlan(id);
		}
				
		/// <summary>
		/// List of test results. Most recent first.
		/// </summary>
		public IEnumerable<TestResultInfo> TestResults
		{
			get
			{
				return Directory.GetFileSystemEntries(ResultsDirectory)
					.Select (resDir => new TestResultInfo()
					{
						Id = Path.GetFileName(resDir),
						Name = Path.GetFileName (resDir),
						Description = "A test plan run",
						Success = File.Exists (resDir + "/success"),
						Detail = "",
						Artifacts = GetArtifactInfos(resDir)
					});
			}
		}
		
		
		IEnumerable<ArtifactInfo> GetArtifactInfos(string resultDir)
		{
			FileBasedResultBin resultBin = new FileBasedResultBin(resultDir);
			foreach(var artifact in resultBin.GetArtifactPaths())
			{
				ArtifactInfo ai = new ArtifactInfo()
				{
					Id = artifact.Key,
					Name = artifact.Key,
					Description = artifact.Key,
					Length = new FileInfo(artifact.Value).Length,
					OpenStream = () => File.OpenRead (artifact.Value)
				};
				
				yield return ai;
			}
		}
		
		
		
		/// <summary>
		/// Deletes a test result.
		/// </summary>
		public void DeleteResult(string testResultId)
		{
			throw new NotImplementedException();
		}
		
		public void EnqueueTestPlan (string machineId, string testPlanId, TaskStateListener listener)
		{
			var machine = machines.GetMachine (machineId);
			
			string testPlanCode;
			using(Stream tpStream = tpr.ReadTestPlan(testPlanId))
			{
				using(StreamReader reader = new StreamReader(tpStream))
				{
					testPlanCode = reader.ReadToEnd();
				}
			}
			
			FileBasedResultBin results = new FileBasedResultBin(ResultsDirectory + "/" + DateTime.UtcNow.Ticks);
			
			JSTestRunner runner = new JSTestRunner(testDriverManager.Drivers, machine, results);
			
			listener(TaskState.PENDING);
			Thread t = new Thread(delegate()
			{
				listener(TaskState.RUNNING);
				try
				{
					runner.Execute(testPlanCode);
					listener(TaskState.COMPLETE);
				}
				catch(Exception)
				{
					listener(TaskState.FAILED);
				}
				
			});
			
			t.Start();
		}
	}
}

