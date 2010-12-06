using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace Requestor {

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

	public IResponse GetResponse(string verb, string url, IDictionary<string, string> postVariables, IDictionary<string, string> requestHeaders) {
	    HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
	    request.AllowAutoRedirect = false;
	    request.UserAgent = "Requestor";

	    if (cookies != null)
		request.CookieContainer = cookies;

	    if (verb == "PUT" || verb == "DELETE") {
		request.Method = "POST";
		if (postVariables == null)
		    postVariables = new Dictionary<string, string>();
		postVariables.Add(MethodVariable, verb);
	    } else
		request.Method = verb;

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
}
