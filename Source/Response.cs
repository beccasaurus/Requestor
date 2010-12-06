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
    /// </remarks>
    public class Response : IResponse {
	public int                        Status  { get; set; }
	public string                     Body    { get; set; }
	public IDictionary<string,string> Headers { get; set; }
    }
}
