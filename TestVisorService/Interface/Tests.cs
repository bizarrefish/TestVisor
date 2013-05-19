using System;
using System.Collections.Generic;
using System.IO;
using Bizarrefish.VMTestLib;

namespace Bizarrefish.TestVisorService.Interface
{
	public partial interface ITestVisorService
	{
		
		/// <summary>
		/// List of test types.
		/// </summary>
		IEnumerable<TestType> TestTypes { get; }
		
		/// <summary>
		/// List of tests.
		/// </summary>
		IEnumerable<TestInfo> Tests { get; }
		
		/// <summary>
		/// Create a test of a given type.
		/// </summary>
		TestInfo CreateTest(Stream s, string name, string testTypeId);
		
		/// <summary>
		/// Sets the test's info.
		/// </summary>
		void SetInfo(TestInfo info);
		
		/// <summary>
		/// Delete a test
		/// </summary>
		void DeleteTest(string id);
	}
}

