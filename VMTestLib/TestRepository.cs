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
		Stream Write();
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

	public class ArtifactInfo
	{
		public string Name;
		public string Description;
		public string FileName;

		public ArtifactInfo()
		{
		}

		public ArtifactInfo(ArtifactInfo ai)
		{
			this.Name = ai.Name;
			this.Description = ai.Description;
			this.FileName = ai.FileName;
		}
	}

	/// <summary>
	/// A per-testplan place to put artifacts.
	/// 
	/// Artifacts are grouped by 'Test Keys'
	/// </summary>
	public interface ITestResultBin
	{
		void PutArtifact(ArtifactInfo info, Stream stream);
	}
}

