using System;
using System.Collections.Generic;
using System.IO;
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
			Message.Handle<NameRequest>((m) =>
			{
				handlerWasCalled = true;
			});
			var waitHandle = new ManualResetEvent(false);
			Message.Post(new NameRequest()).Callback(()=> waitHandle.Set());
			waitHandle.WaitOne();
			Assert.True(handlerWasCalled);			
		}

		[Fact]
		public void Get_a_callback()
		{
			bool handlerWasCalled = false;
			bool callbackWasCalled = false;

			Message.Init(new Messenger());
			Message.Handle<NameRequest>((m) =>
			{
				handlerWasCalled = true;
			});
			var waitHandle = new ManualResetEvent(false);
			Message.Post(new NameRequest())
				   .Callback(() =>
				   {
					   System.Diagnostics.Debug.WriteLine("On thread {0}", Thread.CurrentThread.Name);
					   Thread.Sleep(100);
					   callbackWasCalled = true;
					   waitHandle.Set();
				   });

			waitHandle.WaitOne();
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
			Message.Handle<NameRequest, Account>(m =>
			{
				handlerWasCalled = true;
				return new Account();
			});
			var waitHandle = new ManualResetEvent(false);
			Message.Post(new NameRequest())
				.Callback<Account>(a =>
				{
					System.Diagnostics.Debug.WriteLine("On thread {0}", Thread.CurrentThread.Name);
					callbackWasCalled = true;
					reply = a;
					waitHandle.Set();
				});

			waitHandle.WaitOne();

			Assert.True(handlerWasCalled);
			Assert.True(callbackWasCalled);
			Assert.NotNull(reply);
		}

		[Fact]
		public void Handle_exceptions()
		{
			Message.Init(new Messenger());
			Message.Handle<object>(c =>
			{
				throw new InvalidOperationException();
			});

			var receipt = Message.Post(new object());
			
		}

		[Fact]
		public void ScanAndLoadHandlers()
		{
			var messenger = new Messenger();
			messenger.ScanAndLoadHandlers(typeof(Messenger).Assembly);

		}

		class Account { }
	}

	public class NameRequest
	{

	}
}
