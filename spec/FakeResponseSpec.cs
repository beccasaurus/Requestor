using System;
using System.Collections.Generic;
using NUnit.Framework;
using Requestoring;

namespace Requestoring.Specs {

    [TestFixture]
    public class FakeResponseSpec : Spec {

		[TestFixture]
		public class disabling_real_requests : FakeResponseSpec {

			[Test]
			public void can_disable_real_requests_for_an_instance_of_Requestor() {
				var requestor = new Requestor(RootUrl);
				requestor.Get("/").Body.ShouldEqual("Hello World");

				requestor.DisableRealRequests();
				Should.Throw<RealRequestsDisabledException>("Real requests are disabled. GET http://localhost:3000/", () => {
					requestor.Get("/");
				});

				requestor.EnableRealRequests();
				requestor.Get("/").Body.ShouldEqual("Hello World");
			}

			[Test]
			public void can_disable_real_requests_globally() {
				new Requestor(RootUrl).Get("/").Body.ShouldEqual("Hello World");

				Requestor.Global.DisableRealRequests();
				Should.Throw<RealRequestsDisabledException>("Real requests are disabled. GET http://localhost:3000/", () => {
					new Requestor(RootUrl).Get("/");
				});

				Requestor.Global.EnableRealRequests();
				new Requestor(RootUrl).Get("/").Body.ShouldEqual("Hello World");
			}

			[Test]
			public void can_override_for_an_instance() {
				new Requestor(RootUrl).Get("/").Body.ShouldEqual("Hello World");

				Requestor.Global.DisableRealRequests();
				var requestor = new Requestor(RootUrl);
				Should.Throw<RealRequestsDisabledException>("Real requests are disabled. GET http://localhost:3000/", () => {
					requestor.Get("/");
				});

				requestor.EnableRealRequests();
				requestor.Get("/").Body.ShouldEqual("Hello World");
			}
		}

		[Test]
		public void can_FakeResponse_with_an_IResponse_for_an_instance_of_Requestor() {
			var requestor = new Requestor(RootUrl);
			requestor.DisableRealRequests();
			Should.Throw<RealRequestsDisabledException>(() => requestor.Get("/"));

			requestor.FakeResponse("GET", "http://localhost:3000/", new Response {
				Status  = 400,
				Body    = "Something Blew Up",
				Headers = new Vars {{"Foo","Bar"}}
			});

			requestor.Get("/");
			requestor.LastResponse.Status.ShouldEqual(400);
			requestor.LastResponse.Body.ShouldEqual("Something Blew Up");
			requestor.LastResponse.Headers["Foo"].ShouldEqual("Bar");

			// works again, because no maximum number of requests was set ...
			requestor.Get("/");

			// blows up for any different url/path
			Should.Throw<RealRequestsDisabledException>("Real requests are disabled. GET http://localhost:3000/different", () => {
				requestor.Get("/different");
			});
		}

		[Test]
		public void can_FakeResponse_with_an_IResponse_globally() {
			var requestor = new Requestor(RootUrl);
			requestor.DisableRealRequests();
			Should.Throw<RealRequestsDisabledException>(() => requestor.Get("/"));

			Requestor.Global.FakeResponse("GET", "http://localhost:3000/", new Response {
				Status  = 400,
				Body    = "Something Global Blew Up",
				Headers = new Vars {{"Foo","Bar"}}
			});

			requestor.Get("/");
			requestor.LastResponse.Status.ShouldEqual(400);
			requestor.LastResponse.Body.ShouldEqual("Something Global Blew Up");
			requestor.LastResponse.Headers["Foo"].ShouldEqual("Bar");

			// works again, because no maximum number of requests was set ...
			requestor.Get("/");

			// blows up for any different url/path
			Should.Throw<RealRequestsDisabledException>("Real requests are disabled. GET http://localhost:3000/different", () => {
				requestor.Get("/different");
			});
		}

		[Test]
		public void FakeResponse_is_returned_even_if_real_requests_are_enabled() {
			var requestor = new Requestor(RootUrl);
			requestor.Get("/").Body.ShouldEqual("Hello World");
			requestor.Get("/foo").Body.ShouldEqual("Not Found: GET /foo");
			requestor.Get("/bar").Body.ShouldEqual("Not Found: GET /bar");

			// fake globally
			Requestor.Global.FakeResponse("GET", "http://localhost:3000/foo", new Response { Body = "Foo!" });
			requestor.Get("/").Body.ShouldEqual("Hello World");
			requestor.Get("/foo").Body.ShouldEqual("Foo!");
			requestor.Get("/bar").Body.ShouldEqual("Not Found: GET /bar");

			// override on instance
			requestor.FakeResponse("GET", "http://localhost:3000/foo", new Response { Body = "Foo via Instance" });
			requestor.Get("/").Body.ShouldEqual("Hello World");
			requestor.Get("/foo").Body.ShouldEqual("Foo via Instance");
			requestor.Get("/bar").Body.ShouldEqual("Not Found: GET /bar");
			new Requestor(RootUrl).Get("/foo").Body.ShouldEqual("Foo!"); // <--- new requestor uses the global fake response
			
			// fake another on instance
			requestor.FakeResponse("GET", "http://localhost:3000/bar", new Response { Body = "BAR" });
			requestor.Get("/").Body.ShouldEqual("Hello World");
			requestor.Get("/foo").Body.ShouldEqual("Foo via Instance");
			requestor.Get("/bar").Body.ShouldEqual("BAR");

			// if you disable real connections, the fake responses still work, but not the real ones ...
			requestor.DisableRealRequests();
			requestor.Get("/foo").Body.ShouldEqual("Foo via Instance");
			requestor.Get("/bar").Body.ShouldEqual("BAR");
			Should.Throw<RealRequestsDisabledException>("Real requests are disabled. GET http://localhost:3000/", () => {
				requestor.Get("/");
			});
		}

		[Test]
		public void can_specify_a_number_of_times_that_the_fake_response_should_be_returned() {
			var requestor = new Requestor(RootUrl);

			// once globally
			requestor.Get("/foo").Body.ShouldEqual("Not Found: GET /foo");
			Requestor.Global.FakeResponseOnce("GET", "http://localhost:3000/foo", new Response { Body = "Foo!" });
			requestor.Get("/foo").Body.ShouldEqual("Foo!");
			requestor.Get("/foo").Body.ShouldEqual("Not Found: GET /foo");

			// twice globally
			requestor.Get("/foo").Body.ShouldEqual("Not Found: GET /foo");
			Requestor.Global.FakeResponse(2, "GET", "http://localhost:3000/foo", new Response { Body = "Foo!" });
			requestor.Get("/foo").Body.ShouldEqual("Foo!");
			requestor.Get("/foo").Body.ShouldEqual("Foo!");
			requestor.Get("/foo").Body.ShouldEqual("Not Found: GET /foo");

			// once on instance
			requestor.Get("/foo").Body.ShouldEqual("Not Found: GET /foo");
			requestor.FakeResponseOnce("GET", "http://localhost:3000/foo", new Response { Body = "Foo!" });
			requestor.Get("/foo").Body.ShouldEqual("Foo!");
			requestor.Get("/foo").Body.ShouldEqual("Not Found: GET /foo");
			
			// 3 times on instance
			requestor.Get("/foo").Body.ShouldEqual("Not Found: GET /foo");
			requestor.FakeResponse(3, "GET", "http://localhost:3000/foo", new Response { Body = "Foo!" });
			requestor.Get("/foo").Body.ShouldEqual("Foo!");
			requestor.Get("/foo").Body.ShouldEqual("Foo!");
			requestor.Get("/foo").Body.ShouldEqual("Foo!");
			requestor.Get("/foo").Body.ShouldEqual("Not Found: GET /foo");

			// once globally, once on instance (both will happen)
			requestor.Get("/foo").Body.ShouldEqual("Not Found: GET /foo");
			Requestor.Global.FakeResponseOnce("GET", "http://localhost:3000/foo", new Response { Body = "Foo!" });
			requestor.FakeResponseOnce("GET", "http://localhost:3000/foo", new Response { Body = "Foo!" });
			requestor.Get("/foo").Body.ShouldEqual("Foo!");
			requestor.Get("/foo").Body.ShouldEqual("Foo!");
			requestor.Get("/foo").Body.ShouldEqual("Not Found: GET /foo");
		}
    }
}
