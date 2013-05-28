using System;
using Bizarrefish.WebLib;
using Bizarrefish.TestVisorService.Interface;
using System.Collections.Generic;
using Bizarrefish.VMTestLib;
using System.Linq;

namespace Visor
{
	public class GetTestResultsRequest
	{
		public string TestPlanId;
		public int Skip;
		public int Limit;
	}

	public partial class Visor
	{
		static TestRunInfo[] GetTestResults(GetTestResultsRequest req, VisorSessionData session)
		{
			return tvs.TestRuns
				.Where(tr => tr.TestPlanId == req.TestPlanId || true)
				.Skip(req.Skip)
					.Take (req.Limit).ToArray ();
		}

	}
}

