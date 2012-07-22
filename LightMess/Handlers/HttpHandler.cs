using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroBugBounce.LightMess.Handlers
{
	public class HttpHandler : Handler<HttpRequest>
	{
		public override Task<Envelope> Handle(HttpRequest message, CancellationToken cancellation)
		{
			var taskCompletionSource = new TaskCompletionSource<Envelope>();
			var webRequest = WebRequest.Create(message.Url);
			webRequest.BeginGetResponse(EndGetResponse, new HttpRequestState(webRequest));

			return taskCompletionSource.Task;
		}

		void EndGetResponse(IAsyncResult asyncResult)
		{
			//var httpState = asyncResult.AsyncState as HttpRequestState;
			//var webRequest = httpState.WebRequest;
			//var webResponse = webRequest.EndGetResponse(asyncResult);
		
			//var responseStream = webResponse.GetResponseStream();
			//responseStream.r

		}

		class HttpRequestState
		{
			public HttpRequestState(WebRequest webRequest)
			{
				WebRequest = webRequest;
			}

			public WebRequest WebRequest { get; private set; }
		}

		class HttpResponseState
		{
			public HttpResponseState(WebResponse webResponse)
			{
				WebResponse = webResponse;
			}

			public WebResponse WebResponse {get; private set;}
		}
	}

	public class HttpRequest
	{
		public HttpRequest(string url)
		{

		}

		public string Url { get; private set; }
	}

	public class HttpResponse
	{

	}
}
