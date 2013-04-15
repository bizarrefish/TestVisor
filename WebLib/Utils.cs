using System;
using System.Web.Script.Serialization;
using System.Reflection;
using System.Linq;

namespace Bizarrefish.WebLib
{
	internal static class Utils
	{
		public const string GenRPC =
@"function GenRPC(methodName, argNames) {
	return function(args, success, fail) {
		var reqKey = null;

		// The initial request
		var doRequest = function() {
			ajax('/' + methodName, {Args: args}, callback);
		}

		// The poll request
		var doPoll = function(withArgs) {
			ajax('/' + methodName, {Key: reqKey}, callback);
		}

		// The callback to handle the responses
		var callback = function(data) {
			reqKey = data.Key;
			if(data.Status === 'SUCCESS') {
				success(data.Result);
			} else if(data.Status === 'ERROR') {
				if(fail !== undefined) fail(data.ErrorText);
			} else if(data.Status === 'BUSY') {
				setTimeout(doPoll, 1000);		// Poll again in a couple of seconds if busy.
			}
		}

		for(var i in argNames) {
			if(!args.hasOwnProperty(argNames[i])) {
				throw new Error('[RPC Error] Argument not provided:' + argNames[i]);
			}
		}

		doRequest();
	}
}";
		
		public static JavaScriptSerializer Serializer = new JavaScriptSerializer();
		
		public static object DeserializeToType(object obj, Type t)
		{
			MethodInfo mi = typeof(JavaScriptSerializer).GetMethod("Deserialize");
			return mi.MakeGenericMethod(t).Invoke(Serializer, new[] { obj });
		}
		
		public static object MakeJavascript(JSFunction jsf)
		{
			var fields = jsf.RequestType.GetFields();
			string fieldsArray = Serializer.Serialize(fields.Select ((f) => f.Name));
			return "var " + jsf.Name + " = GenRPC('" + jsf.Name + "', " + fieldsArray + ");";
		}
	}
	
}

