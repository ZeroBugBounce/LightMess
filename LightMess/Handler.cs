using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ZeroBugBounce.LightMess
{
	public abstract class Handler<T>
	{
		public abstract Envelope Handle(T message, CancellationToken cancellation);
	}

	public abstract class Handler<T, TReply> : Handler<T>
	{

	}

	public class LambdaHandler<T> : Handler<T>
	{
		Action<T, CancellationToken> handler;
		public LambdaHandler(Action<T, CancellationToken> action)
		{
			handler = action;
		}

		public override Envelope Handle(T message, CancellationToken cancellation)
		{
			handler(message, cancellation);
			return null; // no reply so nothing needed here
		}
	}

	public class LambdaHandler<T, TReply> : Handler<T, TReply>
	{
		Func<T, CancellationToken, TReply> handler;
		public LambdaHandler(Func<T, CancellationToken, TReply> function)
		{
			handler = function;
		}
		public override Envelope Handle(T message, CancellationToken cancellation)
		{
			return new Envelope<TReply>(handler(message, cancellation));
		}
	}
}
