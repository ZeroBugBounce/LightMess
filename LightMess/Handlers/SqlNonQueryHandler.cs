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

			var command = message.Connection.CreateCommand();
			command.CommandText = message.CommandText;

			command.BeginExecuteNonQuery(EndExecuteNonQuery, new SqlNonQueryState(command, taskCompletionSource));
			
			return taskCompletionSource.Task;
		}

		void EndExecuteNonQuery(IAsyncResult asyncResult)
		{
			var sqlNonQueryState = asyncResult.AsyncState as SqlNonQueryState;
			int affectedRecords = sqlNonQueryState.Command.EndExecuteNonQuery(asyncResult);

			sqlNonQueryState.TaskCompletionSource.TrySetResult(new Envelope<SqlNonQueryResponse>(
				new SqlNonQueryResponse(affectedRecords)));
		}

		class SqlNonQueryState
		{
			public SqlNonQueryState(SqlCommand command, TaskCompletionSource<Envelope> taskCompletionEventSource)
			{
				Command = command;
				TaskCompletionSource = taskCompletionEventSource;
			}

			public SqlCommand Command { get; private set; }
			public TaskCompletionSource<Envelope> TaskCompletionSource { get; set; }
		}
	}

	public class SqlNonQueryRequest
	{
		public SqlNonQueryRequest(string commandText, SqlConnection connection)
		{
			CommandText = commandText;
			Connection = connection;
		}

		public string CommandText { get; private set; }
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
