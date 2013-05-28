using System;
using Bizarrefish.TestVisorService.Impl;
using System.Linq;
using System.IO;
using Bizarrefish.TestVisorService.Interface;
using Bizarrefish.VMTestLib;
using System.Collections.Generic;

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
	
		static void ShowResults()
		{
			foreach(var run in tvs.TestRuns)
			{
				Console.WriteLine ("TestRun: " + run.Id);
				foreach(var result in run.Results)
				{
					Console.WriteLine ("\tTestKey: " + result.Key);
					Console.WriteLine ("\tSuccess: " + result.Value.Result.Success);
					foreach(var artifact in result.Value.Artifacts)
					{
						Console.WriteLine ("\t\tArtifact: " + artifact.Name);
					}
				}
			}
		}


		static void RunTest()
		{
			Console.WriteLine ("Machines:");
			foreach(var machine in tvs.Machines)
			{
				Console.WriteLine(machine.Id + " - " + machine.Name);
			}
			
			Console.Write ("Machine: ");
			string machineId = Console.ReadLine ();

			ShowTests();

			Console.Write ("Test: ");
			string testName = Console.ReadLine();

			tvs.RunStandaloneTest(testName, machineId, new Dictionary<string, string>(), delegate(TestRunInfo run)
			{
				foreach(var testKey in run.Results.Keys)
				{
					var result = run.Results[testKey];

					Console.WriteLine("\tTest Key: " + testKey);
					Console.WriteLine("\tSuccess: " + result.Result.Success);

					foreach(var artifact in result.Artifacts)
					{
						Console.WriteLine ("Artifact: " + artifact.Name);
					}
				}
			});
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

			tvs.EnqueueTestPlan(machineId, testPlanId, delegate(string runId, TaskState state)
			{
				Console.WriteLine(runId + " : " + state.ToString());
				if(state == TaskState.COMPLETE)
				{
					Console.WriteLine ("Task Complete!");
					foreach(var run in tvs.TestRuns)
					{
						Console.WriteLine ("TestRun: " + run.Id);
						foreach(var result in run.Results)
						{
							Console.WriteLine ("\tTestKey: " + result.Key);
							Console.WriteLine ("\tSuccess: " + result.Value.Result.Success);
							foreach(var artifact in result.Value.Artifacts)
							{
								Console.WriteLine ("\t\tArtifact: " + artifact.Name);
							}
						}
					}
				}
			});
		}
		
		public static void Main(string[] args)
		{
			while(true)
			{
				Console.Write ("> ");
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

				case "ShowResults":
					ShowResults();
					break;

				case "RunTest":
					RunTest();
					break;
					
				default:
					Console.WriteLine ("Unrecognised command: " + cmd);
					break;
				}
			}
		}
	}
}

