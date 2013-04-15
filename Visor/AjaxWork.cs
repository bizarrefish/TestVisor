using System;

namespace Visor
{
	// Some work, connected to an endpoint
	/*public class AjaxWork<TSessionData, TResult> : Web.PollingAjaxEndpoint<TSessionData>.IPolledObject
	{
		WorkSubmitter<TResult> sub;
		
		public Web.StatusObject GetPollStatus(TSessionData sessionData)
		{
			return new Web.StatusObject()
			{
				CompleteFraction = 0,
				Done = false
			};
		}
		
		public void Go(Visor.Web.AjaxRequestObject req)
		{
		}
		
		public AjaxWork (Worker w)
		{
			sub = new WorkSubmitter<TResult>(w);
		}
	}*/
}

