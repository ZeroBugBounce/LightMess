using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ZeroBugBounce.LightMess;
using System;

namespace Testing
{
	class SingleThreadedHandlerTests
	{
		[Fact]
		public void Process_messages_on_single_threaded_handler()
		{
			int answer = 0;
			int threadId = -1;

			Message.Init(new Messenger());

			Message.Handle<int, int>((i, c) =>
			{
				if (threadId > -1 && threadId != Thread.CurrentThread.ManagedThreadId)
				{
					throw new InvalidOperationException();
				}
				else
				{
					threadId = Thread.CurrentThread.ManagedThreadId;
				}

				return i + 1;
			}, HandleOption.SingleThread);

			var wait = new ManualResetEventSlim(false);
			Message.Post(1).Callback<int>((t, r) =>
			{
				answer = r;
				wait.Set();
			});

			wait.Wait();

			Assert.Equal(2, answer);
		}
	}
}
