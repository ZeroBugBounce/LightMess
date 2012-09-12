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
		Action<T, CancellationToken> handler;
		public LambdaHandler(Action<T, CancellationToken> handlerAction)
		{
			handler = handlerAction;
		}

		public override Task<Envelope> Handle(T message, CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew<Envelope>(() =>
			{
				handler(message, cancellationToken);
				return null; // no reply so nothing needed here
			}, cancellationToken);
		}
	}

	public class LambdaHandler<T, TResult> : Handler<T, TResult>
	{
		Func<T, CancellationToken, TResult> handler;
		public LambdaHandler(Func<T, CancellationToken, TResult> function)
		{
			handler = function;
		}

		public override Task<Envelope> Handle(T message, CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew<Envelope>(() =>
			{
				return new Envelope<TResult>(handler(message, cancellationToken));
			}, cancellationToken);
		}
	}
}
