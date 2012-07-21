using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ZeroBugBounce.LightMess;

namespace Testing
{
	public class Tests
	{
		[Fact]
		public void Post_a_message()
		{
			bool handlerWasCalled = false;

			Message.Init(new Messenger());
			Message.Handle<NameRequest>((m, c) =>
			{
				handlerWasCalled = true;
			});

			Message.Post(new NameRequest()).Wait(TimeSpan.MaxValue);
			Assert.True(handlerWasCalled);			
		}

		[Fact]
		public void Get_a_callback()
		{
			bool handlerWasCalled = false;
			bool callbackWasCalled = false;

			Message.Init(new Messenger());
			Message.Handle<NameRequest>((m, c) =>
			{
				handlerWasCalled = true;
			});

			Message.Post(new NameRequest())
				   .Callback(t => {
					   System.Diagnostics.Debug.WriteLine("On thread {0}", Thread.CurrentThread.Name);
					   Thread.Sleep(100);  callbackWasCalled = true; })
				   .Wait(TimeSpan.MaxValue);

			Assert.True(handlerWasCalled);
			Assert.True(callbackWasCalled);
		}

		[Fact]
		public void Get_a_reply()
		{
			bool handlerWasCalled = false;
			bool callbackWasCalled = false;
			Account reply = null;

			Message.Init(new Messenger());
			Message.Handle<NameRequest, Account>((m, c) =>
			{
				handlerWasCalled = true;
				return new Account();
			});

			Message.Post(new NameRequest())
				.Callback<Account>((t, a) => {
					System.Diagnostics.Debug.WriteLine("On thread {0}", Thread.CurrentThread.Name);
					callbackWasCalled = true;
					reply = a;})
				.Wait();

			Assert.True(handlerWasCalled);
			Assert.True(callbackWasCalled);
			Assert.NotNull(reply);
		}

		class Account { }

		//[Fact]
		//public void Sending_a_message()
		//{
		//    int count = 0;
		//    var messener = new Messenger();
		//    messener.Handle<int>(i => count+=i);

		//    messener.Send(1);
		//    Thread.Sleep(100);
		//    Assert.Equal(1, count);
		//}

		//[Fact]
		//public void Sending_an_implicitly_created_message()
		//{
		//    object result = null;
		//    var messenger = new Messenger();
		//    messenger.Handle<object>(o => result = o);

		//    messenger.Send<Object>();			
		//    Thread.Sleep(100);
		//    Assert.NotNull(result);
		//}
	}

	public class NameRequest
	{

	}
}
