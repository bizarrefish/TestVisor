using System;
using org.mozilla.javascript;
using Bizarrefish.VMLib;

namespace Bizarrefish.VMTestLib.JS
{

	public class SnapshotFunction : Callable
	{
		/// <summary>
		/// IDs of snapshots taken at the behest of this test plan.
		/// </summary>
		public Action CleanupSnapshots = () => {};

		MachineInitFunc initFunc;

		IJSTestProvider provider;
		public SnapshotFunction(MachineInitFunc initFunc, IJSTestProvider provider)
		{
			this.initFunc = initFunc;
			this.provider = provider;
		}


		public object call (Context c, Scriptable scope, Scriptable thisObj, object[] objarr)
		{
			if(objarr.Length < 1)
			{
				throw new Exception("Snapshot function requires machine argument");
			}

			IMachine machine = new MachineResolver(provider).Resolve(objarr[0]);

			initFunc(machine);

			var snapshotId = machine.MakeSnapshot();

			// We need to clean up the snapshots at some point.
			Action oldAction = CleanupSnapshots;
			CleanupSnapshots = () =>
			{
				machine.DeleteSnapshot(snapshotId);
				oldAction();
			};
			
			return new SnapshotObject(machine, snapshotId);
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
}

