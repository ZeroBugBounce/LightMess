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
				var command = message.Command;
				var connection = new SqlConnection(message.ConnectionBuilder.ConnectionString);
				connection.Open();
				command.Connection = connection;

				command.BeginExecuteReader(EndExecuteReader, new SqlReaderState(command, taskCompletionSource));

				return taskCompletionSource.Task;
			}
			catch (Exception ex)
			{
				taskCompletionSource.SetException(ex);
			}

			return taskCompletionSource.Task;
		}

		void EndExecuteReader(IAsyncResult asyncResult)
		{
			var sqlReaderState = asyncResult.AsyncState as SqlReaderState;

			try
			{
				var sqlReader = sqlReaderState.Command.EndExecuteReader(asyncResult);

				sqlReaderState.TaskCompletionSource.TrySetResult(
					new Envelope<SqlReaderResponse>(new SqlReaderResponse(sqlReader, sqlReaderState.Command.Connection.Close)));
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
		public SqlReaderRequest(SqlCommand command, SqlConnectionStringBuilder connectionBuilder)
		{
			Command = command;
			ConnectionBuilder = connectionBuilder;
		}

		public SqlCommand Command { get; private set; }
		public SqlConnectionStringBuilder ConnectionBuilder { get; private set; }
	}

	public class SqlReaderResponse : IDisposable
	{
		Action closeConnection;

		public SqlReaderResponse(SqlDataReader dataReader, Action closeSqlConnection)
		{
			DataReader = dataReader;
			closeConnection = closeSqlConnection;
		}

		public SqlDataReader DataReader { get; private set; }

		public void Dispose()
		{
			closeConnection();
		}
	}
}
