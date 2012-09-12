using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public abstract class Handler<T>
	{
		public Messenger Message { get; internal set; }
		public abstract Task<Envelope> Handle(T message, CancellationToken cancellationToken);
	}

	public abstract class Handler<T, TResult> : Handler<T>
	{

	}
}
