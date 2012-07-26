using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using ZeroBugBounce.LightMess;

namespace Testing
{
	public class ErrorHandlingTests
	{
		[Fact]
		public void Default_error_handling()
		{
			
			var messenger = new Messenger();
			messenger.Handle<int, int>((t, c) =>
			{
				throw new InvalidOperationException();
			});

			Assert.Throws<AggregateException>(() =>
			{
				messenger.Post(123).Wait();
			});
		}
	}
}
