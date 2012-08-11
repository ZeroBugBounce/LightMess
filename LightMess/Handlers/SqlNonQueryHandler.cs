using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess
{
	public class SqlNonQueryHandler : Handler<SqlNonQueryRequest>
	{
		public override Task<Envelope> Handle(SqlNonQueryRequest message, CancellationToken cancellation)
		{
			var taskCompletionSource = new TaskCompletionSource<Envelope>();

			try
			{
				if (cancellation.IsCancellationRequested)
				{
					taskCompletionSource.TrySetCanceled();
					return taskCompletionSource.Task;
				}

				message.Command.BeginExecuteNonQuery(EndExecuteNonQuery, new SqlNonQueryState(message.Command, taskCompletionSource, cancellation));

				return taskCompletionSource.Task;
			}
			catch (Exception ex)
			{
				taskCompletionSource.SetException(ex);
			}

			return null;
		}

		void EndExecuteNonQuery(IAsyncResult asyncResult)
		{
			var sqlNonQueryState = asyncResult.AsyncState as SqlNonQueryState;
			try
			{
				if (sqlNonQueryState.CancellationToken.IsCancellationRequested)
				{
					sqlNonQueryState.TaskCompletionSource.TrySetCanceled();
					sqlNonQueryState.Command.Cancel();
					return;
				}

				int affectedRecords = sqlNonQueryState.Command.EndExecuteNonQuery(asyncResult);

				sqlNonQueryState.TaskCompletionSource.TrySetResult(new Envelope<SqlNonQueryResponse>(
					new SqlNonQueryResponse(affectedRecords)));
			}
			catch (Exception ex)
			{
				sqlNonQueryState.TaskCompletionSource.TrySetException(ex);
			}
		}

		class SqlNonQueryState
		{
			public SqlNonQueryState(SqlCommand command, TaskCompletionSource<Envelope> taskCompletionEventSource,
				CancellationToken cancellationToken)
			{
				Command = command;
				TaskCompletionSource = taskCompletionEventSource;
				CancellationToken = cancellationToken;
			}

			public SqlCommand Command { get; private set; }
			public TaskCompletionSource<Envelope> TaskCompletionSource { get; set; }
			public CancellationToken CancellationToken { get; private set; }
		}
	}

	public class SqlNonQueryRequest
	{
		public SqlNonQueryRequest(SqlCommand command, SqlConnection connection)
		{
			Command = command;
			Connection = connection;
		}

		public SqlCommand Command { get; private set; }
		public SqlConnection Connection { get; private set; }
	}

	public class SqlNonQueryResponse
	{
		public SqlNonQueryResponse(int affectedRecords)
		{
			AffectedRecords = affectedRecords;
		}

		public int AffectedRecords { get; private set; }
	}
}
