using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroBugBounce.LightMess
{
	public struct Receipt
	{
		public Receipt(object obj)
		{

		}

		public bool Cancel()
		{
			return false;
		}

		public bool Wait(TimeSpan timeToWait)
		{
			return false;
		}
	}
}
