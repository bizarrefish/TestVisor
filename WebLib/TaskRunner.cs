using System;
using System.Threading;

namespace Bizarrefish.WebLib
{
	class TaskRunner<TResult>
	{
		bool Complete, Errored;
		Thread TaskThread;
		string ErrorMessage;
		TResult Result;
		Func<TResult> func;
		
		public TaskRunner(Func<TResult> func)
		{
			Complete = false;
			Errored = false;
			ErrorMessage = "";
			this.func = func;
			TaskThread = new Thread(Run);
			TaskThread.Start ();
			TaskThread.Join(500); // Wait here in case it's a quickie.
		}
		
		void Run()
		{
			try
			{
				Result = func();
			}
			catch(Exception e)
			{
				Errored = true;
				ErrorMessage = e.Message;
			}
			Complete = true;
		}
		
		public bool CheckDone(ref bool error, ref TResult result, ref string errorMessage)
		{
			if(Complete)
			{
				error = Errored;
				result = Result;
				errorMessage = ErrorMessage;
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}

