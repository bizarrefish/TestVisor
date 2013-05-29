using System;
using ServiceStack.Redis;
using System.Web.Script.Serialization;

namespace Bizarrefish.TestVisorStorage
{
	public interface IInfo
	{
		string Id { get; set; }
	}

	public class RedisInfoCollection<TInfo>
		where TInfo : IInfo
	{
		static JavaScriptSerializer jss = new JavaScriptSerializer();

		IRedisClient client;
		Func<TInfo> factory;

		string idSetKey;
		string infoPrefix;

		string MakeInfoKey(string infoId)
		{
			return infoPrefix + "/" + infoId;
		}

		public RedisInfoCollection (IRedisClient client, Func<TInfo> factory)
		{
			this.client = client;
			this.factory = factory;
			this.idSetKey = "/Info/" + typeof(TInfo).FullName + "/Index";
			this.infoPrefix = "/Info/" + typeof(TInfo).FullName + "/";
		}

		public void Store(TInfo info)
		{
			client.AddItemToSet(idSetKey, info.Id);
			string infoString = jss.Serialize(info);
			client.SetEntry(MakeInfoKey(info.Id), infoString);
		}

		public TInfo Load(string id)
		{
			string infoString = client.GetValue (MakeInfoKey(id));

			if(infoString != null && infoString.Length > 0)
			{
				var ret = jss.Deserialize<TInfo>(infoString);
				ret.Id = id;
				return ret;
			}
			else
			{
				TInfo n = factory();
				n.Id = id;
				return n;
			}

		}

		public void Delete(string id)
		{
			client.Remove (MakeInfoKey(id));
			client.RemoveItemFromSet(idSetKey, id);
		}
	}
}

