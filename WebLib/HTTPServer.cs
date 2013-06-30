using System;
using System.Web;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Bizarrefish.WebLib
{
	public delegate string AjaxCallback<TSessionData>(TSessionData data, string req);

	public class HTTPServer<TSessionData>
		where TSessionData : new()
	{
		//IDictionary<string, DateTime> sessionsTime = new Dictionary<string, DateTime>();
		
		IDictionary<long, TSessionData> sessions = new Dictionary<long, TSessionData>();

		IDictionary<string, string> contentTypes = new Dictionary<string, string>()
		{
			{ "png", "image/png" },
			{ "gif", "image/gif" },
			{ "js", "text/javascript" },
			{ "html", "text/html" },
			{ "css", "text/css" },
			{ "ico", "image/ico" }
		};
			
		
		const string COOKIE_ID = "COOKIE_ID";

		const string AJAX_PREFIX = "/Ajax";
		const string STREAM_PREFIX = "/Stream";

		long sessionCounter;

		string staticDir;
		
		HttpListener listener;

		IDictionary<string, AjaxCallback<TSessionData>> callbacks =
			new Dictionary<string, AjaxCallback<TSessionData>>();

		IDictionary<string, Func<string, Stream>> streamFuncs =
			new Dictionary<string, Func<string, Stream>>();

		public HTTPServer (int port, string staticDir)
		{
			listener = new HttpListener();
			listener.Prefixes.Add ("http://*:" + port + "/");
			this.staticDir = staticDir;
		}

		public void AddCallback(string name, AjaxCallback<TSessionData> callback)
		{
			callbacks[name] = callback;
		}

		public void AddStreamFunc(string name, Func<string, Stream> func)
		{
			streamFuncs[name] = func;
		}

		public string GetCallbackUrl(string name)
		{
			return AJAX_PREFIX + "/" + name;
		}

		public string GetStreamUrl(string name, string id)
		{
			return STREAM_PREFIX + "/" + name + "/" + id;
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
				//sessionsTime[id] = DateTime.Now;
			}
			else
			{
				session = new TSessionData();
				sessions[id] = session;
			}
			
			resp.SetCookie(c);
			
			return session;
		}

		public void Start()
		{
			listener.Start();
			while(true)
			{
				var context = listener.GetContext();
				
				try 
				{
					
					TSessionData session = GetSessionObject(context.Request, context.Response);

					context.Response.StatusCode = 200;

					if(context.Request.Url.LocalPath.StartsWith (AJAX_PREFIX))
					{

						string callbackName = context.Request.Url.LocalPath.Replace(AJAX_PREFIX + "/", "");
						
						context.Response.ContentType = "application/json";

						AjaxCallback<TSessionData> callback;
						if(callbacks.TryGetValue(callbackName, out callback))
						{
							var sr = new StreamReader(context.Request.InputStream);
							string reqString = sr.ReadToEnd();
							
							string resString = callback(session, reqString);

							byte[] bytes = Encoding.UTF8.GetBytes(resString);
							
							context.Response.OutputStream.Write(bytes, 0, bytes.Length);
						}
						else
						{
							context.Response.StatusCode = 404;
						}
					}
					else if(context.Request.Url.LocalPath.StartsWith(STREAM_PREFIX))
					{
						string[] parts = context.Request.Url.LocalPath.Replace(STREAM_PREFIX + "/", "").Split ('/');
						string streamKey = parts[0];
						string streamId = parts[1];

						Func<string, Stream> streamFunc;

						if(streamFuncs.TryGetValue(streamKey, out streamFunc))
						{
							using(Stream s = streamFunc(streamId))
							{
								switch(context.Request.HttpMethod)
								{
								case "GET":
									context.Response.ContentType = "application/octet-stream";
									s.CopyTo(context.Response.OutputStream);
									break;

								case "POST":
									context.Request.InputStream.CopyTo(s);
									break;
								}
							}
						}
						else
						{
							context.Response.StatusCode = 404;
						}

					}
					else
					{
						string path = context.Request.Url.LocalPath;
						string[] pathParts = path.Split ('.');
						context.Response.StatusCode = 200;
						context.Response.ContentType = contentTypes[pathParts[pathParts.Length-1]];
						
						string fileName = staticDir + path.Replace ("..", "");
						
						if(File.Exists (fileName)) 
						{
							using(var st = File.OpenRead(fileName))
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

