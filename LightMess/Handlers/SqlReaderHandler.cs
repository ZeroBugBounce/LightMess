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
		public override void Handle(SqlReaderRequest message, Receipt receipt)
		{
			var command = message.Command;
			var connection = new SqlConnection(message.ConnectionBuilder.ConnectionString);
			connection.Open();
			command.Connection = connection;

			command.BeginExecuteReader(EndExecuteReader, new SqlReaderState(command, receipt));
		}

		void EndExecuteReader(IAsyncResult asyncResult)
		{
			var sqlReaderState = asyncResult.AsyncState as SqlReaderState;
			var sqlReader = sqlReaderState.Command.EndExecuteReader(asyncResult);
			sqlReaderState.Receipt.FireCallback(new SqlReaderResponse(sqlReader, sqlReaderState.Command.Connection.Close));
		}

		class SqlReaderState
		{
			public SqlReaderState(SqlCommand command, Receipt receipt)
			{
				Command = command;
				Receipt = receipt;
			}

			public SqlCommand Command { get; private set; }
			public Receipt Receipt { get; private set; }
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
