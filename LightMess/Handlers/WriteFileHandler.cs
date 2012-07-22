using System;
using System.IO;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	/// <summary>
	/// Writes a file using IO completion ports
	/// </summary>
	public class WriteFileHandler : Handler<WriteFileRequest>
	{
		public override Task<Envelope> Handle(WriteFileRequest message, System.Threading.CancellationToken cancellation)
		{
			var outStream = new FileStream(message.Path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
			var taskCompletionSource = new TaskCompletionSource<Envelope>();

			outStream.BeginWrite(message.Contents, 0, message.Contents.Length, EndWrite,
				new WriteState(message.Path, outStream, message.Contents, taskCompletionSource));
			return taskCompletionSource.Task;
		}

		void EndWrite(IAsyncResult asyncResult)
		{
			WriteState writeState = asyncResult.AsyncState as WriteState;
			writeState.OutStream.EndWrite(asyncResult);
			writeState.OutStream.Dispose();
			writeState.TaskCompletionSource.TrySetResult(
				new Envelope<WriteFileResponse>(new WriteFileResponse(writeState.Path)));
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

	public class WriteFileRequest
	{
		public WriteFileRequest(string path, byte[] contents)
		{
			Path = path;
			Contents = contents;
		}

		public string Path { get; internal set; }
		public byte[] Contents { get; internal set; }
	}

	public class WriteFileResponse
	{
		public WriteFileResponse(string path)
		{
			Path = path;
		}

		public string Path { get; internal set; }
	}
}
