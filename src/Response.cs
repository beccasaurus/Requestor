using System;
using System.Collections.Generic;

namespace Requestoring {

	/// <summary>
	/// Represents a simple HTTP response
	/// </summary>
	/// <remarks>
	/// Nearly all of the <c>Requestor</c> and <c>IRequestor</c> methods return an <c>IResponse</c>
	/// </remarks>
	public interface IResponse {
		int                        Status  { get; }
		string                     Body    { get; }
		IDictionary<string,string> Headers { get; }
	}

	/// <summary>
	/// Default, simple implementation of <c>IResponse</c>
	/// </summary>
	/// <remarks>
	/// If you want to return an IResponse, this is likely what you want to use.
	///
	/// The reason we have IResponse is to allow you to do something like wrap a complex 
	/// Response object with your own IResponse implementation.  It's not always convenient 
	/// to return a dumb struct-like object, such as this.
	///
	/// This has sane defaults.
	/// </remarks>
	public class Response : IResponse {

		public Response()                                                : this(-1, null, null)        {}
		public Response(int status)                                      : this(status, null, null)    {}
		public Response(string body)                                     : this(-1, body, null)        {}
		public Response(IDictionary<string,string> headers)              : this(-1, null, headers)     {}
		public Response(int status, string body)                         : this(status, body, null)    {}
		public Response(int status, IDictionary<string,string> headers)  : this(status, null, headers) {}
		public Response(string body, IDictionary<string,string> headers) : this(-1, body, headers)     {}

		public Response(int status, string body, IDictionary<string,string> headers) {
			Status  = (status == -1)    ? 200                             : status;
			Body    = (body == null)    ? ""                              : body;
			Headers = (headers == null) ? new Dictionary<string,string>() : headers;
		}

		public int Status                         { get; set; }
		public string Body                        { get; set; }
		public IDictionary<string,string> Headers { get; set; }
	}
}
