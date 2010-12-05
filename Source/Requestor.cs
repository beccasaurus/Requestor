using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Reflection;
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

	public IResponse Post(string path, object variables) {
	    var postString = "";
	    foreach (var variable in DictionaryFromObject(variables))
		postString += variable.Key + "=" + HttpUtility.UrlEncode(variable.Value.ToString()) + "&";
	    Console.WriteLine("POST string: {0}", postString);
	    var bytes = Encoding.ASCII.GetBytes(postString);

	    HttpWebRequest request = (HttpWebRequest) WebRequest.Create(RootUrl + path);
	    request.Method        = "POST";
	    request.ContentType   = "application/x-www-form-urlencoded";
	    request.ContentLength = bytes.Length;

	    using (var stream = request.GetRequestStream())
		stream.Write(bytes, 0, bytes.Length);

	    // TODO DRY
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

        static IDictionary<string, object> DictionaryFromObject(object anonymousType) {
            var attr = BindingFlags.Public | BindingFlags.Instance;
            var dict = new Dictionary<string, object>();
            foreach (var property in anonymousType.GetType().GetProperties(attr))
                if (property.CanRead)
                    dict.Add(property.Name, property.GetValue(anonymousType, null));
            return dict;
        }
    }

}
