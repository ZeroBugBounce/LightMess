﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public class ReadFileHandler : Handler<ReadFileRequest>
	{
		public override Task<Envelope> Handle(ReadFileRequest message, CancellationToken cancellation)
		{
			var inStream = new FileStream(message.Path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
			var outStream = new MemoryStream(4096);
			var buffer = new byte[4096];

			var taskCompletionSource = new TaskCompletionSource<Envelope>();
			cancellation.Register((tcs) => 
			{ 
				((TaskCompletionSource<Envelope>)tcs).TrySetCanceled();
				Console.WriteLine("((TaskCompletionSource<Envelope>)tcs).TrySetCanceled();");
			}, taskCompletionSource);

			inStream.BeginRead(buffer, 0, 4096, EndRead, new ReadState(message.Path, inStream, buffer, outStream, taskCompletionSource));

			return taskCompletionSource.Task;
		}

		void EndRead(IAsyncResult asyncResult)
		{
			ReadState readState = asyncResult.AsyncState as ReadState;
			var task = readState.TaskCompletionSource.Task;
			if (task.IsCanceled) { readState.InStream.Dispose(); return; }

			int bytesRead = readState.InStream.EndRead(asyncResult);
			if (task.IsCanceled) { readState.InStream.Dispose(); return; }

			if (bytesRead > 0)
			{
				if (task.IsCanceled) { readState.InStream.Dispose(); return; }
				readState.OutStream.Write(readState.Buffer, 0, bytesRead);

				if (task.IsCanceled) { readState.InStream.Dispose(); return; }
				readState.InStream.BeginRead(readState.Buffer, 0, 4096, EndRead, readState);
			}
			else
			{
				if (task.IsCanceled) { readState.InStream.Dispose(); return; }
				readState.TaskCompletionSource.SetResult(new Envelope<ReadFileResponse>(
					new ReadFileResponse(readState.Path, readState.OutStream.ToArray())));

				Console.WriteLine("readState.TaskCompletionSource.SetResult");
			}
		}

		class ReadState
		{
			public ReadState(string path, FileStream inStream, byte[] buffer, MemoryStream outStream, 
				TaskCompletionSource<Envelope> taskCompletionSource) 
			{
				Path = path;
				InStream = inStream;
				Buffer = buffer;
				OutStream = outStream;
				TaskCompletionSource = taskCompletionSource;
			}

			public string Path { get; set;}
			public FileStream InStream { get; set; }
			public byte[] Buffer { get; set;}
			public MemoryStream OutStream { get; set;}
			public TaskCompletionSource<Envelope> TaskCompletionSource { get; set;}
		}
	}

	public class ReadFileRequest
	{
		public ReadFileRequest(string path)
		{
			Path = path;
		}

		public string Path {get; private set;}
	}

	public class ReadFileResponse
	{
		public ReadFileResponse(string path, byte[] contents)
		{
			Path = path;
			Contents = contents;
		}

		public string Path { get; private set;}
		public byte[] Contents { get; private set;}
	}
}
