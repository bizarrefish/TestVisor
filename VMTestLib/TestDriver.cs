using System;
using System.Collections.Generic;
using System.IO;
using Bizarrefish.VMLib;

namespace Bizarrefish.VMTestLib
{	
	
	public enum TestResult
	{
		PASSED,
		FAILED,
		ERRORED
	}
	
	/// <summary>
	/// This class manages a repository of tests
	/// and knows how to run them.
	/// </summary>
	public interface ITestDriver
	{	
		ITestRepository Repo { set; }
		
		string[] FileExtensions { get; }
		
		string Name { get; }
		string Description { get; }
		
		string Id { get; }
		
		void InstallTest(string name, Stream source);
		
		void RemoveTest(string name);
		
		IEnumerable<string> Tests { get; }
		
		TestResult RunTest(string name, IMachine machine, ITestResultBin bin, IDictionary<string, string> env);
	}
}

