using System;
using System.Web.Script.Serialization;
using System.IO;

namespace Bizarrefish.TestVisorService
{
	public class PersistentStore<TData> where TData : new()
	{
		public TData Data;
		
		static JavaScriptSerializer jss = new JavaScriptSerializer();
		
		string fileName;
		public PersistentStore (string fileName)
		{
			this.fileName = fileName;
			
			if(File.Exists(fileName))
			{
				Data = jss.Deserialize<TData>(File.ReadAllText(fileName));
			}
			else
			{
				Data = new TData();
			}
		}
		
		public void Save()
		{
			File.WriteAllText(fileName, jss.Serialize(Data));
		}
	}
	
}

