using System;
using System.Linq;
using System.Collections.Generic;

namespace Bizarrefish.VMLib.Virtualbox
{

	
	public class VirtualboxDriver : IVMDriver<VirtualboxMachine>
	{	
		IDictionary<string, VirtualboxMachine> machineMap =
			new Dictionary<string, VirtualboxMachine>();
		
		public IEnumerable<VirtualboxMachine> Machines {
			get {
				return machineMap.Values;
			}
		}
		
		public string Name {
			get {
				return "Virtualbox";
			}
		}
		
		void LoadMachines()
		{
			machineMap.Clear();
			IEnumerable<string> machineStrings;
			VirtualboxUtils.VBoxManage ("list vms", out machineStrings);
			foreach(var str in machineStrings)
			{
				var parts = str.Split(new[] {'"'}, StringSplitOptions.RemoveEmptyEntries);
				string name = parts[0].Trim ();
				string uuid = parts[1].Trim ();
				VirtualboxMachine machine = new VirtualboxMachine(this, name, uuid);
				machineMap.Add (uuid, machine);
			}
		}
		
		public VirtualboxDriver ()
		{
			LoadMachines ();
		}

		public void CreateMachine (string name, VMProperties properties)
		{
			VirtualboxUtils.VBoxManage("createvm --name " + name + " --register");
		}
	}
}