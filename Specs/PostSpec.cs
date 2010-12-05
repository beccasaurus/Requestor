using System;
using System.Collections.Generic;
using Machine.Specifications;
using Requestor;

using System.Reflection;
namespace Requestor.Specs {

    [Subject(typeof(Requestor))]
    public class Post {
	Establish context =()=> request = new Requestor("http://localhost:3000"); // TODO move url into a base class or something

	It can_post =()=> {
	    request.Post("/info").Body.ShouldContain("You did: POST /info");
	};

	It can_post_a_variable =()=> {
	    request.Post("/info").Body.ShouldNotContain("foo ... bar");
	    request.Post("/info", new { foo="bar" }).Body.ShouldContain("POST Variable: foo = bar");
	};

	It can_post_multiple_variables =()=> {
	    request.Post("/info", new { foo="bar"             }).Body.ShouldNotContain("POST Variable: hi = there");
	    request.Post("/info", new { foo="bar", hi="there" }).Body.ShouldContain("POST Variable: hi = there");
	    request.Post("/info", new { foo="bar", hi="there" }).Body.ShouldContain("POST Variable: foo = bar");
	};

	It can_post_and_supply_query_strings =()=> {
	    var body = request.Post("/info", new { foo="bar", QueryStrings = new { hi="there" } }).Body;
	    body.ShouldContain("POST Variable: foo = bar");
	    body.ShouldContain("QueryString: hi = there");
	    body.ShouldNotContain("POST Variable: hi = there");
	    body.ShouldNotContain("QueryString: foo = bar");
	};

	It can_post_and_supply_query_strings_and_custom_headers =()=> {
	    var body = request.Post("/info", new { 
		    foo = "bar", 
		    QueryStrings = new { hi = "there"      }, 
		    Headers      = new { CUSTOM = "header" }
	    }).Body;

	    body.ShouldContain(   "QueryString: hi = there");
	    body.ShouldNotContain("QueryString: foo = bar");
	    body.ShouldNotContain("QueryString: HTTP_CUSTOM = header");

	    body.ShouldNotContain("POST Variable: hi = there");
	    body.ShouldContain(   "POST Variable: foo = bar");
	    body.ShouldNotContain("POST Variable: HTTP_CUSTOM = header");

	    body.ShouldNotContain("Header: hi = there");
	    body.ShouldNotContain("Header: foo = bar");
	    body.ShouldContain(   "Header: HTTP_CUSTOM = header");
	};

	public static Requestor request;
    }
}
