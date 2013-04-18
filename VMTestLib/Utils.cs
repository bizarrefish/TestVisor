using System;
using System.IO;

namespace Bizarrefish.VMTestLib
{
	public static class Utils
	{
		const int BuffSizeBytes = 4096;
		
		public static void CopyStream(Stream input, Stream output)
		{
			
			byte[] buffer = new byte[BuffSizeBytes];
			int nBytes = 0;
			
			while(true)
			{
				nBytes = input.Read(buffer, 0, BuffSizeBytes);
				if(nBytes > 0)
					output.Write(buffer, 0, nBytes);
				else
					break;
			}
			
		}
	}
}

