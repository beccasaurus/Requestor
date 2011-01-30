using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Requestoring {

	/// <summary>
	/// All Requestors must implement this simple, single method interface
	/// </summary>
	public interface IRequestor {
		IResponse GetResponse(string verb, string url, IDictionary<string, string> postVariables, IDictionary<string, string> requestHeaders);
	}

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
		public void Add(string method, string url, IResponse response) {
			Add(new FakeResponse {
				Method   = method,
				Url      = url,
				Response = response
			});
		}
		public void Add(int maxTimesToReturn, string method, string url, IResponse response) {
			Add(new FakeResponse {
				Method   = method,
				Url      = url,
				Response = response,
				MaxUsage = maxTimesToReturn
			});
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

			public IDictionary<string,string> DefaultHeaders = new Dictionary<string,string>();

			// Faking responses ... copy/pasted from Requestor ... TODO DRY this up!
			public void EnableRealRequests()  { AllowRealRequests = true;  }
			public void DisableRealRequests() { AllowRealRequests = false; }
			public FakeResponseList FakeResponses = new FakeResponseList();
			public void FakeResponse(string method, string url, IResponse response) {
				FakeResponses.Add(method, url, response);
			}
			public void FakeResponse(int times, string method, string url, IResponse response) {
				FakeResponses.Add(times, method, url, response);
			}
			public void FakeResponseOnce(string method, string url, IResponse response) {
				FakeResponse(1, method, url, response);
			}

			public string RootUrl;

			public void Reset() {
				FakeResponses.Clear();
				DefaultHeaders.Clear();
				Implementation = null;
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

		// Faking responses
		public void DisableRealRequests() { AllowRealRequests = false; }
		public void EnableRealRequests()  { AllowRealRequests = true;  }
		public FakeResponseList FakeResponses = new FakeResponseList();
		public void FakeResponse(string method, string url, IResponse response) {
			FakeResponses.Add(method, url, response);
		}
		public void FakeResponse(int times, string method, string url, IResponse response) {
			FakeResponses.Add(times, method, url, response);
		}
		public void FakeResponseOnce(string method, string url, IResponse response) {
			FakeResponse(1, method, url, response);
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
		public IResponse Request(string method, string path, RequestInfo info) {
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
			var response = requestor.GetResponse(method, Url(path, info.QueryStrings), info.PostData, MergeWithDefaultHeaders(info.Headers));

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
			else 
				return Get(response.Headers["Location"]);
		}

		public void EnableCookies() {
			if (Implementation is IHaveCookies)
				(Implementation as IHaveCookies).EnableCookies();
			else
				throw new Exception(string.Format("Cannot enable cookies.  Requestor Implementation {0} does not implement IHaveCookies", Implementation));
		}

		public void DisableCookies() {
			if (Implementation is IHaveCookies)
				(Implementation as IHaveCookies).DisableCookies();
			else
				throw new Exception(string.Format("Cannot disable cookies.  Requestor Implementation {0} does not implement IHaveCookies", Implementation));
		}

		public void ResetCookies() {
			if (Implementation is IHaveCookies)
				(Implementation as IHaveCookies).ResetCookies();
			else
				throw new Exception(string.Format("Cannot reset cookies.  Requestor Implementation {0} does not implement IHaveCookies", Implementation));
		}

		public void Reset() {
			Implementation = null;
			ResetLastResponse();
			DefaultHeaders.Clear();
			FakeResponses.Clear();
		}

		public void ResetLastResponse() {
			LastResponse = null;
		}

		public void AddHeader(string key, string value) {
			Headers.Add(key, value);
		}

		public void AddQueryString(string key, string value) {
			QueryStrings.Add(key, value);
		}

		public void AddPostData(string key, string value) {
			PostData.Add(key, value);
		}

		public void SetPostData(string value) {
			PostData.Clear();
			PostData.Add(value, null);
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

		internal IResponse SetLastResponse(IResponse response) {
			// clear out the stored headers, querystrings, and post data for this request
			Headers      = new Dictionary<string,string>();
			QueryStrings = new Dictionary<string,string>();
			PostData     = new Dictionary<string,string>();

			_lastResponse = response;
			return response;
		}

		internal static IDictionary<string, string> ToStringDictionary(object anonymousType) {
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

		internal static IDictionary<string, object> ToObjectDictionary(object anonymousType) {
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
