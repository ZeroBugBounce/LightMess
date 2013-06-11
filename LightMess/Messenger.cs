using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;

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
			Handler<T> handler = null;
			Interlocked.CompareExchange(ref handler, Store<T>.Handler, null);

			var receipt = new Receipt();
			handler.Handle(message, receipt);
			return receipt;
		}

		public void Handle<T>(Action<T> action)
		{
			Interlocked.CompareExchange(ref Store<T>.Handler, new LambdaHandler<T>(action), null);
		}

		public void Handle<T, TResult>(Func<T, TResult> function)
		{
			Interlocked.CompareExchange(ref Store<T>.Handler, new LambdaHandler<T, TResult>(function), null);
		}

		public void Handle<T>(Action<T> action, HandleOption options)
		{
			if (options.HasFlag(HandleOption.SingleThread))
			{
				Interlocked.CompareExchange(ref Store<T>.Handler, new SingleThreadedLambdaHandler<T>(action), null);
			}
			else
			{
				Interlocked.CompareExchange(ref Store<T>.Handler, new LambdaHandler<T>(action), null);
			}
		}

		public void Handle<T, TResult>(Func<T, TResult> function, HandleOption options)
		{
			if (options.HasFlag(HandleOption.SingleThread))
			{
				Interlocked.CompareExchange(ref Store<T>.Handler, new SingleThreadedLambdaHandler<T, TResult>(function), null);
			}
			else
			{
				Interlocked.CompareExchange(ref Store<T>.Handler, new LambdaHandler<T, TResult>(function), null);
			}
		}

		public void AddHandler<T>(Handler<T> handler)
		{
			handlerLock.Enter();
			handler.Message = this;
			Interlocked.CompareExchange(ref Store<T>.Handler, handler, null);
			handlerLock.Leave();
		}

		public void ScanAndLoadHandlers(Assembly assembly)
		{
			var handlerTypes = assembly.GetTypes()
				.Where(t => t.IsClass && t.BaseType.IsGenericType &&
						(t.InheritsFrom(typeof(Handler<>)) ||
						 t.InheritsFrom(typeof(Handler<,>))) &&
							t.GetConstructors()
							 .Where(c => c != null && c.GetParameters() != null &&
									c.GetParameters().Length == 0).Any()).ToArray();

			var detectedHandlers = new List<Tuple<Type, Object>>();

			foreach (var handlerType in handlerTypes)
			{
				Handler handler = Activator.CreateInstance(handlerType) as Handler;
				handler.Message = this;
				detectedHandlers.Add(new Tuple<Type, Object>(handlerType.BaseType.GetGenericArguments()[0], handler));
			}

			foreach (var newHandler in detectedHandlers)
			{
				var type = typeof(Store<>).MakeGenericType(new[] { newHandler.Item1 });
				var field = type.GetField("Handler");
				field.SetValue(type, newHandler.Item2);
			}
		}

		internal static class Store<T>
		{
			public static Handler<T> Handler;
		}
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

		public static void Handle<T>(Action<T> action)
		{
			messenger.Handle(action);
		}

		public static void Handle<T, TResult>(Func<T, TResult> function)
		{
			messenger.Handle<T, TResult>(function);
		}

		public static void Handle<T>(Action<T> action, HandleOption options)
		{
			messenger.Handle<T>(action, options);
		}

		public static void Handle<T, TResult>(Func<T, TResult> function, HandleOption options)
		{
			messenger.Handle<T, TResult>(function, options);
		}

		public static void AddHandler<T>(Handler<T> handler)
		{
			messenger.AddHandler(handler);
		}
	}

	[Flags]
	public enum HandleOption : int 
	{
		None = 0,
		SingleThread = 1
	}
}
