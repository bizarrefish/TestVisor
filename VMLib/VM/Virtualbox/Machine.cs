using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Bizarrefish.VMLib;

namespace Bizarrefish.VMLib.Virtualbox
{	
	public class VirtualboxMachine : IMachine
	{
		string uuid, name;
		
		SnapshotManager snapshotManager;
		VirtualboxDriver driver;
		
		public VirtualboxMachine(VirtualboxDriver driver, string name, string uuid)
		{
			this.uuid = uuid;
			this.name = name;
			snapshotManager = new SnapshotManager(uuid);
			this.driver = driver;
		}
		

		public void Shutdown ()
		{
			VirtualboxUtils.VBoxManage(" controlvm " + uuid + " poweroff");
			Thread.Sleep (4000);
		}

		public string MakeSnapshot ()
		{
			return snapshotManager.TakeSnapshot();
		}

		public string Name { get { return name; } }
		
		public string Id { get { return uuid; } }

		public string Description { get { return name; } }

		public MachineStatus GetCurrentStatus ()
		{
			lock(driver)
			{
				IEnumerable<string> output;
				bool running = false;
				
				if(VirtualboxUtils.VBoxManage("list runningvms", out output))
				{
					running = output.Where (line => line.Contains (uuid)).Any ();
				}
				
				if(running) return MachineStatus.STARTED;
				else return MachineStatus.STOPPED;
			}
		}

		public void Start (string snapshotId)
		{			
			lock(driver)
			{
				var ms = GetCurrentStatus();
				if(ms == MachineStatus.STARTED)
				{
					Shutdown();
				}
				
				snapshotManager.RestoreSnapshot(snapshotId);

				VirtualboxUtils.VBoxManage("startvm " + uuid);
			}
		}
	
		public Snapshot GetSnapshot (string id)
		{
			lock(driver)
			{
				return snapshotManager.Snapshots.Where (ss => ss.Id == id).FirstOrDefault();
			}
		}

		public IEnumerable<Snapshot> GetSnapshots ()
		{
			lock(driver)
			{
				return snapshotManager.Snapshots;
			}
		}

		public void DeleteSnapshot (string id)
		{
			lock(driver)
			{
				snapshotManager.DeleteSnapshot(id);
			}
		}

		public VMProperties LoadProperties ()
		{
			lock(driver)
			{
				var props = new VMProperties();
				IEnumerable<string> propLines;
				if(VirtualboxUtils.VBoxManage("showvminfo " + uuid + " --machinereadable", out propLines))
				{
					
					var propMap = propLines.Select (s => s.Split ('=')).ToDictionary(parts => parts[0], parts => parts[1]);
					
					props.MemorySize = int.Parse (propMap["memory"]);
					props.VideoMemorySize = int.Parse (propMap["vram"]);
					props.CPUCount = int.Parse (propMap["cpus"]);
				}
				
				return props;
			}
		}

		public void SaveProperties (VMProperties props)
		{
			lock(driver)
			{
				if(GetCurrentStatus() == MachineStatus.STARTED) Shutdown ();
				
				VirtualboxUtils.VBoxManage("modifyvm " + uuid + " " +
					"--memory " + props.MemorySize + " " +
				    "--vram " + props.VideoMemorySize + " " +
				    "--cpus " + props.CPUCount);
			}
		}
		
		public bool DownloadFile (string url, string destination)
		{
			lock(driver)
			{
				string argString = string.Format(
					"guestcontrol {0} cp \"{1}\" \"{2}\" --username Administrator", uuid, url, destination);
				
				return VirtualboxUtils.VBoxManage("guestcontrol " + uuid +
					" cp '" + url + "' '" + destination + "' --username Administrator");
			}
		}

		public ProgramResult RunProgram (string programName, IDictionary<string, string> env)
		{
			if(env.Count > 0)
				throw new NotImplementedException("Environmental controls offline");
			
			lock(driver)
			{
				IEnumerable<string> output;
				string argString = string.Format(
					"guestcontrol {0} execute --image \"{1}\" --wait-exit --username Administrator", uuid, programName);
				if(VirtualboxUtils.VBoxManage(argString, out output))
				{
					return new ProgramResult
					{
						ExitCode = 0,
						StandardOutput = output.Aggregate("", (acc, el) => acc + "\n" + el),
						StandardError = ""
					};
				}
				else
				{
					return new ProgramResult() { ExitCode = 1 };
				}
			}
		}
	}
}

