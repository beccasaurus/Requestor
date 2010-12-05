using System;
using System.Collections.Generic;
using Machine.Specifications;
using Requestor;

namespace Requestor.Specs {

    [Subject(typeof(Requestor))]
    public class Get : Spec {
	Establish context =()=> request = new Requestor(TestUrl);

	It can_get_response_body =()=> {
	    request.Get("/"        ).Body.ShouldEqual("Hello World");
	    request.Get("/info"    ).Body.ShouldContain("You did: GET /info");
	    request.Get("/boom"    ).Body.ShouldEqual("Boom!");
	    request.Get("/redirect").Body.ShouldEqual("Redirecting");
	    request.Get("/headers" ).Body.ShouldEqual("This has custom headers FOO and BAR");
	    request.Get("/notfound").Body.ShouldEqual("Not Found: GET /notfound");
	};

	It can_get_response_status =()=> {
	    request.Get("/"        ).Status.ShouldEqual(200);
	    request.Get("/info"    ).Status.ShouldEqual(200);
	    request.Get("/boom"    ).Status.ShouldEqual(500);
	    request.Get("/redirect").Status.ShouldEqual(302);
	    request.Get("/headers" ).Status.ShouldEqual(200);
	    request.Get("/notfound").Status.ShouldEqual(404);
	};

	It can_get_response_headers =()=> {
	    request.Get("/").Headers["Content-Type"].ShouldEqual("text/html");
	    request.Get("/").Headers.Keys.ShouldNotContain("FOO");
	    request.Get("/headers").Headers.Keys.ShouldContain("FOO");
	    request.Get("/headers").Headers["FOO"].ShouldEqual("This is the value of foo");
	    request.Get("/headers").Headers["BAR"].ShouldEqual("Bar is different");
	};

	It can_request_with_query_strings_in_url =()=> {
	    request.Get("/info"        ).Body.ShouldNotContain("QueryString: foo = bar");
	    request.Get("/info?foo=bar").Body.ShouldContain(   "QueryString: foo = bar");
	    // TODO check for clean PATH_INFO
	};

	It can_request_with_query_strings_as_object =()=> {
	    request.Get("/info", new { foo="bar" }).Body.ShouldContain("QueryString: foo = bar");
	    request.Get("/info", new { foo="bar" }).Body.ShouldNotContain("QueryString: hi = there");
	    request.Get("/info", new { foo="bar", hi="there" }).Body.ShouldContain("QueryString: hi = there");
	    request.Get("/info", new { foo="bar", hi="there" }).Body.ShouldContain("QueryString: foo = bar");
	    // TODO check for clean PATH_INFO
	};

	It can_supply_query_strings_and_custom_headers =()=> {
	    request.Get("/info", new { QueryStrings = new {foo="bar"} }).Body.ShouldContain("QueryString: foo = bar");
	    request.Get("/info", new { QueryStrings = new {foo="bar"} }).Body.ShouldNotContain("ABC = DEF");
	    request.Get("/info", new { QueryStrings = new {foo="bar"}, Headers = new { ABC="DEF" } }).Body.ShouldContain("ABC = DEF");
	    request.Get("/info", new { QueryStrings = new {foo="bar"}, Headers = new { ABC="DEF" } }).Body.ShouldContain("QueryString: foo = bar");
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
	    request.Get("/redirect");
	    request.LastResponse.Status.ShouldEqual(302);
	    request.LastResponse.Body.ShouldEqual("Redirecting");
	    request.LastResponse.Headers.Keys.ShouldContain("Location");
	    request.LastResponse.Headers["Location"].ShouldEqual("/info?redirected=true");

	    request.FollowRedirect();
	    request.LastResponse.Status.ShouldEqual(200);
	    request.LastResponse.Body.ShouldContain("GET /info");
	    request.LastResponse.Body.ShouldContain("QueryString: redirected = true");
	    request.LastResponse.Headers.Keys.ShouldNotContain("Location");
	};

	public static Requestor request;
    }
}
