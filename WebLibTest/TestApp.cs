using System;
using Bizarrefish.WebLib;
using System.IO;
using System.Threading;

namespace WebLibTest
{
	public class SessionData
	{
		public long count = 0;
	}
	
	public class CountRequest
	{
		public long CountNumber;
	}
	
	public class CountResponse
	{
		public string Message;
		public long NewCount;
	}
	
	public class TestApp
	{
		static CountResponse CountUp(CountRequest req, SessionData session)
		{
			Thread.Sleep(4000);
			session.count = session.count + req.CountNumber;
			return new CountResponse()
			{
				Message = "Counted up!",
				NewCount = session.count
			};
		}
		
		static CountResponse CountDown(CountRequest req, SessionData session)
		{
			throw new Exception("ERROR, THERE IS AN ERROR!");
			session.count = session.count - req.CountNumber;
			return new CountResponse()
			{
				Message = "Counted down!",
				NewCount = session.count
			};
		}
		
		public TestApp ()
		{
			AjaxHandler<SessionData> handler = new AjaxHandler<SessionData>();
			handler.AddEndpoint<CountRequest, CountResponse>("CountUp", CountUp);
			handler.AddEndpoint<CountRequest, CountResponse>("CountDown", CountDown);
			
			File.Delete("../../ajax.js");
			File.WriteAllText("../../ajax.js", handler.GetJavascript());
			
			HTTPServer<SessionData> server = new HTTPServer<SessionData>(8088);
			
			server.AddFunc("CountUp");
			server.AddFunc("CountDown");
			
			server.Start (handler.Handle);
		}
	}
	
	public static class Program
	{
		public static void Main(string[] args)
		{
			new TestApp();
		}
	}
}

