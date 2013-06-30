using System;
using System.Collections.Generic;
using System.IO;

namespace Bizarrefish.TestVisorService.Interface
{
	public partial interface ITestVisorService
	{
		/// <summary>
		/// List of test plans.
		/// </summary>
		IEnumerable<TestPlanInfo> TestPlans { get; }
		
		/// <summary>
		/// Creates a new, empty test plan.
		/// </summary>
		TestPlanInfo CreateTestPlan(string name);
		
		/// <summary>
		/// Sets the test plan's info.
		/// </summary>
		void SetInfo(TestPlanInfo info);
		
		/// <summary>
		/// Deletes a test plan.
		/// </summary>
		/// <param name='id'>
		/// Test plan id.
		/// </param>
		void DeleteTestPlan(string id);
		
		/// <summary>
		/// Gets a stream to read a test plan's contents.
		/// </summary>
		Stream ReadTestPlan(string id);
		
		/// <summary>
		/// Gets a stream to write a test plan's contents.
		/// </summary>
		Stream WriteTestPlan(string id);
				
		/// <summary>
		/// List of test results. Most recent first.
		/// </summary>
		IEnumerable<TestRunInfo> GetTestRuns(int start, int max);

		/// <summary>
		/// Gets a single test run
		/// </summary>
		TestRunInfo GetTestRun(string id);

		/// <summary>
		/// Read an artifact
		/// </summary>
		Stream ReadArtifact(string testRunId, string resultId, int artifactNumber);

		/// <summary>
		/// Deletes a test result.
		/// </summary>
		void DeleteRun(string testResultId);

		/// <summary>
		/// Run Standalone test
		/// </summary>
		void RunStandaloneTest(string name, string machineId, IDictionary<string, string> env, Action<TestRunInfo> resultFunc);
	}
}

