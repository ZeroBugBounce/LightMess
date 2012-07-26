using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ZeroBugBounce.LightMess;

namespace Testing
{
	class FileHandlerTests
	{
		[Fact]
		public void Read_a_file()
		{
			bool callbackWasCalled = false;
			byte[] output = null;

			Message.Init(new Messenger());
			Message.AddHandler(new FileReadHandler());

			var tempFile = Path.GetTempFileName();

			string guid = "{8A265D86-D763-4046-BACE-3531BA3DE517}";
			File.WriteAllText(tempFile, guid);

			Message.Post(new FileReadRequest(tempFile))
				.Callback<FileReadResponse>((t, r) =>
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
		public void Error_reading_a_file()
		{
			Message.Init(new Messenger());
			Message.AddHandler(new FileReadHandler());

			var tempFile = Path.GetTempFileName();
			File.WriteAllText(tempFile, Guid.NewGuid().ToString());

			var unsharingStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.None);

			var receipt = Message.Post(new FileReadRequest(tempFile));
			receipt.Callback<FileReadResponse>((t, r) =>
			{

			})
			.Wait();
		}

		[Fact]
		public void Cancel_reading_a_file()
		{
			bool callbackWasCalled = false;
			bool wasCancelled = false;

			byte[] output = null;

			Message.Init(new Messenger());
			Message.AddHandler(new FileReadHandler());

			var tempFile = Path.GetTempFileName();

			string guid = "{8A265D86-D763-4046-BACE-3531BA3DE517}";
			File.WriteAllText(tempFile, guid);

			var receipt = Message.Post(new FileReadRequest(tempFile)).Cancel();

			receipt.Callback<FileReadResponse>((t, r) =>
				{
					callbackWasCalled = true;
					wasCancelled = t.IsCanceled;

					if (wasCancelled)
					{
						Console.WriteLine("Read file cancelled");
					}
					else
					{
						Console.WriteLine("Read file {0} for {1} bytes]", r.Path, r.Contents.Length);
					}
				}).Wait();

			Thread.Sleep(10);

 			Assert.True(callbackWasCalled);
			Assert.True(wasCancelled);
			Assert.NotNull(output);
			Assert.Equal(Encoding.Default.GetByteCount(guid), output.Length);
			Assert.Equal(guid, Encoding.Default.GetString(output));
		}

		[Fact]
		public void Write_a_file()
		{
			bool callbackWasCalled = false;
			Message.Init(new Messenger());
			Message.AddHandler(new FileWriteHandler());

			string guid = "{70E2D385-26C3-4EE8-9A92-66FBA19DF9A8}";
			var tempFile = Path.Combine(Path.GetTempPath(), guid + ".txt");

			Message.Post(new FileWriteRequest(tempFile, Encoding.Default.GetBytes(guid)))
				.Callback(t =>
				{
					callbackWasCalled = true;
				})
				.Wait();

			Assert.True(callbackWasCalled);
			Assert.True(File.Exists(tempFile));
			Assert.Equal(guid, File.ReadAllText(tempFile));
		}
	}
}
