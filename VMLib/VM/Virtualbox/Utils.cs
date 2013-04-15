using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;

namespace Bizarrefish.VMLib.Virtualbox
{
	static class VirtualboxUtils 
	{
		
		public static bool VBoxManage(string args)
		{
			IEnumerable<string> outputLines;
			
			return VBoxManage(args, out outputLines);
		}
		
		public static bool VBoxManage(string args, out IEnumerable<string> outputLines)
		{
			try
			{
				ProcessStartInfo si = new ProcessStartInfo("VBoxManage", args);
				si.RedirectStandardOutput = true;
				si.RedirectStandardError = true;
				si.UseShellExecute = false;
				var proc = Process.Start (si);
				proc.WaitForExit();
				bool res = proc.ExitCode == 0;
				
				var outputList = new List<string>();
				while(!proc.StandardOutput.EndOfStream)
					outputList.Add (proc.StandardOutput.ReadLine());
				
				outputLines = outputList;
				return res;
			} catch(Exception e)
			{
				outputLines = new string[0];
				return false;
			}
		}
	}
}

