using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public abstract class Handler<T>
	{
		public Messenger Message { get; internal set; }
		public abstract Task<Envelope> Handle(T message, CancellationToken cancellation);
	}

	public abstract class Handler<T, TResult> : Handler<T>
	{

	}

	public class LambdaHandler<T> : Handler<T>
	{
		Action<T, CancellationToken> handler;
		public LambdaHandler(Action<T, CancellationToken> action)
		{
			handler = action;
		}

		public override Task<Envelope> Handle(T message, CancellationToken cancellation)
		{
			return Task.Factory.StartNew<Envelope>(() =>
			{
				handler(message, cancellation);
				return null; // no reply so nothing needed here
			}, cancellation);
		}
	}

	public class LambdaHandler<T, TResult> : Handler<T, TResult>
	{
		Func<T, CancellationToken, TResult> handler;
		public LambdaHandler(Func<T, CancellationToken, TResult> function)
		{
			handler = function;
		}

		public override Task<Envelope> Handle(T message, CancellationToken cancellation)
		{
			return Task.Factory.StartNew<Envelope>(() =>
			{
				return new Envelope<TResult>(handler(message, cancellation));
			}, cancellation);			
		}
	}
}
