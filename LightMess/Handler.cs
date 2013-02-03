using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public class Handler
	{
		public Messenger Message { get; internal set; }
	}

	public abstract class Handler<T> : Handler
	{		
		public abstract Task<Envelope> Handle(T message, CancellationToken cancellationToken);
	}

	public abstract class Handler<T, TResult> : Handler<T>
	{

	}
}
