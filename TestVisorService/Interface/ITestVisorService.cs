using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bizarrefish.TestVisorService.Interface
{	
	public delegate void TaskStateListener(TaskState newState);
	public delegate void TestRunListener(string testRunId, TaskState newState);
	
	public partial interface ITestVisorService
	{
		
		/// <summary>
		/// Starts a test run
		/// </summary>
		/// <returns>
		/// Test result id
		/// </returns>
		string EnqueueTestPlan(string testPlanId, IDictionary<string, string> args, TestRunListener listener);
	}
}

