using System;
using System.Collections.Generic;
using NUnit.Framework;
using Requestoring;

namespace Requestoring.Specs {

    [TestFixture]
    public class PutSpec : Spec {
		Before each =(s)=> HttpRequestor.MethodVariable = "X-HTTP-Method-Override";

		[Test]
		public void can_put_using_default_put_variable_and_post_variables() {
			Put("/info", new { Foo = "Bar" });
			LastResponse.Body.ShouldContain("PUT /");
			LastResponse.Body.ShouldContain("POST Variable: X-HTTP-Method-Override = PUT");
			LastResponse.Body.ShouldContain("POST Variable: Foo = Bar");
		}

		[Test]
		public void can_put_using_custom_put_variable() {
			Put("/info").Body.ShouldContain("POST Variable: X-HTTP-Method-Override = PUT");
			HttpRequestor.MethodVariable = "_method";
			Put("/info").Body.ShouldNotContain("POST Variable: X-HTTP-Method-Override = PUT");
			Put("/info").Body.ShouldContain("POST Variable: _method = PUT");
		}

		[Test]
		public void can_disable_custom_put_variable() {
			Put("/info").Body.ShouldContain("POST Variable: X-HTTP-Method-Override =");
			HttpRequestor.MethodVariable = null;
			Put("/info").Body.ShouldNotContain("POST Variable: X-HTTP-Method-Override =");
		}
	}

	[TestFixture]
	public class DeleteSpec : Spec {
		Before each =(s)=> HttpRequestor.MethodVariable = "X-HTTP-Method-Override";

		[Test]
		public void can_delete_using_default_delete_variable_and_post_variables() {
			Delete("/info", new { Foo = "Bar" });
			LastResponse.Body.ShouldContain("DELETE /");
			LastResponse.Body.ShouldContain("POST Variable: X-HTTP-Method-Override = DELETE");
			LastResponse.Body.ShouldContain("POST Variable: Foo = Bar");
		}

		[Test]
		public void can_delete_using_custom_delete_variable() {
			Delete("/info").Body.ShouldContain("POST Variable: X-HTTP-Method-Override = DELETE");
			HttpRequestor.MethodVariable = "_method";
			Delete("/info").Body.ShouldNotContain("POST Variable: X-HTTP-Method-Override = DELETE");
			Delete("/info").Body.ShouldContain("POST Variable: _method = DELETE");
		}

		[Test]
		public void can_disable_custom_delete_variable() {
			Delete("/info").Body.ShouldContain("POST Variable: X-HTTP-Method-Override =");
			HttpRequestor.MethodVariable = null;
			Delete("/info").Body.ShouldNotContain("POST Variable: X-HTTP-Method-Override =");
		}
    }
}
