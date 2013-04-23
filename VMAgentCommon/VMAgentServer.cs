using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Web.Script.Serialization;
using System.Threading;

namespace Bizarrefish.VMAgent
{
	public static class VMAgent
	{
		const int BuffSizeBytes = 4096;
		const int QuickieSleepTime = 2000;
		
		static IDictionary<string, ProgramResult> results = new Dictionary<string, ProgramResult>();
		
		public static JavaScriptSerializer jss = new JavaScriptSerializer();
		
		public static void CopyStream(Stream input, Stream output)
		{
			
			byte[] buffer = new byte[BuffSizeBytes];
			int nBytes = 0;
			
			while(true)
			{
				nBytes = input.Read(buffer, 0, BuffSizeBytes);
				if(nBytes > 0)
					output.Write(buffer, 0, nBytes);
				else
					break;
			}
			
		}
		
		static void PutFileHandler(HttpListenerRequest request, HttpListenerResponse response)
		{
			var query = request.QueryString;
			var stream = request.InputStream;
			
			string targetLocation = query["target"];
			
			Log ("PutFile: " + targetLocation + "...");
			string targetDir = Path.GetDirectoryName(targetLocation);
			if(!Directory.Exists(targetDir))
			{
				Log ("Creating directory: " + targetDir);
				Directory.CreateDirectory(targetDir);
			}
			
			byte[] responseBytes = new byte[0];
			try
			{
				var outStream = File.Create(targetLocation);
				CopyStream(stream, outStream);
				outStream.Close ();
				response.StatusCode = 200;
				response.ContentType = "text/plain";
				responseBytes = Encoding.UTF8.GetBytes ("SUCCESS");
				Log ("Succeeded");
			}
			catch(Exception e)
			{
				Log (e.Message);
				Log ("Failed");
			}

			response.OutputStream.Write (responseBytes, 0, responseBytes.Length);
			response.OutputStream.Close ();
		}
		
		static void GetFileHandler(HttpListenerRequest request, HttpListenerResponse response)
		{
			var query = request.QueryString;
			string targetLocation = query["target"];
			Log ("GetFile: " + targetLocation + "...");
			
			if(File.Exists(targetLocation))
			{
				try
				{
					using(var inStream = File.OpenRead(targetLocation))
					{
						response.StatusCode = 200;
						response.ContentType = "application/octet-stream";
						CopyStream(inStream, response.OutputStream);
					}
				}
				catch(Exception e)
				{
					Log (e.Message);
					response.StatusCode = 501;
				}
			}
			else
			{
				response.StatusCode = 501;
				Log ("File doesn't exist");
			}
			response.OutputStream.Close ();
		}
		
		static void GetFileListHandler(HttpListenerRequest request, HttpListenerResponse response)
		{
			var query = request.QueryString;
			string targetLocation = query["target"];
			Log ("GetFileList: " + targetLocation + "...");
			
			var files = Directory.GetFileSystemEntries(targetLocation);
			
			string output = "";
			foreach(var file in files) output = output + file + "\n";
			
			response.StatusCode = 200;
			response.ContentType = "text/plain";
			byte[] responseBytes = Encoding.UTF8.GetBytes(output);
			response.OutputStream.Write (responseBytes, 0, responseBytes.Length);
			response.OutputStream.Close ();
		}
		
		static void StartProgramHandler(HttpListenerRequest request, HttpListenerResponse response)
		{
			string programPath = "";
			string args = "";
			string workingDir = null;
			var env = new List<Tuple<string, string>>();
			
			var query = request.QueryString;
			foreach(var itemName in query.AllKeys)
			{
				var itemValue = query[itemName];
				if(itemName == "target")
					programPath = itemValue;
				else if(itemName == "workingDir")
					workingDir = itemValue;
				else if(itemName == "args")
					args = itemValue;
				
				else if(itemName.StartsWith("env."))
					env.Add(Tuple.Create (itemName.Replace ("env.",""), itemValue));
			}
			
			Log ("StartProgram: " + programPath);
			Log ("	Args: " + args);
			Log ("	Working Directory: " + workingDir);
			
			ProcessStartInfo ps = new ProcessStartInfo(programPath, args);
			
			foreach(var e in env)
			{
				Log ("	Environment: " + e.Item1 + " = " + e.Item2);
				ps.EnvironmentVariables[e.Item1] = e.Item2; 
			}
			
			if(workingDir == null)
				workingDir = Path.GetDirectoryName(programPath);
			
			ps.WorkingDirectory = workingDir;
			ps.RedirectStandardError = ps.RedirectStandardOutput = true;
			ps.UseShellExecute = false;
			
			Process p = new Process();
			p.StartInfo = ps;
			p.EnableRaisingEvents = true;
			p.Start ();
			string id = p.Id.ToString();
			
			Log ("	Id: " + id);
			
			ProgramResult result = new ProgramResult()
			{
				Complete = false,
				ProgramId = id
			};
			
			results[id] = result;
			
			p.Exited += (sender, e) =>
			{
				lock(results)
				{
					Log ("Program Exited: " + programPath + " (" + p.ExitCode + ")");
					result.StandardOutput = p.StandardOutput.ReadToEnd();
					result.StandardError = p.StandardError.ReadToEnd ();
					result.ExitCode = p.ExitCode;
					result.Complete = true;
				}
			};
			
			Thread.Sleep(QuickieSleepTime);
			
			SendProgramResponse(id, response);
		}
		
		static void SendProgramResponse(string id, HttpListenerResponse response)
		{
			response.StatusCode = 200;
			
			ProgramResult res = null;
			lock(results)
			{
				res = results[id];
			}
			
			byte[] bytes = Encoding.UTF8.GetBytes (jss.Serialize (res));
			response.OutputStream.Write(bytes, 0, bytes.Length);
			response.OutputStream.Close();
		}
		
		static void GetProgramResultHandler(HttpListenerRequest request, HttpListenerResponse response)
		{
			var query = request.QueryString;
			var pid = query["id"];
			
			Log ("GetProgramResult: " + pid);
			
			SendProgramResponse(pid, response);
		}
		
		static void Log(string message)
		{
			Console.WriteLine (message);
		}
		
		public static void Main(string[] args)
		{
			int port = 9001;
			var server = new VMAgentServer(port);
			
			server.InstallHandler("GetFile", GetFileHandler);
			server.InstallHandler("PutFile", PutFileHandler);
			server.InstallHandler("GetFileList", GetFileListHandler);
			server.InstallHandler ("StartProgram", StartProgramHandler);
			server.InstallHandler("GetProgramResult", GetProgramResultHandler);
			
			new Thread(server.Run).Start ();
			/*
			var remoteAgent = new RemoteVMAgent("localhost", port);
			
			Console.WriteLine (remoteAgent.GetFileList("/").Aggregate(" ", (acc, e) => acc + e + "\n"));
			
			//var strm = new MemoryStream();
			//remoteAgent.GetFile ("/initrd.img", strm);
			//Console.WriteLine("Position: " + strm.Position);
			//strm.Seek (0, SeekOrigin.Begin);
			
			using(FileStream fs = File.OpenRead("/initrd.img"))
			{
				remoteAgent.PutFile(fs, "/home/lee/lol.img");
			}
			
			//remoteAgent.GetFile("/initrd.img", ); */
		}
	}
	
	public class VMAgentServer
	{
		public delegate void HandlerFunc(HttpListenerRequest request, HttpListenerResponse response);
		
		HttpListener listener;
		
		IDictionary<string, HandlerFunc> dict = new Dictionary<string, HandlerFunc>();
		
		public VMAgentServer (int port)
		{
			listener = new HttpListener();
			listener.Prefixes.Add ("http://*:" + port.ToString() + "/");
		}
		
		public void InstallHandler(string name, HandlerFunc func)
		{
			dict["/" + name] = func;
		}
		
		public void Run()
		{
			listener.Start ();
			while(true)
			{
				var context = listener.GetContext();
				
				bool error = false;
				try
				{
					HandlerFunc handler;
					if(dict.TryGetValue(context.Request.Url.LocalPath, out handler))
					{
						handler(context.Request, context.Response);
						context.Response.Close ();
					}
					else
					{
						error = true;
					}
				}
				catch(Exception)
				{
					error = true;
				}
				
				if(error)
				{
					context.Response.StatusCode = 501;
					context.Response.Close ();
				}
			}
		}
	}
}

