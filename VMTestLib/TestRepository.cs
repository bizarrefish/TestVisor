using System;
using System.Collections.Generic;
using Bizarrefish.VMLib;
using System.IO;

namespace Bizarrefish.VMTestLib
{
	/// <summary>
	/// Something that a test driver would want
	/// to place on the vm before running a test.
	/// </summary>
	public interface ITestResource
	{
		Stream Read();
		void Write(Stream s);
	}
	
	/// <summary>
	/// A place to store resources.
	/// </summary>
	public interface ITestRepository
	{
		// Accessor functions for repo blob
		void Store<TBlob>(TBlob blob);
		TBlob Load<TBlob>() where TBlob : new();
		
		// Resource files
		IEnumerable<string> Resources { get; }
		ITestResource CreateResource(string name);
		ITestResource GetResource(string name);
		void DeleteResource(string name);
	}
	
	/// <summary>
	/// A per-test place to put artifacts and results.
	/// </summary>
	public interface ITestResultBin
	{
		void PutResult(string testName, bool success, string detail);
		void PutArtifact(string name, Stream stream);
	}
}

