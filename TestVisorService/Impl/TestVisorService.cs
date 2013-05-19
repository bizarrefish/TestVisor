using System;
using System.Collections.Generic;
using System.Linq;

using Bizarrefish.VMLib;
using Bizarrefish.TestVisorService.Interface;
using Bizarrefish.VMTestLib;
using Bizarrefish.VMTestLib.TestDrivers.WindowsBatch;
using Bizarrefish.VMLib.Virtualbox;
using System.IO;

namespace Bizarrefish.TestVisorService.Impl
{
	public partial class TestVisorService : ITestVisorService
	{
		public static Uri RedisUri = new Uri("redis://localhost:6379");

		string baseDirectory;
		public TestVisorService(string baseDirectory)
		{
			this.baseDirectory = baseDirectory;
			InitMachines();
			InitTests ();
			InitTestPlans ();
		}
	}
}

