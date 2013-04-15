using System;
using Bizarrefish.VMLib;
using System.Collections.Generic;

namespace Bizarrefish.VMTestLib
{
	public class TestResult
	{
		public bool Succeeded;
		public string Detail;
	}
	
	public interface ITest
	{
		TestResult RunTest(IMachine machine, IDictionary<string, object> parameters);
	}
	
	public class Test
	{
		public Test ()
		{
		}
	}
}

