using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using ZeroBugBounce.LightMess;

namespace ConsoleTestApp
{
	class Program
	{
		static void Main(string[] args)
		{
			var messenger = new Messenger();
			messenger.ScanAndLoadHandlers(typeof(Messenger).Assembly);

			//throw new InvalidOperationException();
			//ErrorHandling();
			//SqlReaderHandlerTest();
			//SqlNonQueryHandlerTest();
			//HttpRequestIOCompletionPortTests();
			MessagingSpeedTest();
			//FileStreamIOCompletionPortsTest();
		}

		static void ErrorHandling()
		{
			var messenger = new Messenger();
			messenger.Handle<int, int>((m, c) =>
			{
				throw new InvalidOperationException();
			});

			messenger.Post(123);
		}

		static void SqlReaderHandlerTest()
		{
			Message.Init(new Messenger());
			Message.AddHandler(new SqlReaderHandler());

			var connection = new SqlConnection(@"Data Source=howard-jr\SQLEXPRESS;
				Initial Catalog=LightMess;Trusted_Connection=SSPI;Asynchronous Processing=true");

			connection.Open();

			var receipt = Message.Post(new SqlReaderRequest(new SqlCommand("SELECT * FROM [GenderByAge]", connection)));

			receipt.Callback<SqlReaderResponse>((t, r) =>
			{
			    var reader = r.DataReader;
			    while (reader.Read())
			    {
			        Console.WriteLine(reader.GetString(reader.GetOrdinal("Name")));
			    }
			});

			receipt.Wait();
			connection.Close();
		}

		static void SqlNonQueryHandlerTest()
		{
			Message.Init(new Messenger());
			Message.AddHandler(new SqlNonQueryHandler());

			var connection = new SqlConnection(@"Data Source=howard-jr\SQLEXPRESS;
				Initial Catalog=LightMess;Trusted_Connection=SSPI;Asynchronous Processing=true");

			connection.Open();

			var receipt = Message.Post(new SqlNonQueryRequest(new SqlCommand(@"update GenderByAge 
set Name = Name
FROM GenderByAge", connection)));
			receipt.Callback<SqlNonQueryResponse>((t, r) =>
			{
				Console.WriteLine("{0} records affected", r.AffectedRecords);
			});

			receipt.Wait();
			connection.Close();

			Thread.Sleep(1000);			
		}

		static void HttpRequestIOCompletionPortTests()
		{
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

			int iterations = 10;
			Message.Init(new Messenger());
			Message.AddHandler(new HttpHandler());

			var timer = new Stopwatch();
			timer.Start();
			for (int i = 0; i < iterations; i++)
			{
				Message.Post(new HttpRequest("http://www.tradingtechnologies.com/"))
					.Callback<HttpResponse>((t, r) =>
					{
						Console.WriteLine("Received {0:###,###} bytes starting with {1}", r.Data.Length,
							Encoding.Default.GetString(r.Data, 0, 100));
					})
					.Wait();

			}

			timer.Stop();

			Console.WriteLine("{0} msg took {1} or {2} msg/s or {3}µs", iterations, timer.Elapsed,
				((double)iterations) / timer.Elapsed.TotalSeconds, 1000 * (timer.Elapsed.TotalMilliseconds / ((double)iterations)));
		}

		static void MessagingSpeedTest()
		{
			int iterations = 100000;
			var messenger = new Messenger();
			messenger.Handle<int, int>((i, c) => i + 1);

			var timer = new Stopwatch();
			
			timer.Start();
			for (int i = 0; i < iterations; i++)
			{
				var receipt = messenger.Post(i);
				receipt.Callback<int>((t, r) => r++);
			}
			timer.Stop();
			

			Console.WriteLine("{0} msg took {1} or {2} msg/s or {3}µs", iterations, timer.Elapsed,
				((double)iterations) / timer.Elapsed.TotalSeconds, 1000 * (timer.Elapsed.TotalMilliseconds / ((double)iterations)));
		}

		static void FileStreamIOCompletionPortsTest()
		{
			int iterations = 1000;
			var messenger = new Messenger();
			messenger.AddHandler(new FileWriteHandler());

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
				var receipt = messenger.Post(new FileWriteRequest(Path.GetTempFileName(), Encoding.Default.GetBytes(Guid.NewGuid().ToString())));
				Thread.Sleep(0);
			}
			timer.Stop();

			Console.WriteLine("{0} msg took {1} or {2} msg/s or {3}µs", iterations, timer.Elapsed,
				((double)iterations) / timer.Elapsed.TotalSeconds, 1000 * (timer.Elapsed.TotalMilliseconds / ((double)iterations)));
		}
	}
}