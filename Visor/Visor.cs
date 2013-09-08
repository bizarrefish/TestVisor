using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using Bizarrefish.TestVisorService.Interface;
using Bizarrefish.TestVisorService.Impl;
using System.IO;
using Bizarrefish.WebLib;
using Bizarrefish.VMTestLib;


namespace Visor
{
	public class VisorSessionData
	{
	}


	public partial class Visor
	{
		static ITestVisorService tvs;

		static IAjaxHandler<VisorSessionData> ajaxHandler;

		static Bizarrefish.WebLib.HTTPServer<VisorSessionData> server;

		public static void Main(string[] args)
		{
			server = new Bizarrefish.WebLib.HTTPServer<VisorSessionData>(25565,"../../WebStatic");

			ajaxHandler = new AjaxHandler<VisorSessionData>(server);

			tvs = new TestVisorService(Directory.GetCurrentDirectory());
			
			Streams<VisorSessionData> streams = new Streams<VisorSessionData>(server, tvs);


			Results.tvs = tvs;
			Results.streams = streams;
			Plans.tvs = tvs;
			Machines.tvs = tvs;
			Status.tvs = tvs;
			Tests.tvs = tvs;
			Tests.streams = streams;

			ajaxHandler.AddClass<Results>();
			ajaxHandler.AddClass<Plans>();
			ajaxHandler.AddClass<Machines>();
			ajaxHandler.AddClass<Status>();
			ajaxHandler.AddClass<Tests>();

			File.Delete("../../ajax.js");
			File.WriteAllText("../../WebStatic/ajax.js", ajaxHandler.GetJavascript());

			server.Start();

		}
	}
}

