using System;
using System.Collections.Generic;

namespace Bizarrefish.VMLib
{
	
	public interface IVMDriver<out TMachine> where TMachine : IMachine
	{
		string Name { get; }
		
		IEnumerable<TMachine> Machines { get; }
		
		void CreateMachine(string name, VMProperties properties);
	}
}

