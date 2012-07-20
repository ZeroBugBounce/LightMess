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
}
