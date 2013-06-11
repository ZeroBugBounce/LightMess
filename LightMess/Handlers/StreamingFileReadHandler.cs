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
		public override void Handle(StreamingFileReadRequest message, Receipt receipt)
		{
			var path = message.Path;
			var bufferSize = message.BufferSize;
			var taskCompletionSource = new TaskCompletionSource<Envelope>();
			var inStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
			var buffer = new byte[bufferSize];

			inStream.BeginRead(buffer, 0, bufferSize, EndRead, new ReadState(path, inStream, buffer, receipt, message.ReadCallback));
		}

		void EndRead(IAsyncResult asyncResult)
		{
			var readState = asyncResult.AsyncState as ReadState;
				int bytesRead = readState.InStream.EndRead(asyncResult);

				if (bytesRead > 0)
				{
					// async report back the read data:
					ThreadPool.QueueUserWorkItem(_ =>
					{
						readState.ReadCallback(readState.InStream.Position, readState.Buffer);
					});

					readState.InStream.BeginRead(readState.Buffer, 0, 4096, EndRead, readState);
				}
				else
				{
					readState.Receipt.FireCallback(new StreamingFileReadResponse(readState.Path, readState.InStream.Length));
				}
		}

		class ReadState
		{
			public ReadState(string path, FileStream inStream, byte[] buffer, Receipt receipt, Action<Int64, Byte[]> readCallback) 
			{
				Path = path;
				InStream = inStream;
				Buffer = buffer;
				Receipt = receipt;
				ReadCallback = readCallback;
			}

			public string Path { get; set;}
			public FileStream InStream { get; set; }
			public byte[] Buffer { get; set;}
			public Receipt Receipt { get; set; }
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
