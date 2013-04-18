using System;
using System.Collections.Generic;
using System.IO;
using Bizarrefish.VMLib;

namespace Bizarrefish.VMTestLib
{	
	
	/// <summary>
	/// This class manages a repository of tests
	/// and knows how to run them.
	/// </summary>
	public interface ITestDriver
	{	
		ITestRepository Repo { set; }
		
		void InstallTest(string name, Stream source);
		
		void RemoveTest(string name);
		
		IEnumerable<string> Tests { get; }
		
		void RunTest(string name, IMachine machine, ITestResultBin bin, IDictionary<string, string> env);
	}
}

