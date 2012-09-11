using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ZeroBugBounce.LightMess;

namespace Testing
{
	class SingleThreadedHandlerTests
	{
		[Fact]
		public void Process_messages_on_single_threaded_handler()
		{
			int answer = 0;
			Message.Init(new Messenger());
			Message.AddHandler(new SingleThreadHandler());
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

	class SingleThreadHandler : Handler<int, int>
	{
		SingleThreadTaskScheduler scheduler = new SingleThreadTaskScheduler();

		public override Task<Envelope> Handle(int message, CancellationToken cancellationToken)
		{
			return Task<Envelope>.Factory.StartNew(() =>
			{
				return new Envelope<int>(message + 1);
			}, default(CancellationToken), TaskCreationOptions.None, scheduler);
		}
	}


}
