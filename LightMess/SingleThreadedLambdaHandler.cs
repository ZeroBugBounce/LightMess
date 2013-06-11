using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public class SingleThreadedLambdaHandler<T> : Handler<T>
	{
		Action<T> handler;
		SingleThreadedDispatcher dispatcher;

		public SingleThreadedLambdaHandler(Action<T> handlerAction) : this(handlerAction, new SingleThreadedDispatcher()) { }

		public SingleThreadedLambdaHandler(Action<T> handlerAction, SingleThreadedDispatcher singleThreadedDispatcher)
		{
			handler = handlerAction;
			dispatcher = singleThreadedDispatcher;
		}

		public override void Handle(T message, Receipt receipt)
		{
			dispatcher.Post(() =>
			{
				handler(message);
				receipt.FireCallback();
			});
		}
	}

	public class SingleThreadedLambdaHandler<T, TReply> : Handler<T, TReply>
	{
		Func<T, TReply> handler;
		SingleThreadedDispatcher dispatcher;

		public SingleThreadedLambdaHandler(Func<T, TReply> handlerAction) : this(handlerAction, new SingleThreadedDispatcher()) { }

		public SingleThreadedLambdaHandler(Func<T, TReply> handlerFunction, SingleThreadedDispatcher singleTheadedDispatcher)
		{
			handler = handlerFunction;
			dispatcher = singleTheadedDispatcher;
		}

		public override void Handle(T message, Receipt receipt)
		{
			dispatcher.Post(() => receipt.FireCallback(handler(message)));
		}
	}

	public class SingleThreadedDispatcher
	{
		Thread thread;
		Queue<Action> queue;
		readonly Object syncLock = new Object();
		ManualResetEventSlim startupWaitHandle;

		public bool ExecutingSingleThreaded
		{
			get { return Thread.CurrentThread == thread;}
		}

		public void Post(Action action)
		{
			EnsureStarted();

			if (!ExecutingSingleThreaded)
			{
				Monitor.Enter(syncLock);
				queue.Enqueue(action);
				Monitor.Pulse(syncLock);
				Monitor.Exit(syncLock);
			}
		}

		public Int32 QueueLength
		{
			get
			{
				return queue.Count;
			}
		}

		void EnsureStarted()
		{
			if (thread == null)
			{
				startupWaitHandle = new ManualResetEventSlim(false);
				queue = new Queue<Action>();
				thread = new Thread(_ => Process());
				thread.IsBackground = true;
				thread.Start();
				startupWaitHandle.Wait();
			}
		}


		void Process()
		{
			Monitor.Enter(syncLock);
			startupWaitHandle.Set();

			while (true)
			{
				Monitor.Wait(syncLock);
				do
				{
					var work = queue.Dequeue();
					Monitor.Exit(syncLock);

					work();

					Monitor.Enter(syncLock);
				}
				while (queue.Count > 0);
			}

		}
	}
}
