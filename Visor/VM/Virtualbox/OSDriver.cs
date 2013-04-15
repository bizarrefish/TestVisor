using System;
using Visor;

namespace Visor.VM.Virtualbox
{
	public class VirtualboxOSDriver : IOSDriver
	{
		string uuid;
		
		public VirtualboxOSDriver (string uuid)
		{
			this.uuid = uuid;
		}

		public string StartProgram (string programPath, string[] arguments)
		{
			VirtualboxUtils.VBoxManage("guestcontrol execute " + uuid + "\"" + programPath);
			return "";
		}

		public bool KillProgram (string id)
		{
			throw new NotImplementedException ();
		}

		public System.Collections.Generic.IEnumerable<RunningProgram> Programs {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

