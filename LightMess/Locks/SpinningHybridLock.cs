using System;
using System.Threading;

namespace ZeroBugBounce.LightMess
{
	public sealed class SpinningHybridLock : ILock, IDisposable
	{
		public SpinningHybridLock(int spinCount = 4000)
		{
			_spincount = spinCount;
			_waiters = 0;
			_waiterLock = new AutoResetEvent(false);
			_spincount = 4000;
			_owningThreadId = 0;
			m_recursion = 0;
		}

		// The Int32 is used by the primitive user-mode constructs (Interlocked methods) 
		private Int32 _waiters;

		// The AutoResetEvent is the primitive kernel-mode construct 
		private AutoResetEvent _waiterLock;

		// This field controls spinning in an effort to improve performance 
		private Int32 _spincount;   // Arbitrarily chosen count 

		// These fields indicate which thread owns the lock and how many times it owns it 
		private Int32 _owningThreadId, m_recursion;

		public void Enter()
		{
			// If calling thread already owns the lock, increment recursion count and return 
			Int32 threadId = Thread.CurrentThread.ManagedThreadId;
			if (threadId == _owningThreadId) { m_recursion++; return; }

			// The calling thread doesn't own the lock, try to get it 
			SpinWait spinwait = new SpinWait();
			for (Int32 spinCount = 0; spinCount < _spincount; spinCount++)
			{
				// If the lock was free, this thread got it; set some state and return 
				if (Interlocked.CompareExchange(ref _waiters, 1, 0) == 0) goto GotLock;

				// Black magic: give other threads a chance to run  
				// in hopes that the lock will be released 
				spinwait.SpinOnce();
			}

			// Spinning is over and the lock was still not obtained, try one more time 
			if (Interlocked.Increment(ref _waiters) > 1)
			{
				// Other threads are blocked and this thread must block too 
				_waiterLock.WaitOne(); // Wait for the lock; performance hit 
				// When this thread wakes, it owns the lock; set some state and return 
			}

		GotLock:
			// When a thread gets the lock, we record its ID and  
			// indicate that the thread owns the lock once 
			_owningThreadId = threadId; m_recursion = 1;
		}

		public void Leave()
		{
			// If the calling thread doesn't own the lock, there is a bug 
			Int32 threadId = Thread.CurrentThread.ManagedThreadId;
			if (threadId != _owningThreadId)
				throw new SynchronizationLockException("Lock not owned by calling thread");

			// Decrement the recursion count. If this thread still owns the lock, just return 
			if (--m_recursion > 0) return;

			_owningThreadId = 0;   // No thread owns the lock now 

			// If no other threads are blocked, just return 
			if (Interlocked.Decrement(ref _waiters) == 0)
				return;

			// Other threads are blocked, wake 1 of them 
			_waiterLock.Set();     // Bad performance hit here 
		}

		public void Dispose() { _waiterLock.Dispose(); }
	}
}
