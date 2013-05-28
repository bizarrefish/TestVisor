using System;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace Bizarrefish.WebLib
{
	/// <summary>
	/// Used to mark methods for ajax exposure.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class AjaxAttribute : Attribute
	{
		public AjaxAttribute()
		{
		}
	}

	internal class AjaxMethodInfo
	{
		/// <summary>
		/// Method argument names -> types
		/// </summary>
		public Tuple<string, Type>[] Args;

	}

	public class AjaxServer<THandler> where THandler : new()
	{
		static JavaScriptSerializer jss = new JavaScriptSerializer();

		IDictionary<string, AjaxMethodInfo> methodDict = new Dictionary<string, AjaxMethodInfo>();

		THandler instance;

		string jsFilename;

		const string RPCFileUri = "/ajax.js";

		public AjaxServer()
		{
			instance = new THandler();
			InitMethods (instance.GetType());
			InitJavascript();
		}

		void InitMethods(Type t)
		{
			/*
			methodDict.Clear();

			foreach(var method in t.GetMethods())
			{
				if(method.GetCustomAttributes(typeof(AjaxAttribute), true))
				{
					AjaxMethodInfo ami = new AjaxMethodInfo();

					ami.Args = method.GetParameters()
						.Select(par => Tuple.Create(par.Name, par.ParameterType))
							.ToArray();

					methodDict[method.Name] = ami;
				}
			}*/
		}

		void InitJavascript()
		{
			/*
			jsFilename = Path.GetTempFileName();

			using(var fs = File.OpenWrite(jsFilename))
			{
				using(var writer = new StreamWriter(fs))
				{
					// Write RPC header
					writer.Write(Utils.GenRPC);

					writer.WriteLine("\nFunctions:\n");

					// Write the methods in
					foreach(var method in methodDict)
					{

						AjaxMethodInfo ami = method.Value;

						writer.WriteLine("var " + method.Key + " = GenRPC('" + method.Key + "'," +
						             jss.Serialize(ami.Args.Keys) + ");");
					}

				}
			}*/
		}

		string InvokeMethod(string name, IDictionary<string, object> args)
		{
			return "";
			/*
			AjaxMethodInfo ami;

			object[] argArray = new object[args.Count];

			int i = 0;

			if(methodDict.TryGetValue(name, out ami))
			{
				foreach(var methodArg in ami.Args)
				{
					Type argType = methodArg.Item2;

					string argObject;
					if(args.TryGetValue(methodArg.Item1, out argObject))
					{
						ServiceStack.Text.JsonSerializer.
						argArray[i] = ServiceStack.Text.JsonSerializer.DeserializeFromString(argObject, argType);
					}
					else
					{
						throw new Exception("Argument not provided: " + methodArg.Item1);
					}
					i++;
				}
			}
			else
			{
				throw new Exception("Unknown method: " + name);
			}

			object retObj = instance.GetType().GetMethod(name).Invoke(instance, argArray);
			return ServiceStack.Text.JsonSerializer.SerializeToString(retObj, retObj.GetType());
			*/
		}


		void SendJavascript(HttpListenerResponse resp)
		{
			resp.ContentType = "application/javascript";
			resp.StatusCode = 200;
			using(Stream s = File.OpenRead(jsFilename))
			{
				s.CopyTo(resp.OutputStream);
			}
			resp.OutputStream.Close ();
		}


		void Handle(HttpListenerRequest req, HttpListenerResponse resp)
		{
			/*
			AjaxMethodInfo ami;

			if(req.Url.LocalPath == RPCFileUri)
			{
				SendJavascript(resp);
			}
			else
			{
				var argDict = new Dictionary<string, object>();

				argDict = ServiceStack.Text.JsonSerializer.DeserializeFromStream(typeof(Dictionary<string, object>), req.InputStream);

				//InvokeMethod(req.Url.LocalPath.Substring(1), );
			}
			*/
		}

	}
}

