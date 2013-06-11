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
		public abstract void Handle(T message, Receipt receipt);
	}

	public abstract class Handler<T, TResult> : Handler<T>
	{

	}
}
