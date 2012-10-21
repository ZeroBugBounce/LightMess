using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public abstract class SqlNonQueryComposableBase<TMessage> : Handler<TMessage> where TMessage : IConnectionMessage
	{
		public abstract SqlCommand PrepareCommand(TMessage message, CancellationToken cancellationToken);

		public sealed override Task<Envelope> Handle(TMessage message, CancellationToken cancellationToken)
		{
			var taskComletionSource = new TaskCompletionSource<Envelope>();

			Task.Factory.StartNew(() =>
			{
				try
				{
					Message.Post(new SqlNonQueryRequest(PrepareCommand(message, cancellationToken), message.ConnectionBuilder))
						.Callback<SqlNonQueryResponse>((t, r) =>
						{
							try
							{
								taskComletionSource.SetResult(new Envelope<int>(r.AffectedRecords));
							}
							finally
							{
								r.Dispose();
							}
						});
				}
				catch (Exception ex)
				{
					taskComletionSource.SetException(ex);
				}
			});

			return taskComletionSource.Task;
		}
	}
}
