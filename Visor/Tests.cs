using System;
using Bizarrefish.TestVisorService.Interface;
using Bizarrefish.WebLib;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Visor
{
	public class Tests
	{
		public static ITestVisorService tvs;
		public static Streams<VisorSessionData> streams;

		public class GetTests : IAjaxMethod<VisorSessionData, TestInfo[]>
		{
			public TestInfo[] Call(VisorSessionData session)
			{
				return tvs.Tests.ToArray();
			}
		}

		public class GetTestUploadUrl : IAjaxMethod<VisorSessionData, string>
		{
			public string TypeId;
			public string Name;

			public string Call(VisorSessionData session)
			{
				return streams.GetTestUploadUrl(Name, TypeId);
			}
		}
	}
}

