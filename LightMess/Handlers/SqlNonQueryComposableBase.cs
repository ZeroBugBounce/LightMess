using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public abstract class SqlNonQueryComposableBase<TMessage> : Handler<TMessage> where TMessage : ISqlConnectionMessage
	{
		public abstract SqlCommand PrepareCommand(TMessage message, Receipt receipt);

		public sealed override void Handle(TMessage message, Receipt receipt)
		{
			Message.Post(new SqlNonQueryRequest(PrepareCommand(message, receipt), message.ConnectionBuilder))
				.Callback<SqlNonQueryResponse>(r =>
				{
					try
					{
						receipt.FireCallback(r.AffectedRecords);
					}
					finally
					{
						r.Dispose();
					}
				});
		}
	}
}
