using System;

using Bizarrefish.WebLib;

using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Threading;
using System.Reflection;
using System.Linq;

namespace Bizarrefish.WebLib
{
	public delegate TResponse AjaxFunction<TRequest, TSession, TResponse>(TRequest request, TSession session);
	
	/// <summary>
	/// This manages a collection of live Ajax Modules.
	/// It also generates javascript method stubs.
	/// </summary>
	public interface IAjaxHandler<TSession>
	{
		void AddClass<TClass>();

		/// <summary>
		/// Generates a javascript library with method stubs for all added modules.
		/// </summary>
		string GetJavascript();

	}

	public interface IAjaxMethod<TSession, TResponse>
	{
		TResponse Call(TSession session);
	}

	public class AjaxHandler<TSession> : IAjaxHandler<TSession>
		where TSession : new()
	{
		IDictionary<string, TaskRunner<object>> tasks = new Dictionary<string, TaskRunner<object>>();

		IDictionary<string, string[]> funcFields = new Dictionary<string, string[]>();

		long keyCounter = 0;

		HTTPServer<TSession> server;

		public AjaxHandler(HTTPServer<TSession> server)
		{
			this.server = server;
		}

		/// <summary>
		/// Given a JSONRequest object.
		/// handleFunc receives the JSONRequest's Arg object.
		/// 
		/// Return a JSONResponse.
		/// </summary>
		JSONResponse HandleJSONRequest(object jreq, Func<object, object> handleFunc)
		{
			JSONResponse jresp = new JSONResponse();

			// Get the important parts out of the request
			string reqKey = JSONRequest<object>.GetKey(jreq);
			object argsObj = JSONRequest<object>.GetArgs(jreq);

			TaskRunner<object> runner = null;

			bool newRunner = false;
			
			if(reqKey == null)
			{
				// New request
				runner = new TaskRunner<object>(() => handleFunc(argsObj));
				// At this point, we may or may not have already completed the task
				reqKey = (keyCounter++).ToString ();
				newRunner = true;
			}
			else if (!tasks.TryGetValue(reqKey, out runner))
				throw new Exception("Invalid Request Key");
			
			// We send the key back with the response.
			jresp.Key = reqKey;
			
			bool error = false;
			if(runner.CheckDone(ref error, ref jresp.Result, ref jresp.ErrorText))
			{
				jresp.Status = (error ? ResponseStatus.ERROR : ResponseStatus.SUCCESS).ToString();
			}
			else
			{
				// This is gonna take a while
				
				// If we only just started this task,
				// stick it in the dictionary
				if(newRunner) tasks[reqKey] = runner;
				
				// We're busy now.
				jresp.Status = ResponseStatus.BUSY.ToString();
			}

			return jresp;
		}

		/// <summary>
		/// Add a method.
		/// </summary>
		/// <param name="methodName">Name of the method</param>
		/// <param name="argType">Type of the method arg structure</param>
		/// <param name="handleFunc">Function to handle this method with.</param>
		/// <param name="methodName">Name of the method</param>
		public void AddMethod(string methodName, Type argType, Func<object, TSession, object> handleFunc)
		{
			// A JSON Serialization parameterized with argType
			Type rType = typeof(JSONRequest<>).MakeGenericType(new[] { argType });

			server.AddCallback(methodName, delegate(TSession session, string reqString)
			{
				object jreq = Utils.DeserializeToType(reqString, rType);
				if(jreq == null) jreq = Activator.CreateInstance(argType);

				object jresp = HandleJSONRequest(jreq, (req) => handleFunc(req, session));

				return Utils.Serializer.Serialize(jresp);
			});

			funcFields[methodName] = argType
					.GetFields()
					.Select (f => f.Name)
						.ToArray();
		}

		/// <summary>
		/// Add a method
		/// </summary>
		/// <param name="argType">Argument structure type.</param>
		/// <param name="prefix">Prefix for the method.</param>
		void AddMethod(Type argType, string prefix)
		{
			// The method-class' interface declaration.
			Type iFaceType = argType.GetInterfaces().FirstOrDefault();

			if(iFaceType.GetGenericTypeDefinition() == typeof(IAjaxMethod<,>))
			{
				string methodName = prefix + "_" + argType.Name;

				AddMethod(methodName, argType, delegate(object reqObj, TSession session)
				{
					object[] pars = new object[] { session };
					return argType.GetMethod("Call").Invoke (reqObj, pars);
				});
			}
		}

		public void AddMethod<TArg>(string prefix)
		{
			AddMethod (typeof(TArg), prefix);
		}

		public void AddClass<TClass>()
		{
			Type classType = typeof(TClass);
			var nested = classType.GetNestedTypes();

			foreach(var type in nested)
			{
				AddMethod(type, classType.Name);
			}

		}

		public string GetJavascript ()
		{
			return
				"// Autogenerated RPC code:\n" +
				Utils.GenRPC + "\n" +
				funcFields.Aggregate("", (acc, f) => acc = acc + "\n" +
					                     Utils.MakeJavascript(server.GetCallbackUrl(f.Key), f.Key, f.Value));
		}
	}
}

