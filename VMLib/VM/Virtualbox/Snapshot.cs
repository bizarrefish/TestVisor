using System;
using System.Linq;
using System.Collections.Generic;

namespace Bizarrefish.VMLib.Virtualbox
{
	public class SnapshotManager
	{
		public IEnumerable<Snapshot> Snapshots { get { return dict.Values; } }
		
		string uuid;
		
		IDictionary<string, Snapshot> dict = new Dictionary<string, Snapshot>();
		
		public SnapshotManager(string uuid)
		{
			this.uuid = uuid;
			LoadSnapshots();
		}
		
		public void LoadSnapshots()
		{
			dict.Clear ();
			
			IEnumerable<string> result;
			if(VirtualboxUtils.VBoxManage("snapshot " + uuid + " list --machinereadable", out result))
			{
				string[] resultArray = result.ToArray();
				
				for(var i = 0; i < resultArray.Length-1; i+=2)
				{
					var name = resultArray[i].Split ('=')[1].Trim('"');
					var suuid = resultArray[i+1].Split ('=')[1].Trim('"');
					Snapshot ss = new Snapshot();
					ss.Id = suuid;
					ss.Name = name;
					dict[suuid] = ss;
				}
			}
			
		}
		
		public string TakeSnapshot()
		{
			var key = uuid + ":" + DateTime.Now.ToFileTime();
			
			VirtualboxUtils.VBoxManage("snapshot " + uuid + " take \"" + key + "\" --pause");
			LoadSnapshots ();
			
			return Snapshots.Where (ss => ss.Name == key).First ().Id;
		}
		
		public void RestoreSnapshot(string Id)
		{
			VirtualboxUtils.VBoxManage("snapshot " + uuid + " restore " + Id);
		}
		
		public void DeleteSnapshot(string Id)
		{
			VirtualboxUtils.VBoxManage("snapshot " + uuid + " delete " + Id);
			LoadSnapshots();
		}
	}
}
