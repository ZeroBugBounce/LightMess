using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public class LambdaHandler<T> : Handler<T>
	{
		Action<T> handler;

		public LambdaHandler(Action<T> handlerAction)
		{
			handler = handlerAction;
		}

		public override void Handle(T message, Receipt receipt)
		{
			ThreadPool.QueueUserWorkItem(_ =>
			{
				handler(message);
				receipt.FireCallback();				
			});
		}
	}

	public class LambdaHandler<T, TResult> : Handler<T, TResult>
	{
		Func<T, TResult> handler;
		public LambdaHandler(Func<T, TResult> function)
		{
			handler = function;
		}

		public override void Handle(T message, Receipt receipt)
		{
			ThreadPool.QueueUserWorkItem(_ =>
			{
				receipt.FireCallback(handler(message));
			});
		}
	}
}
