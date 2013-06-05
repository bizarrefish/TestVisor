using System;
using Bizarrefish.WebLib;
using Bizarrefish.TestVisorService.Interface;
using System.Collections.Generic;
using Bizarrefish.VMTestLib;
using System.Linq;
using System.Threading;

namespace Visor
{

	public class Plans
	{
		public static ITestVisorService tvs;
		public class GetTestPlans : IAjaxMethod<VisorSessionData, TestPlanInfo[]>
		{
			public TestPlanInfo[] Call (VisorSessionData session)
			{
				return tvs.TestPlans.ToArray();
			}
		}
	}

	public class Results
	{
		public static ITestVisorService tvs;

		public class GetTestResults : IAjaxMethod<VisorSessionData, TestRunInfo[]>
		{
			public string TestPlanId;
			public int Skip;
			public int Limit;

			public TestRunInfo[] Call(VisorSessionData session)
			{
				Thread.Sleep(3000);
				return tvs.GetTestRuns(Skip, Limit)
					.Where(tr => tr.TestPlanId == TestPlanId || true).ToArray ();
			}
		}
	}
}

