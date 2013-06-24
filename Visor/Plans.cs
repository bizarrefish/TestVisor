using System;
using Bizarrefish.TestVisorService.Interface;
using Bizarrefish.WebLib;
using System.Collections.Generic;
using System.Linq;
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

		public class Create : IAjaxMethod<VisorSessionData, TestPlanInfo>
		{
			public TestPlanInfo Info;

			public TestPlanInfo Call(VisorSessionData session)
			{
				var tpi = tvs.CreateTestPlan("");
				Info.Id = tpi.Id;
				tvs.SetInfo(Info);
				return Info;
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
			public IDictionary<string, string> Arguments;

			public string Call(VisorSessionData session)
			{
				return tvs.EnqueueTestPlan(TestPlanId, Arguments, (testRunId, newState) => { });
			}
		}
	}
}

