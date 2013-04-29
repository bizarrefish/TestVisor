using System;
using System.Collections.Generic;
using System.IO;


namespace Bizarrefish.TestVisorService.Interface
{
	public class InfoObject
	{
		public string Id;
		public string Name;
		public string Description;
		
		public override string ToString ()
		{
			return GetType().Name + "(" + Name + ")";
		}
	}
	
	public class TestPlanInfo : InfoObject
	{ }
	
	public class MachineInfo : InfoObject
	{
		public string HypervisorId;
	}
	
	public class TestType : InfoObject
	{
		public string[] FileExtensions;
	}
	
	public class TestInfo : InfoObject
	{
		public string TestTypeId;
	}
	
	public class MachineStatus
	{
		public string CurrentTestPlanId;
		public string Description;
	}
	
	public enum TaskState
	{
		PENDING,
		RUNNING,
		COMPLETE,
		FAILED
	}
	
	/// <summary>
	/// Result of a test plan.
	/// </summary>
	public class TestResultInfo : InfoObject
	{
		public string TestPlanId;
		public bool Success;
		public string Detail;
		public IEnumerable<ArtifactInfo> Artifacts;
	}
	
	public class ArtifactInfo : InfoObject
	{
		public string TestKey;
		public long Length;
		public Func<Stream> OpenStream;
	}
	
	public class HypervisorInfo : InfoObject
	{ }
}