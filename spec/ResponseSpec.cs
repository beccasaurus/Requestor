using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using Requestoring;

namespace Requestoring.Specs {

	[TestFixture]
	public class ResponseSpec : Spec {

		[Test]
		public void default_status_is_200() {
			new Response().Status.ShouldEqual(200);
		}

		[Test]
		public void default_body_is_empty() {
			new Response().Body.ShouldEqual("");
		}

		[Test]
		public void default_headers_are_empty() {
			new Response().Headers.Should(Be.Empty);
		}

		[Test]
		public void can_instantiate_with_body() {
			var response = new Response("Hi");
			response.Status.ShouldEqual(200);
			response.Body.ShouldEqual("Hi");
			response.Headers.Should(Be.Empty);
		}

		[Test]
		public void can_instantiate_with_status() {
			var response = new Response(400);
			response.Status.ShouldEqual(400);
			response.Body.ShouldEqual("");
			response.Headers.Should(Be.Empty);
		}

		[Test]
		public void can_instantiate_with_status_and_body() {
			var response = new Response(404, "Didn't find this");
			response.Status.ShouldEqual(404);
			response.Body.ShouldEqual("Didn't find this");
			response.Headers.Should(Be.Empty);
		}

		[Test]
		public void can_instantiate_with_status_and_body_and_headers() {
			var response = new Response(302, "Redirecting ...", new Dictionary<string,string> {{"Location","/foo"}});
			response.Status.ShouldEqual(302);
			response.Body.ShouldEqual("Redirecting ...");
			response.Headers.ShouldEqual(new Dictionary<string,string> {{"Location","/foo"}});
		}

		[Test]
		public void can_instantiate_with_status_and_headers() {
			var response = new Response(302, new Dictionary<string,string> {{"Location","/foo"}});
			response.Status.ShouldEqual(302);
			response.Body.ShouldEqual("");
			response.Headers.ShouldEqual(new Dictionary<string,string> {{"Location","/foo"}});
		}

		[Test]
		public void can_instantiate_with_body_and_headers() {
			var response = new Response("Hi there", new Dictionary<string,string> {{"Location","/foo"}});
			response.Status.ShouldEqual(200);
			response.Body.ShouldEqual("Hi there");
			response.Headers.ShouldEqual(new Dictionary<string,string> {{"Location","/foo"}});
		}

		string SavedResponse(string filename) {
			var specDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "spec", "saved-responses");
			return File.ReadAllText(Path.Combine(specDir, filename));
		}

		[Test]
		public void can_read_Curl_string_into_a_Response() {
			var response = Response.FromHttpResponse(SavedResponse("home"));
			response.Status.ShouldEqual(200);
			response.Body.ShouldEqual("Hello World");
			response.Headers.Count.ShouldEqual(5);
			response.Headers["Content-Type"].ShouldEqual("text/html");
			response.Headers["Content-Length"].ShouldEqual("11");
			response.Headers["Set-Cookie"].ShouldContain("rack.session=");
			response.Headers["Connection"].ShouldEqual("keep-alive");
			response.Headers.Keys.ShouldContain("Server"); // thin or webrick or whatever

			response = Response.FromHttpResponse(SavedResponse("redirect"));
			response.Status.ShouldEqual(302);
			response.Body.ShouldEqual("Redirecting");
			response.Headers.Count.ShouldEqual(6);
			response.Headers["Location"].ShouldEqual("/info?redirected=true");

			response = Response.FromHttpResponse(SavedResponse("headers"));
			response.Status.ShouldEqual(200);
			response.Body.ShouldEqual("This has custom headers FOO and BAR");
			response.Headers.Count.ShouldEqual(7);
			response.Headers["FOO"].ShouldEqual("This is the value of foo");
			response.Headers["BAR"].ShouldEqual("Bar is different");
		}
	}
}
