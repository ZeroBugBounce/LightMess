using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ZeroBugBounce.LightMess
{
	/// <summary>
	/// Post and Handle messages
	/// </summary>
	public class Messenger
	{
		ILock handlerLock = new SpinningHybridLock(spinCount: 1200);
		public Receipt Post<T>(T message)
		{
			Handler<T> handler;				
			handlerLock.Enter();
			handler = handlers[typeof(T)] as Handler<T>;
			handlerLock.Leave();

			var cancellationSource = new CancellationTokenSource();
			var receipt = new Receipt(cancellationSource);

			receipt.task = handler.Handle(message, cancellationSource.Token);

			return receipt;
		}

		public void Handle<T>(Action<T, CancellationToken> action)
		{
			handlers.Add(typeof(T), new LambdaHandler<T>(action));
		}

		public void Handle<T, TReply>(Func<T, CancellationToken, TReply> function)
		{
			handlers.Add(typeof(T), new LambdaHandler<T, TReply>(function));
		}

		public void AddHandler<T>(Handler<T> handler)
		{
			handlerLock.Enter();
			handlers.Add(typeof(T), handler);
			handlerLock.Leave();
		}

		Dictionary<Type, Object> handlers = new Dictionary<Type, Object>();
	}

	/// <summary>
	/// Static convenience for a Messenger instance
	/// </summary>
	public static class Message
	{
		static Messenger messenger;
		public static void Init(Messenger messenger)
		{
			Message.messenger = messenger;
		}

		public static Receipt Post<T>(T message)
		{
			return messenger.Post(message);
		}

		public static void Handle<T>(Action<T, CancellationToken> action)
		{
			messenger.Handle(action);
		}

		public static void Handle<T, TReply>(Func<T, CancellationToken, TReply> function)
		{
			messenger.Handle<T, TReply>(function);
		}

		public static void AddHandler<T>(Handler<T> handler)
		{
			messenger.AddHandler(handler);
		}
	}
}
