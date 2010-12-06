using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Requestoring {

    /// <summary>
    /// <c>IRequestor</c> implementation that uses real HTTP via <c>System.Net.HttpWebRequest</c>
    /// </summary>
    /// <remarks>
    /// Currently, this is the default (only) built-in <c>IRequestor</c> implementation
    ///
    /// This is ideal for testing web APIs that you don't have access to the code for.
    ///
    /// Currently, there's no easy way to do full-stack ASP.NET testing without going over 
    /// HTTP, so this is great for that.  Eventually, we hope to have a WSGI/Rack-like interface 
    /// created that ASP.NET can run on top off, and we can make an <c>IRequestor</c> using that.
    /// </remarks>
    public class HttpRequestor : IRequestor, IHaveCookies {

	public static string MethodVariable = "X-HTTP-Method-Override";

	CookieContainer cookies;

	public void EnableCookies() {
	    ResetCookies();
	}

	public void DisableCookies() {
	    cookies = null;
	}

	public void ResetCookies() {
	    cookies = new CookieContainer();
	}

	// TODO this method is definitely big enough and complex enough now that we should refactor into smaller methods!
	public IResponse GetResponse(string verb, string url, IDictionary<string, string> postVariables, IDictionary<string, string> requestHeaders) {
	    HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
	    request.AllowAutoRedirect = false;
	    request.UserAgent         = "Requestor";
	    request.Method            = verb;

	    // If the postVariables dictionary only has 1 item which has a null value, it represents ALL of the PostData
	    //
	    // NOTE: we're going through some pain to get the first value because we're support 3.5 
	    //       which doesn't have the 4.0 IEnumerable Linq extensions
	    string postString = null;
	    if (postVariables != null && postVariables.Count == 1) {
		string firstKey = null;
		foreach (string key in postVariables.Keys){ firstKey = key; break; }
		if (postVariables[firstKey] == null)
		    postString = firstKey; // <--- the key has the actual PostData
	    }

	    // If we've enabled cookies (cookies isn't null), attach them to the new request
	    if (cookies != null) request.CookieContainer = cookies;

	    // Add MethodVariable POST variable if it's set and we're doing a PUT or DELETE
	    if (verb == "PUT" || verb == "DELETE") {
		if (MethodVariable != null) {
		    if (postVariables == null)
		        postVariables = new Dictionary<string, string>();
		    postVariables.Add(MethodVariable, verb);
		}
	    }

	    if (requestHeaders != null) AddHeadersToRequest(requestHeaders, request);

	    if (postVariables != null && postVariables.Count > 0) {
		if (postString == null) {
		    postString = "";
		    foreach (var variable in postVariables)
			postString += variable.Key + "=" + HttpUtility.UrlEncode(variable.Value) + "&";
		}
		var bytes = Encoding.ASCII.GetBytes(postString);
		if (request.ContentType == null)
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

	void AddHeadersToRequest(IDictionary<string,string> headers, HttpWebRequest request) {
	    foreach (var header in headers) {
		switch (header.Key) {
		    case "ContentType":
		    case "Content-Type":
			request.ContentType = header.Value;
			break;
		    case "UserAgent":
		    case "User-Agent":
			request.UserAgent = header.Value;
			break;
		    default:
			request.Headers.Add(header.Key, header.Value);
			break;
		}
	    }
	}
    }
}
