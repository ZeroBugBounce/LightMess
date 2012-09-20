using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public class SingleThreadedLambdaHandler<T> : Handler<T>
	{
		SingleThreadedTaskScheduler taskScheduler;
		Action<T, CancellationToken> handler;

		public SingleThreadedLambdaHandler(Action<T, CancellationToken> handlerAction)
			: this(new SingleThreadedTaskScheduler(), handlerAction) { }

		public SingleThreadedLambdaHandler(SingleThreadedTaskScheduler scheduler, Action<T, CancellationToken> handlerAction)
		{
			taskScheduler = scheduler;
			handler = handlerAction;
		}

		public override Task<Envelope> Handle(T message, CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew<Envelope>(() =>
			{
				handler(message, cancellationToken);
				return null; // no reply so nothing needed here
			}, cancellationToken, TaskCreationOptions.None, taskScheduler);
		}
	}

	public class SingleThreadedLambdaHandler<T, TReply> : Handler<T, TReply>
	{
		SingleThreadedTaskScheduler taskScheduler;
		Func<T, CancellationToken, TReply> handler;

		public SingleThreadedLambdaHandler(Func<T, CancellationToken, TReply> handlerFunction)
			: this(new SingleThreadedTaskScheduler(), handlerFunction) { }

		public SingleThreadedLambdaHandler(SingleThreadedTaskScheduler scheduler, Func<T, CancellationToken, TReply> handlerFunction)
		{
			taskScheduler = scheduler;
			handler = handlerFunction;
		}

		public override Task<Envelope> Handle(T message, CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew<Envelope>(() =>
			{
				return new Envelope<TReply>(handler(message, cancellationToken));
			}, cancellationToken, TaskCreationOptions.None, taskScheduler);
		}
	}
}
