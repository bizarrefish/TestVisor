using System;
using System.Collections.Generic;
using System.IO;
using Bizarrefish.VMLib;

namespace Bizarrefish.VMTestLib
{	

	public class TestResult
	{
		public string TestKey;

		public long? ExecutionTime = null;
		public bool? Success = null;
		public IDictionary<string, object> Properties =
			new Dictionary<string, object>();

		public string StandardOutput = null;
		public string StandardError = null;
	}
	
	/// <summary>
	/// This class manages a repository of tests
	/// and knows how to run them.
	/// </summary>
	public interface ITestDriver
	{	
		/// <summary>
		/// The repository to use for tests and such.
		/// Every test driver needs a repository.
		/// </summary>
		ITestRepository Repo { set; }
		
		/// <summary>
		/// The file extensions associated with this kind of test.
		/// </summary
		string[] FileExtensions { get; }
		
		/// <summary>
		/// The name of this test driver.
		/// </summary>
		string Name { get; }
		
		/// <summary>
		/// A description of this test driver.
		/// </summary>
		string Description { get; }
		
		/// <summary>
		/// A unique id for this test driver.
		/// </summary>
		string Id { get; }
		
		/// <summary>
		/// Installs a new test.
		/// </summary>
		/// <param name='name'>
		/// Name of the new test.
		/// </param>
		/// <param name='source'>
		/// Stream to read the new test from.
		/// </param>
		void InstallTest(string name, Stream source);
		
		/// <summary>
		/// Remove a test.
		/// </summary>
		void RemoveTest(string name);
		
		/// <summary>
		/// The available tests.
		/// </summary>
		IEnumerable<string> Tests { get; }

		/// <summary>
		/// Get the parameter names and descriptions available for a particular test
		/// </summary>
		IDictionary<string, string> GetTestParamters(string testName);

		/// <summary>
		/// Run a test on a machine.
		/// </summary>
		/// <returns>
		/// The test result.
		/// </returns>
		/// <param name='name'>
		/// Name of the test to run.
		/// </param>
		/// <param name='machine'>
		/// Machine to run the test on.
		/// </param>
		/// <param name='bin'>
		/// Place to put artifacts.
		/// </param>
		/// <param name='env'>
		/// Variables to pass to the test.
		/// </param>
		TestResult RunTest(string name,  IMachine machine, ITestResultBin bin, IDictionary<string, string> env);
	}
}

