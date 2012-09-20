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
			int answer1 = 0;
			char answer2 = char.MinValue;

			var messenger = new Messenger();

			messenger.Handle<int, int>((msg, xxl) =>
			{
				return msg + 1;
			});

			messenger.Handle<char, char>((msg, xxl) =>
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
