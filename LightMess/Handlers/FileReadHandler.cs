using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	/// <summary>
	/// Reads a file asynchronously into memory and returns the entire file.
	/// </summary>
	public class FileReadHandler : Handler<FileReadRequest>
	{
		public override void Handle(FileReadRequest message, Receipt receipt)
		{
			var inStream = new FileStream(message.Path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
			var outStream = new MemoryStream(4096);
			var buffer = new byte[4096];

			inStream.BeginRead(buffer, 0, 4096, EndRead, new ReadState(message.Path, inStream, buffer, outStream, receipt));
		}

		void EndRead(IAsyncResult asyncResult)
		{
			var readState = asyncResult.AsyncState as ReadState;
			int bytesRead = readState.InStream.EndRead(asyncResult);
			if (bytesRead > 0)
			{
				readState.OutStream.Write(readState.Buffer, 0, bytesRead);
				readState.InStream.BeginRead(readState.Buffer, 0, 4096, EndRead, readState);
			}
			else
			{
				readState.Receipt.FireCallback(new FileReadResponse(readState.Path, readState.OutStream.ToArray()));
			}
		}

		class ReadState
		{
			public ReadState(string path, FileStream inStream, byte[] buffer, MemoryStream outStream, Receipt receipt)
			{
				Path = path;
				InStream = inStream;
				Buffer = buffer;
				OutStream = outStream;
				Receipt = Receipt;
			}

			public string Path { get; set; }
			public FileStream InStream { get; set; }
			public byte[] Buffer { get; set; }
			public MemoryStream OutStream { get; set; }
			public Receipt Receipt { get; set; }
		}
	}

	public class FileReadRequest
	{
		public FileReadRequest(string path)
		{
			Path = path;
		}

		public string Path { get; private set; }
	}

	public class FileReadResponse
	{
		public FileReadResponse(string path, byte[] contents)
		{
			Path = path;
			Contents = contents;
		}

		public string Path { get; private set; }
		public byte[] Contents { get; private set; }
	}
}
