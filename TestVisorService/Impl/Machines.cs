using System;
using Bizarrefish.TestVisorService.Interface;
using System.Collections.Generic;
using Bizarrefish.VMLib;
using System.IO;
using Bizarrefish.VMLib.Virtualbox;

namespace Bizarrefish.TestVisorService.Impl
{
	public partial class TestVisorService : ITestVisorService
	{
		/// <summary>
		/// List of available hypervisors.
		/// </summary>
		public IEnumerable<HypervisorInfo> Hypervisors
		{
			get { return machines.Hypervisors; }
		}
		
		/// <summary>
		/// List of machines.
		/// </summary>
		public IEnumerable<MachineInfo> Machines
		{
			get { return machines.Machines; }
		}
		
		MachineRepository machines;
		
		void InitMachines()
		{
			var vmDrivers = new List<IVMDriver<IMachine>>();
			vmDrivers.Add(new VirtualboxDriver());
			
			machines = new MachineRepository(vmDrivers);
		}
		
		/// <summary>
		/// Creates a new machine from a VM file.
		/// </summary>
		public MachineInfo CreateMachine(string hypervisorId, Stream s)
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Sets the machine's info.
		/// </summary>
		public void SetInfo(MachineInfo info)
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Gets the machine status.
		/// </summary>
		/// <returns>
		/// The machine status.
		/// </returns>
		/// <param name='id'>
		/// Machine id.
		/// </param>
		public Bizarrefish.TestVisorService.Interface.MachineStatus GetMachineStatus(string id)
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Deletes a machine.
		/// </summary>
		/// <param name='id'>
		/// Machine id.
		/// </param>
		public void DeleteMachine(string id)
		{
			throw new NotImplementedException();
		}
		
	}
}

