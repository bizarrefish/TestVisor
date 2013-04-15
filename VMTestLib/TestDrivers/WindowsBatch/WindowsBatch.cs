using System;

using System.Collections.Generic;
using Bizarrefish.VMLib;

namespace Bizarrefish.VMTestLib.TestDrivers.WindowsBatch
{
	/// <summary>
	/// The simplest test driver you can think of.
	/// Just downloads and runs a batch file.
	/// </summary>
	public class WindowsBatch : ITestDriver
	{
		public TestResult RunTest (IMachine target, string name, string url, Dictionary<string, string> parameters)
		{
			if(target.GetCurrentStatus() != MachineStatus.STARTED)
				throw new Exception("Machine not started");
			
			string targetFileName = "C:\\Tests\\WindowsBatch\\" + name;
			
			target.DownloadFile(url, targetFileName);
			ProgramResult res = target.RunProgram(targetFileName,parameters);
			
			return new TestResult()
			{
				Succeeded = (res.ExitCode == 0),
				Detail = (res.StandardOutput)
			};
		}

		public IEnumerable<string> FileRegexes {
			get {
				return new[] { "\\.bat$" };
			}
		}
		
		public string DriverName { get { return "Windows Batch File (.bat)"; } }
	}
}

