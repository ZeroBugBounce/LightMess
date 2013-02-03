using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public abstract class SqlReaderComposableBase<TMessage, TOut> : Handler<TMessage> where TMessage : ISqlConnectionMessage
	{
		public abstract SqlCommand PrepareCommand(TMessage message, CancellationToken cancellationToken);

		public abstract TOut ProcessReader(TMessage message, SqlDataReader reader, CancellationToken cancellationToken);

		public sealed override Task<Envelope> Handle(TMessage message, CancellationToken cancellationToken)
		{
			var taskCompletionSource = new TaskCompletionSource<Envelope>();

			Task.Factory.StartNew(() =>
			{
				try
				{
					var command = PrepareCommand(message, cancellationToken);
					Message.Post(new SqlReaderRequest(command, message.ConnectionBuilder))
						   .Callback<SqlReaderResponse>((t, r) =>
							{
								try
								{
									taskCompletionSource.SetResult(new Envelope<TOut>(
										ProcessReader(message, r.DataReader, cancellationToken)));
								}
								catch (Exception ex)
								{
									taskCompletionSource.SetException(ex);
								}
								finally
								{
									r.Dispose(); 
								}
							});
				}
				catch (Exception ex)
				{
					taskCompletionSource.SetException(ex);
				}
			});

			return taskCompletionSource.Task;
		}
	}

	public interface ISqlConnectionMessage
	{
		SqlConnectionStringBuilder ConnectionBuilder { get; }
	}
}
