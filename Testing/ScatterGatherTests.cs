using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using ZeroBugBounce.LightMess;
using System.Threading.Tasks;

namespace Testing
{
	public class ScatterGatherTests
	{
		[Fact]
		public void Simple_scatter_gather()
		{
			var messenger = new Messenger();

			messenger.Handle<int, int>(msg =>
			{
				return msg + 1;
			});

			messenger.Handle<char, char>((msg) =>
			{
				return (char)(((int)msg) + 1);
			});

			// scatter:
			var receipt1 = messenger.Post(3);
			var receipt2 = messenger.Post('A');

			// gather

		//	var receiptBoth = Receipt.Combine(receipt1, receipt2);
		}
	}
}
