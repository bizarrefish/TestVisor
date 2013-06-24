using System;
using org.mozilla.javascript;
using Bizarrefish.VMLib;
using System.Collections.Generic;
using System.Linq;

namespace Bizarrefish.VMTestLib.JS
{
	public delegate void MachineInitFunc(IMachine machine);

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

	public class MachineResolver
	{
		IJSTestProvider provider;

		public MachineResolver(IJSTestProvider provider)
		{
			this.provider = provider;
		}

		public IMachine Resolve(dynamic d)
		{
			return GetMachine (d);
		}

		IMachine GetMachine(IMachine m)
		{
			return m;
		}

		IMachine GetMachine(string nameOrId)
		{
			// Search for machine by id:
			IEnumerable<IMachine> machines =
				provider.GetMachines().Where (m => m.Id == nameOrId);

			if(machines.Any ()) return machines.First ();

			machines = provider.GetMachines().Where (m => m.Name == nameOrId);

			if(machines.Any ()) return machines.First ();

			throw new Exception("Unable to find machine for string: " + nameOrId);
		}

		IMachine GetMachine(object o)
		{
			throw new Exception("Unexpected argument type");
		}
	}
}

