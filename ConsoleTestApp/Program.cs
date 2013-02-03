using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroBugBounce.LightMess;

namespace ConsoleTestApp
{
	class Program
	{
		static void Main(string[] args)
		{
			//StreamReadLargeFile();

			//Scan();

			//STSchedulerMultipleHandler();
			//SqlNonQueryHandlerTest();
			//SqlNonQueryComposableBaseTest();
			// SqlReaderComposableBaseTest();
			//var messenger = new Messenger();
			//messenger.ScanAndLoadHandlers(typeof(Messenger).Assembly);
			//SqlReaderHandlerErrorTest();
			//throw new InvalidOperationException();
			//ErrorHandling();
			//SqlReaderHandlerTest();
			//SqlNonQueryHandlerTest();
			//HttpRequestIOCompletionPortTests();
			MessagingSpeedTest();
			//FileStreamIOCompletionPortsTest();
		}

		static void TaskAll()
		{
			
		}

		static void StreamReadLargeFile()
		{
			var largeFilePath = @"X:\Downloads\6.0.6001.18000.367-KRMSDK_EN.iso"; // > 1 GB
			var messenger = new Messenger();
			messenger.AddHandler(new StreamingFileReadHandler());

			var receipt = messenger.Post(new StreamingFileReadRequest(largeFilePath, 10 * 1024 * 1024, (l, b) =>
			{
				if (l % 1000000 == 0)
				{
					Console.WriteLine("Buffer starts at {0:0,000}", l);
					Console.WriteLine("Current working set: {0:0,000}", Process.GetCurrentProcess().WorkingSet64);
				}
			}));

			receipt.Callback<StreamingFileReadResponse>((t, r) =>
			{
				Console.WriteLine("Total bytes read: {0:0,000}", r.BytesRead);
				Console.WriteLine("Current working set: {0:0,000}", Process.GetCurrentProcess().WorkingSet64);
			});
		}

		static void ReadLargeFile()
		{
			var largeFilePath = @"X:\Downloads\FT54.wmv"; // > 1 GB
			var messenger = new Messenger();
			messenger.AddHandler(new FileReadHandler());

			var receipt = messenger.Post(new FileReadRequest(largeFilePath));

			receipt.Callback<FileReadResponse>((t, r) =>
			{
				Console.WriteLine("File size: {0}", r.Contents.Length);
				Console.WriteLine("Current working set: {0}", Process.GetCurrentProcess().WorkingSet64);
			});
		}

		static void Scan()
		{
			var messenger = new Messenger();
			messenger.ScanAndLoadHandlers(typeof(Program).Assembly);

		}

		static void STSchedulerMultipleHandler()
		{
			Message.Init(new Messenger());
			
		}
		
		static void STScheduler()
		{
			int count = 2500;
			Message.Init(new Messenger());
			Message.AddHandler(new SingleThreadHandler());
			ArrayList list = ArrayList.Synchronized(new ArrayList());
			Random rng = new Random();
						
			while (count > 0)
			{
				ThreadPool.QueueUserWorkItem(state =>
				{
					list.Add(state);
					Message.Post((int)state).Callback<int>((t, r) =>
					{
						Thread.Sleep(0);
						list.Remove(r-1);
						//Console.WriteLine("Answer is {0}", r);
					});
				}, count--);
			}

			Console.WriteLine("Finished adding tasks");

			Thread.Sleep(10);

			char exitChar = '1';

			while (exitChar != 'x')
			{
				Console.WriteLine("{0} items remain: {1}", list.Count, string.Join(",", list.OfType<int>().Select(i => i.ToString()).ToArray()));
				exitChar = Console.ReadKey(true).KeyChar;
			}
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

		static void SqlNonQueryComposableBaseTest()
		{
			Message.Init(new Messenger());
			Message.AddHandler(new SqlNonQueryHandler());
			Message.AddHandler(new CountQueryHandler());

			Message.Post(new NonQueryRequest
			{
				ConnectionBuilder = new SqlConnectionStringBuilder(@"Data Source=howard-jr\SQLEXPRESS;
				Initial Catalog=LightMess;Trusted_Connection=SSPI;Asynchronous Processing=true")
			})
			.Callback<int>((t, r) =>
			{
				Console.WriteLine("{0} records affected", r);
			});
		}

		static void SqlReaderComposableBaseTest()
		{
			Message.Init(new Messenger());
			Message.AddHandler(new ProcessQueryHandler());
			Message.AddHandler(new SqlReaderHandler());

			Message.Post(new QueryRequest
			{
				ConnectionBuilder = new SqlConnectionStringBuilder(@"Data Source=howard-jr\SQLEXPRESS;
				Initial Catalog=LightMess;Trusted_Connection=SSPI;Asynchronous Processing=true")
			})
			.Callback<QueryResponse>((t, r) =>
			{
				r.Names.ForEach(Console.WriteLine);
			});
		}

		static void SqlReaderHandlerErrorTest()
		{
			Message.Init(new Messenger());
			Message.AddHandler(new SqlReaderHandler());

			var connectionBuilder = new SqlConnectionStringBuilder(@"Data Source=how123432ard-jr\SQLEXPRESS;
				Initial Catalog=LightMess;Trusted_Connection=SSPI;Asynchronous Processing=true");

			connectionBuilder.ConnectTimeout = 2;

			var receipt = Message.Post(
				new SqlReaderRequest(new SqlCommand("SELECT * FROM [GenderByAge]"), connectionBuilder));

			receipt.Callback<SqlReaderResponse>((t, r) =>
			{
				try
				{
					var reader = r.DataReader;
					while (reader.Read())
					{
						Console.WriteLine(reader.GetString(reader.GetOrdinal("Name")));
					}
				}
				finally
				{
					r.Dispose();
				}
			});

			receipt.Wait();
		}

		static void SqlReaderHandlerTest()
		{
			Message.Init(new Messenger());
			Message.AddHandler(new SqlReaderHandler());

			var connectionBuilder = new SqlConnectionStringBuilder(@"Data Source=howard-jr\SQLEXPRESS;
				Initial Catalog=LightMess;Trusted_Connection=SSPI;Asynchronous Processing=true");

			var receipt = Message.Post(
				new SqlReaderRequest(new SqlCommand("SELECT * FROM [GenderByAge]"), connectionBuilder));

			receipt.Callback<SqlReaderResponse>((t, r) =>
			{
				try
				{
					var reader = r.DataReader;
					while (reader.Read())
					{
						Console.WriteLine(reader.GetString(reader.GetOrdinal("Name")));
					}
				}
				finally
				{
					r.Dispose();
				}
			});

			receipt.Wait();
		}

		static void SqlNonQueryHandlerTest()
		{
			Message.Init(new Messenger());
			Message.AddHandler(new SqlNonQueryHandler());

			var connectionBuilder = new SqlConnectionStringBuilder(@"Data Source=howard\SQLEXPRESS;
				Initial Catalog=LightMess;Trusted_Connection=SSPI;Asynchronous Processing=true");

			var timer = new Stopwatch();
			timer.Start();

			var receipt = Message.Post(new SqlNonQueryRequest(new SqlCommand(@"update GenderByAge 
set Name = Name
FROM GenderByAge"), connectionBuilder));
			receipt.Callback<SqlNonQueryResponse>((t, r) =>
			{
				Console.WriteLine("{0} records affected", r.AffectedRecords);
			});

			receipt.Wait();
			timer.Stop();

			Console.WriteLine("SqlNonQueryHandlerTest took {0:0.00}ms", timer.ElapsedMilliseconds);

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
			int iterations = 200000;
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

	public class SingleThreadHandler : Handler<int, int>
	{
		SingleThreadedTaskScheduler scheduler = new SingleThreadedTaskScheduler();

		public override Task<Envelope> Handle(int message, CancellationToken cancellationToken)
		{
			return Task<Envelope>.Factory.StartNew(() =>
			{
				return new Envelope<int>(message + 1);
			}, default(CancellationToken), TaskCreationOptions.None, scheduler);
		}
	}

	public class CountQueryHandler : SqlNonQueryComposableBase<NonQueryRequest>
	{
		public override SqlCommand PrepareCommand(NonQueryRequest message, CancellationToken cancellationToken)
		{
			return new SqlCommand(@"update GenderByAge 
set Name = Name
FROM GenderByAge");
		}
	}

	public class NonQueryRequest : ISqlConnectionMessage
	{
		public SqlConnectionStringBuilder ConnectionBuilder { get; internal set; }
	}

	public class QueryRequest : ISqlConnectionMessage
	{
		public SqlConnectionStringBuilder ConnectionBuilder { get; internal set; }
	}

	public class QueryResponse
	{
		public List<string> Names { get; internal set; }
	}

	public class ProcessQueryHandler : SqlReaderComposableBase<QueryRequest, QueryResponse>
	{
		public override SqlCommand PrepareCommand(QueryRequest message, CancellationToken cancellationToken)
		{
			return new SqlCommand("SET COUNT ON; SELECT * FROM [GenderByAge]; UPDATE [GenderByAge] WHERE 1 = 0");
		}

		public override QueryResponse ProcessReader(QueryRequest message, SqlDataReader reader, CancellationToken cancellationToken)
		{
			var names = new List<string>();
			while (reader.Read())
			{
				names.Add(reader.GetString(0));
			}

			return new QueryResponse { Names = names };
		}
	}
}