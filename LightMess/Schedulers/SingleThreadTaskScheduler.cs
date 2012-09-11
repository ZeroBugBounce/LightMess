using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public class SingleThreadTaskScheduler : TaskScheduler
	{
		Thread thread;
		long run = 1;
		readonly object synclock = new object();
		Queue<Task> taskQueue = new Queue<Task>();

		public SingleThreadTaskScheduler()
		{
			thread = new Thread(RunScheduler);
			thread.IsBackground = true;
			thread.Start();
		}

		void RunScheduler()
		{
			while (true)
			{
				Monitor.Enter(synclock);
				Monitor.Wait(synclock);
				var localQueue = new List<Task>();
				while (taskQueue.Count > 0)
				{
					localQueue.Add(taskQueue.Dequeue());
				}
				Monitor.Exit(synclock);

				localQueue.ForEach(t => TryExecuteTask(t));

				if (Interlocked.Read(ref run) == 0L)
				{
					break;
				}
			}
		}

		protected override IEnumerable<Task> GetScheduledTasks()
		{
			throw new NotImplementedException();
		}

		protected override void QueueTask(Task task)
		{
			Monitor.Enter(synclock);
			taskQueue.Enqueue(task);
			Monitor.Pulse(synclock);
			Monitor.Exit(synclock);
		}

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			task.RunSynchronously();
			return true;
		}
	}
}
