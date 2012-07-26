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
	public class SqlReaderHandler : Handler<SqlReaderRequest>
	{
		public override Task<Envelope> Handle(SqlReaderRequest message, CancellationToken cancellation)
		{
			var taskCompletionSource = new TaskCompletionSource<Envelope>();

			try
			{
				var command = message.Connection.CreateCommand();
				command.CommandText = message.CommandText;

				command.BeginExecuteReader(EndExecuteReader, new SqlReaderState(command, taskCompletionSource));

				return taskCompletionSource.Task;
			}
			catch (Exception ex)
			{
				taskCompletionSource.SetException(ex);
			}

			return null;
		}

		void EndExecuteReader(IAsyncResult asyncResult)
		{
			var sqlReaderState = asyncResult.AsyncState as SqlReaderState;

			try
			{
				var sqlReader = sqlReaderState.Command.EndExecuteReader(asyncResult);

				sqlReaderState.TaskCompletionSource.TrySetResult(
					new Envelope<SqlReaderResponse>(new SqlReaderResponse(sqlReader)));
			}
			catch (Exception ex)
			{
				sqlReaderState.TaskCompletionSource.SetException(ex);
			}
		}

		class SqlReaderState
		{
			public SqlReaderState(SqlCommand command, TaskCompletionSource<Envelope> taskCompletionEventSource)
			{
				Command = command;
				TaskCompletionSource = taskCompletionEventSource;
			}

			public SqlCommand Command { get; private set; }
			public TaskCompletionSource<Envelope> TaskCompletionSource { get; set; }
		}
	}

	public class SqlReaderRequest
	{
		public SqlReaderRequest(string commandText, SqlConnection connection)
		{
			CommandText = commandText;
			Connection = connection;
		}

		public string CommandText { get; private set; }
		public SqlConnection Connection { get; private set; }
	}

	public class SqlReaderResponse
	{
		public SqlReaderResponse(SqlDataReader dataReader)
		{
			DataReader = dataReader;
		}

		public SqlDataReader DataReader { get; private set; }
	}
}
