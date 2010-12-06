using System;
using System.Collections.Generic;
using Machine.Specifications;
using Requestoring;

namespace Requestoring.Specs {

    [Subject(typeof(Requestor))][SetupForEachSpecification]
    public class SettingHeaders : Spec {
	Cleanup after_each =()=> DefaultHeaders.Clear();

	It can_set_default_values_for_headers =()=> {
	    Get("/info").Body.ShouldNotContain("Header: CONTENT_TYPE = application/json");

	    DefaultHeaders.Add("Content-Type", "application/json");
	    Get("/info").Body.ShouldContain("Header: CONTENT_TYPE = application/json");

	    DefaultHeaders.Remove("Content-Type");
	    Get("/info").Body.ShouldNotContain("Header: CONTENT_TYPE = application/json");
	};

	It can_override_default_values_for_headers =()=> {
	    DefaultHeaders.Add("FOO", "bar");
	    DefaultHeaders.Add("HI",  "there");
	    Get("/info").Body.ShouldContain("Header: HTTP_FOO = bar");
	    Get("/info").Body.ShouldContain("Header: HTTP_HI = there");
	    Get("/info").Body.ShouldNotContain("Header: HTTP_FOO = overriden");
	    
	    // HI's default value is still passed along but FOO get overriden
	    Get("/info", new { Headers = new { FOO="overriden!" }}).Body.ShouldContain("Header: HTTP_FOO = overriden!");
	    Get("/info", new { Headers = new { FOO="overriden!" }}).Body.ShouldContain("Header: HTTP_HI = there");
	};

	It can_set_content_type =()=> {
	    Get("/info"                                                                    ).Body.ShouldNotContain("Header: CONTENT_TYPE = application/json");
	    Get("/info", new { Headers = new Vars {{ "Content-Type", "application/json" }}}).Body.ShouldContain(   "Header: CONTENT_TYPE = application/json");
	    Get("/info", new { Headers = new { ContentType = "application/json"          }}).Body.ShouldContain(   "Header: CONTENT_TYPE = application/json");
	};

	It can_set_user_agent =()=> {
	    Get("/info"                                                        ).Body.ShouldNotContain("Header: HTTP_USER_AGENT = My App");
	    Get("/info", new { Headers = new Vars {{ "User-Agent", "My App" }}}).Body.ShouldContain(   "Header: HTTP_USER_AGENT = My App");
	    Get("/info", new { Headers = new { UserAgent = "My App"          }}).Body.ShouldContain(   "Header: HTTP_USER_AGENT = My App");
	};

	It can_set_accept;
    }
}
