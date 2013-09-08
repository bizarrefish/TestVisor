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

		public static Streams<VisorSessionData> streams;

		
		class WebArtifactInfo : ArtifactInfo
		{
			public string DownloadUrl;
			public WebArtifactInfo(ArtifactInfo ai, string url) : base(ai)
			{
				this.DownloadUrl = url;
			}
		}
		
		static WebArtifactInfo ProcessArtifactInfo(string runId, string testKey, int artifactIndex, ArtifactInfo ai)
		{
			return new WebArtifactInfo(ai, streams.GetArtifactUrl(runId, testKey, artifactIndex));
		}

		static TestRunInfo ProcessTestRunInfo(TestRunInfo tri)
		{
			var tri2 = new TestRunInfo(tri);
			tri2.Results = tri.Results
				.ToDictionary(a => a.Key, delegate(KeyValuePair<string, TestResultInfo> entry) {
					var replacement = new TestResultInfo(entry.Value);
					replacement.Artifacts = entry.Value.Artifacts
						.Select((ai, i) => ProcessArtifactInfo(tri.Id, entry.Key, i, ai)).ToArray();;
					return replacement;
				});
			return tri2;
		}

		public class GetTestResults : IAjaxMethod<VisorSessionData, TestRunInfo[]>
		{
			public string TestPlanId;
			public int Skip;
			public int Limit;


			public TestRunInfo[] Call(VisorSessionData session)
			{
				return tvs.GetTestRuns(Skip, Limit)
					.Where(tr => tr.TestPlanId == TestPlanId || true)
						.Select (ProcessTestRunInfo)					// Apply the test result URL
						.ToArray ();
			}
		}


		public class GetTestResult : IAjaxMethod<VisorSessionData, TestRunInfo>
		{
			public string Id;

			public TestRunInfo Call(VisorSessionData session)
			{
				return ProcessTestRunInfo(tvs.GetTestRun(Id));
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

