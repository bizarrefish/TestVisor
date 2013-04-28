using System;
using System.Collections.Generic;
using System.IO;

namespace Bizarrefish.TestVisorService.Interface
{
	public partial interface ITestVisorService
	{
		/// <summary>
		/// List of available hypervisors.
		/// </summary>
		IEnumerable<HypervisorInfo> Hypervisors { get; }
		
		/// <summary>
		/// List of machines.
		/// </summary>
		IEnumerable<MachineInfo> Machines { get; }
		
		/// <summary>
		/// Creates a new machine from a VM file.
		/// </summary>
		MachineInfo CreateMachine(string hypervisorId, Stream s);
		
		/// <summary>
		/// Sets the machine's info.
		/// </summary>
		void SetInfo(MachineInfo info);
		
		/// <summary>
		/// Gets the machine status.
		/// </summary>
		/// <returns>
		/// The machine status.
		/// </returns>
		/// <param name='id'>
		/// Machine id.
		/// </param>
		MachineStatus GetMachineStatus(string id);
		
		/// <summary>
		/// Deletes a machine.
		/// </summary>
		/// <param name='id'>
		/// Machine id.
		/// </param>
		void DeleteMachine(string id);
	}
}

