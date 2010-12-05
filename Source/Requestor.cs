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

	    // request.UserAgent = "my custom user agent";

	    if (requestHeaders != null)
		foreach (var header in requestHeaders)
		    request.Headers.Add(header.Key, header.Value);

	    if (postVariables != null && postVariables.Count > 0) {
		var postString = "";
		foreach (var variable in postVariables)
		    postString += variable.Key + "=" + HttpUtility.UrlEncode(variable.Value) + "&";
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

	string Url(string path, IDictionary<string, string> queryStrings) {
	    if (queryStrings != null && queryStrings.Count > 0) {
		var url = Url(path) + "?";
		foreach (var queryString in queryStrings)
		    url += queryString.Key + "=" + HttpUtility.UrlEncode(queryString.Value) + "&";
		return url;
	    } else
		return Url(path);
	}

	public IResponse Get(string path){  return Implementation.GetResponse("GET", Url(path), null, null);  }
	public IResponse Get(string path, object variables){
	    var info = new RequestInfo(variables, "QueryStrings");
	    return Implementation.GetResponse("GET", Url(path, info.QueryStrings), info.PostData, info.Headers);
	}

	public IResponse Post(string path){ return Implementation.GetResponse("POST", Url(path), null, null); }
	public IResponse Post(string path, object variables){
	    var info = new RequestInfo(variables, "PostData");
	    return Implementation.GetResponse("POST", Url(path, info.QueryStrings), info.PostData, info.Headers);
	}

	// TODO refector to use ToObjectDictionary
        internal static IDictionary<string, string> ToStringDictionary(object anonymousType) {
            var dict = new Dictionary<string, string>();
	    foreach (var item in ToObjectDictionary(anonymousType))
		if (item.Value == null)
		    dict.Add(item.Key, null);
		else
		    dict.Add(item.Key, item.Value.ToString());
	    return dict;
        }

        internal static IDictionary<string, object> ToObjectDictionary(object anonymousType) {
            var dict = new Dictionary<string, object>();
            foreach (var property in anonymousType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (property.CanRead)
		    dict.Add(property.Name, property.GetValue(anonymousType, null));
            return dict;
        }

	// new RequestInfo(new { Foo = "Bar" }, "QueryStrings"); will add Foo to QueryStrings
	// new RequestInfo(new { Foo = "Bar" }, "PostData"); will add Foo to PostData
	// new RequestInfo(new { QueryStrings = new { Foo = "Bar" } }, "PostData"); will add Foo to QueryStrings
	// new RequestInfo(new { QueryStrings = new { Foo = "Bar" }, PostData = new { Hi = "There" } }, "PostData"); will add Foo to QueryStrings and Hi to PostData
	// new RequestInfo(new { Hi = "There", QueryStrings = new { Foo = "Bar" }}, "PostData"); will add Foo to QueryStrings and Hi to PostData
	// new RequestInfo(new { Hi = "There", QueryStrings = new { Foo = "Bar" }}, "Headers"); will add Foo to QueryStrings and Hi to Headers
	public class RequestInfo {
	    public IDictionary<string, string> QueryStrings = new Dictionary<string, string>();
	    public IDictionary<string, string> PostData     = new Dictionary<string, string>();
	    public IDictionary<string, string> Headers      = new Dictionary<string, string>();

	    public RequestInfo(object anonymousType, string defaultField) {
		foreach (var variable in Requestor.ToObjectDictionary(anonymousType)) {
		    switch (variable.Key) {
			case "QueryStrings":
			    QueryStrings = Requestor.ToStringDictionary(variable.Value); break;

			case "PostData":
			    PostData = Requestor.ToStringDictionary(variable.Value); break;

			case "Headers":
			    Headers = Requestor.ToStringDictionary(variable.Value); break;

			default:
			    switch (defaultField) {
				case "QueryStrings": QueryStrings.Add(variable.Key, variable.Value.ToString()); break;
				case "PostData":     PostData.Add(variable.Key, variable.Value.ToString()); break;
				case "Headers":      Headers.Add(variable.Key, variable.Value.ToString()); break;
				default: throw new Exception("Unknown default type: " + defaultField + ". Expected QueryStrings, PostData, or Headers.");
			    }
			    break;
		    }
		}
	    }
	}
    }
}
