using System;
using org.mozilla.javascript;
using Bizarrefish.VMLib;
using System.Collections.Generic;
using System.Linq;

namespace Bizarrefish.VMTestLib.JS
{
	public delegate TestResult TestFunc(IMachine machine, IDictionary<string, string> arguments, ITestResultBin bin);

	class CallableTest : Callable
	{
		const string ARG_TESTKEY = "Key";
		const string ARG_MACHINE = "Machine";
		const string DEFAULT_TESTKEY = "default";

		IJSTestProvider provider;
		MachineInitFunc initFunc;

		IDictionary<string, string> planArguments;

		TestFunc func;

		public CallableTest(TestFunc func, MachineInitFunc initFunc, IDictionary<string, string> planArguments, IJSTestProvider provider)
		{
			this.provider = provider;
			this.planArguments = planArguments;
			this.func = func;
			this.initFunc = initFunc;
		}
		
		public object call (Context c, Scriptable s1, Scriptable s2, object[] objarr)
		{
			IDictionary<string, object> testParams = new Dictionary<string, object>();

			IMachine machine = null;

			string testKey = DEFAULT_TESTKEY;

			// Initialize test arguments with plan arguments.
			foreach(var arg in planArguments)
			{
				testParams[arg.Key] = arg.Value;
			}


			// Override above with user arguments
			if(objarr.Length > 0)
			{
				IdScriptableObject sObj = objarr[0] as IdScriptableObject;

				if(sObj != null)
				{
					foreach(object id in sObj.getIds())
					{
						object value = sObj.get (id);
						testParams[id.ToString()] = value;
					}
				}
			}

			// Find magic arguments
			foreach(var parm in testParams)
			{
				object value = parm.Value;

				switch(parm.Key)
				{
				case ARG_TESTKEY:
					testKey = value.ToString();
					break;

				case ARG_MACHINE:
					machine = new MachineResolver(provider).Resolve(value);
					break;
				}
			}

			if(machine == null)
			{
				throw new Exception("Test requires '" + ARG_MACHINE + "' argument");
			}

			var stringParams = testParams.ToDictionary(e => e.Key, e => e.Value.ToString());

			return Context.javaToJS(InvokeTest (machine, testKey, stringParams), s1);
		}

		TestResult InvokeTest(IMachine machine, string testKey, IDictionary<string, string> parameters)
		{
			initFunc(machine);
			var testResult = func(machine, parameters, provider.CreateBin(testKey));
			provider.OnResult(testKey, testResult);
			return testResult;
		}
	}
}

