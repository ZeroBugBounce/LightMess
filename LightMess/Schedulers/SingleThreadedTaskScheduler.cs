using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public class SingleThreadedTaskScheduler : TaskScheduler
	{
		Thread thread;
		readonly object synclock = new object();
		Queue<Task> taskQueue = new Queue<Task>();

		public SingleThreadedTaskScheduler()
		{
			thread = new Thread(RunScheduler);
			thread.IsBackground = true;
			thread.Start();
		}

		void RunScheduler()
		{
			Monitor.Enter(synclock);
			while (true)
			{
				Monitor.Wait(synclock);
			dequeueAndProcess:
				var task = taskQueue.Dequeue();

				Monitor.Exit(synclock);

				if (!TryExecuteTask(task))
				{
					throw new InvalidOperationException("Something prevented the task from executing");
				}

				Monitor.Enter(synclock);

				if (taskQueue.Count > 0)
				{
					goto dequeueAndProcess;
				}
			}
			// Monitor.Exit(synclock);
		}

		protected override IEnumerable<Task> GetScheduledTasks()
		{
			Monitor.Enter(synclock);
			var tasks = taskQueue.ToArray();
			Monitor.Pulse(synclock);
			Monitor.Exit(synclock);

			return tasks;
		}

		public override int MaximumConcurrencyLevel
		{
			get
			{
				return 1;
			}
		}

		protected override bool TryDequeue(Task task)
		{
			return false;
		}

		protected override void QueueTask(Task task)
		{
			Monitor.Enter(synclock);
			Debug.WriteLine("QueueTask {0}", task.GetHashCode());
			taskQueue.Enqueue(task);
			Monitor.Pulse(synclock);
			Monitor.Exit(synclock);
		}

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			throw new NotImplementedException();
		}
	}
}
