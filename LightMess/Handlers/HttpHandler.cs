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
		public override Task<Envelope> Handle(HttpRequest message, CancellationToken cancellation)
		{
			var taskCompletionSource = new TaskCompletionSource<Envelope>();

			try
			{
				var webRequest = WebRequest.Create(message.Url);
				webRequest.BeginGetResponse(EndGetResponse, new HttpRequestState(webRequest, taskCompletionSource));

				return taskCompletionSource.Task;
			}
			catch (NotSupportedException nsEx)
			{
				taskCompletionSource.SetException(nsEx);
			}
			catch (ArgumentNullException anEx)
			{
				taskCompletionSource.SetException(anEx);
			}
			catch (SecurityException sEx)
			{
				taskCompletionSource.SetException(sEx);
			}
			catch (UriFormatException ufEx)
			{
				taskCompletionSource.SetException(ufEx);
			}

			return null;
		}

		void EndGetResponse(IAsyncResult asyncResult)
		{
			var httpRequestState = asyncResult.AsyncState as HttpRequestState;

			try
			{
				var webRequest = httpRequestState.WebRequest;
				var webResponse = webRequest.EndGetResponse(asyncResult);

				var responseStream = webResponse.GetResponseStream();
				var outStream = new MemoryStream();
				byte[] buffer = new byte[4096];

				responseStream.BeginRead(buffer, 0, 4096, EndRead,
					new HttpResponseState(webResponse, buffer, outStream, httpRequestState.TaskCompletionSource));
			}
			catch (Exception ex)
			{
				httpRequestState.TaskCompletionSource.SetException(ex);
			}
		}

		void EndRead(IAsyncResult asyncResult)
		{
			var httpResponseState = asyncResult.AsyncState as HttpResponseState;

			try
			{
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
					httpResponseState.TaskCompletionSource.TrySetResult(
						new Envelope<HttpResponse>(new HttpResponse(outStream.ToArray())));

					responseStream.Close();
					httpResponseState.WebResponse.Close();
				}
			}
			catch (Exception ex)
			{
				httpResponseState.TaskCompletionSource.SetException(ex);
			}
		}

		class HttpRequestState
		{
			public HttpRequestState(WebRequest webRequest,
				TaskCompletionSource<Envelope> taskCompletionSource)
			{
				WebRequest = webRequest;
				TaskCompletionSource = taskCompletionSource;
			}

			public WebRequest WebRequest { get; private set; }
			public TaskCompletionSource<Envelope> TaskCompletionSource { get; set; }
		}

		class HttpResponseState
		{
			public HttpResponseState(WebResponse webResponse, byte[] buffer, 
				MemoryStream outStream, TaskCompletionSource<Envelope> taskCompletionSource)
			{
				WebResponse = webResponse;
				Buffer = buffer;
				OutStream = outStream;
				TaskCompletionSource = taskCompletionSource;
			}

			public WebResponse WebResponse {get; private set;}
			public byte[] Buffer { get; set; }
			public MemoryStream OutStream { get; set; }
			public TaskCompletionSource<Envelope> TaskCompletionSource { get; set; }
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
