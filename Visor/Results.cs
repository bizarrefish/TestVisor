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
		
		public class SetInfo : IAjaxMethod<VisorSessionData, bool>
		{
			public TestPlanInfo Info;

			public bool Call(VisorSessionData session)
			{
				tvs.SetInfo(Info);
				return true;
			}
		}

		public class GetSource : IAjaxMethod<VisorSessionData, string>
		{
			public string TestPlanId;

			public string Call(VisorSessionData session)
			{
				using(Stream s = tvs.ReadTestPlan(TestPlanId))
				{
					using(StreamReader reader = new StreamReader(s))
					{
						return reader.ReadToEnd();
					}
				}
			}
		}

		public class SetSource : IAjaxMethod<VisorSessionData, bool>
		{
			public string TestPlanId;
			public string Source;

			public bool Call(VisorSessionData session)
			{
				using(Stream s = tvs.WriteTestPlan(TestPlanId))
				{
					using(StreamWriter writer = new StreamWriter(s))
					{
						writer.Write(Source);
						return true;
					}
				}
			}
		}

		/// Starts a test plan on a machine
		/// Returns the test run id
		public class Start : IAjaxMethod<VisorSessionData, string>
		{
			public string TestPlanId;
			public string MachineId;

			public string Call(VisorSessionData session)
			{
				return tvs.EnqueueTestPlan(MachineId, TestPlanId, (testRunId, newState) => { });
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
				Thread.Sleep(1000);	// For realism
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

