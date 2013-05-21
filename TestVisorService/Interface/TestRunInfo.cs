using System;
using System.Collections.Generic;
using System.IO;
using Bizarrefish.VMTestLib;


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

	public class TestResultInfo
	{
		public TestResult Result;
		public IEnumerable<Tuple<ArtifactInfo, Func<Stream>>> Artifacts;
	}

	/// <summary>
	/// Result of a test plan.
	/// </summary>
	public class TestRunInfo : InfoObject
	{
		public string TestPlanId;
		public string Detail;
		public IDictionary<string, TestResultInfo> Results;
	}

	public class HypervisorInfo : InfoObject
	{ }
}