using System;
using System.IO;
using System.Net;
using Mono.Http;
using System.Collections.Generic;
using System.Web;

namespace Bizarrefish.VMAgent
{
	public class ProgramResult
	{
		public int ExitCode;
		public string ProgramId;
		public string StandardOutput;
		public string StandardError;
		public bool Complete;
	}
	
	public interface IRemoteVMAgent
	{
		void PutFile(Stream s, string destination);
		void GetFile(string source, Stream s);
		string[] GetFileList(string dir);
		string StartProgram(string programPath, string workingDir, string args, IDictionary<string, string> env);
		ProgramResult GetProgramResult(string programId);
		string GetDesktopViewerAddress();
	}
	
	public class RemoteVMAgent : IRemoteVMAgent
	{
		string uriPrefix;
		public RemoteVMAgent(string server, int port)
		{
			this.uriPrefix = "http://" + server + ":" + port + "/";
		}
		
		public void PutFile (Stream s, string destination)
		{
			var r = HttpWebRequest.Create (uriPrefix + "PutFile?target=" + HttpUtility.UrlEncode(destination));
			r.Proxy = new WebProxy();
			r.Method = "POST";
			r.ContentType = "application/octet-stream";
			r.ContentLength = s.Length;
			
			using(var reqStream = r.GetRequestStream())
			{
				VMAgent.CopyStream(s, reqStream);
			}
			
			using(var response = r.GetResponse())
			{
				using(var respStream = response.GetResponseStream())
				{
					using(var reader = new StreamReader(respStream))
					{
						string resp = reader.ReadToEnd();
						if(resp != "SUCCESS")
							throw new Exception("Error! Unable to copy file!");
					}
				}
			}
		}

		public void GetFile (string source, Stream s)
		{
			var r = HttpWebRequest.Create(uriPrefix + "GetFile?target=" + HttpUtility.UrlEncode(source));
			r.Proxy = new WebProxy();
			r.Method = "GET";
			r.ContentType = "application/octet-stream";
			using(var response = r.GetResponse())
			{
				using(var respStream = response.GetResponseStream())
				{
					VMAgent.CopyStream(respStream, s);
				}
			}
		}

		public string[] GetFileList (string dir)
		{
			var r = HttpWebRequest.Create (uriPrefix + "GetFileList?target=" + HttpUtility.UrlEncode(dir));
			r.Proxy = new WebProxy();
			r.Method = "GET";
			using(var response = r.GetResponse())
			{
				using(var respStream = response.GetResponseStream())
				{
					using(var reader = new StreamReader(respStream))
					{
						List<string> result = new List<string>();
						while(!reader.EndOfStream)
						{
							result.Add (reader.ReadLine());
						}
						return result.ToArray();
					}
				}
			}
		}


		public string GetDesktopViewerAddress ()
		{
			throw new NotImplementedException ();
		}
		
		ProgramResult GetResult(WebResponse response)
		{
			using(var stream = response.GetResponseStream())
			{
				using(var reader = new StreamReader(stream))
				{
					string str = reader.ReadToEnd();
					return VMAgent.jss.Deserialize<ProgramResult>(str);
				}
			}
		}
		
		public string StartProgram (string programPath, string workingDir, string args, IDictionary<string, string> env)
		{
			var reqString = uriPrefix + "StartProgram?" +
				"target=" + HttpUtility.UrlEncode(programPath) +
				"&workingDir=" + HttpUtility.UrlEncode(workingDir) +
				"&args=" + HttpUtility.UrlEncode(args);
			
			foreach(var e in env)
			{
				reqString += "&env." + e.Key + "=" + e.Value;
			}
			
			var request = HttpWebRequest.Create (reqString);
			request.Proxy = new WebProxy();
			request.Method = "GET";
			using(var response = request.GetResponse())
			{
				var result = GetResult(response);
				return result.ProgramId;
			}
			
		}

		public ProgramResult GetProgramResult (string programId)
		{
			string reqString = uriPrefix + "GetProgramResult?" + "id=" + HttpUtility.UrlEncode(programId);
			var request = HttpWebRequest.Create (reqString);
			request.Proxy = new WebProxy();
			request.Method = "GET";
			using(var response = request.GetResponse())
			{
				return GetResult(response);
			}
		}
	}
}

