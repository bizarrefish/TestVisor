using System;
using Bizarrefish.VMLib.Virtualbox;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Bizarrefish.VMTestLib.TestDrivers.WindowsBatch;
using System.IO;

namespace Bizarrefish.VMTestLib
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var vmDriver = new VirtualboxDriver();
			
			var machine = vmDriver.Machines.Where (m => m.Name == "Windows 7").First();
			
			var batchDriver = new BatchFileDriver();
			
			var resultBin = new FileBasedResultBin("../../ResultBin");
			
			batchDriver.Repo = new FileBasedTestRepository("../../BatchTestRepo");
			
			var testFile = "../../BatchTest.bat";
			
			var testName = "BatchTest";
			
			foreach(var test in batchDriver.Tests)
				batchDriver.RemoveTest(test);
			
			Console.WriteLine("Installing test...");
			using(FileStream fs = File.OpenRead (testFile))
			{
				batchDriver.InstallTest(testName, fs);
			}
			
			Console.WriteLine("Initializing VM...");
			var initSnapshotId = machine.GetSnapshots().Where(ss => ss.Name == "TEST_INIT").First().Id;
			machine.Start (initSnapshotId);
			
			Console.WriteLine ("Running test...");
			JSTestRunner testRunner = new JSTestRunner(new ITestDriver[] { batchDriver }, machine, resultBin);
			
			string testCode =
@"

var InitialSnapshot

Debug('This is some javascript code!');
Debug('Lets start by taking a snapshot');
InitialSnapshot = Snapshot();
Debug('Heres the snapshot: ' + InitialSnapshot);

Debug('Time to run a test! Here, we will be telling it to fail...')
var firstResult = BatchTest({PLEASEFAIL: true});
Debug('Test over! The result was: ' + firstResult);
if(firstResult !== false)
{
	throw new Error('That should not have worked... Oops');
}

Debug('Now lets revert to the last snapshot and run the test again.');
Debug('Reverting....');
InitialSnapshot.Restore();
Debug('Done. Now we will run the test in a non-faily way');
var secondResult = BatchTest();
Debug('Awesome! The result was: ' + secondResult);
";
			
			testRunner.Execute(testCode);
			
			//batchDriver.RunTest(testName, machine, resultBin, new Dictionary<string, string>());
		}
	}
	
	/*public class DummyTest : ITest
	{
		public TestResult Result;
		public int WaitTime = 3000;
		public TestResult RunTest (Bizarrefish.VMLib.IMachine machine, System.Collections.Generic.IDictionary<string, object> parameters)
		{
			Thread.Sleep(WaitTime);
			return Result;
		}
	}
	
	public class TestTest
	{
		public TestTest ()
		{
			TestPlan plan = new TestPlan() { LineNumber = 1 };
			plan.Children = new List<ITestPlanNode>();
			plan.Children.Add (new LoadFile()
			{
				URL = "/initrd.img",
				Destination = "/",
				LineNumber = 1
			});
			
			plan.Children.Add (new RunTest() {
				LineNumber = 3,
				Parameters = new Dictionary<string, object>(),
				TestName = "myPassingTest"
			});
			
			TestPlan subPlan = new TestPlan() { LineNumber = 2 };
			subPlan.Children = new List<ITestPlanNode>();
			plan.Children.Add (subPlan);
			subPlan.Children.Add (new LoadFile()
			{
				URL = "/initrd.img",
				Destination = "/initrd.img",
				LineNumber = 2
			});
			
			subPlan.Children.Add (new RunTest() {
				LineNumber = 3,
				Parameters = new Dictionary<string, object>(),
				TestName = "myFailingTest"
			});
			
			var tests = new Dictionary<string, ITest>();
			var passingTest = new DummyTest()
			{
				Result = new TestResult()
				{
					Detail = "This was a passing dummy test",
					Succeeded = true
				},
				WaitTime = 1000
			};
			var failingTest = new DummyTest()
			{
				Result = new TestResult()
				{
					Detail = "This was a failing dummy test",
					Succeeded = false
				},
				WaitTime = 1000
			};
			
			tests["myPassingTest"] = passingTest;
			tests["myFailingTest"] = failingTest;
			
			var vbox = new VirtualboxDriver();
			var firstMachine = vbox.Machines.Where (m => m.Name == "Windows 7").First();
			
			firstMachine.DownloadFile("/initrd.img", "/");
			var interp = new TestPlanInterpreter(plan, firstMachine, tests);
			
			interp.Run();
			
			firstMachine.Shutdown();
		}
	}
	
	public static class Program
	{
		public static void Main(string[] args)
		{
			new TestTest();
		}
	}*/
}

