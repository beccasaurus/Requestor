using System;
using System.Collections.Generic;
using Machine.Specifications;
using Requestoring;

namespace Requestoring.Specs {

    [Subject(typeof(Requestor))]
    public class Cookies : Spec {

	It can_enable_cookies =()=> {
	    var body = Get("/info").Body;
	    body.ShouldContain(   "Times requested: 1");
	    body.ShouldNotContain("Times requested: 2");

	    // try again ...
	    body = Get("/info").Body;
	    body.ShouldContain(   "Times requested: 1");
	    body.ShouldNotContain("Times requested: 2");

	    Instance.EnableCookies();

	    body = Get("/info").Body;
	    body.ShouldContain(   "Times requested: 1");
	    body.ShouldNotContain("Times requested: 2");

	    // second time with cookies
	    body = Get("/info").Body;
	    body.ShouldNotContain("Times requested: 1");
	    body.ShouldContain(   "Times requested: 2");
	    body.ShouldNotContain("Times requested: 3");

	    // third time with cookies
	    body = Get("/info").Body;
	    body.ShouldNotContain("Times requested: 1");
	    body.ShouldNotContain("Times requested: 2");
	    body.ShouldContain(   "Times requested: 3");

	    Instance.ResetCookies();

	    // back to the "first" time
	    body = Get("/info").Body;
	    body.ShouldContain(   "Times requested: 1");
	    body.ShouldNotContain("Times requested: 2");

	    // back to the "second" time
	    body = Get("/info").Body;
	    body.ShouldNotContain("Times requested: 1");
	    body.ShouldContain(   "Times requested: 2");
	    body.ShouldNotContain("Times requested: 3");

	    Instance.DisableCookies();

	    body = Get("/info").Body;
	    body.ShouldContain(   "Times requested: 1");
	    body.ShouldNotContain("Times requested: 2");

	    // try again ...
	    body = Get("/info").Body;
	    body.ShouldContain(   "Times requested: 1");
	    body.ShouldNotContain("Times requested: 2");
	};
    }
}
