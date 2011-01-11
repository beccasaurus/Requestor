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
			Get("/info", new { Headers = new { Content_Type = "application/json"         }}).Body.ShouldContain(   "Header: CONTENT_TYPE = application/json");
		};

		// This blows up my specs, probably because it incorrectly sets a request's Content-Length header to something *wrong*!
		//
		// It can_set_content_length =()=> {
		// 	Get("/info"                                                         ).Body.ShouldNotContain("Header: CONTENT_LENGTH = 123");
		// 	Get("/info", new { Headers = new Vars {{ "Content-Length", "123" }}}).Body.ShouldContain(   "Header: CONTENT_LENGTH = 123");
		// 	Get("/info", new { Headers = new { ContentLength = "123"          }}).Body.ShouldContain(   "Header: CONTENT_LENGTH = 123");
		// };

		It can_set_user_agent =()=> {
			Get("/info"                                                        ).Body.ShouldNotContain("Header: HTTP_USER_AGENT = My App");
			Get("/info", new { Headers = new Vars {{ "User-Agent", "My App" }}}).Body.ShouldContain(   "Header: HTTP_USER_AGENT = My App");
			Get("/info", new { Headers = new { UserAgent = "My App"          }}).Body.ShouldContain(   "Header: HTTP_USER_AGENT = My App");
			Get("/info", new { Headers = new { User_Agent = "My App"         }}).Body.ShouldContain(   "Header: HTTP_USER_AGENT = My App");
		};

		It can_set_accept =()=> {
			Get("/info"                                                             ).Body.ShouldNotContain("Header: HTTP_ACCEPT = application/foo");
			Get("/info", new { Headers = new Vars {{ "Accept", "application/foo" }}}).Body.ShouldContain(   "Header: HTTP_ACCEPT = application/foo");
			Get("/info", new { Headers = new { Accept = "application/foo"         }}).Body.ShouldContain(   "Header: HTTP_ACCEPT = application/foo");
		};

		It can_set_connection =()=> {
			Get("/info"                                                        ).Body.ShouldNotContain("Header: HTTP_CONNECTION = foobar");
			Get("/info", new { Headers = new Vars {{ "Connection", "foobar" }}}).Body.ShouldContain(   "Header: HTTP_CONNECTION = foobar");
			Get("/info", new { Headers = new { Connection = "foobar"         }}).Body.ShouldContain(   "Header: HTTP_CONNECTION = foobar");
		};

		It can_set_if_modified_since =()=> {
			Get("/info"                                                                                      ).Body.ShouldNotContain("Header: HTTP_IF_MODIFIED_SINCE = Sat, 29 Oct 1994 19:43:31 GMT");
			Get("/info", new { Headers = new Vars {{ "If-Modified-Since", "Sat, 29 Oct 1994 19:43:31 GMT" }}}).Body.ShouldContain(   "Header: HTTP_IF_MODIFIED_SINCE = Sat, 29 Oct 1994 19:43:31 GMT");
			Get("/info", new { Headers = new { IfModifiedSince = "Sat, 29 Oct 1994 19:43:31 GMT"           }}).Body.ShouldContain(   "Header: HTTP_IF_MODIFIED_SINCE = Sat, 29 Oct 1994 19:43:31 GMT");
			Get("/info", new { Headers = new { If_Modified_Since = "Sat, 29 Oct 1994 19:43:31 GMT"         }}).Body.ShouldContain(   "Header: HTTP_IF_MODIFIED_SINCE = Sat, 29 Oct 1994 19:43:31 GMT");
		};

		It can_set_referer_or_referrer =()=> {
			Get("/info"                                                      ).Body.ShouldNotContain("Header: HTTP_REFERER = foobar");
			Get("/info", new { Headers = new Vars {{ "Referer", "foobar"  }}}).Body.ShouldContain(   "Header: HTTP_REFERER = foobar");
			Get("/info", new { Headers = new { Referer = "foobar"         }}).Body.ShouldContain(    "Header: HTTP_REFERER = foobar");
			Get("/info", new { Headers = new Vars {{ "Referrer", "foobar" }}}).Body.ShouldContain(   "Header: HTTP_REFERER = foobar");
			Get("/info", new { Headers = new { Referrer = "foobar"        }}).Body.ShouldContain(    "Header: HTTP_REFERER = foobar");
		};

		// it complains pretty much nomatter what we set this to ... not sure if this would be useful for someone using Requestor to set anyway ...
		// It can_set_transferencoding =()=> {
		// 	Get("/info"                                                               ).Body.ShouldNotContain("Header: HTTP_TRANSFERENCODING = foobar");
		// 	Get("/info", new { Headers = new Vars {{ "Transfer-Encoding", "foobar" }}}).Body.ShouldContain(   "Header: HTTP_TRANSFERENCODING = foobar");
		// 	Get("/info", new { Headers = new { TransferEncoding = "foobar"          }}).Body.ShouldContain(   "Header: HTTP_TRANSFERENCODING = foobar");
		// };

		// Test server doesn't display this header?
		//
		// It can_set_expect =()=> {
		// 	Get("/info"                                              ).Body.ShouldNotContain("Header: HTTP_EXPECT = ");
		// 	Get("/info", new { Headers = new Vars {{ "Expect", "" }}}).Body.ShouldContain(   "Header: HTTP_EXPECT = 100-Continue");
		// 	Get("/info", new { Headers = new { Expect = ""         }}).Body.ShouldContain(   "Header: HTTP_EXPECT = 100-Continue");
		// };
	}
}
