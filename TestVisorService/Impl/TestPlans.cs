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
				
		/// <summary>
		/// List of test results. Most recent first.
		/// </summary>
		public IEnumerable<TestRunInfo> TestRuns
		{
			get
			{
				return results.GetRuns().Select(delegate(string runId)
				{
					return new TestRunInfo()
					{
						Id = runId,
						Description = "Test Run: " + runId,
						Results = GetResultInfos(runId)
					};
				});
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
			
			string runId = "Test run on: " + DateTime.Now;

			string initSnapshotId = machine.GetSnapshots().Where (ss => ss.Name == "TEST_INIT").First ().Id;

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
					throw e;
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

		public void RunStandaloneTest(string name, IMachine machine, IDictionary<string, string> env, Action<TestResult> resultFunc)
		{
			string runId = "Standalone test run on: " + DateTime.Now;


			var driver = testDriverManager.GetTestDriver(name);

			new Thread(() =>
			{
				try
				{
					TestResult res = driver.RunTest(name, "standalone test", machine,results.CreateResultBin(runId, "default"), env);
					results.SetResult(runId, "default", res);
				}
				catch(Exception e)
				{
					resultFunc(new TestResult()
					{
						Success = false,
						StandardError = e.Message
					});
				}

			}).Start();
		}

		public IDictionary<string, string> GetTestParams(string name)
		{
			var driver = testDriverManager.GetTestDriver(name);
			return driver.GetTestParamters(name);
		}
	}
}

