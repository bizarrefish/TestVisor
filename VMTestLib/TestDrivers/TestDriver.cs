using System;
using Bizarrefish.VMLib;
using System.Collections.Generic;
using System.IO;

namespace Bizarrefish.VMTestLib.TestDrivers
{
	public class TestResult
	{
		public bool Succeeded;
		public string Detail;
	}
	
	/// <summary>
	/// A Test Driver knows how to run tests.
	/// </summary>
	public interface ITestDriver
	{
		/// <summary>
		/// Regexes which match filenames appropriate for installation
		/// into this driver
		/// </summary>
		IEnumerable<string> FileRegexes { get; }
		
		/// <summary>
		/// Gets the name of the driver.
		/// </summary>
		string DriverName { get; }
		
		/// <summary>
		/// Run a test on a target machine.
		/// This may copy files to the target machine.
		/// </summary>
		TestResult RunTest(IMachine target, string name, string url, Dictionary<string, string> parameters);
		
	}
}