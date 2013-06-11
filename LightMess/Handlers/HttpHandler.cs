using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Security;

namespace ZeroBugBounce.LightMess
{
	/// <summary>
	/// Not HttpHandler in the ASP.NET sense, this handler makes HTTP requests
	/// </summary>
	public class HttpHandler : Handler<HttpRequest>
	{
		public override void Handle(HttpRequest message, Receipt receipt)
		{
			var webRequest = WebRequest.Create(message.Url);
			webRequest.BeginGetResponse(EndGetResponse, new HttpRequestState(webRequest, receipt));
		}

		void EndGetResponse(IAsyncResult asyncResult)
		{
			var httpRequestState = asyncResult.AsyncState as HttpRequestState;

			var webRequest = httpRequestState.WebRequest;
			var webResponse = webRequest.EndGetResponse(asyncResult);

			var responseStream = webResponse.GetResponseStream();
			var outStream = new MemoryStream();
			byte[] buffer = new byte[4096];

			responseStream.BeginRead(buffer, 0, 4096, EndRead,
				new HttpResponseState(webResponse, buffer, outStream, httpRequestState.Receipt));
		}

		void EndRead(IAsyncResult asyncResult)
		{
			var httpResponseState = asyncResult.AsyncState as HttpResponseState;

			int bytesRead = httpResponseState.WebResponse.GetResponseStream().EndRead(asyncResult);
			var outStream = httpResponseState.OutStream;
			byte[] buffer = httpResponseState.Buffer;
			var responseStream = httpResponseState.WebResponse.GetResponseStream();

			if (bytesRead > 0)
			{
				outStream.Write(buffer, 0, bytesRead);
				responseStream.BeginRead(buffer, 0, 4096, EndRead, httpResponseState);
			}
			else
			{
				responseStream.Close();
				httpResponseState.WebResponse.Close();
				httpResponseState.Receipt.FireCallback(new HttpResponse(outStream.ToArray()));
			}
		}

		class HttpRequestState
		{
			public HttpRequestState(WebRequest webRequest,
				Receipt receipt)
			{
				WebRequest = webRequest;
				Receipt = receipt;
			}

			public WebRequest WebRequest { get; private set; }
			public Receipt Receipt { get; set; }
		}

		class HttpResponseState
		{
			public HttpResponseState(WebResponse webResponse, byte[] buffer,
				MemoryStream outStream, Receipt receipt)
			{
				WebResponse = webResponse;
				Buffer = buffer;
				OutStream = outStream;
				Receipt = receipt;
			}

			public WebResponse WebResponse { get; private set; }
			public byte[] Buffer { get; set; }
			public MemoryStream OutStream { get; set; }
			public Receipt Receipt { get; set; }
		}
	}

	public class HttpRequest
	{
		public HttpRequest(string url)
		{
			Url = url;
		}

		public string Url { get; private set; }
	}

	public class HttpResponse
	{
		public HttpResponse(byte[] data)
		{
			Data = data;
		}

		public byte[] Data { get; private set; }

	}
}
