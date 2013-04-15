using System;
using System.Collections.Generic;
using Bizarrefish.VMLib;
using System.Linq;

namespace Bizarrefish.VMTestLib
{
	
	public interface ITestPlanNode
	{
		int LineNumber { get; set;}
	}
	
	public class LoadFile : ITestPlanNode
	{
		public string URL;
		public string Destination;
		public int LineNumber { get; set; }
	}
	
	public class RunTest : ITestPlanNode
	{
		public IDictionary<string, object> Parameters;
		public string TestName;
		public int LineNumber { get; set; }
	}
	
	public class TestPlan : ITestPlanNode
	{
		public IList<ITestPlanNode> Children;
		public int LineNumber { get; set; }
	}
	
	public class TestPlanInterpreter
	{
		
		IMachine machine;
		IDictionary<string, ITest> testCollection;
		
		public int LineNumber;
		const string InitSnapshotName = "TEST_INIT";
		public TestResult[] TestResults;
		
		Func<TestResult>[] testFuncs;
		int currentFunc;
		
		public TestPlanInterpreter(TestPlan plan, IMachine machine,
			IDictionary<string, ITest> testCollection)
		{
			this.machine = machine;
			this.testCollection = testCollection;
			
			// Trash old snapshots
			foreach(var snap in machine.GetSnapshots().ToArray())
			{
				if(snap.Name != InitSnapshotName) machine.DeleteSnapshot(snap.Id);
			}
			
			// Start init snapshot
			machine.Start(machine.GetSnapshots().Where (ss => ss.Name == InitSnapshotName).First ().Id);
			
			LineNumber = plan.LineNumber;
			testFuncs = GetFunctions(plan).ToArray();
			TestResults = new TestResult[testFuncs.Length];
			currentFunc = 0;
		}
		
		public TestResult[] Run()
		{
			// Just step until done
			while(Step ())
			{ }
			
			return TestResults;
		}
		
		public bool Step()
		{
			if(currentFunc < testFuncs.Length)
			{
				TestResults[currentFunc] = testFuncs[currentFunc]();
				currentFunc++;
			}
			
			return currentFunc < testFuncs.Length;
		}

		IEnumerable<Func<TestResult>> GetFunctions(TestPlan plan)
		{
			string snapshotId = null;
			Func<TestResult> snapshotFunc = delegate()
			{
				
				LineNumber = plan.LineNumber;
				snapshotId = machine.MakeSnapshot();
				return new TestResult()
				{
					Succeeded = true,
					Detail = "Created Snapshot: " + snapshotId
				};
			};
			
			Func<TestResult> restoreFunc = delegate()
			{
				LineNumber = plan.LineNumber;
				machine.Start(snapshotId);
				return new TestResult()
				{
					Succeeded = true,
					Detail = "Restored Snapshot: " + snapshotId
				};
			};
			
			var funcList = new List<Func<TestResult>>();
			funcList.Add (snapshotFunc);
			foreach(dynamic child in plan.Children)
			{
				funcList.AddRange (GetFunctions(child));
			}
			
			funcList.Add (restoreFunc);
			return funcList;
		}
			
		IEnumerable<Func<TestResult>> GetFunctions(LoadFile node)
		{
			return new Func<TestResult>[]
			{
				() => new TestResult()
				{
					Succeeded = machine.DownloadFile(node.URL, node.Destination),
					Detail = "Download file: " + node.URL + " to VM at: " + node.Destination
				}
			};
		}
			
		IEnumerable<Func<TestResult>> GetFunctions(RunTest node)
		{
			return new Func<TestResult>[]
			{
				delegate()
				{
					ITest test = testCollection[node.TestName];
					return test.RunTest(machine, node.Parameters);
				}
			};
		}
	}
}

