using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public class Receipt
	{
		CancellationTokenSource cancellation;
		internal Task<Envelope> task;

		public Receipt(CancellationTokenSource cancellationSource)
		{
			cancellation = cancellationSource;
			task = null;
		}

		public void Cancel()
		{
			cancellation.Cancel();
		}

		public Receipt Callback(Action<Task> callback)
		{
			task.ContinueWith(callback, TaskContinuationOptions.ExecuteSynchronously).Wait();
			return this;
		}

		public Receipt Callback<TReply>(Action<Task, TReply> callback)
		{
			task.ContinueWith(t => callback(task, ((Envelope<TReply>)task.Result).Contents)).Wait();
			return this;
		}

		public bool Wait(TimeSpan timeToWait)
		{
			if (timeToWait.TotalMilliseconds > int.MaxValue)
			{
				timeToWait = TimeSpan.FromMilliseconds(int.MaxValue);
			}

			return task.Wait((int)timeToWait.TotalMilliseconds, cancellation.Token);
		}

		public void Wait()
		{
			task.Wait(cancellation.Token);
		}
	}

	public class Receipt<TReply>
	{

	}

}
