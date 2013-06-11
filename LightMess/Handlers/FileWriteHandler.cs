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
		public override void Handle(FileWriteRequest message, Receipt receipt)
		{
				var outStream = new FileStream(message.Path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

				outStream.BeginWrite(message.Contents, 0, message.Contents.Length, EndWrite,
					new WriteState(message.Path, outStream, message.Contents, receipt));
		}

		void EndWrite(IAsyncResult asyncResult)
		{
			var writeState = asyncResult.AsyncState as WriteState;

				writeState.OutStream.EndWrite(asyncResult);
				writeState.OutStream.Dispose();
				writeState.Receipt.FireCallback(new FileWriteResponse(writeState.Path));
		}

		class WriteState
		{
			public WriteState(string path, FileStream outStream, byte[] buffer, Receipt receipt)
			{
				Path = path;
				OutStream = outStream;
				Buffer = buffer;
				Receipt  = receipt;
			}

			public string Path { get; private set; }
			public FileStream OutStream { get; private set; }
			public byte[] Buffer { get; private set; }
			public Receipt Receipt { get; private set; }
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
