Requestor
=========

Requestor makes it easy to make HTTP GET/POST/PUT/DELETE requests.  This can help you consume RESTful web services or test your own.

Requestor is inspired by [Merb][]'s "request specs" and [rack-test][], which is used to test Ruby web applications.

An [effort is underway][owin] to bring a web server gateway interface to .NET web development, similar to [Rack][], [WSGI][], [JSGI], [PSGI][], etc.  
Once this is working, we will create an adapter (IRequestor) that can support this interface.  That will mean that, if you use Requestor, you can 
write tests that run against real HTTP *or* you can use Requestor to make "mock" requests against your ASP.NET web application (or any other .NET 
web framework).

At the moment, Requestor is just an abstraction layer around `System.Net.HttpWebRequest`.  Once a .NET web server gateway interface is working, I hope 
to be able to make my existing Requestor specs run against this new interface by simply swapping our the driver I'm using!

**NOTE**: The current focus of Requestor is on *testing*.

Download
--------

Latest version: 1.0.1.3

[Download .dll][]

[Browse Source][]

Usage
-----

The Requestor API is very simple.  It's based on the HTTP method you wish to perform on a given URL.

    using Requestoring; // silly namespace name, so you don't have to say Requestor.Requestor in your code

    var request  = new Requestor("http://localhost:1234"); // you can (optionally) pass a RootUrl, as we're doing here
    var response = request.Get("/");

Response is a Requestor.IResponse and it has:

 - `int` Status
 - `string` Body
 - `IDictionary<string,string>` Headers

For example:

    Console.WriteLine(response.Status);
    200

    Console.WriteLine(response.Body);
    Hello World!

    foeach (var header in response.Headers)
      Console.WriteLine("{0} is {1}", header.Key, header.Value);
    Content-Type is text/html
    Content-Length is 12
    Connection is keep-alive

### GET

You can easily pass query strings when doing a GET:

    // this will GET http://localhost:1234/?Foo=Bar&Hi=There
    Get("/", new { Foo = "Bar", Hi = "There" });

Ofcourse, you can build the path yourself:

    Get("/?Foo=Bar&Hi=There");

### POST

You can easily pass POST variables when doing a POST:

    // this will POST to / with Foo=Bar&Hi=There as POST data
    Post("/", new { Foo = "Bar", Hi = "There" });

You can post a simple string too:

    // this will POST to / with "I might be some JSON or XML" as POST data
    Post("/", "I might be some JSON or XML");

If you want to use query strings too, you can *explicitly* pass a group of query strings:

    // this will POST to /?query=string with Foo=Bar&Hi=There as POST data
    Post("/", new { Foo = "Bar", Hi = "There", QueryStrings = new { query = "string" }});

You could *explicitly* pass along the POST variable too, if you want to.  This will do the same thing as the example above:

    Post("/", new { 
        PostData     = new { Foo = "Bar", Hi = "There" }, 
        QueryStrings = new { query = "string"          }
    });

When you do a GET, we implicitly make all of the variables that you pass `QueryStrings`.

When you do a POST, we implicitly make all of the variables that you pass `PostData`.

### Custom Headers

You can always pass along custom headers by explicitly passing along `Headers` variables.

    Get("/", new { Headers = new { HTTP_FOO = "foo value", HTTP_BAR = "bar value" }});

`Headers` can be provided with or without `PostData` and/or `QueryStrings`

If you want to use a key for any of these variables that has non-alphanumeric characters, eg. `If-Modified-Since`, 
you cannot use a .NET anonymous type, as you cannot have a dash in a property name.

To solve this, you can manually pass in an `IDictionary<string, string>`:

    Get("/something", new { Headers = new Dictionary<string, string>{ {"If-Modified-Since", "Fri, 22 Oct 1999 12:08:38 GMT"} }});

That's pretty verbose and it can be difficult to see the keys/values, we you can use Requestor.Vars instead, which is an alias for `Dictionary<string, string>`:

    Get("/something", new { Headers = new Vars{ {"If-Modified-Since", "Fri, 22 Oct 1999 12:08:38 GMT"} }});

### PUT and DELETE

Put() and Delete() act just like Post().  Any variables passed are assumed to be `PostData`.  It's up to the `IRequestor` implementation that you're using 
how to handle PUT or DELETE requests.

Using the default backend that Requestor uses (`HttpRequestor`), doing a PUT or a DELETE will add a POST variable with the name `X-HTTP-Method-Override`.  
This should work with ASP.NET MVC.

If you need to change the name of this variable to something else (eg. Ruby on Rails uses `_method`), this is configurable:

    HttpRequestor.MethodVariable = "_method";

If you want to disable this and not POST any special variable:

    HttpRequestor.MethodVariable = null;

### Sessions / Cookies

By default, cookies are not used.  If you want to enable cookies:

    request.EnableCookies();

Cookies will be tracked for every request that the given instance of Requestor makes.  You can also reset the cookies or disable them again.

    request.ResetCookies();
    request.DisableCookies();

### LastResponse

To help make your tests more readable, we store the last response that a Requestor received.

    var request = new Requestor("http://localhost:1234");
    request.LastResponse; // NULL

    request.Get("/");
    request.LastResponse.Body;   // "Hello World"
    request.LastResponse.Status; // 200

### Following Redirects

Because Requestor is meant for low-level access to HTTP responses, we don't automatically follow redirects.  But we make it easy to follow one:

    var request = new Requestor("http://localhost:1234");
    request.Get("/old/path");

    request.LastResponse.Status;               // 302
    request.LastResponse.Headers["Location"];  // "/new/path"
    request.LastResponse.Body;                 // "Redirecting ..."

    request.FollowRedirect(); // will Get("/new/path");

    request.LastResponse.Status; // 200
    request.LastResponse.Body;   // "Welcome to the new path!"

### DSL

If you're writing tests, it creates a lot of noise if you're constantly calling Get(), LastResponse, etc on an instance of Requestor.

It's much easier if you have your test's base class inherit from Requestor:

    [TestFixture]
    public class MyTest : Requestor {

        [SetUp]
        public void Setup() {
            this.RootUrl = "http://localhost:1234";  // set the RootUrl property for our requests
            this.SetLastResponse(null);              // if you want to clear the last response
        }

        [Test]
        public void RootShouldRedirectToDashboard() {
       	    Get("/");
            Assert.That(LastResponse.Status, Is.EqualTo(302));
            Assert.That(LastResponse.Headers["Location"], Is.EqualTo("/dashboard"));

            FollowRedirect();
            Assert.That(LastResponse.Status, Is.EqualTo(200));
            Assert.That(LastResponse.Body, Is.StringContaining("My Dashboard"));
        }
    }

We also made a static, singleton instance of Requestor available as `Requestor.Static.Instance` to help you if 
your testing environment prefers static variables, like [MSpec][] does:

    [Subject("Getting Profile")]
    public class when_logged_in : Requestor.Static {

        Establish context =()=> Instance.RootUrl = "http://localhost:1234";

        It can_get_profile_using_user_id =()=> {
            Get("/profile", new { UserID = "5"; });
            Assert.That(LastResponse.Body.AsJson()["Name"], Is.EqualTo("Bob Smith"));       // AsJson isn't part of Requestor
            Assert.That(LastResponse.Body.AsJson()["Email"], Is.EqualTo("bob@smith.com"));
        };

    }

To see more examples, you can [browse Requestor's Specs][specs]

License
-------

Requestor is released under the MIT license.

TODO
----

 - *CRITICAL* add the ability to set all Headers that are restricted but may be set by properties
 - Add the ability to set default headers ... for different kinds of requests?  ... for all types of requests?
 - Set default Headers/PostData/QueryStrings that will go out on every request (unless set to null or overriden) - some web APIs will require an Auth token header for EVERY request.
 - Add (extension?) methods for easily sending/receiving JSON and XML data ... may or may not be useful.  I will add this if I find it to be useful.
 - Add some more specs to try testing edge cases.
 - Make the specs easy to run.  As it is now, you need to manually boot up the Ruby Rack application found in the Specs directory
 - Require .NET 4.0 or add extensions to extend for 4.0.  If we had Named Arguments, we could write cleaner code, eg. `Get("/", headers: new { ContentType="application/json" });`

[merb]: http://www.merbivore.com/
[rack-test]: https://github.com/brynary/rack-test
[owin]: http://groups.google.com/group/net-http-abstractions
[rack]: http://rack.rubyforge.org/
[wsgi]: http://wsgi.org/wsgi/
[jsgi]: http://jackjs.org/
[mspec]: https://github.com/machine/machine.specifications
[specs]: http://github.com/remi/Requestor/tree/master/Specs
[psgi]: http://plackperl.org/

[Download .dll]: http://github.com/remi/Requestor/raw/1.0.1.3/Build/Release/Requestor.dll
[Browse Source]: http://github.com/remi/Requestor/tree/1.0.1.3
