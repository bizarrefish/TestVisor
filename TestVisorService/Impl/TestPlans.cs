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
using Bizarrefish.VMTestLib.JS;

namespace Bizarrefish.TestVisorService.Impl
{
	public partial class TestVisorService : ITestVisorService
	{
		TestPlanRepository tpr;
		
		string ResultsDirectory;

		TestInvocationManager tim;

		RedisResultCollection results;
		
		void InitTestPlans()
		{
			var client = new RedisClient(TestVisorService.RedisUri);

			tpr = new TestPlanRepository(client, baseDirectory + "/TestPlans", "TestPlans");
			ResultsDirectory = baseDirectory + "/TestResults";
			results = new RedisResultCollection(client, ResultsDirectory);

			tim = new TestInvocationManager(results, machines, tpr, testDriverManager);
			tim.Start();
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
			TestPlanInfo info = tpr.CreateTestPlan();
			info.Name = name;
			tpr.SetInfo(info);
			return info;
		}
		
		/// <summary>
		/// Sets the test plan's info.
		/// </summary>
		public void SetInfo(TestPlanInfo info)
		{
			tpr.SetInfo(info);
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

		TestRunInfo ToInfo(TestRun run)
		{
			var tri = new TestRunInfo();
			tri.Id = run.Id;
			tri.Name = run.Name;
			tri.When = run.When;
			tri.Description = "Test run on " + run.When;
			tri.Results = GetResultInfos(run.Id);
			return tri;
		}

		public IEnumerable<TestRunInfo> GetTestRuns(int start, int max)
		{
			IEnumerable<TestRun> runs = results.GetRuns(start, max);

			return runs.Select (ToInfo);
		}

		public TestRunInfo GetTestRun(string id)
		{
			var run = results.GetRun(id);
			if(run != null)
			{
				return ToInfo(run);
			}
			else
			{
				return null;
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
			return results.ReadArtifact(testRunId, resultId, artifactNumber);
		}

		
		/// <summary>
		/// Deletes a test result.
		/// </summary>
		public void DeleteRun(string runId)
		{
			results.DeleteRun(runId);
		}

		public string EnqueueTestPlan (string testPlanId, IDictionary<string, string> args, TestRunListener listener)
		{
			return tim.EnqueueTestPlan(testPlanId, args);
		}
		
		public SystemStatus GetCurrentStatus ()
		{
			return tim.GetCurrentStatus();
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
					TestResult res = driver.RunTest(name, machine,results.CreateResultBin(runId, "default"), env);
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
			}).Start();


		}

		public IDictionary<string, string> GetTestParams(string name)
		{
			var driver = testDriverManager.GetTestDriver(name);
			return driver.GetTestParamters(name);
		}

	}

	class TestProvider : IJSTestProvider
	{
		RedisResultCollection results;
		string runId;
		IEnumerable<IMachine> machines;
		IEnumerable<ITestDriver> drivers;

		public TestProvider(IEnumerable<IMachine> machines, IEnumerable<ITestDriver> drivers, RedisResultCollection results, string runId)
		{
			this.results = results;
			this.runId = runId;
			this.machines = machines;
			this.drivers = drivers;
		}

		public ITestResultBin CreateBin (string testKey)
		{
			return results.CreateResultBin(runId, testKey);
		}

		public void OnResult (string testKey, TestResult result)
		{
			results.SetResult(runId, testKey, result);
		}

		public IEnumerable<IMachine> GetMachines ()
		{
			return machines;
		}

		public IEnumerable<ITestDriver> GetTestDrivers ()
		{
			return drivers;
		}
	}
}

