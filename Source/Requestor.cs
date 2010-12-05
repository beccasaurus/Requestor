using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace Requestor {

    // Not sure if this interface is necessary but it could end up being useful ?  Might wanna kill it for now
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
	IResponse GetResponse(string verb, string url, IDictionary<string, string> postVariables, IDictionary<string, string> requestHeaders);
    }

    public class HttpRequestor : IRequestor {
	public IResponse GetResponse(string verb, string url, IDictionary<string, string> postVariables, IDictionary<string, string> requestHeaders) {
	    HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
	    request.AllowAutoRedirect = false;
	    request.Method            = verb;

	    if (postVariables != null) {
		var postString = "";
		foreach (var variable in postVariables)
		    postString += variable.Key + "=" + HttpUtility.UrlEncode(variable.Value.ToString()) + "&";
		var bytes = Encoding.ASCII.GetBytes(postString);
		request.ContentType   = "application/x-www-form-urlencoded";
		request.ContentLength = bytes.Length;
		using (var stream = request.GetRequestStream())
		    stream.Write(bytes, 0, bytes.Length);
	    }

	    HttpWebResponse response = null;
	    try {
		response = request.GetResponse() as HttpWebResponse;
	    } catch (WebException ex) {
		response = ex.Response as HttpWebResponse;
	    }

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

    public class Requestor {

	public Requestor() {}
	public Requestor(string rootUrl) {
	    RootUrl = rootUrl;
	}
	public Requestor(IRequestor implementation) {
	    Implementation = implementation;
	}

	public string RootUrl { get; set; }

	IRequestor _implementation;
	public IRequestor Implementation {
	    get {
		if (_implementation == null) _implementation = new HttpRequestor();
		return _implementation;
	    }
	    set { _implementation = value; }
	}

	string Url(string path) {
	    if (RootUrl == null)
		return path;
	    else
		return RootUrl + path;
	}

	public IResponse Get(string path){  return Implementation.GetResponse("GET", Url(path), null, null);  }
	public IResponse Post(string path){ return Implementation.GetResponse("POST", Url(path), null, null); }
	
	public IResponse Post(string path, object variables){
	    return Implementation.GetResponse("POST", Url(path), DictionaryFromObject(variables), null); // for now, assume that variables only holds POST variables
	}

        static IDictionary<string, string> DictionaryFromObject(object anonymousType) {
            var attr = BindingFlags.Public | BindingFlags.Instance;
            var dict = new Dictionary<string, string>();
            foreach (var property in anonymousType.GetType().GetProperties(attr))
                if (property.CanRead) {
		    var value = property.GetValue(anonymousType, null);
		    if (value == null)
			dict.Add(property.Name, null);
		    else
			dict.Add(property.Name, value.ToString());
		}
            return dict;
        }
    }
}
