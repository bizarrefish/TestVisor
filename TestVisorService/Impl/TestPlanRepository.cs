using System;
using Bizarrefish.VMTestLib;
using Bizarrefish.TestVisorService.Interface;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Bizarrefish.TestVisorStorage;

namespace Bizarrefish.TestVisorService.Impl
{
	public class TestPlanRepository
	{	
		ITestRepository repo;
		
		public IEnumerable<TestPlanInfo> TestPlans;
		
		public void Refresh()
		{
			var testPlans = new List<TestPlanInfo>();
			this.TestPlans = testPlans;
			
			foreach(var resource in repo.Resources)
			{
				testPlans.Add (new TestPlanInfo()
				{
					Id = resource,
					Description = resource,
					Name = resource
				});
			}
		}
		
		public TestPlanRepository (string baseDir, string dbKey)
		{

			repo = new RedisTestRepository(TestVisorService.RedisUri, baseDir, dbKey);
			
			Refresh();
		}
		
		public void CreateTestPlan(string name)
		{
			repo.CreateResource(name);
			Refresh();
		}
		
		public Stream WriteTestPlan(string name)
		{
			return repo.GetResource(name).Write ();
		}
		
		public Stream ReadTestPlan(string name)
		{
			return repo.GetResource(name).Read ();
		}
		
		public void DeleteTestPlan(string name)
		{
			repo.DeleteResource(name);
			Refresh ();
		}
	}
}

