using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public abstract class SqlReaderComposableBase<TMessage, TOut> : Handler<TMessage> where TMessage : ISqlConnectionMessage
	{
		public abstract SqlCommand PrepareCommand(TMessage message, Receipt receipt);

		public abstract TOut ProcessReader(TMessage message, SqlDataReader reader, Receipt receipt);

		public sealed override void Handle(TMessage message, Receipt receipt)
		{
			var taskCompletionSource = new TaskCompletionSource<Envelope>();

			var command = PrepareCommand(message, receipt);
			Message.Post(new SqlReaderRequest(command, message.ConnectionBuilder))
					.Callback<SqlReaderResponse>(r =>
					{
						try
						{
							receipt.FireCallback(ProcessReader(message, r.DataReader, receipt));
						}
						finally
						{
							r.Dispose(); 
						}
					});
		}
	}

	public interface ISqlConnectionMessage
	{
		SqlConnectionStringBuilder ConnectionBuilder { get; }
	}
}
