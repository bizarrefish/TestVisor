using System;
using System.Web;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace WebLibTest
{
	public class HTTPServer<TSessionData>
		where TSessionData : new()
	{
		//IDictionary<string, DateTime> sessionsTime = new Dictionary<string, DateTime>();
		
		IDictionary<long, TSessionData> sessions = new Dictionary<long, TSessionData>();
		
		ISet<string> funcs = new HashSet<string>();
		
		IDictionary<string, string> contentTypes = new Dictionary<string, string>()
		{
			{ "png", "image/png" },
			{ "gif", "image/gif" },
			{ "js", "text/javascript" },
			{ "html", "text/html" },
			{ "css", "text/css" }
		};
			
		
		const string COOKIE_ID = "COOKIE_ID";
		
		long sessionCounter;
		
		HttpListener listener;
		
		public HTTPServer (int port)
		{
			listener = new HttpListener();
			listener.Prefixes.Add ("http://*:" + port + "/");
		}
		
		public void AddFunc(string func)
		{
			funcs.Add("/" + func);
		}
		
		
		TSessionData GetSessionObject(HttpListenerRequest req, HttpListenerResponse resp)
		{	
			var cookies = req.Cookies;
			Cookie c = cookies[COOKIE_ID];
			
			long id;
			if(c != null)
			{
				id = long.Parse(c.Value);
			}
			else
			{
				id = sessionCounter++;
				c = new Cookie(COOKIE_ID, id.ToString ());
			}
			
			TSessionData session = new TSessionData();
			
			if(sessions.TryGetValue(id, out session))
			{
				//sessionsTime.Remove (session.LastActive);
				//session.LastActive = now;
			}
			else
			{
				session = new TSessionData();
				//session.SessionId = id.ToString();
				sessions[id] = session;
			}
			
			//sessionsTime[now] = session;
			
			resp.SetCookie(c);
			
			return session;
		}
		
		public void Start(Func<string, TSessionData, string, string> handleFunc)
		{
			listener.Start();
			while(true)
			{
				var context = listener.GetContext();
				
				try 
				{
					if(funcs.Contains(context.Request.Url.LocalPath))
					{
						context.Response.StatusCode = 200;
						context.Response.ContentType = "application/json";
						
						TSessionData session = GetSessionObject(context.Request, context.Response);
						
						var sr = new StreamReader(context.Request.InputStream);
						string reqString = sr.ReadToEnd();
						
						string resString = handleFunc(context.Request.Url.LocalPath, session, reqString);
						
						byte[] bytes = Encoding.UTF8.GetBytes(resString);
						
						context.Response.OutputStream.Write(bytes, 0, bytes.Length);
					}
					else
					{
						var pathParts = context.Request.Url.LocalPath.Split ('.');
						context.Response.StatusCode = 200;
						context.Response.ContentType = contentTypes[pathParts[pathParts.Length-1]];
						
						string fileName = "../../" + context.Request.Url.LocalPath.Replace ("..", "");
						
						if(File.Exists (fileName)) 
						{
							using(var st = new FileStream(fileName, FileMode.Open))
							{
								var reader = new BinaryReader(st);
								const int bufferSize = 4096;
								int byteCount = 0;
								do
								{
									byte[] bytes = reader.ReadBytes(bufferSize);
									byteCount = bytes.Length;
									if(byteCount > 0) context.Response.OutputStream.Write(bytes, 0, byteCount);
								}
								while(byteCount > 0);
							}
						}
						else
						{
							context.Response.StatusCode = 404;
						}
						
					}
				}
				catch(Exception e)
				{
					context.Response.StatusCode = 500;
					context.Response.ContentType = "text/plain";
					byte[] bytes = Encoding.UTF8.GetBytes(e.Message + "\n" + e.StackTrace);
					context.Response.OutputStream.Write (bytes, 0, bytes.Length);
					
				}
				context.Response.OutputStream.Close ();
			}
		}
	}
}

