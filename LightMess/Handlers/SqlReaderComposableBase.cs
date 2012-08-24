using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public abstract class SqlReaderComposableBase<TMessage, TOut> : Handler<TMessage> where TMessage : IConnectionMessage
	{
		public abstract SqlCommand PrepareCommand(TMessage message);

		public abstract TOut ProcessReader(SqlDataReader reader);

		public override Task<Envelope> Handle(TMessage message, CancellationToken cancellation)
		{
			var taskCompletionSource = new TaskCompletionSource<Envelope>();

			Task.Factory.StartNew(() =>
			{
				try
				{
					var command = PrepareCommand(message);
					Message.Post(new SqlReaderRequest(command, message.ConnectionBuilder))
						   .Callback<SqlReaderResponse>((t, r) =>
							{
								try
								{
									taskCompletionSource.SetResult(new Envelope<TOut>(ProcessReader(r.DataReader)));
								}
								catch (Exception ex)
								{
									taskCompletionSource.SetException(ex);
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

	public interface IConnectionMessage
	{
		SqlConnectionStringBuilder ConnectionBuilder { get; }
	}
}
