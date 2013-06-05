using System;
using Bizarrefish.VMTestLib;
using Bizarrefish.TestVisorService.Interface;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Bizarrefish.TestVisorStorage;
using ServiceStack.Redis;
using System.Linq;

namespace Bizarrefish.TestVisorService.Impl
{
	public class TestPlanRepository
	{	
		ITestRepository repo;
		
		public IEnumerable<TestPlanInfo> TestPlans
		{
			get
			{
				return repo.Resources.Select (re => planInfos.Load(re));
			}
		}

		public RedisInfoCollection<TestPlanInfo> planInfos;
		
		public TestPlanRepository (RedisClient client, string baseDir, string dbKey)
		{
			repo = new RedisTestRepository(client, baseDir, dbKey);
			planInfos = new RedisInfoCollection<TestPlanInfo>(client, () => new TestPlanInfo());
		}
		
		public void CreateTestPlan(string name)
		{
			repo.CreateResource(name);
		}

		public void SetInfo(TestPlanInfo info)
		{
			planInfos.Store(info);
		}

		public TestPlanInfo GetInfo(string id)
		{
			return planInfos.Load(id);
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
		}
	}
}

