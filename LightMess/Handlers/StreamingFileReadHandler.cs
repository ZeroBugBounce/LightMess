using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace ZeroBugBounce.LightMess
{
	/// <summary>
	/// Streams a file asynchronously for processing a chunk at a time,
	/// rather than loading the whole file into memory.
	/// </summary>
	public class StreamingFileReadHandler : Handler<StreamingFileReadRequest>
	{
		public override Task<Envelope> Handle(StreamingFileReadRequest message, CancellationToken cancellation)
		{
			var path = message.Path;
			var bufferSize = message.BufferSize;
			var taskCompletionSource = new TaskCompletionSource<Envelope>();
			var inStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
			var buffer = new byte[bufferSize];

			cancellation.Register((tcs) =>
			{
				((TaskCompletionSource<Envelope>)tcs).TrySetCanceled();
			}, taskCompletionSource);

			inStream.BeginRead(buffer, 0, bufferSize, EndRead, new ReadState(path, inStream, buffer, taskCompletionSource, message.ReadCallback));

			return taskCompletionSource.Task;
		}

		void EndRead(IAsyncResult asyncResult)
		{
			var readState = asyncResult.AsyncState as ReadState;

			try 
			{
				var task = readState.TaskCompletionSource.Task;

				if (task.IsCanceled) { readState.InStream.Dispose(); return; }

				int bytesRead = readState.InStream.EndRead(asyncResult);
				if (task.IsCanceled) { readState.InStream.Dispose(); return; }

				if (bytesRead > 0)
				{
					if (task.IsCanceled) { readState.InStream.Dispose(); return; }

					// async report back the read data:
					Task.Factory.StartNew(() =>
					{
						readState.ReadCallback(readState.InStream.Position, readState.Buffer);
					});

					if (task.IsCanceled) { readState.InStream.Dispose(); return; }
					readState.InStream.BeginRead(readState.Buffer, 0, 4096, EndRead, readState);
				}
				else
				{
					if (task.IsCanceled) { readState.InStream.Dispose(); return; }

					readState.TaskCompletionSource.SetResult(new Envelope<StreamingFileReadResponse>(new StreamingFileReadResponse(readState.Path, readState.InStream.Length)));
				}
			}
			catch(Exception ex)
			{
				readState.TaskCompletionSource.SetException(ex);
			}
		}

		class ReadState
		{
			public ReadState(string path, FileStream inStream, byte[] buffer, TaskCompletionSource<Envelope> taskCompletionSource, Action<Int64, Byte[]> readCallback) 
			{
				Path = path;
				InStream = inStream;
				Buffer = buffer;
				TaskCompletionSource = taskCompletionSource;
				ReadCallback = readCallback;
			}

			public string Path { get; set;}
			public FileStream InStream { get; set; }
			public byte[] Buffer { get; set;}
			public TaskCompletionSource<Envelope> TaskCompletionSource { get; set;}
			public Action<Int64, Byte[]> ReadCallback { get; set; }
		}
	}

	public class StreamingFileReadRequest
	{
		public StreamingFileReadRequest(string path, int bufferSize, Action<Int64, Byte[]> readCallback)
		{
			Path = path;
			BufferSize = bufferSize;
			ReadCallback = readCallback;
		}

		public string Path {get; private set;}
		public int BufferSize { get; private set; }
		public Action<Int64, Byte[]> ReadCallback { get; private set; }
	}

	public class StreamingFileReadResponse
	{
		public StreamingFileReadResponse(string path, Int64 bytesRead)
		{
			Path = path;
			BytesRead = bytesRead;
		}

		public string Path { get; private set; }
		public Int64 BytesRead { get; private set; }
	}
}
