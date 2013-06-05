using System;
using Bizarrefish.TestVisorService.Interface;
using System.Collections.Generic;
using Bizarrefish.VMLib;
using Bizarrefish.VMTestLib;
using System.IO;
using Bizarrefish.VMTestLib.TestDrivers.WindowsBatch;
using System.Linq;
using Bizarrefish.VMTestLib.TestDrivers;
using Bizarrefish.VMTestLib.TestDrivers.FileDownloader;
using Bizarrefish.TestVisorStorage;
using ServiceStack.Redis;

namespace Bizarrefish.TestVisorService.Impl
{
	public partial class TestVisorService : ITestVisorService
	{
		TestDriverManager testDriverManager;
		
		void InitTests()
		{
			var client = new RedisClient(TestVisorService.RedisUri);

			testDriverManager = new TestDriverManager(client, baseDirectory + "/TestRepos", new ITestDriver[]
			{
				new BatchFileDriver(),
				new FileDownloader()
			});
		}
		
		/// <summary>
		/// List of test types.
		/// </summary>
		public IEnumerable<TestType> TestTypes
		{
			get { return testDriverManager.TestTypes; }
		}
		
		/// <summary>
		/// List of tests.
		/// </summary>
		public IEnumerable<TestInfo> Tests
		{
			get { return testDriverManager.Tests; }
		}
		
		/// <summary>
		/// Create a test of a given type.
		/// </summary>
		public TestInfo CreateTest(Stream s, string name, string testTypeId)
		{
			var driver = testDriverManager.GetTestDriver(testTypeId);
			driver.InstallTest(name, s);
			testDriverManager.RefreshTests();
			return testDriverManager.Tests.Where (t => t.Id == name).First ();
		}
		
		/// <summary>
		/// Sets the test's info.
		/// </summary>
		public void SetInfo(TestInfo info)
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Delete a test
		/// </summary>
		public void DeleteTest(string id)
		{
			TestInfo testInfo = Tests.Where (t => t.Id == id).First ();
			
			ITestDriver driver = testDriverManager.GetTestDriver(testInfo.TestTypeId);
			driver.RemoveTest(id);
			testDriverManager.RefreshTests();
		}
	}
}

