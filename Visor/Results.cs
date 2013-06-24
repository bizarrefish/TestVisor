using System;
using Bizarrefish.WebLib;
using Bizarrefish.TestVisorService.Interface;
using System.Collections.Generic;
using Bizarrefish.VMTestLib;
using System.Linq;
using System.Threading;
using System.IO;

namespace Visor
{

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
				//Thread.Sleep(1000);	// For realism
				return tvs.GetTestRuns(Skip, Limit)
					.Where(tr => tr.TestPlanId == TestPlanId || true).ToArray ();
			}
		}
	}

	public class Machines
	{
		public static ITestVisorService tvs;
		public class GetMachines : IAjaxMethod<VisorSessionData, MachineInfo[]>
		{
			public MachineInfo[] Call(VisorSessionData session)
			{
				return tvs.Machines.ToArray();
			}
		}
	}
}

