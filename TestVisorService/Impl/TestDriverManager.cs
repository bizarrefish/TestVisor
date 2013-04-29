using System;
using Bizarrefish.VMTestLib;
using System.Collections.Generic;
using Bizarrefish.TestVisorService.Interface;

namespace Bizarrefish.TestVisorService.Impl
{
	public class TestDriverManager
	{
		IDictionary<string, ITestDriver> driverDict = new Dictionary<string, ITestDriver>();
		
		public IEnumerable<TestInfo> Tests;
		
		public IEnumerable<TestType> TestTypes;
		
		public IEnumerable<ITestDriver> Drivers { get { return driverDict.Values; } }
		
		public void RefreshTests()
		{
			var testList = new List<TestInfo>();
			
			foreach(var driver in driverDict.Values)
			{
				foreach(var test in driver.Tests)
				{
					testList.Add (new TestInfo()
					{
						Id = test,
						Name = test,
						Description = test,
						TestTypeId = driver.Id
					});
				}
			}
			
			Tests = testList;
		}
		
		public ITestDriver GetTestDriver(string id)
		{
			return driverDict[id];
		}
		
		public TestDriverManager (string baseDirectory, IEnumerable<ITestDriver> testDrivers)
		{
			var testTypes = new List<TestType>();
			this.TestTypes = testTypes;
			
			foreach(var driver in testDrivers)
			{
				driver.Repo = new FileBasedTestRepository(baseDirectory + "/" + driver.Id);
				driverDict[driver.Id] = driver;
				
				testTypes.Add (new TestType()
				{
					Id = driver.Id,
					Name = driver.Name,
					Description = driver.Description,
					FileExtensions = driver.FileExtensions
				});
			}
			
			RefreshTests();
		}
	}
}

