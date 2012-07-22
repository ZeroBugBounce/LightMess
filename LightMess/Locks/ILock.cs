using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroBugBounce.LightMess
{
	public interface ILock
	{
		void Enter();
		void Leave();
	}
}
