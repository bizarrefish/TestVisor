using System;
using System.Collections.Generic;
using System.IO;

namespace Bizarrefish.VMLib
{
	public enum MachineStatus
	{
		STARTED,
		STOPPED,
		PAUSED
	}
	
	public class Snapshot
	{
		public string Name;
		public string Id;
	}
	
	public class ProgramResult
	{
		public string StandardOutput;
		public string StandardError;
		public int ExitCode;
	}
	
	public interface IMachine
	{
		string Id { get; }
		string Name { get; }
		string Description { get; }
		
		MachineStatus GetCurrentStatus();
		
		void Start(string snapshotId);
		
		void Shutdown();
		
		Snapshot GetSnapshot(string Id);
		IEnumerable<Snapshot> GetSnapshots();
		
		string MakeSnapshot();
		void DeleteSnapshot(string Id);
		
		VMProperties LoadProperties();
		void SaveProperties(VMProperties props);
		
		bool PutFile(string path, Stream s);
		Stream GetFile(string path);
		
		IEnumerable<string> ListFiles(string directory);
		
		bool DownloadFile(string url, string destination);
		ProgramResult RunProgram(string programName, IDictionary<string, string> env);
	}
}

