using System;
using System.Collections.Generic;
using Machine.Specifications;
using Requestor;

namespace Requestor.Specs {

    [Subject(typeof(Requestor))][SetupForEachSpecification] // <--- todo Fork MSpec and make them fix this bullshit
    public class Put : Spec {
	Cleanup after_each =()=>
	    HttpRequestor.MethodVariable = "X-HTTP-Method-Override"; // reset to default

	It can_put_using_default_put_variable =()=>
	    Put("/info").Body.ShouldContain("POST Variable: X-HTTP-Method-Override = PUT");

	It can_put_using_default_put_variable_and_post_variables =()=> {
	    Put("/info", new { Foo = "Bar" });
	    LastResponse.Body.ShouldContain("PUT /");
	    LastResponse.Body.ShouldContain("POST Variable: X-HTTP-Method-Override = PUT");
	    LastResponse.Body.ShouldContain("POST Variable: Foo = Bar");
	};

	It can_put_using_custom_put_variable =()=> {
	    Put("/info").Body.ShouldContain("POST Variable: X-HTTP-Method-Override = PUT");
	    HttpRequestor.MethodVariable = "_method";
	    Put("/info").Body.ShouldNotContain("POST Variable: X-HTTP-Method-Override = PUT");
	    Put("/info").Body.ShouldContain("POST Variable: _method = PUT");
	};

	It can_disable_custom_put_variable =()=> {
	    Put("/info").Body.ShouldContain("POST Variable: X-HTTP-Method-Override =");
	    HttpRequestor.MethodVariable = null;
	    Put("/info").Body.ShouldNotContain("POST Variable: X-HTTP-Method-Override =");
	};
    }

    [Subject(typeof(Requestor))][SetupForEachSpecification]
    public class Delete : Spec {
	Cleanup after_each =()=>
	    HttpRequestor.MethodVariable = "X-HTTP-Method-Override"; // reset to default

	It can_delete_using_default_delete_variable =()=>
	    Delete("/info").Body.ShouldContain("POST Variable: X-HTTP-Method-Override = DELETE");

	It can_delete_using_default_delete_variable_and_post_variables =()=> {
	    Delete("/info", new { Foo = "Bar" });
	    LastResponse.Body.ShouldContain("DELETE /");
	    LastResponse.Body.ShouldContain("POST Variable: X-HTTP-Method-Override = DELETE");
	    LastResponse.Body.ShouldContain("POST Variable: Foo = Bar");
	};

	It can_delete_using_custom_delete_variable =()=> {
	    Delete("/info").Body.ShouldContain("POST Variable: X-HTTP-Method-Override = DELETE");
	    HttpRequestor.MethodVariable = "_method";
	    Delete("/info").Body.ShouldNotContain("POST Variable: X-HTTP-Method-Override = DELETE");
	    Delete("/info").Body.ShouldContain("POST Variable: _method = DELETE");
	};

	It can_disable_custom_delete_variable =()=> {
	    Delete("/info").Body.ShouldContain("POST Variable: X-HTTP-Method-Override =");
	    HttpRequestor.MethodVariable = null;
	    Delete("/info").Body.ShouldNotContain("POST Variable: X-HTTP-Method-Override =");
	};
    }
}
