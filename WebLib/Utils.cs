using System;
using System.Web.Script.Serialization;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Bizarrefish.WebLib
{
	internal static class Utils
	{
		public static string GenRPC =
@"function GenRPC(url, methodName, argNames) {
	return function(args, success, fail) {
		var reqKey = null;

		// The initial request
		var doRequest = function() {
			ajax(url, {Args: args}, callback);
		}

		// The poll request
		var doPoll = function(withArgs) {
			ajax(url, {Key: reqKey}, callback);
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
		
		public static object MakeJavascript(string url, string funcName, string[] funcFields)
		{
			string fieldsArray = Serializer.Serialize(funcFields);
			return "var " + funcName + " = GenRPC('"+ url + "', '" + funcName + "', " + fieldsArray + ");";
		}

	}

	public static class WebUtils
	{
				/// <summary>
		/// Consume a stream until a particular byte sequence is consumed.
		/// 
		/// Optionally, pipe everything up to the byte sequence through outStream.
		/// </summary>
		static bool ReadStreamUntil(Stream s, byte[] until, Stream outStream)
		{
			byte[] ring = new byte[until.Length];
			int fill = 0;

			int ringPtr = 0;

			while(s.CanRead)
			{
				// If true, we're about to overwrite an old byte
				bool overwrite = ring.Length == fill;

				byte oldByte = ring[ringPtr];

				// Add one byte to the ring
				if(s.Read(ring, ringPtr, 1) < 1) break;
				if(fill < until.Length) fill++;
				ringPtr = (ringPtr + 1) % until.Length;

				// Does the ring now match?
				bool wrong = false;
				for(int i = 0; i < until.Length; i++)
				{
					if(until[i] != ring[(ringPtr + i) % until.Length])
					{
						wrong = true;
						break;
					}
				}

				// Regardless, we may need to output the byte we overwrote.
				if(outStream != null)
				{
					if(overwrite)
					{
						outStream.Write (new[] { oldByte }, 0, 1);
					}
				}

				// If the ring now matches, we're done.
				if(!wrong)
				{
					return true;
				}
			}

			return false;

		}


		/// <summary>
		/// Read a line from a stream
		/// </summary>
		static byte[] ExtractCurrentLine(Stream inStream)
		{
			// The boundary code is encoded: CODE\n
			using(MemoryStream ms = new MemoryStream())
			{
				if(!ReadStreamUntil(inStream, Encoding.ASCII.GetBytes("\r\n"), ms))
					return null;

				// Miss out the last byte (the '\n').
				byte[] ret = new byte[ms.Length];

				for(int i = 0; i < ms.Length; i++)
				{
					ret[i] = ms.GetBuffer()[i];
				}
				return ret;
			}

		}

		/// <summary>
		/// Consume the next header from a multipart stream.
		/// Check if the header is for a given field name.
		/// 
		/// This routine also consumes the two newlines after the header,
		/// leaving the data next in the stream.
		/// </summary>
		static bool CheckHeader(Stream s, string targetFieldName)
		{
			const int MaxHeaderSize = 1024;

			using(MemoryStream ms = new MemoryStream())
			{
				if(!ReadStreamUntil (s, Encoding.ASCII.GetBytes("\r\n\r\n"), ms))
					return false;

				if(ms.Length > MaxHeaderSize)
						return false;
				else
				{
					ms.Seek(0, SeekOrigin.Begin);
					using(StreamReader rdr = new StreamReader(ms))
					{
						// Parse(Mostly ignore) the rest of the header, checking to see if
						// it's the one we're looking for.
						while(!rdr.EndOfStream)
						{
							string hLine = rdr.ReadLine ();
							if(hLine.Contains ("name=\"" + targetFieldName + "\""))
							{
								return true;
							}
						}
					}
				}

				return false;
			}

		}

		/// <summary>
		/// Read a stream until a boundary code is reached.
		/// </summary>
		static bool ExtractToBoundary(Stream inStream, byte[] boundaryCode, Stream outStream)
		{
			if(!ReadStreamUntil(inStream, boundaryCode, outStream))
				return false;

			return true;
		}

		public static bool ExtractMultipartFileData(string fieldName, Stream inStream, Stream outStream)
		{
			byte[] newLine = Encoding.ASCII.GetBytes("\r\n");

			// Read the boundary code from the first line. Stick it between some '\n' characters.
			byte[] boundaryCode = newLine.Concat(ExtractCurrentLine(inStream)).Concat(newLine).ToArray();

			// Go through the headers
			while(inStream.CanRead)
			{
				if(CheckHeader(inStream, fieldName))
				{
					// This is the right header, grab the data.
					return ExtractToBoundary(inStream, boundaryCode, outStream);
				}
				else
				{
					// This is the wrong header, read until the next boundary, throwing away what's there.
					if(!ExtractToBoundary(inStream, boundaryCode, null))
						return false;
				}
			}
			return false;
		}

		public static T WithFileStreams<T>(Func<Func<string, FileStream>, T> body)
		{
			var streams = new List<FileStream>();

			try
			{
				return body(delegate(string fileName)
				{
					var stream = File.Open (fileName, FileMode.OpenOrCreate);
					streams.Add(stream);
					return stream;
				});
			}
			finally
			{
				foreach(var str in streams)
					str.Dispose();
			}
		}

		public static void Main(string[] args)
		{
			WithFileStreams(toStream => {

				toStream("/").CopyTo(toStream("//"));

				return 0;

			});

		}
	}
}

