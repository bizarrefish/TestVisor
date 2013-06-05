using System;
using System.Collections.Generic;
using System.IO;
using Bizarrefish.VMTestLib;
using Bizarrefish.TestVisorStorage;


namespace Bizarrefish.TestVisorService.Interface
{
	public class InfoObject : IInfo
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		
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
		public ArtifactInfo[] Artifacts;
	}

	/// <summary>
	/// Result of a test plan.
	/// </summary>
	public class TestRunInfo : InfoObject, IInfo
	{
		public string TestPlanId;
		public string Detail;
		public DateTime When;
		public IDictionary<string, TestResultInfo> Results;
	}

	public class HypervisorInfo : InfoObject
	{ }
}