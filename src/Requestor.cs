using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// TODO - split this up into files!  It's just kept growing and growing ... 

namespace Requestoring {

	/// <summary> /// All Requestors must implement this simple, single method interface /// </summary>
	public interface IRequestor {
		IResponse GetResponse(string verb, string url, IDictionary<string, string> postVariables, IDictionary<string, string> requestHeaders);
	}

	/// <summary>Any Requestor that supports Cookies needs to implement this.</summary>
	public interface IHaveCookies {
		void EnableCookies();
		void DisableCookies();
		void ResetCookies();
	}

	public class RealRequestsDisabledException : Exception {
		public RealRequestsDisabledException(string message) : base(message) {}
	}

	public class FakeResponse {
		public int TimesUsed = 0;
		public int MaxUsage  = -1;

		public string Method      { get; set; }
		public string Url         { get; set; }
		public IResponse Response { get; set; }
	}

	public class FakeResponseList : List<FakeResponse>, IRequestor {
		public FakeResponseList Add(string method, string url, IResponse response) {
			Add(new FakeResponse {
				Method   = method,
				Url      = url,
				Response = response
			});
			return this;
		}
		public FakeResponseList Add(int times, string method, string url, IResponse response) {
			Add(new FakeResponse {
				Method   = method,
				Url      = url,
				Response = response,
				MaxUsage = times
			});
			return this;
		}

		// This is a safe way to get a fake response.  It does NOT increment TimesUsed
		public FakeResponse GetFakeResponse(string verb, string url) {
			return this.FirstOrDefault(fake => fake.Method == verb && fake.Url == url);
		}

		// This will increment TimesUsed and remove the FakeResponse after its last usage
		public IResponse GetResponse(string verb, string url, IDictionary<string, string> postVariables, IDictionary<string, string> requestHeaders) {
			var fake = GetFakeResponse(verb, url);

			if (fake != null && fake.MaxUsage > 0) {
				fake.TimesUsed++;
				if (fake.TimesUsed >= fake.MaxUsage)
					this.Remove(fake);
			}
				
			if (fake == null)
				return null;
			else
				return fake.Response;
		}
	}

	/// <summary>
	/// <c>Requestor</c> has the main API for making requests.  Uses a <c>IRequestor</c> implementation behind the scenes.
	/// </summary>
	public class Requestor {

		public class GlobalConfiguration {
			public bool AutoRedirect { get; set; }
			public bool AllowRealRequests { get; set; }
			
			bool _verbose;
			public bool Verbose {
				get {
					if (Environment.GetEnvironmentVariable("VERBOSE") != null && Environment.GetEnvironmentVariable("VERBOSE") == "true")
						return true;
					else
						return _verbose;
				}
				set { _verbose = value; }
			}

			public IDictionary<string,string> DefaultHeaders = new Dictionary<string,string>();

			// Faking responses ... copy/pasted from Requestor ... TODO DRY this up!
			public GlobalConfiguration EnableRealRequests()  { AllowRealRequests = true;  return this; }
			public GlobalConfiguration DisableRealRequests() { AllowRealRequests = false; return this; }
			public FakeResponseList FakeResponses = new FakeResponseList();
			public GlobalConfiguration FakeResponse(string method, string url, IResponse response) {
				FakeResponses.Add(method, url, response); return this;
			}
			public GlobalConfiguration FakeResponse(int times, string method, string url, IResponse response) {
				FakeResponses.Add(times, method, url, response); return this;
			}
			public GlobalConfiguration FakeResponseOnce(string method, string url, IResponse response) {
				FakeResponse(1, method, url, response); return this;
			}

			public string RootUrl;

			public GlobalConfiguration Reset() {
				FakeResponses.Clear();
				DefaultHeaders.Clear();
				Implementation = null;
				return this;
			}

			IRequestor _implementation;
			public IRequestor Implementation {
				get {
					if (_implementation == null)
						_implementation = Activator.CreateInstance(Requestor.DefaultIRequestor) as IRequestor;
					return _implementation;
				}
				set { _implementation = value; }
			}
		}

		#region Static
		static Type _defaultIRequestor = typeof(HttpRequestor);
		public static Type DefaultIRequestor {
			get { return _defaultIRequestor; }
			set {
				if (IsIRequestor(value))
					_defaultIRequestor = value;
				else
					throw new InvalidCastException("DefaultIRequestor must implement IRequestor");
			}
		}

		public static bool IsIRequestor(Type type) {
			return (type.GetInterface(typeof(IRequestor).FullName) != null);
		}

		static GlobalConfiguration _global = new GlobalConfiguration {
			AllowRealRequests = true
		};

		public static GlobalConfiguration Global {
			get { return _global;  }
			set { _global = value; }
		}
		#endregion

		#region Instance
		public Requestor() {}

		public Requestor(string rootUrl) {
			RootUrl = rootUrl;
		}

		public Requestor(IRequestor implementation) {
			Implementation = implementation;
		}

		bool? _autoRedirect;
		public bool AutoRedirect {
			get { return (bool)(_autoRedirect ?? Requestor.Global.AutoRedirect); }
			set { _autoRedirect = value; }
		}

		bool? _allowRealRequests;
		public bool AllowRealRequests {
			get { return (bool)(_allowRealRequests ?? Requestor.Global.AllowRealRequests); }
			set { _allowRealRequests = value; }
		}

		bool? _verbose;
		public bool Verbose {
			get { return (bool)(_verbose ?? Requestor.Global.Verbose); }
			set { _verbose = value; }
		}

		// Faking responses
		public Requestor DisableRealRequests() { AllowRealRequests = false; return this; }
		public Requestor EnableRealRequests()  { AllowRealRequests = true;  return this; }
		public FakeResponseList FakeResponses = new FakeResponseList();
		public Requestor FakeResponse(string method, string url, IResponse response) {
			FakeResponses.Add(method, url, response); return this;
		}
		public Requestor FakeResponse(int times, string method, string url, IResponse response) {
			FakeResponses.Add(times, method, url, response); return this;
		}
		public Requestor FakeResponseOnce(string method, string url, IResponse response) {
			FakeResponse(1, method, url, response); return this;
		}

		public IDictionary<string,string> DefaultHeaders = new Dictionary<string,string>();
		public IDictionary<string,string> Headers        = new Dictionary<string,string>();
		public IDictionary<string,string> QueryStrings   = new Dictionary<string,string>();
		public IDictionary<string,string> PostData       = new Dictionary<string,string>();

		string _rootUrl;
		public string RootUrl {
			get { return _rootUrl ?? Global.RootUrl; }
			set { _rootUrl = value; }
		}

		IRequestor _implementation;
		public IRequestor Implementation {
			get { return _implementation ?? Global.Implementation; }
			set { _implementation = value; }
		}

		public Uri CurrentUri { get; set; }
		public string CurrentUrl  { get { return (CurrentUri == null) ? null : CurrentUri.ToString();   } }
		public string CurrentPath { get { return (CurrentUri == null) ? null : CurrentUri.PathAndQuery; } }

		public string Url(string path) {
			if (IsAbsoluteUrl(path))
				return path;

			if (RootUrl == null)
				return path;
			else
				return RootUrl + path;
		}

		public string Url(string path, IDictionary<string, string> queryStrings) {
			if (queryStrings != null && queryStrings.Count > 0) {
				var url = Url(path) + "?";
				foreach (var queryString in queryStrings)
					url += queryString.Key + "=" + HttpUtility.UrlEncode(queryString.Value) + "&";
                url.TrimEnd('&');
				return url;
			} else
				return Url(path);
		}

		public IResponse Get(    string path){ return Get(path, null);    }
		public IResponse Post(   string path){ return Post(path, null);   }
		public IResponse Put(    string path){ return Put(path, null);    }
		public IResponse Delete( string path){ return Delete(path, null); }

		public IResponse Get(   string path, object variables){ return Request("GET",    path, variables, "QueryStrings"); }
		public IResponse Post(  string path, object variables){ return Request("POST",   path, variables, "PostData");     }
		public IResponse Put(   string path, object variables){ return Request("PUT",    path, variables, "PostData");     }
		public IResponse Delete(string path, object variables){ return Request("DELETE", path, variables, "PostData");     }

		public IResponse Request(string method, string path) {
			return Request(method, path, null, null);
		}
		public IResponse Request(string method, string path, object variables, string defaultVariableType) {
			return Request(method, path, MergeInfo(new RequestInfo(variables, defaultVariableType)));
		}

		/// <summary>This is THE Request methd that all other ones use.  It will process fake and real requests.</summary>
		public IResponse Request(string method, string path, RequestInfo info) {
			if (Verbose) {
				Console.WriteLine("{0} {1}", method, path);
				foreach (var queryString in info.QueryStrings)
					Console.WriteLine("\tQUERY  {0}: {1}", queryString.Key, queryString.Value);
				foreach (var postItem in info.PostData)
					Console.WriteLine("\tPOST   {0}: {1}", postItem.Key, postItem.Value);
				foreach (var header in info.Headers)
					Console.WriteLine("\tHEADER {0}: {1}", header.Key, header.Value);
			}

			// try instance fake requests
			var instanceFakeResponse = Request(method, path, info, this.FakeResponses);
			if (instanceFakeResponse != null)
				return instanceFakeResponse;
			
			// then try global fake requests
			var globalFakeResponse = Request(method, path, info, Requestor.Global.FakeResponses);
			if (globalFakeResponse != null)
				return globalFakeResponse;
			
			// then, if real requests are disabled, raise exception, else fall back to real implementation
			if (AllowRealRequests)
				return Request(method, path, info, Implementation);
			else
				throw new RealRequestsDisabledException(string.Format("Real requests are disabled. {0} {1}", method, Url(path, info.QueryStrings)));
		}
		public IResponse Request(string method, string path, RequestInfo info, IRequestor requestor) {
			var url = Url(path, info.QueryStrings);
			
			try {
				CurrentUri = new Uri(url);
			} catch (Exception ex) {
				Console.WriteLine("BAD URI.  method:{0} path:{1} url:{2}", method, path, url);
				LastException = ex;
				// We don't return null here because a custom IRequestor could support this URL, even if it's not a valid URI
			}

            IResponse response;

            try {
                response = requestor.GetResponse(method, url, info.PostData, MergeWithDefaultHeaders(info.Headers));
            } catch (Exception ex) {
                Console.WriteLine("Requestor ({0}) failed to respond for {1} {2} [{3}]", requestor.GetType(), method, path, url);
                Console.WriteLine("Requested info:");
				Console.WriteLine("\t{0} {1}", method, url);
                Console.WriteLine("\tPostData: " + string.Join(", ", info.PostData.Select(item => string.Format("{0} => {1}", item.Key, item.Value)).ToArray()));
                Console.WriteLine("\tHeaders:  " + string.Join(", ", info.Headers.Select(item => string.Format("{0} => {1}", item.Key, item.Value)).ToArray()));
				LastException = ex;
                return null;
            }

			if (response == null)
				return null;

	 		if (AutoRedirect)
				while (IsRedirect(response))
					response = FollowRedirect(response);

			return SetLastResponse(response);
		}

		IResponse _lastResponse;
		public IResponse LastResponse {
			get { return _lastResponse;  }
			set { _lastResponse = value; }
		}

		public Exception LastException { get; set; }

		public bool IsRedirect(IResponse response) {
			return (response.Status.ToString().StartsWith("3") && response.Headers.Keys.Contains("Location"));
		}

		public IResponse FollowRedirect() {
			return FollowRedirect(LastResponse);
		}

		public IResponse FollowRedirect(IResponse response) {
			if (response == null)
				throw new Exception("Cannot follow redirect.  response is null.");
			else if (!response.Headers.Keys.Contains("Location"))
				throw new Exception("Cannot follow redirect.  Location header of response is null.");
			else {
                PostData.Clear(); // You cannot have PostData when doing a GET
				return Get(response.Headers["Location"]);
            }
		}

		public Requestor EnableCookies() {
			if (Implementation is IHaveCookies)
				(Implementation as IHaveCookies).EnableCookies();
			else
				throw new Exception(string.Format("Cannot enable cookies.  Requestor Implementation {0} does not implement IHaveCookies", Implementation));
			return this;
		}

		public Requestor DisableCookies() {
			if (Implementation is IHaveCookies)
				(Implementation as IHaveCookies).DisableCookies();
			else
				throw new Exception(string.Format("Cannot disable cookies.  Requestor Implementation {0} does not implement IHaveCookies", Implementation));
			return this;
		}

		public Requestor ResetCookies() {
			if (Implementation is IHaveCookies)
				(Implementation as IHaveCookies).ResetCookies();
			else
				throw new Exception(string.Format("Cannot reset cookies.  Requestor Implementation {0} does not implement IHaveCookies", Implementation));
			return this;
		}

		public Requestor Reset() {
			Implementation = null;
			ResetLastResponse();
			DefaultHeaders.Clear();
			FakeResponses.Clear();
			return this;
		}

		public Requestor ResetLastResponse() {
			LastResponse = null;
			return this;
		}

		public Requestor AddHeader(string key, string value) {
			Headers.Add(key, value);
			return this;
		}

		public Requestor AddQueryString(string key, string value) {
			QueryStrings.Add(key, value);
			return this;
		}

		public Requestor AddPostData(string key, string value) {
			PostData.Add(key, value);
			return this;
		}

		public Requestor SetPostData(string value) {
			PostData.Clear();
			PostData.Add(value, null);
			return this;
		}
		#endregion

		#region private
		bool IsAbsoluteUrl(string path) {
			return Regex.IsMatch(path, @"^\w+://"); // if it starts with whatever://, then it's absolute.
		}

		IDictionary<string,string> MergeWithDefaultHeaders(IDictionary<string,string> headers) {
			return MergeDictionaries(MergeDictionaries(Requestor.Global.DefaultHeaders, DefaultHeaders), headers);
		}

		IDictionary<string,string> MergeDictionaries(IDictionary<string,string> defaults, IDictionary<string,string> overrides) {
			var result = new Dictionary<string,string>(defaults);
			foreach (var item in overrides)
				result[item.Key] = item.Value;
			return result;
		}

		public IResponse SetLastResponse(IResponse response) {
			// clear out the stored headers, querystrings, and post data for this request
			Headers      = new Dictionary<string,string>();
			QueryStrings = new Dictionary<string,string>();
			PostData     = new Dictionary<string,string>();

			_lastResponse = response;
			return response;
		}

		public static IDictionary<string, string> ToStringDictionary(object anonymousType) {
			if (anonymousType is Dictionary<string, string>)
				return anonymousType as Dictionary<string, string>;

			var dict = new Dictionary<string, string>();
			foreach (var item in ToObjectDictionary(anonymousType))
				if (item.Value == null)
					dict.Add(item.Key, null);
				else
					dict.Add(item.Key, item.Value.ToString());
			return dict;
		}

		public static IDictionary<string, object> ToObjectDictionary(object anonymousType) {
			if (anonymousType is Dictionary<string, object>)
				return anonymousType as Dictionary<string, object>;

			var dict = new Dictionary<string, object>();

			if (anonymousType is Dictionary<string, string>) {
				foreach (var item in (anonymousType as Dictionary<string, string>))
					dict.Add(item.Key, item.Value);
				return dict;
			}

			foreach (var property in anonymousType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
				if (property.CanRead)
					dict.Add(property.Name, property.GetValue(anonymousType, null));
			return dict;
		}

		RequestInfo MergeInfo(RequestInfo info) {
			info.QueryStrings = MergeDictionaries(info.QueryStrings, QueryStrings);
			info.Headers      = MergeDictionaries(info.Headers,      Headers);
			info.PostData     = MergeDictionaries(info.PostData,     PostData);
			return info;
		}
		#endregion

		#region RequestInfo
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
				if (anonymousType == null) return;

				// PostData can be a simple string, eg. Post("/dogs", "name=Rover&breed=Something");
				if (defaultField == "PostData" && anonymousType is string) {
					PostData.Add(anonymousType as string, null);
					return;
				}

				foreach (var variable in Requestor.ToObjectDictionary(anonymousType)) {
					switch (variable.Key) {
						case "QueryStrings":
							QueryStrings = Requestor.ToStringDictionary(variable.Value); break;

						// PostData can be a simple string
						case "PostData":
							if (variable.Value is string)
								PostData.Add(variable.Value.ToString(), null);
							else
								PostData = Requestor.ToStringDictionary(variable.Value);
						break;

						case "Headers":
							Headers = Requestor.ToStringDictionary(variable.Value); break;

						default:
						switch (defaultField) {
							case "QueryStrings": QueryStrings.Add(variable.Key, variable.Value.ToString()); break;
							case "PostData":     PostData.Add(variable.Key, variable.Value.ToString());     break;
							case "Headers":      Headers.Add(variable.Key, variable.Value.ToString());      break;
							default: throw new Exception("Unknown default type: " + defaultField + ". Expected QueryStrings, PostData, or Headers.");
						}
						break;
					}
				}
			}
		}
		#endregion
	}
}
