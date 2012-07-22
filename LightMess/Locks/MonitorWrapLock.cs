using System.Threading;

namespace ZeroBugBounce.LightMess
{
	public sealed class MonitorWrapLock : ILock
	{
		public MonitorWrapLock()
		{
			locktarget = new object();
		}

		private readonly object locktarget;

		public void Enter()
		{
			Monitor.Enter(locktarget);
		}

		public void Leave()
		{
			Monitor.Exit(locktarget);
		}

		public void Dispose()
		{

		}
	}
}
