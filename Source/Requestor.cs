using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace Requestor {

    public interface IResponse {
	int                        Status  { get; }
	string                     Body    { get; }
	IDictionary<string,string> Headers { get; }
    }
    public class Response : IResponse {
	public int                        Status  { get; set; }
	public string                     Body    { get; set; }
	public IDictionary<string,string> Headers { get; set; }
    }

    public interface IRequestor {
	IResponse Get(string path);
    }
    public class Requestor : IRequestor {
	public string RootUrl { get; set; }

	public Requestor(string rootUrl) {
	    RootUrl = rootUrl;
	}

	public IResponse Get(string path) {
	    HttpWebRequest request = (HttpWebRequest) WebRequest.Create(RootUrl + path);
	    request.Method = "GET";
	    var response = request.GetResponse() as HttpWebResponse;

	    int status = (int) response.StatusCode;

	    string body = "";
	    using (var reader = new StreamReader(response.GetResponseStream()))
		body = reader.ReadToEnd();

	    var headers = new Dictionary<string, string>();
	    foreach (string headerName in response.Headers.AllKeys)
		headers.Add(headerName, string.Join(", ", response.Headers.GetValues(headerName)));

	    return new Response { Status = status, Body = body, Headers = headers };
	}
    }

}
