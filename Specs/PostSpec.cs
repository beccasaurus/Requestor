using System;
using System.Collections.Generic;
using Machine.Specifications;
using Requestor;

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

	It can_post_and_supply_query_strings;

	public static Requestor request;
    }
}
