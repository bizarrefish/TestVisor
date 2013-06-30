using System;
using Bizarrefish.TestVisorService.Interface;

namespace Visor
{
	public class Streams<TSession>
		where TSession : new()
	{
		ITestVisorService tvs;
		Bizarrefish.WebLib.HTTPServer<TSession> server;

		const string ARTIFACT_KEY = "Artifacts";

		public Streams (Bizarrefish.WebLib.HTTPServer<TSession> server, ITestVisorService tvs)
		{
			this.tvs = tvs;
			this.server = server;

			// Artifacts
			server.AddStreamFunc(ARTIFACT_KEY, key => {
				string runId, testKey;
				int index;
				SplitArtifactKey(key, out runId, out testKey, out index);
				return tvs.ReadArtifact(runId, testKey, index);
			});
		}


		public string GetArtifactUrl(string runId, string testKey, int index)
		{
			string str = runId + ":" + testKey + ":" + index;
			return server.GetStreamUrl(ARTIFACT_KEY, System.Uri.EscapeDataString(str));
		}

		public void SplitArtifactKey(string key, out string runId, out string testKey, out int index)
		{
			string[] parts = System.Uri.UnescapeDataString(key).Split(':');
			runId = parts[0];
			testKey = parts[1];
			index = int.Parse(parts[2]);
		}
	}
}

