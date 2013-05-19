using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Bizarrefish.VMTestLib.TestDrivers.FileDownloader
{
	internal class FileToDownload
	{
		public string Url;
		public string TargetPath;
	}

	internal class FileDownloaderBlob
	{
		public IDictionary<string, IList<FileToDownload>> Tests =
			new Dictionary<string, IList<FileToDownload>>();
	}


	/// <summary>
	/// File downloader driver.
	/// Takes files with lines of the form:
	/// URL,targetpath
	/// </summary>
	public class FileDownloader : ITestDriver
	{
		public FileDownloader ()
		{

		}

		public void InstallTest (string name, System.IO.Stream source)
		{
			var blob = Repo.Load<FileDownloaderBlob>();

			if(blob.Tests.ContainsKey(name))
				throw new Exception("Test: " + name + " already exists");
			else
			{
				blob.Tests[name] = new List<FileToDownload>();
				using(var reader = new StreamReader(source))
				{
					var parts = reader.ReadToEnd().Split (',');
					blob.Tests[name].Add(new FileToDownload()
					{
						TargetPath = parts[1],
						Url = parts[0]
					});
				}
			}

			Repo.Store (blob);
		}

		public void RemoveTest (string name)
		{
			var blob = Repo.Load<FileDownloaderBlob>();
			blob.Tests.Remove(name);
			Repo.Store(blob);
		}

		public TestResult RunTest (string name, string testKey, Bizarrefish.VMLib.IMachine machine, ITestResultBin bin, System.Collections.Generic.IDictionary<string, string> env)
		{
			var blob = Repo.Load<FileDownloaderBlob>();

			WebClient wc = new WebClient();
			wc.Headers.Add ("user-agent", "FileDownloader Test Driver");

			IList<FileToDownload> ftd;
			if(blob.Tests.TryGetValue(name, out ftd))
			{
				foreach(var file in ftd)
				{
					using(Stream s = wc.OpenRead(file.Url))
					{
						machine.PutFile(file.TargetPath, s);
					}
				}
			}
			else
			{
				throw new Exception("Test: " + name + " doesn't exist");
			}

			var tr = new TestResult();
			tr.Success = true;
			return tr;
		}

		public ITestRepository Repo { get; set; }

		public string[] FileExtensions {
			get {
				return new[] { ".csv" };
			}
		}

		public string Name {
			get {
				return "File Downloader";
			}
		}

		public string Description {
			get {
				return "File Downloading Test";
			}
		}

		public string Id {
			get {
				return "FileDownloader";
			}
		}

		public System.Collections.Generic.IEnumerable<string> Tests {
			get {
				var blob = Repo.Load<FileDownloaderBlob>();
				return blob.Tests.Keys;
			}
		}


		public IDictionary<string, string> GetTestParamters (string testName)
		{
			throw new System.NotImplementedException ();
		}

	}
}

