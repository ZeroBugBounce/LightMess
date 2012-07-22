using System;
using System.Threading;

namespace ZeroBugBounce.LightMess
{
	public sealed class NoSpinHybridLock : ILock, IDisposable
	{
		public NoSpinHybridLock()
		{
			m_waiters = 0;
			m_waiterLock = new AutoResetEvent(false);
		}

		// The Int32 is used by the primitive user-mode constructs (Interlocked methods) 
		private Int32 m_waiters;

		// The AutoResetEvent is the primitive kernel-mode construct 
		private AutoResetEvent m_waiterLock;

		public void Enter()
		{
			// Indicate that this thread wants the lock 
			if (Interlocked.Increment(ref m_waiters) == 1)
				return; // Lock was free, no contention, just return 

			// Another thread is waiting. There is contention, block this thread 
			m_waiterLock.WaitOne();  // Bad performance hit here 
			// When WaitOne returns, this thread now has the lock 
		}

		public void Leave()
		{
			// This thread is releasing the lock 
			if (Interlocked.Decrement(ref m_waiters) == 0)
				return; // No other threads are blocked, just return 

			// Other threads are blocked, wake 1 of them 
			m_waiterLock.Set();  // Bad performance hit here 
		}

		public void Dispose() { m_waiterLock.Dispose(); }
	}
}
