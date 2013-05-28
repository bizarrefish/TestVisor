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
	class VisorSessionData
	{
	}

	public partial class Visor
	{
		static ITestVisorService tvs;

		static IAjaxHandler<VisorSessionData> ajaxHandler;

		static Bizarrefish.WebLib.HTTPServer<VisorSessionData> server;

		static void AddFunc<TRequest, TResponse>(string endpointName, AjaxFunction<TRequest,VisorSessionData, TResponse> func)
			where TRequest : class
			where TResponse : class
		{
			ajaxHandler.AddEndpoint<TRequest, TResponse>(endpointName, func);
			server.AddFunc(endpointName);
		}

		public static void Main(string[] args)
		{
			server = new Bizarrefish.WebLib.HTTPServer<VisorSessionData>(8080);

			ajaxHandler = new AjaxHandler<VisorSessionData>();

			tvs = new TestVisorService(Directory.GetCurrentDirectory());

			// Web functions:

			AddFunc<GetTestPlansRequest, IEnumerable<TestPlanInfo>>("GetTestPlans", GetTestPlans);
			AddFunc<RunTestPlanRequest, TestResult>("RunTestPlan", RunTestPlan);
			AddFunc<GetTestResultsRequest, IEnumerable<TestRunInfo>>("GetTestResults", GetTestResults);
			
			File.Delete("../../ajax.js");
			File.WriteAllText("../../ajax.js", ajaxHandler.GetJavascript());

			server.Start (ajaxHandler.Handle);

		}
	}
}

