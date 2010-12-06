using System;
using System.Collections.Generic;
using Machine.Specifications;
using Requestoring;

namespace Requestoring.Specs {

    [Subject(typeof(Requestor))]
    public class SettingHeaders : Spec {

	It can_set_default_values_for_headers;

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
