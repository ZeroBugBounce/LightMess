using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ZeroBugBounce.LightMess;
using System.IO;

namespace Testing
{
	public class Tests
	{
		[Fact]
		public void Post_a_message()
		{
			bool handlerWasCalled = false;

			Message.Init(new Messenger());
			Message.Handle<NameRequest>((m, c) =>
			{
				handlerWasCalled = true;
			});

			Message.Post(new NameRequest()).Wait(TimeSpan.MaxValue);
			Assert.True(handlerWasCalled);			
		}

		[Fact]
		public void Get_a_callback()
		{
			bool handlerWasCalled = false;
			bool callbackWasCalled = false;

			Message.Init(new Messenger());
			Message.Handle<NameRequest>((m, c) =>
			{
				handlerWasCalled = true;
			});

			Message.Post(new NameRequest())
				   .Callback(t => {
					   System.Diagnostics.Debug.WriteLine("On thread {0}", Thread.CurrentThread.Name);
					   Thread.Sleep(100);  callbackWasCalled = true; })
				   .Wait(TimeSpan.MaxValue);

			Assert.True(handlerWasCalled);
			Assert.True(callbackWasCalled);
		}

		[Fact]
		public void Get_a_reply()
		{
			bool handlerWasCalled = false;
			bool callbackWasCalled = false;
			Account reply = null;

			Message.Init(new Messenger());
			Message.Handle<NameRequest, Account>((m, c) =>
			{
				handlerWasCalled = true;
				return new Account();
			});

			Message.Post(new NameRequest())
				.Callback<Account>((t, a) => {
					System.Diagnostics.Debug.WriteLine("On thread {0}", Thread.CurrentThread.Name);
					callbackWasCalled = true;
					reply = a;})
				.Wait();

			Assert.True(handlerWasCalled);
			Assert.True(callbackWasCalled);
			Assert.NotNull(reply);
		}

		[Fact]
		public void Read_a_file()
		{
			bool callbackWasCalled = false;
			byte[] output = null;

			Message.Init(new Messenger());
			Message.AddHandler(new ReadFileHandler());

			var tempFile = Path.GetTempFileName();

			string guid = "{8A265D86-D763-4046-BACE-3531BA3DE517}";
			File.WriteAllText(tempFile, guid);

			Message.Post(new ReadFileRequest(tempFile))
				.Callback<ReadFileResponse>((t, r) =>
				{
					callbackWasCalled = true;
					output = r.Contents;
					Console.WriteLine("Read file {0} for {1} bytes]", r.Path, r.Contents.Length);
				})
				.Wait();

			Assert.True(callbackWasCalled);
			Assert.NotNull(output);
			Assert.Equal(Encoding.Default.GetByteCount(guid), output.Length);
			Assert.Equal(guid, Encoding.Default.GetString(output));
		}

		[Fact]
		public void Write_a_file()
		{
			bool callbackWasCalled = false;
			Message.Init(new Messenger());
			Message.AddHandler(new WriteFileHandler());

			string guid = "{70E2D385-26C3-4EE8-9A92-66FBA19DF9A8}";
			var tempFile = Path.Combine(Path.GetTempPath(), guid + ".txt");

			Message.Post(new WriteFileRequest(tempFile, Encoding.Default.GetBytes(guid)))
				.Callback(t =>
				{
					callbackWasCalled = true;
				})
				.Wait();

			Assert.True(callbackWasCalled);
			Assert.True(File.Exists(tempFile));
			Assert.Equal(guid, File.ReadAllText(tempFile));
		}

		[Fact]
		public void Get_http()
		{

		}

		class Account { }
	}

	public class NameRequest
	{

	}
}
