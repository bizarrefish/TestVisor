using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bizarrefish.TestVisorService.Interface
{	
	public delegate void TaskStateListener(TaskState newState);
	
	public partial interface ITestVisorService
	{
		
		void EnqueueTestPlan(string machineId, string testPlanId, TaskStateListener listener);
	}
}

