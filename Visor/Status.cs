using System;
using Bizarrefish.TestVisorService.Interface;
using Bizarrefish.WebLib;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Visor
{
	public class Status
	{
		public static ITestVisorService tvs;

		public class GetCurrentStatus : IAjaxMethod<VisorSessionData, SystemStatus>
		{
			public SystemStatus Call (VisorSessionData session)
			{
				return tvs.GetCurrentStatus();
			}
		}
	}
}

