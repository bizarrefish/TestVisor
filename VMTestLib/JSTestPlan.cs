using System;
using System.Linq;

using System.Collections.Generic;
using Bizarrefish.VMLib;

using System.Globalization;
using System.Collections.Specialized;

using org.mozilla.javascript;
using org.mozilla.javascript.annotations;
using System.Threading;

namespace Bizarrefish.VMTestLib
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
	
	public class CallableTest : Callable
	{
		Func<IDictionary<string, string>, bool> func;
		
		public CallableTest(Func<IDictionary<string, string>, bool> func)
		{
			this.func = func;
		}
		
		public object call (Context c, Scriptable s1, Scriptable s2, object[] objarr)
		{
			IDictionary<string, string> testParams = new Dictionary<string, string>();
			
			if(objarr.Length > 0)
			{
				IdScriptableObject sObj = objarr[0] as IdScriptableObject;
				
				if(sObj != null)
				{
					foreach(object id in sObj.getIds())
					{
						testParams[id.ToString()] = sObj.get(id).ToString();
					}
				}
			}
			
			return Context.javaToJS(new java.lang.Boolean(func(testParams)), s1);
		}
	}
	
	public class DebugFunction : Callable
	{
		public object call (Context c, Scriptable s1, Scriptable s2, object[] objarr)
		{
			Console.WriteLine(objarr[0].ToString());
			return Undefined.instance;
		}
	}

	public class ObjectReturningFunction : Callable
	{
		object val;
		public ObjectReturningFunction(object val)
		{
			this.val = val;
		}
		
		public object call (Context c, Scriptable s1, Scriptable thisObj, object[] objarr)
		{
			return val;
		}
	}
	
	public class ActionFunction : Callable
	{
		Action act;
		public ActionFunction(Action act)
		{
			this.act = act;
		}
		
		public object call (Context c, Scriptable s1, Scriptable s2, object[] objarr)
		{
			act();
			return Undefined.instance;
		}
		
	}
	
	public class SnapshotObject : IdScriptableObject
	{
		IMachine machine;
		string snapshotId;
		
		public SnapshotObject(IMachine machine, string snapshotId)
		{
			this.machine = machine;
			this.snapshotId = snapshotId;
			defineProperty ("toString",
			                new ObjectReturningFunction(toString ()),
			                ScriptableObject.PERMANENT);
			
			defineProperty("Restore",
			               new ActionFunction(Restore ),
			               ScriptableObject.PERMANENT);
		}
		
		public override object getDefaultValue (java.lang.Class typeHint)
		{
			return toString();
		}
		
		public override string getClassName ()
		{
			return "TestVisorSnapshot";
		}
		
		public override string toString ()
		{
			return string.Format ("[{0}].Snapshot({1})", machine.Name, snapshotId);
		}
		
		public void Restore()
		{
			machine.Start(snapshotId);
		}
	}
	
	public class SnapshotFunction : Callable
	{
		IMachine machine;
		public SnapshotFunction(IMachine m)
		{
			machine = m;
		}
		
		public object call (Context c, Scriptable scope, Scriptable thisObj, object[] objarr)
		{
			var snapshotId = machine.MakeSnapshot();
			
			return new SnapshotObject(machine, snapshotId);
		}
	}
	
	public class JSTestRunner
	{
		Context ctx;
		ScriptableObject scope;
		
		
		public JSTestRunner (IEnumerable<ITestDriver> testDrivers, IMachine machine, ITestResultBin results)
		{
			this.ctx = Context.enter ();
			
			var tests = testDrivers.SelectMany(d =>
			                                   d.Tests.Select (t => new { Driver = d, Name = t}));
			
			this.scope = ctx.initStandardObjects();
			foreach(var test in tests)
			{
				Func<IDictionary<string, string>, bool> runFunc = delegate(IDictionary<string, string> arg) {
					if(arg == null) arg = new Dictionary<string, string>();
					return test.Driver.RunTest(test.Name, machine, results, arg) == TestResult.PASSED;
				};
				
				scope.defineProperty(test.Name, new CallableTest(runFunc), ScriptableObject.PERMANENT);
			}
			
			scope.defineProperty("Debug", new DebugFunction(), ScriptableObject.PERMANENT);
			
			scope.defineProperty("Snapshot", new SnapshotFunction(machine), ScriptableObject.PERMANENT);

		}
		
		public string TestCode(string testName)
		{
			return "//Run test: " + testName + "\n" + testName + "({});\n";
		}
		
		public object Execute(string javascript)
		{
			return ctx.evaluateString(scope, javascript, "script", 0, null);
		}
	}
}

