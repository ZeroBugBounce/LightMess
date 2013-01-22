using System;
using System.IO;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	/// <summary>
	/// Writes a file asynchronously
	/// </summary>
	public class FileWriteHandler : Handler<FileWriteRequest>
	{
		public override Task<Envelope> Handle(FileWriteRequest message, System.Threading.CancellationToken cancellation)
		{
			var taskCompletionSource = new TaskCompletionSource<Envelope>();

			try
			{
				var outStream = new FileStream(message.Path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

				outStream.BeginWrite(message.Contents, 0, message.Contents.Length, EndWrite,
					new WriteState(message.Path, outStream, message.Contents, taskCompletionSource));

				return taskCompletionSource.Task;
			}
			catch (Exception ex)
			{
				taskCompletionSource.SetException(ex);
			}

			return taskCompletionSource.Task;
		}

		void EndWrite(IAsyncResult asyncResult)
		{
			var writeState = asyncResult.AsyncState as WriteState;

			try
			{
				writeState.OutStream.EndWrite(asyncResult);
				writeState.OutStream.Dispose();
				writeState.TaskCompletionSource.TrySetResult(
					new Envelope<FileWriteResponse>(new FileWriteResponse(writeState.Path)));
			}
			catch (Exception ex)
			{
				writeState.TaskCompletionSource.SetException(ex);
			}
		}

		class WriteState
		{
			public WriteState(string path, FileStream outStream, byte[] buffer, 
				TaskCompletionSource<Envelope> taskCompletionSource)
			{
				Path = path;
				OutStream = outStream;
				Buffer = buffer;
				TaskCompletionSource = taskCompletionSource;
			}

			public string Path { get; private set; }
			public FileStream OutStream { get; private set; }
			public byte[] Buffer { get; private set; }
			public TaskCompletionSource<Envelope> TaskCompletionSource { get; private set; }
		}
	}

	public class FileWriteRequest
	{
		public FileWriteRequest(string path, byte[] contents)
		{
			Path = path;
			Contents = contents;
		}

		public string Path { get; internal set; }
		public byte[] Contents { get; internal set; }
	}

	public class FileWriteResponse
	{
		public FileWriteResponse(string path)
		{
			Path = path;
		}

		public string Path { get; internal set; }
	}
}
