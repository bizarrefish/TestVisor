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
			server = new Bizarrefish.WebLib.HTTPServer<VisorSessionData>(8080,"../../WebStatic");

			ajaxHandler = new AjaxHandler<VisorSessionData>(server);

			tvs = new TestVisorService(Directory.GetCurrentDirectory());

			Results.tvs = tvs;
			Plans.tvs = tvs;
			ajaxHandler.AddClass<Results>();
			ajaxHandler.AddClass<Plans>();

			File.Delete("../../ajax.js");
			File.WriteAllText("../../WebStatic/ajax.js", ajaxHandler.GetJavascript());

			server.Start();

		}
	}
}

