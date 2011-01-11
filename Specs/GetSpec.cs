using System;
using System.Collections.Generic;
using Machine.Specifications;
using Requestoring;

namespace Requestoring.Specs {

    [Subject(typeof(Requestor))]
    public class Get : Spec {

		It can_get_response_body =()=> {
			Get("/"        ).Body.ShouldEqual("Hello World");
			Get("/info"    ).Body.ShouldContain("You did: GET /info");
			Get("/boom"    ).Body.ShouldEqual("Boom!");
			Get("/redirect").Body.ShouldEqual("Redirecting");
			Get("/headers" ).Body.ShouldEqual("This has custom headers FOO and BAR");
			Get("/notfound").Body.ShouldEqual("Not Found: GET /notfound");
		};

		It can_get_response_status =()=> {
			Get("/"        ).Status.ShouldEqual(200);
			Get("/info"    ).Status.ShouldEqual(200);
			Get("/boom"    ).Status.ShouldEqual(500);
			Get("/redirect").Status.ShouldEqual(302);
			Get("/headers" ).Status.ShouldEqual(200);
			Get("/notfound").Status.ShouldEqual(404);
		};

		It can_get_response_headers =()=> {
			Get("/").Headers["Content-Type"].ShouldEqual("text/html");
			Get("/").Headers.Keys.ShouldNotContain("FOO");
			Get("/headers").Headers.Keys.ShouldContain("FOO");
			Get("/headers").Headers["FOO"].ShouldEqual("This is the value of foo");
			Get("/headers").Headers["BAR"].ShouldEqual("Bar is different");
		};

		It can_request_with_query_strings_in_url =()=> {
			Get("/info"        ).Body.ShouldNotContain("QueryString: foo = bar");
			Get("/info?foo=bar").Body.ShouldContain(   "QueryString: foo = bar");
			// TODO check for clean PATH_INFO
		};

		It can_request_with_query_strings_as_object =()=> {
			Get("/info", new { foo="bar" }).Body.ShouldContain("QueryString: foo = bar");
			Get("/info", new { foo="bar" }).Body.ShouldNotContain("QueryString: hi = there");
			Get("/info", new { foo="bar", hi="there" }).Body.ShouldContain("QueryString: hi = there");
			Get("/info", new { foo="bar", hi="there" }).Body.ShouldContain("QueryString: foo = bar");
			// TODO check for clean PATH_INFO
		};

		It can_supply_query_strings_and_custom_headers =()=> {
			Get("/info", new { QueryStrings = new {foo="bar"} }).Body.ShouldContain("QueryString: foo = bar");
			Get("/info", new { QueryStrings = new {foo="bar"} }).Body.ShouldNotContain("ABC = DEF");
			Get("/info", new { QueryStrings = new {foo="bar"}, Headers = new { ABC="DEF" } }).Body.ShouldContain("ABC = DEF");
			Get("/info", new { QueryStrings = new {foo="bar"}, Headers = new { ABC="DEF" } }).Body.ShouldContain("QueryString: foo = bar");
		};

		It can_set_headers_before_calling_Get =()=> {
			AddHeader("HELLO", "There");
			Headers["Foo"] = "Bar header value";

			Get("/info");

			LastResponse.Body.ShouldContain("HELLO = There");
			LastResponse.Body.ShouldContain("FOO = Bar header value"); // the spec server uppercases header keys
		};

		It can_set_query_strings_before_calling_Get =()=> {
			AddQueryString("Neat", "O");
		
			Get("/info");

			LastResponse.Body.ShouldContain("QueryString: Neat = O");
		};

		It can_get_last_response =()=> {
			var requestor = new Requestor(TestUrl);
			requestor.LastResponse.ShouldBeNull();

			requestor.Get("/");
			requestor.LastResponse.ShouldNotBeNull();
			requestor.LastResponse.Status.ShouldEqual(200);
			requestor.LastResponse.Body.ShouldEqual("Hello World");

			requestor.Get("/info");
			requestor.LastResponse.ShouldNotBeNull();
			requestor.LastResponse.Status.ShouldEqual(200);
			requestor.LastResponse.Body.ShouldContain("GET /info");
		};

		It can_follow_redirect =()=> {
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
		};
    }
}
