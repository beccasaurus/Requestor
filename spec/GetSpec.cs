using System;
using System.Collections.Generic;
using NUnit.Framework;
using Requestoring;

namespace Requestoring.Specs {

    [TestFixture]
    public class GetSpec : Spec {

		// Maybe?  Not super useful.  But, if we do this, it should support the global RootUrl and whatnot
		//
		// [Test][Ignore]
		// public void can_get_using_global_requestor_instance(){
		// }

		[Test]
		public void can_get_response_body() {
			Get("/"        ).Body.ShouldEqual("Hello World");
			Get("/info"    ).Body.ShouldContain("You did: GET /info");
			Get("/boom"    ).Body.ShouldEqual("Boom!");
			Get("/redirect").Body.ShouldEqual("Redirecting");
			Get("/headers" ).Body.ShouldEqual("This has custom headers FOO and BAR");
			Get("/notfound").Body.ShouldEqual("Not Found: GET /notfound");
		}

		[Test]
		public void can_get_response_status() {
			Get("/"        ).Status.ShouldEqual(200);
			Get("/info"    ).Status.ShouldEqual(200);
			Get("/boom"    ).Status.ShouldEqual(500);
			Get("/redirect").Status.ShouldEqual(302);
			Get("/headers" ).Status.ShouldEqual(200);
			Get("/notfound").Status.ShouldEqual(404);
		}

		[Test]
		public void can_get_response_headers() {
			Get("/").Headers["Content-Type"].ShouldEqual("text/html");
			Get("/").Headers.Keys.ShouldNotContain("FOO");
			Get("/headers").Headers.Keys.ShouldContain("FOO");
			Get("/headers").Headers["FOO"].ShouldEqual("This is the value of foo");
			Get("/headers").Headers["BAR"].ShouldEqual("Bar is different");
		}

		[Test]
		public void can_request_with_query_strings_in_url() {
			Get("/info"        ).Body.ShouldNotContain("QueryString: foo = bar");
			Get("/info?foo=bar").Body.ShouldContain(   "QueryString: foo = bar");
			// TODO check for clean PATH_INFO
		}

		[Test]
		public void can_request_with_query_strings_as_object() {
			Get("/info", new { foo="bar" }).Body.ShouldContain("QueryString: foo = bar");
			Get("/info", new { foo="bar" }).Body.ShouldNotContain("QueryString: hi = there");
			Get("/info", new { foo="bar", hi="there" }).Body.ShouldContain("QueryString: hi = there");
			Get("/info", new { foo="bar", hi="there" }).Body.ShouldContain("QueryString: foo = bar");
			// TODO check for clean PATH_INFO
		}

		[Test]
		public void can_supply_query_strings_and_custom_headers() {
			Get("/info", new { QueryStrings = new {foo="bar"} }).Body.ShouldContain("QueryString: foo = bar");
			Get("/info", new { QueryStrings = new {foo="bar"} }).Body.ShouldNotContain("ABC = DEF");
			Get("/info", new { QueryStrings = new {foo="bar"}, Headers = new { ABC="DEF" } }).Body.ShouldContain("ABC = DEF");
			Get("/info", new { QueryStrings = new {foo="bar"}, Headers = new { ABC="DEF" } }).Body.ShouldContain("QueryString: foo = bar");
		}

		[Test]
		public void can_set_headers_before_calling_Get() {
			AddHeader("HELLO", "There");
			Headers["Foo"] = "Bar header value";

			Get("/info");

			LastResponse.Body.ShouldContain("HELLO = There");
			LastResponse.Body.ShouldContain("FOO = Bar header value"); // the spec server uppercases header keys
		}

		[Test]
		public void can_set_query_strings_before_calling_Get() {
			AddQueryString("Neat", "O");
		
			Get("/info");

			LastResponse.Body.ShouldContain("QueryString: Neat = O");
		}

		[Test]
		public void can_get_last_response() {
			var requestor = new Requestor(RootUrl);
			requestor.LastResponse.Should(Be.Null);

			requestor.Get("/");
			requestor.LastResponse.ShouldNot(Be.Null);
			requestor.LastResponse.Status.ShouldEqual(200);
			requestor.LastResponse.Body.ShouldEqual("Hello World");

			requestor.Get("/info");
			requestor.LastResponse.ShouldNot(Be.Null);
			requestor.LastResponse.Status.ShouldEqual(200);
			requestor.LastResponse.Body.ShouldContain("GET /info");
		}

		[Test]
		public void can_follow_redirect() {
			Get("/redirect");
			LastResponse.Status.ShouldEqual(302);
			LastResponse.Body.ShouldEqual("Redirecting");
			LastResponse.Headers.Keys.ShouldContain("Location");
			LastResponse.Headers["Location"].ShouldEqual("/info?redirected=true");

			FollowRedirect();
			LastResponse.Status.ShouldEqual(200);
			LastResponse.Body.ShouldContain("GET /info");
			LastResponse.Body.ShouldContain("QueryString: redirected = true");
			LastResponse.Headers.Keys.ShouldNotContain("Location");
		}

		class SimpleRequestor : IRequestor {
			public IResponse GetResponse(string verb, string url, IDictionary<string, string> postVariables, IDictionary<string, string> requestHeaders) {
				return new Response {
					Status  = 200,
					Body    = string.Format("You requested: {0} {1}", verb, url),
					Headers = new Dictionary<string, string>()
				};
			}
		}

		[Test]
		public void can_set_RootUrl_on_instance_or_globally() {
			var requestor = new Requestor();
			requestor.RootUrl.Should(Be.Null);

			// trying to do a GET to a relative path without a RootUrl will cause an exception
			var message = "HttpRequestor.GetResponse failed for: GET /foo.  This url generated a System.Net.FileWebRequest instead of a HttpWebRequest.";
			Should.Throw<Exception>(message, () => requestor.Get("/foo"));

			// if we switch to our SimpleRequestor, tho, we're OK
			Requestor.Global.Implementation = new SimpleRequestor();
			requestor.Get("/foo").Body.ShouldEqual("You requested: GET /foo"); // <-- no RootUrl

			// if we pass the full path, it works
			requestor.Get("http://localhost:3000/").Body.ShouldEqual("You requested: GET http://localhost:3000/");

			// set RootUrl globally
			Requestor.Global.RootUrl = "http://google.com";
			requestor.RootUrl.ShouldEqual("http://google.com");
			requestor.Get("/foo").Body.ShouldEqual("You requested: GET http://google.com/foo");

			// override on instance
			requestor.RootUrl = "http://yahoo.com";
			requestor.RootUrl.ShouldEqual("http://yahoo.com");
			requestor.Get("/foo").Body.ShouldEqual("You requested: GET http://yahoo.com/foo");
			new Requestor().Get("/foo").Body.ShouldEqual("You requested: GET http://google.com/foo"); // new still gets global

			// if we pass the full path, it still works, even with a RootUrl
			requestor.Get("http://localhost:3000/").Body.ShouldEqual("You requested: GET http://localhost:3000/");
		}

		class ExampleRequestor : IRequestor {
			public IResponse GetResponse(string verb, string url, IDictionary<string, string> postVariables, IDictionary<string, string> requestHeaders) {
				return new Response {
					Status  = 200,
					Body    = string.Format("{0}.GetResponse({1}, {2})", this.GetType().Name, verb, url),
					Headers = new Dictionary<string, string>()
				};
			}
		}

		class RequestorA : ExampleRequestor, IRequestor {}
		class RequestorB : ExampleRequestor, IRequestor {}
		class RequestorC : ExampleRequestor, IRequestor {}

		[Test]
		public void can_set_Implementation_on_instance_or_globally() {
			// global default
			Requestor.Global.Implementation = null;
			Requestor.DefaultIRequestor = typeof(RequestorA); // TODO move this to Requestor.Global, cause that's where everything else is ...
			new Requestor().Get("/").Body.ShouldEqual("RequestorA.GetResponse(GET, /)");

			// instance implementation
			var requestor = new Requestor();
			requestor.Implementation = new RequestorB();
			requestor.Get("/").Body.ShouldEqual("RequestorB.GetResponse(GET, /)");
			new Requestor().Get("/").Body.ShouldEqual("RequestorA.GetResponse(GET, /)"); // other instances still use A [the global default]

			// global implementation
			Requestor.Global.Implementation = new RequestorC();
			new Requestor().Get("/").Body.ShouldEqual("RequestorC.GetResponse(GET, /)");
			requestor = new Requestor();
			requestor.Get("/").Body.ShouldEqual("RequestorC.GetResponse(GET, /)");
			
			// override with instance implementation
			requestor.Implementation = new RequestorB(); // override global and set it back to B
			requestor.Get("/").Body.ShouldEqual("RequestorB.GetResponse(GET, /)");
			new Requestor().Get("/").Body.ShouldEqual("RequestorC.GetResponse(GET, /)"); // other instances still use C [the global implementation]
		}
    }
}
