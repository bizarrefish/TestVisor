using System;
using Bizarrefish.VMLib.Virtualbox;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace Bizarrefish.VMTestLib
{
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

