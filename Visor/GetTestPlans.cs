using System;
using Bizarrefish.WebLib;
using Bizarrefish.TestVisorService.Interface;
using System.Collections.Generic;
using Bizarrefish.VMTestLib;
using System.Linq;

namespace Visor
{
	public class GetTestPlansRequest
	{
	}

	public class RunTestPlanRequest
	{
		public string MachineId;
		public string TestPlanId;
	}

	public partial class Visor
	{
		static TestPlanInfo[] GetTestPlans(GetTestPlansRequest req, VisorSessionData session)
		{
			return tvs.TestPlans.ToArray();
		}

		static TestResult RunTestPlan(RunTestPlanRequest req, VisorSessionData session)
		{
			tvs.EnqueueTestPlan(req.MachineId, req.TestPlanId, delegate(string testRunId, TaskState newState) {

			});

			return new TestResult();
		}
	}
}

