using System;
using Bizarrefish.TestVisorService.Interface;
using System.Linq;
using System.IO;

namespace Bizarrefish.TestVisorService
{
	using Bizarrefish.TestVisorService.Impl;
	public static class Program
	{
		public static void Main(string[] args)
		{
			ITestVisorService tvs = new TestVisorService();
			
			foreach(var tt in tvs.TestTypes)
			{
				Console.WriteLine (tt);
			}
			
			foreach(var t in tvs.Tests)
			{
				Console.WriteLine (t);
			}
			
			using(FileStream s = File.OpenRead("../../BatchTest.bat"))
			{
				tvs.CreateTest(s, "BatchTest", tvs.TestTypes.First().Id);
			}
		}
	}
}

