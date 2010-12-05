using System;
using System.Collections.Generic;
using Machine.Specifications;
using Requestor;

namespace Requestor.Specs {

    [Subject(typeof(Requestor))]
    public class Post : Spec {
	It can_post =()=> {
	    Post("/info").Body.ShouldContain("You did: POST /info");
	};

	It can_post_a_variable =()=> {
	    Post("/info").Body.ShouldNotContain("foo ... bar");
	    Post("/info", new { foo="bar" }).Body.ShouldContain("POST Variable: foo = bar");
	};

	It can_post_multiple_variables =()=> {
	    Post("/info", new { foo="bar"             }).Body.ShouldNotContain("POST Variable: hi = there");
	    Post("/info", new { foo="bar", hi="there" }).Body.ShouldContain("POST Variable: hi = there");
	    Post("/info", new { foo="bar", hi="there" }).Body.ShouldContain("POST Variable: foo = bar");
	};

	It can_post_and_supply_query_strings =()=> {
	    Post("/info", new { foo="bar", QueryStrings = new { hi="there" } });

	    LastResponse.Body.ShouldContain("POST Variable: foo = bar");
	    LastResponse.Body.ShouldContain("QueryString: hi = there");
	    LastResponse.Body.ShouldNotContain("POST Variable: hi = there");
	    LastResponse.Body.ShouldNotContain("QueryString: foo = bar");
	};

	It can_post_and_supply_query_strings_and_custom_headers =()=> {
	    Post("/info", new { 
		    foo = "bar", 
		    QueryStrings = new { hi = "there"      }, 
		    Headers      = new { CUSTOM = "header" }
	    });

	    var body = LastResponse.Body;

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
    }
}
