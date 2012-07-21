using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public class Envelope
	{

	}

	public class Envelope<T> : Envelope
	{
		public Envelope(T contents)
		{
			Contents = contents;
		}

		public T Contents { get; internal set; }
	}
}
