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
		public class CountUp : IAjaxMethod<SessionData, CountResponse>
		{
			public int CountNumber;

			public CountResponse Call (SessionData session)
			{
				Thread.Sleep(4000);
				session.count = session.count + CountNumber;
				return new CountResponse()
				{
					Message = "Counted up!",
					NewCount = session.count
				};
			}
		}

		public class CountDown : IAjaxMethod<SessionData, CountResponse>
		{
			public int CountNumber;

			public CountResponse Call (SessionData session)
			{
				session.count = session.count - CountNumber;
				return new CountResponse()
				{
					Message = "Counted down!",
					NewCount = session.count
				};
			}
		}

		public TestApp ()
		{
			HTTPServer<SessionData> server = new HTTPServer<SessionData>(8080, "../../");
			AjaxHandler<SessionData> handler = new AjaxHandler<SessionData>(server);

			handler.AddClass<TestApp>();
			
			File.Delete("../../ajax.js");
			File.WriteAllText("../../ajax.js", handler.GetJavascript());

			server.Start();

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

