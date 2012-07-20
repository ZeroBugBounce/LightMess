using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public class Messenger
	{
		//public void Send<T>(T message)
		//{
		//    var handler = Handlers[typeof(T)] as Action<T>;
		//    Task.Factory.StartNew(() => handler(message));
		//}

		//public void Send<T>() where T : new()
		//{
		//    var handler = Handlers[typeof(T)];
		//    Task.Factory.StartNew(h => ((Action<T>)h)(new T()), handler);
		//}

		//public void Handle<T>(Action<T> handler)
		//{
		//    Handlers.Add(typeof(T), handler);
		//}

		//Dictionary<Type, Delegate> Handlers = new Dictionary<Type, Delegate>();
	}

	public static class Message
	{
		static Messenger messenger;
		public static void Init(Messenger messenger)
		{
			Message.messenger = messenger;
		}

		public static Receipt Post<T>(T message)
		{
			return default(Receipt);
		}

		public static Receipt Post<T>(T message, Action callback)
		{
			return default(Receipt);
		}


	}
}
