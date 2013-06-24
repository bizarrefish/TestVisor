using System;
using System.Linq;

using System.Collections.Generic;
using Bizarrefish.VMLib;

using System.Globalization;
using System.Collections.Specialized;

using org.mozilla.javascript;
using org.mozilla.javascript.annotations;
using System.Threading;

namespace Bizarrefish.VMTestLib.JS
{
	public class JSScope : org.mozilla.javascript.ScriptableObject
	{
		
		public JSScope()
		{
		}
		
		public void Debug(string debugStr)
		{
			Console.WriteLine (debugStr);
		}

		public override string getClassName ()
		{
			return "TestVisor";
		}
	}

	public interface IJSTestProvider
	{
		ITestResultBin CreateBin(string testKey);

		void OnResult(string testKey, TestResult tr);

		IEnumerable<IMachine> GetMachines();

		IEnumerable<ITestDriver> GetTestDrivers();
	}
	
	public class JSTestRunner : IDisposable
	{
		Context ctx;
		ScriptableObject scope;

		SnapshotFunction ssFunc;
		string initSnapshotName;

		IDictionary<string, string> planArguments;

		ISet<string> usedMachines = new HashSet<string>();

		IJSTestProvider provider;

		string GetInitSnapshotId(IMachine m)
		{
			return m.GetSnapshots()
					.Where (ss => ss.Name == initSnapshotName)
						.Select (ss => ss.Id)
						.FirstOrDefault();
		}

		/// <summary>
		/// If the machine has not been accessed before, revert it to initial snapshot
		/// </summary>
		void PossiblyInitializeMachine(IMachine m)
		{
			if(!usedMachines.Contains(m.Id))
			{
				usedMachines.Add(m.Id);

				string ssId = GetInitSnapshotId(m);

				if(ssId != null)
				{
					m.Start(ssId);
				}
			}
		}

		public JSTestRunner (string initSnapshotName, IDictionary<string, string> planArguments, IJSTestProvider provider)
		{

			this.ctx = Context.enter ();
			this.provider = provider;
			this.initSnapshotName = initSnapshotName;
			var tests = provider.GetTestDrivers().SelectMany(d =>
			                                   d.Tests.Select (t => new { Driver = d, Name = t}));

			this.planArguments = planArguments;

			MachineInitFunc initFunc = PossiblyInitializeMachine;

			this.scope = ctx.initStandardObjects();
			foreach(var thisTest in tests)
			{
				var test = thisTest;
				TestFunc func = delegate(IMachine machine, IDictionary<string, string> arguments, ITestResultBin bin)
				{
					return test.Driver.RunTest(test.Name, machine, bin, arguments);
				};

				scope.defineProperty(test.Name, new CallableTest(func, initFunc, planArguments, provider), ScriptableObject.READONLY + ScriptableObject.PERMANENT);
			}

			var jsArgsObject = ctx.newObject(scope);
			foreach(var arg in planArguments)
			{
				jsArgsObject.put (arg.Key, jsArgsObject, arg.Value);
			}

			foreach(var name in new[] { "args", "Args", "ARGS"})
			{
				scope.defineProperty(name, jsArgsObject, ScriptableObject.READONLY + ScriptableObject.PERMANENT);
			}

			scope.defineProperty("Debug", new DebugFunction(), ScriptableObject.READONLY + ScriptableObject.PERMANENT);
			
			ssFunc = new SnapshotFunction(initFunc, provider);
			
			scope.defineProperty("Snapshot", ssFunc, ScriptableObject.READONLY + ScriptableObject.PERMANENT);

		}
		
		public string TestCode(string testName)
		{
			return "//Run test: " + testName + "\n" + testName + "({});\n";
		}
		
		public object Execute(string javascript, IDictionary<string, string> parameters)
		{
			Script script = null;
			// Compile script
			try
			{
				script = ctx.compileString(javascript, "Javascript Test Plan", 1, null);
			}
			catch (RhinoException ex)
			{
				throw new Exception("Compile error at line " + ex.lineNumber() + ": " + ex.details());
			}

			try
			{
				var parmScope = ctx.newObject(scope);

				foreach(var parm in parameters)
				{
					parmScope.put (parm.Key, parmScope, parm.Value);
				}

				return script.exec(ctx, parmScope);
			}
			catch(RhinoException err)
			{
				throw new Exception("Runtime error: " + err.getMessage());
			}
		}

		public void CleanUp()
		{
			if(usedMachines.Count > 0)
			{
				foreach(IMachine machine in provider.GetMachines())
				{
					if(usedMachines.Contains (machine.Id))
					{
						// Revert to initial snapshot
						string ssId = GetInitSnapshotId(machine);
						if(ssId != null)
						{
							machine.Start (ssId);
						}

						// Remove the junk from our trunk
						ssFunc.CleanupSnapshots();

						// Shutdown VM
						machine.Shutdown();
					}
				}

				usedMachines.Clear ();
			}
		}

		public void Dispose ()
		{
			CleanUp ();
		}

	}
}

