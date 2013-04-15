using System;
using System.Collections.Generic;

namespace Bizarrefish.VMLib
{
	public class VMProperties
	{
		/// <summary>
		/// The size of the memory. (MB)
		/// </summary>
		public int MemorySize;
		
		/// <summary>
		/// The size of the video memory.
		/// </summary>
		public int VideoMemorySize;
		
		/// <summary>
		/// The width of the screen.
		/// </summary>
		public int ScreenWidth;
		
		/// <summary>
		/// The height of the screen.
		/// </summary>
		public int ScreenHeight;
		
		/// <summary>
		/// The number of CPUs.
		/// </summary>
		public int CPUCount;
		
		/// <summary>
		/// The size of the graphics memory.
		/// </summary>
		public int GraphicsMemorySize;
	}
}

