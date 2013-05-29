using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bizarrefish.VMTestLib;
using Bizarrefish.TestVisorService.Interface;
using System.Threading;

using Bizarrefish.TestVisorStorage;
using Bizarrefish.VMLib;
using ServiceStack.Redis;

namespace Bizarrefish.TestVisorService.Impl
{
	public partial class TestVisorService : ITestVisorService
	{
		TestPlanRepository tpr;
		
		string ResultsDirectory;

		RedisResultCollection results;
		
		void InitTestPlans()
		{
			tpr = new TestPlanRepository(baseDirectory + "/TestPlans", "TestPlans");
			ResultsDirectory = baseDirectory + "/TestResults";
			results = new RedisResultCollection(new RedisClient(TestVisorService.RedisUri), ResultsDirectory);
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

		public IEnumerable<TestRunInfo> GetTestRuns(int start, int max)
		{
			IEnumerable<TestRun> runs = results.GetRuns(start, max);

			foreach(var run in runs)
			{
				var tri = new TestRunInfo();
				tri.Id = run.Id;
				tri.Name = run.Name;
				tri.When = run.When;
				tri.Description = "Test run on " + run.When;
				tri.Results = GetResultInfos(run.Id);
				yield return tri;
			}
		}


		IDictionary<string, TestResultInfo> GetResultInfos(string runId)
		{
			return results.GetResults(runId)
				.ToDictionary(entry => entry.Key, delegate(KeyValuePair<string, TestResult> entry) {
					var retVal = new TestResultInfo();
					retVal.Result = entry.Value;
					retVal.Artifacts = results.GetArtifacts(runId, entry.Key);
					return retVal;
				});
		}
		
		/// <summary>
		/// Read an artifact
		/// </summary>
		public Stream ReadArtifact(string testRunId, string resultId, int artifactNumber)
		{
			throw new NotImplementedException();
		}

		
		/// <summary>
		/// Deletes a test result.
		/// </summary>
		public void DeleteRun(string runId)
		{
			results.DeleteRun(runId);
		}
		
		public string EnqueueTestPlan (string machineId, string testPlanId, TestRunListener listener)
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

			string initSnapshotId = machine.GetSnapshots().Where (ss => ss.Name == "TEST_INIT").First ().Id;

			string runId = results.CreateRun("Run at " + DateTime.Now.TimeOfDay);

			listener(runId, TaskState.PENDING);
			Thread t = new Thread(delegate()
			{
				// Open "TEST_INIT" snapshot
				machine.Start(initSnapshotId);
				
				JSTestRunner runner = new JSTestRunner(testDriverManager.Drivers, machine,
				                                       testKey => results.CreateResultBin(runId, testKey),
				                                       (testKey, result) => results.SetResult(runId, testKey, result));
				listener(runId, TaskState.RUNNING);
				try
				{
					// Run our javascript
					runner.Execute(testPlanCode);
					listener(runId, TaskState.COMPLETE);
				}
				catch(Exception e)
				{
					listener(runId, TaskState.FAILED);
				}
				
				// Revert to initial snapshot
				machine.Start(initSnapshotId);
				
				// Delete accumulated snapshots
				runner.DeleteSnapshots();
				
				// Shutdown VM
				machine.Shutdown();	
			});
			
			t.Start();
			
			return runId;
		}

		public void RunStandaloneTest(string name, string machineId, IDictionary<string, string> env, Action<TestRunInfo> resultFunc)
		{
			string runId = "Standalone test run on: " + DateTime.Now;

			var driver = testDriverManager.GetTestDriver(
					testDriverManager.Tests.Where (t => t.Id == name)
					.First ().TestTypeId);

			var machine = machines.GetMachine(machineId);

			string initSnapshotId = machine.GetSnapshots().Where (ss => ss.Name == "TEST_INIT").First ().Id;

			new Thread(() =>
			{
				try
				{
					machine.Start(initSnapshotId);
					TestResult res = driver.RunTest(name, "default", machine,results.CreateResultBin(runId, "default"), env);
					results.SetResult(runId, "default", res);
				}
				catch(Exception e)
				{
					results.SetResult(runId, "default", new TestResult()
					{
						Success = false,
						StandardError = e.Message
					});
				}
				//resultFunc(GetTestRunInfo(runId));
			}).Start();


		}

		public IDictionary<string, string> GetTestParams(string name)
		{
			var driver = testDriverManager.GetTestDriver(name);
			return driver.GetTestParamters(name);
		}
	}
}

