using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using ZeroBugBounce.LightMess;
using System.IO;

namespace ConsoleTestApp
{
	class Program
	{
		static void Main(string[] args)
		{
			int iterations = 10000;
			var messenger = new Messenger();
			messenger.AddHandler(new WriteFileHandler());

			bool measureThreads = true;

			int initWorkerThreads;
			int initCompletionPortThreads;
			ThreadPool.GetAvailableThreads(out initWorkerThreads, out initCompletionPortThreads);
			Console.WriteLine("{0} completionPortThreads", initCompletionPortThreads);

			var measure = new Thread(context =>
			{
				while (measureThreads)
				{
					int workerThreads;
					int completionPortThreads;

					ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);

					if (initCompletionPortThreads != completionPortThreads)
					{
						Console.WriteLine("{0} completionPortThreads", completionPortThreads);
					}

					initWorkerThreads = workerThreads;
					initCompletionPortThreads = completionPortThreads;

					Thread.Sleep(1);
				}
			});
			measure.IsBackground = true;
			measure.Start();

			var timer = new Stopwatch();
			
			timer.Start();
			for (int i = 0; i < iterations; i++)
			{
				var receipt = messenger.Post(new WriteFileRequest(Path.GetTempFileName(), Encoding.Default.GetBytes(Guid.NewGuid().ToString())));
			}
			timer.Stop();
			
			Console.WriteLine("{0} msg took {1} or {2} msg/s or {3}µs", iterations, timer.Elapsed,
				((double)iterations) / timer.Elapsed.TotalSeconds, 1000 * (timer.Elapsed.TotalMilliseconds / ((double)iterations)));
		}
	}
}
