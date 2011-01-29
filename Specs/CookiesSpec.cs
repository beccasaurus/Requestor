using System;
using System.Collections.Generic;
using NUnit.Framework;
using Requestoring;

namespace Requestoring.Specs {

	[TestFixture]
    public class CookiesSpec : Spec {
		Before each =(s)=> s.DisableCookies();

		[Test]
		public void can_enable_cookies() {
			var body = Get("/info").Body;
			body.ShouldContain(   "Times requested: 1");
			body.ShouldNotContain("Times requested: 2");

			// try again ...
			body = Get("/info").Body;
			body.ShouldContain(   "Times requested: 1");
			body.ShouldNotContain("Times requested: 2");

			EnableCookies();

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

			ResetCookies();

			// back to the "first" time
			body = Get("/info").Body;
			body.ShouldContain(   "Times requested: 1");
			body.ShouldNotContain("Times requested: 2");

			// back to the "second" time
			body = Get("/info").Body;
			body.ShouldNotContain("Times requested: 1");
			body.ShouldContain(   "Times requested: 2");
			body.ShouldNotContain("Times requested: 3");

			DisableCookies();

			body = Get("/info").Body;
			body.ShouldContain(   "Times requested: 1");
			body.ShouldNotContain("Times requested: 2");

			// try again ...
			body = Get("/info").Body;
			body.ShouldContain(   "Times requested: 1");
			body.ShouldNotContain("Times requested: 2");
		}
    }
}
