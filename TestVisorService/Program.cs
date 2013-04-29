using System;
using Bizarrefish.TestVisorService.Impl;
using System.Linq;
using System.IO;
using Bizarrefish.TestVisorService.Interface;

namespace Bizarrefish.TestVisorService
{
	public static class Program
	{	
		static ITestVisorService tvs;
		
		static Program()
		{
			tvs = new Bizarrefish.TestVisorService.Impl.TestVisorService(Directory.GetCurrentDirectory());
		}
		
		static void PrintTestPlans()
		{
			Console.WriteLine("Test Plans:");
			foreach(var plan in tvs.TestPlans)
			{
				Console.WriteLine (plan.Name);
			}
		}
		
		static void ShowTests()
		{
			var typeDict = tvs.TestTypes.ToDictionary(tt => tt.Id, tt => tt.Name);
			
			Console.WriteLine ("Tests:");
			foreach(var test in tvs.Tests)
			{
				Console.WriteLine (test.Name + " (" + typeDict[test.TestTypeId] + ")");
			}
		}
		
		static void AddTestPlan()
		{
			Console.Write("New Test Plan: ");
			string planName = Console.ReadLine();
			
			Console.Write ("Source: ");
			using(var inStream = File.OpenRead(Console.ReadLine ()))
			{
				var tp = tvs.CreateTestPlan(planName);
				using(var outStream = tvs.WriteTestPlan(tp.Id))
				{
					inStream.CopyTo(outStream);
				}
			}
			
			Console.WriteLine (planName + " written. ");
		}
		
		static void AddTest()
		{
			Console.WriteLine ("Create new test...\nTest Types:");
			
			foreach(var type in tvs.TestTypes)
			{
				Console.WriteLine (type.Id + " - " + type.Name);
			}
			
			Console.Write ("Type: ");
			string typeId = Console.ReadLine();
			
			Console.Write ("Name: ");
			string name = Console.ReadLine ();
			
			Console.Write ("Path: ");
			string path = Console.ReadLine();
			
			using(var stream = File.OpenRead(path))
			{
				Console.WriteLine ("Installing test...");
				tvs.CreateTest(stream, name, typeId);
			}
			
			Console.WriteLine("Complete!");
			
		}
	
		
		static void RunTestPlan()
		{
			Console.WriteLine ("Machines:");
			foreach(var machine in tvs.Machines)
			{
				Console.WriteLine (machine.Id + " - " + machine.Name);
			}
			
			Console.Write ("Machine: ");
			string machineId = Console.ReadLine ();
			
			PrintTestPlans();
			
			Console.Write("Test Plan: ");
			string testPlanId = Console.ReadLine ();
			
			tvs.EnqueueTestPlan(machineId, testPlanId, delegate(TaskState state)
			{
				if(state == TaskState.COMPLETE)
				{
					Console.WriteLine ("Task Complete!");
				}
			});
		}
		
		public static void Main(string[] args)
		{
			
			if(tvs.Tests.Select (t => t.Name).Contains ("BatchTest"))
				tvs.DeleteTest("BatchTest");
			
			while(true)
			{
				Console.Write (">");
				string cmd = Console.ReadLine ();
				
				switch(cmd)
				{
				case "ShowTests":
					ShowTests();
					break;
					
				case "ShowTestPlans":
					PrintTestPlans();
					break;
					
				case "NewTestPlan":
					AddTestPlan();
					break;
					
				case "NewTest":
					AddTest();
					break;
					
				case "RunTestPlan":
					RunTestPlan ();
					break;
					
					
				default:
					Console.WriteLine ("Unrecognised command: " + cmd);
					break;
				}
			}
		}
	}
}

