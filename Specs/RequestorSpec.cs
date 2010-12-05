using System;
using System.Collections.Generic;
using Machine.Specifications;
using Requestor;

namespace Requestor.Specs {

    [Subject("Requestor")]
    public class GET {
	Establish context =()=>
	    response = new Requestor("http://localhost:4567").Get("/");

	It LastResponse_Body =()=>
	    response.Body.ShouldEqual("You did: GET /");

	It LastResponse_Status =()=>
	    response.Status.ShouldEqual(200);

	It LastResponse_Headers =()=>
	    response.Headers["Content-Type"].ShouldContain("text/html");

	public static IResponse response;
    }
}
