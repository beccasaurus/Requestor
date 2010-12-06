using System;
using System.Collections.Generic;
using Machine.Specifications;
using Requestor;

namespace Requestor.Specs {

    [Subject(typeof(Requestor))]
    public class Put : Spec {
	Cleanup after_each =()=>
	    HttpRequestor.MethodVariable = "X-HTTP-Method-Override"; // reset to default

	It can_put_using_default_put_variable =()=>
	    Put("/info").Body.ShouldContain("POST Variable: X-HTTP-Method-Override = PUT");

	It can_put_using_default_put_variable_and_post_variables =()=> {
	    Put("/info", new { Foo = "Bar" }).Body.ShouldContain("POST Variable: X-HTTP-Method-Override = PUT");
	    Put("/info", new { Foo = "Bar" }).Body.ShouldContain("POST Variable: Foo = Bar");
	};

	It can_put_using_custom_put_variable =()=> {
	    Put("/info").Body.ShouldContain("POST Variable: X-HTTP-Method-Override = PUT");
	    HttpRequestor.MethodVariable = "_method";
	    Put("/info").Body.ShouldNotContain("POST Variable: X-HTTP-Method-Override = PUT");
	    Put("/info").Body.ShouldContain("POST Variable: _method = PUT");
	};
    }

    [Subject(typeof(Requestor))]
    public class Delete : Spec {

	It can_delete_using_default_delete_variable =()=>
	    Delete("/info").Body.ShouldContain("POST Variable: X-HTTP-Method-Override = DELETE");

	It can_delete_using_default_delete_variable_and_post_variables =()=> {
	    Delete("/info", new { Foo = "Bar" }).Body.ShouldContain("POST Variable: X-HTTP-Method-Override = DELETE");
	    Delete("/info", new { Foo = "Bar" }).Body.ShouldContain("POST Variable: Foo = Bar");
	};

	It can_delete_using_custom_delete_variable =()=> {
	    Delete("/info").Body.ShouldContain("POST Variable: X-HTTP-Method-Override = DELETE");
	    HttpRequestor.MethodVariable = "_method";
	    Delete("/info").Body.ShouldNotContain("POST Variable: X-HTTP-Method-Override = DELETE");
	    Delete("/info").Body.ShouldContain("POST Variable: _method = DELETE");
	};
    }
}
