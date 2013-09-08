using System;
using Bizarrefish.TestVisorService.Interface;
using System.IO;
using Bizarrefish.WebLib;

namespace Visor
{
	public class Streams<TSession>
		where TSession : new()
	{
		ITestVisorService tvs;
		Bizarrefish.WebLib.HTTPServer<TSession> server;

		const string ARTIFACT_KEY = "Artifacts";
		const string TESTS_KEY = "Tests";

		const string TEST_FILE_FIELD_NAME = "testFile";

		public Streams (Bizarrefish.WebLib.HTTPServer<TSession> server, ITestVisorService tvs)
		{
			this.tvs = tvs;
			this.server = server;

			// Artifacts
			server.AddStreamReadFunc(ARTIFACT_KEY, key => {
				string runId, testKey;
				int index;
				SplitArtifactKey(key, out runId, out testKey, out index);
				return tvs.ReadArtifact(runId, testKey, index);
			});

			// Tests
			server.AddStreamWriteFunc(TESTS_KEY, (key, stream) => {
				string testTypeId, testName;
				SplitTestUploadKey(key, out testTypeId, out testName);

				string tempFile = Path.GetTempFileName();
				try
				{
					using(var fileStream = File.Open(tempFile, FileMode.OpenOrCreate))
					{
						WebUtils.ExtractMultipartFileData(TEST_FILE_FIELD_NAME, stream, fileStream);

						fileStream.Seek (0, SeekOrigin.Begin);

						tvs.CreateTest(fileStream, testName, testTypeId);
					}
				}
				finally
				{
					File.Delete(tempFile);
				}

			});
		}


		public string GetArtifactUrl(string runId, string testKey, int index)
		{
			string str = runId + ":" + testKey + ":" + index;
			return server.GetStreamUrl(ARTIFACT_KEY, System.Uri.EscapeDataString(str));
		}

		public string GetTestUploadUrl(string testName, string testTypeId)
		{
			string str = testTypeId + ":" + testName;
			return server.GetStreamUrl(TESTS_KEY, System.Uri.EscapeDataString(str));
		}

		public void SplitArtifactKey(string key, out string runId, out string testKey, out int index)
		{
			string[] parts = System.Uri.UnescapeDataString(key).Split(':');
			runId = parts[0];
			testKey = parts[1];
			index = int.Parse(parts[2]);
		}

		public void SplitTestUploadKey(string key, out string testTypeId, out string testName)
		{

			string[] parts = System.Uri.UnescapeDataString(key).Split(':');
			testTypeId = parts[0];
			testName = parts[1];
		}
	}
}

