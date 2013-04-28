using System;
using System.Collections.Generic;

using Bizarrefish.VMLib;
using System.Linq;
using Bizarrefish.TestVisorService.Interface;

namespace Bizarrefish.TestVisorService.Impl
{
	
	internal class MachineRepository
	{
		IDictionary<string, IMachine> machineIndex;
		
		public IEnumerable<HypervisorInfo> Hypervisors;
		
		IEnumerable<IVMDriver<IMachine>> Drivers;
		
		public IEnumerable<MachineInfo> Machines { get; set; }
		
		void Refresh()
		{
			Hypervisors = Drivers.Select (d => new HypervisorInfo()
			{
				Id = d.Name,
				Description = d.Name,
				Name = d.Name
			});
			
			Machines = Drivers.SelectMany(d =>
				d.Machines.Select(m => new MachineInfo()
				{
					HypervisorId = d.Name,
					Name = m.Name,
					Description = m.Name,
					Id = m.Id
				}));
			
			machineIndex = Drivers.SelectMany(d => d.Machines).ToDictionary(m => m.Id, m => m);
		}
		
		public MachineRepository (IEnumerable<IVMDriver<IMachine>> drivers)
		{
			Drivers = drivers;
			Refresh();
		}
		
		public IMachine GetMachine(string id, bool exclusive = true)
		{
			return machineIndex[id];
		}
	}
}

