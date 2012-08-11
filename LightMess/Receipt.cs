using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public class Receipt
	{
		CancellationTokenSource cancellation;

		public Task<Envelope> Task { get; internal set; }

		public Receipt(CancellationTokenSource cancellationSource)
		{
			cancellation = cancellationSource;
			Task = null;
		}

		public Receipt Cancel()
		{
			cancellation.Cancel();
			return this;
		}

		public Receipt Callback(Action<Task> callback)
		{
			Task.ContinueWith(callback, TaskContinuationOptions.ExecuteSynchronously).Wait();
			return this;
		}

		public Receipt Callback<TResult>(Action<Task, TResult> callback)
		{
			if (Task.IsCanceled)
			{
				Task.ContinueWith(t => callback(t, default(TResult)),
					TaskContinuationOptions.ExecuteSynchronously).Wait();
			}
			else
			{
				Task.ContinueWith(t => callback(t, ((Envelope<TResult>)t.Result).Contents), 
					TaskContinuationOptions.ExecuteSynchronously).Wait();
			}
			return this;
		}

		public TResult Result<TResult>()
		{
			return ((Envelope<TResult>)Task.Result).Contents;
		}

		public bool Wait(TimeSpan timeToWait)
		{
			if (timeToWait.TotalMilliseconds > int.MaxValue)
			{
				timeToWait = TimeSpan.FromMilliseconds(int.MaxValue);
			}

			return Task.Wait((int)timeToWait.TotalMilliseconds, cancellation.Token);
		}

		public void Wait()
		{
			Task.Wait(cancellation.Token);
		}
	}
}
