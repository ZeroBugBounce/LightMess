using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public class Receipt
	{
		Delegate callback;
		Envelope result;
		long callbackAttempted = 0;

		internal Receipt()
		{

		}

		public Receipt Callback(Action callback)
		{
			if (Interlocked.Read(ref callbackAttempted) != 0)
			{
				callback();
			}
			else
			{
				this.callback = callback;
			}

			return this;
		}

		public Receipt Callback<TResult>(Action<TResult> callback)
		{
			if (Interlocked.Read(ref callbackAttempted) != 0)
			{
				callback(((Envelope<TResult>)result).Contents);
			}
			else
			{
				this.callback = callback;
			}
			return this;
		}

		internal void FireCallback()
		{
			if (callback != null)
			{
				((Action)callback)();
			}

			Interlocked.CompareExchange(ref callbackAttempted, 1, 0);
		}

		internal void FireCallback<TResult>(TResult result)
		{
			if (callback != null)
			{
				((Action<TResult>)callback)(result);
			}
			else
			{
				this.result = new Envelope<TResult>(result);
			}

			Interlocked.CompareExchange(ref callbackAttempted, 1, 0);
		}
	}
}
