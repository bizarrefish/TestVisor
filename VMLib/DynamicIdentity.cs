using System;

namespace Bizarrefish.VMLib
{
	/// <summary>
	/// A dynamic identity is an identity which can be imposed upon a virtual machine
	/// by the agent running in it.
	/// It's essential to making the machine behave properly in terms of networking
	/// </summary>
	public class DynamicIdentity
	{
		public string Hostname;
		
		public string NetAddress;
	}
}

