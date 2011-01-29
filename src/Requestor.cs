﻿using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

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

	/// <summary>
	/// <c>Requestor</c> has the main API for making requests.  Uses a <c>IRequestor</c> implementation behind the scenes.
	/// </summary>
	public class Requestor {

		/// <summary>Empty constructor</summary>
		public Requestor() {}

		public Requestor(string rootUrl) {
			RootUrl = rootUrl;
		}

		public Requestor(IRequestor implementation) {
			Implementation = implementation;
		}

		public IDictionary<string,string> DefaultHeaders = new Dictionary<string,string>();
		public IDictionary<string,string> Headers        = new Dictionary<string,string>();
		public IDictionary<string,string> QueryStrings   = new Dictionary<string,string>();
		public IDictionary<string,string> PostData       = new Dictionary<string,string>();

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

		public IResponse Get(string path){ return Get(path, null);  }
		public IResponse Get(string path, object variables){
			var info = MergeInfo(new RequestInfo(variables, "QueryStrings"));
			return SetLastResponse(Implementation.GetResponse("GET", Url(path, info.QueryStrings), info.PostData, MergeWithDefaultHeaders(info.Headers)));
		}

		public IResponse Post(string path){ return Post(path, null); }
		public IResponse Post(string path, object variables){
			var info = MergeInfo(new RequestInfo(variables, "PostData"));
			return SetLastResponse(Implementation.GetResponse("POST", Url(path, info.QueryStrings), info.PostData, MergeWithDefaultHeaders(info.Headers)));
		}

		public IResponse Put(string path){ return Put(path, null); }
		public IResponse Put(string path, object variables){
			var info = MergeInfo(new RequestInfo(variables, "PostData"));
			return SetLastResponse(Implementation.GetResponse("PUT", Url(path, info.QueryStrings), info.PostData, MergeWithDefaultHeaders(info.Headers)));
		}

		public IResponse Delete(string path){ return Delete(path, null); }
		public IResponse Delete(string path, object variables){
			var info = MergeInfo(new RequestInfo(variables, "PostData"));
			return SetLastResponse(Implementation.GetResponse("DELETE", Url(path, info.QueryStrings), info.PostData, MergeWithDefaultHeaders(info.Headers)));
		}

		IResponse _lastResponse;
		public IResponse LastResponse {
			get { return _lastResponse;  }
			set { _lastResponse = value; }
		}

		public IResponse FollowRedirect() {
			if (LastResponse == null)
				throw new Exception("Cannot follow redirect.  LastResponse is null.");
			else if (!LastResponse.Headers.Keys.Contains("Location"))
				throw new Exception("Cannot follow redirect.  Location header of LastResponse is null.");
			else
				return Get(LastResponse.Headers["Location"]);
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
			if (Implementation is IHaveCookies)
				ResetCookies();
			ResetLastResponse();
			DefaultHeaders.Clear();
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

		// PRIVATE
		
		IDictionary<string,string> MergeWithDefaultHeaders(IDictionary<string,string> headers) {
			return MergeDictionaries(DefaultHeaders, headers);
		}

		IDictionary<string,string> MergeDictionaries(IDictionary<string,string> a, IDictionary<string,string> b) {
			var result = new Dictionary<string,string>(a);
			foreach (var item in b)
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
	}
}
