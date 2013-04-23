using System;
using System.Linq;
using Visor.VM;
using Visor.VM.Virtualbox;
using Visor.Web;
using Visor.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;


namespace Visor
{
	public class VisorSession : IPollingAjaxMethodSession<object>
	{
		public int number = 0;

		public string CurrentAjaxMethod { get; set; }
		public object AjaxWorkResult { get; set; }
		
		public VisorSession()
		{
			AjaxWorkResult = WorkState.SUCCESS;
		}
	}
	
	public class GetMachinesArgs
	{
	}
	
	public static class Visor
	{
		
		static IVMDriver<IMachine> Driver;
		
		public static Worker W = new Worker();
		
		/*public static object GetMachines(Web.AjaxRequestObject<VisorSession> req)
		{
			return Driver.Machines;
		}*/
		
		public static object GetMachines(VisorSession session, GetMachinesArgs args)
		{
			
			Thread.Sleep (10000);
			return Driver.Machines;
		}
		
		public static object GetSnapshots(Web.AjaxRequestObject<VisorSession> req)
		{
			return Driver.Machines.Where(m => m.Id == req.Params["machine"]).First().GetSnapshots ();
		}
		
		public static object GetMachineStatus(Web.AjaxRequestObject<VisorSession> req)
		{
			return Driver.Machines.Where (m => m.Id == req.Params["machine"]).First ().GetCurrentStatus().ToString();
		}
		
		public class MachineStatusRow
		{
			public string Name;
			public string Id;
			public bool Running;
			
			public MachineStatusRow(IMachine m)
			{
				Name = m.Name;
				Id = m.Id;
				Running = m.GetCurrentStatus() == VM.MachineStatus.STARTED;
			}
		}
		
		public static MachineStatusRow[] GetMultiStatus()
		{
			var rowList = new List<MachineStatusRow>();
			
			foreach(var m in Driver.Machines)
			{
				rowList.Add(new MachineStatusRow(m));
			}
			
			return rowList.ToArray();
		}
		
		public static void Main(string[] args)
		{
			Driver = new VirtualboxDriver();
			
			//var cont = new DynContainer(typeof(Dashboard));
			//DynContainer.GenJavascript("/GetMachines", typeof(Visor).GetMethod ("GetMachines", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
			
			Worker w = new Worker();
			w.SetLogger(e => MessageBox.Show(e.ToString()));
			
			w.Start ();
			
			
			var result = w.SubmitWork<string>(() => { Thread.Sleep(1000); return "hello"; }, "dohello");
			Console.WriteLine (result.State + ", " + result.Result);
			Thread.Sleep(3000);
			Console.WriteLine (result.State + ", " + result.Result);
			
			var ar = new Web.AjaxResponder<VisorSession>(8088);
			//ar.Bind ("/GetMachines", GetMachines);
			
			var method = new PollingAjaxMethod<VisorSession, GetMachinesArgs, object>
				(w, "GetMachines", GetMachines);
			
			ar.Bind ("/GetMachines", (AjaxRequestObject<VisorSession> req) =>
			         method.Handle (req.Session.Data, new GetMachinesArgs()));
			
			
			Console.Write (PollingAjaxMethod<VisorSession, GetMachinesArgs, object>.JavascriptRPCFunc);
			Console.Write (method.GenerateRPC());
			
			
			ar.Bind ("/GetSnapshots", GetSnapshots);
			ar.Bind ("/GetMachineStatus", GetMachineStatus);
			//ar.Start();
			ar.Bind ("/haha", r => new string[] { r.Uri});
			
			
			
			var driver = new VirtualboxDriver();
			var win7Machine = driver.Machines.Where(m => m.Name == "Windows 7").First ();
			
			ar.Bind("/Machines", req => driver.Machines.ToDictionary(m => m.Id, m => m.Name));
			
			var props = win7Machine.LoadProperties();
			
			props.MemorySize /= 2;
			
			win7Machine.SaveProperties(props);
			
			win7Machine.Start(null);
			
			Console.WriteLine("Available snapshots:");
			var snapshots = win7Machine.GetSnapshots();
			foreach(var ss in snapshots) Console.WriteLine(ss.Name);
			
			ar.Bind ("/snapshots", req => driver.Machines.Where (m => m.Id == req.Params["machine"]).First().GetSnapshots());
			
			ar.Start();
			
			Console.WriteLine("Starting first snapshot...");
			win7Machine.Start(snapshots.First ().Id);
			
			Console.WriteLine("Making snapshot...");
			string ssId = win7Machine.MakeSnapshot();
			
			Console.WriteLine("New snapshot: " + win7Machine.GetSnapshot(ssId).Name);
			
			Console.WriteLine ("Reverting to snapshot");
			win7Machine.Start (ssId);
			
			Console.WriteLine("Powering off");
			win7Machine.Shutdown();
			
		}
	}
}

