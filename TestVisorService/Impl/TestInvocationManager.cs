using System;
using Bizarrefish.TestVisorService.Interface;
using System.Threading;
using System.Collections.Generic;
using Bizarrefish.TestVisorStorage;
using System.Linq;
using System.IO;
using Bizarrefish.VMTestLib.JS;

namespace Bizarrefish.TestVisorService.Impl
{
	class TestInvocationManager
	{
		Thread worker;

		volatile SystemStatus CurrentSystemStatus;

		IList<Action> queue = new List<Action>();

		RedisResultCollection results;
		TestPlanRepository testPlans;

		TestDriverManager testDriverManager;

		MachineRepository machines;

		public TestInvocationManager (RedisResultCollection results, MachineRepository machines, TestPlanRepository tpr, TestDriverManager testDriverManager)
		{
			this.results = results;
			this.machines = machines;
			this.testPlans = tpr;
			this.testDriverManager = testDriverManager;
			worker = new Thread(Run);
		}

		public void Start()
		{
			worker.Start();
		}



		void UpdateStatus(SystemStatus stat)
		{
			CurrentSystemStatus = stat.Clone() as SystemStatus;
		}

		public string EnqueueTestPlan(string testPlanId, IDictionary<string, string> args)
		{
			// Create a new run
			string runId = results.CreateRun("Run at " + DateTime.Now.TimeOfDay);

			lock(queue)
			{
				queue.Add (delegate()
				{
					
					SystemStatus stat = new SystemStatus();

					stat.CurrentTestPlan = testPlanId;
					stat.CurrentTestRun = runId;
					stat.MicroStatus = "Initializing Test Plan";

					UpdateStatus(stat);	// Atomic

					string testPlanCode;
					using(Stream tpStream = testPlans.ReadTestPlan(testPlanId))
					{
						using(StreamReader reader = new StreamReader(tpStream))
						{
							testPlanCode = reader.ReadToEnd();
						}
					}

					// Environment for this run
					IJSTestProvider provider = new TestProvider(
						machines.Drivers.SelectMany (d => d.Machines),
						testDriverManager.Drivers,
						results,
						runId);

					// Javascript runner
					using(JSTestRunner runner = new JSTestRunner("TEST_INIT", args, provider))
					{
						try
						{
							stat.MicroStatus = "Running Test Plan";
							UpdateStatus(stat);

							// Run our javascript
							runner.Execute(testPlanCode, new Dictionary<string, string>());
						}
						catch(Exception e)
						{
							Console.WriteLine(e.Message);
						}


						stat.MicroStatus = "Cleaning Up Snapshots";
						UpdateStatus(stat);

						// This will get run by the "using" block, regardless.
						runner.CleanUp();
					}

					stat.CurrentTestPlan = null;
					stat.CurrentTestRun = null;
					stat.CurrentMachine = null;
					UpdateStatus(stat);
				});
			}

			return runId;
		}

		public SystemStatus GetCurrentStatus()
		{
			return CurrentSystemStatus;
		}


		Action WaitForAction()
		{
			Action act = null;
			while(act == null)
			{
				lock(queue)
				{
					if(queue.Count > 0)
					{
						act = queue[0];
						queue.RemoveAt(0);
					}
				}

				Thread.Sleep(1000);
			}

			return act;
		}

		void Run()
		{
			while(true)
			{
				Action act = WaitForAction();

				act();

			}
		}

	}
}

