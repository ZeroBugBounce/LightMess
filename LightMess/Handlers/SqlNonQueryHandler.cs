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
		public override void Handle(SqlNonQueryRequest message, Receipt receipt)
		{
			var command = message.Command;
			var connection = new SqlConnection(message.ConnectionBuilder.ConnectionString);
			connection.Open();
			command.Connection = connection;

			message.Command.BeginExecuteNonQuery(EndExecuteNonQuery, new SqlNonQueryState(message.Command, receipt));
		}

		void EndExecuteNonQuery(IAsyncResult asyncResult)
		{
			var sqlNonQueryState = asyncResult.AsyncState as SqlNonQueryState;
			int affectedRecords = sqlNonQueryState.Command.EndExecuteNonQuery(asyncResult);
		}

		class SqlNonQueryState
		{
			public SqlNonQueryState(SqlCommand command, Receipt receipt)
			{
				Command = command;
				Receipt = receipt;
			}

			public SqlCommand Command { get; private set; }
			public Receipt Receipt { get; private set; }
		}
	}

	public class SqlNonQueryRequest
	{
		public SqlNonQueryRequest(SqlCommand command, SqlConnectionStringBuilder connectionBuilder)
		{
			Command = command;
			ConnectionBuilder = connectionBuilder;
		}

		public SqlCommand Command { get; private set; }
		public SqlConnectionStringBuilder ConnectionBuilder { get; private set; }
	}

	public class SqlNonQueryResponse : IDisposable
	{
		Action closeConnection;

		public SqlNonQueryResponse(int affectedRecords, Action closeSqlConnection)
		{
			AffectedRecords = affectedRecords;
			closeConnection = closeSqlConnection;
		}

		public int AffectedRecords { get; private set; }

		public void Dispose()
		{
			closeConnection();
		}
	}
}
