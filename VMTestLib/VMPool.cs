using System;
using System.Collections.Generic;
using Bizarrefish.VMLib;

namespace Bizarrefish.VMTestLib
{
	public class VMPool
	{
		public List<IMachine> Machines = new List<IMachine>();

		
		public IMachine GetMachine()
		{
			if(Machines.Count == 0)
			{
				throw new Exception("Machine pool empty");
			} else
			{
				var result = Machines[0];
				Machines.RemoveAt (0);
				return result;
			}
		}
		
		public IMachine PutMachine()
		{
			return null;
		}
	}
}

