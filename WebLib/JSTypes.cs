using System;
using System.Reflection;
using System.Dynamic;

namespace Bizarrefish.WebLib
{
	enum ResponseStatus
	{
		BUSY,
		SUCCESS,
		ERROR
	}
	
	class JSONRequest<ArgType>
	{
		public ArgType Args;
		public string Key;
		
		public static object GetArgs(dynamic req)
		{
			return req.Args;
		}
		
		public static string GetKey(dynamic req)
		{
			return req.Key;
		}
	}
	
	class JSONResponse
	{
		public string Status;
		public object Result;
		public string ErrorText;
		public string Key;
	}
}

