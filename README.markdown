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

**NOTE**: The current focus of Requestor is on *testing*, although it's also useful for things like manually calling REST APIs.

Download
--------

Latest version: 1.0.2.2

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

Or you can add query strings to the path you're about to request:

    AddQueryString("Foo", "Bar");
    Get("/");

Or you can manipulate the QueryStrings dictionary manually:

    QueryStrings["Foo"] = "Bar";
    Get("/");

### RootUrl and absolute Urls

You can specify a RootUrl to use for *all* requests make by all instances of Requestor, or you can specify a RootUrl for one instance:

    Requestor.Global.RootUrl = "http://google.com";

    // GET http://google.com/foo
    new Requestor().Get("/foo");

    var requestor = new Requestor("http://github.com/api/v2/json");

    // GET http://github.com/api/v2/json/user/show/remi
    requestor.Get("/user/show/remi");

    requestor.RootUrl = "http://different.com";

    // GET http://different.com/user/show/remi
    requestor.Get("/user/show/remi");

    // GET http://remi.org
    requestor.Get("http://remi.org"); // even though a RootUrl is specified, it won't be used because we specified an absolute Url

### POST

You can easily pass POST variables when doing a POST:

    // this will POST to / with Foo=Bar&Hi=There as POST data
    Post("/", new { Foo = "Bar", Hi = "There" });

You can post a simple string too:

    // this will POST to / with "I might be some JSON or XML" as POST data
    Post("/", "I might be some JSON or XML");

Just like Query strings, you can use call `AddPostData(key, value)` or `SetPostData(string)` or you can manipulate the `PostData` dictionary:

    AddPostData("Foo", "Bar");
    AddPostData("Hi", "There");
    Post("/");

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

Alternatively, you can use the methods/dictionaries for setting QueryStrings, PostData, and Headers for the request you're about to make.

### Dynamic HTTP Request Method

Calling `Get("/foo")` is really just a helpful shortcut for calling `Request("GET", "/foo")` so, if you need to set the HTTP Method dynamically, you can use `Request()`

### Custom Headers

You can always pass along custom headers by explicitly passing along `Headers` variables.

    Get("/", new { Headers = new { HTTP_FOO = "foo value", HTTP_BAR = "bar value" }});

`Headers` can be provided with or without `PostData` and/or `QueryStrings`

If you want to use a key for any of these variables that has non-alphanumeric characters, eg. `If-Modified-Since`, 
you cannot use a .NET anonymous type, as you cannot have a dash in a property name.

To solve this, you can manually pass in an `IDictionary<string, string>`:

    Get("/something", new { Headers = new Dictionary<string, string>{ {"If-Modified-Since", "Fri, 22 Oct 1999 12:08:38 GMT"} }});

That's pretty verbose and it can be difficult to see the keys/values, we you can use Requestoring.Vars instead, which is an alias for `Dictionary<string, string>`:

    Get("/something", new { Headers = new Vars{ {"If-Modified-Since", "Fri, 22 Oct 1999 12:08:38 GMT"} }});

Or you can use `AddHeader` or manipulate the `Headers` dictionary before making your request:

    AddHeader("If-Modified-Since", "Fri, 22 Oct 1999 12:08:38 GMT");
    Get("/something");

### Default Headers

There may be some headers that you want to send with EVERY request that you make with a Requestor instance.

You can add any headers you want to Requestor.DefaultHeaders and they will be sent with EVERY request that Requestor makes:

    DefaultHeaders["Content-Type"] = "application/json";

    Get("/dogs.json"); // will do a GET to /dogs.json with Content-Type=application/json

    AddHeader("Foo", "Bar");
    Get("/dogs.json"); // will do a GET to /dogs.json with Content-Type=application/json and Foo=Bar

    Get("/dogs.json"); // will do a GET to /dogs.json with Content-Type=application/json ... the Foo header is only used for *1* request

You can set default headers globally, too, so they are used for *all* Requestor instances:

    Requestor.Global.DefaultHeaders["Content-Type"] = "application/json";

### PUT and DELETE

Put() and Delete() act just like Post() besides the HTTP method that they use.  Any variables passed are assumed to be `PostData`. 
It's up to the `IRequestor` implementation that you're using how to handle PUT or DELETE requests.

Using the default backend that Requestor uses (`HttpRequestor`), doing a PUT or a DELETE will POST an additionary variable with the value of the HTTP method 
being performed if HttpRequestor.MethodVariable is set.

If you set the name of this variable, this is configurable:

    // When doing a PUT, we will pass X-HTTP-Method-Override=PUT
    HttpRequestor.MethodVariable = "X-HTTP-Method-Override"; // ASP.NET

    // When doing a PUT, we will pass _method=PUT
    HttpRequestor.MethodVariable = "_method"; // Ruby on Rails

    // When doing a PUT, we will not pass along any additional POST parameter
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

Because Requestor is meant for low-level access to HTTP responses, we don't automatically follow redirects by default.  But we make it easy to follow one:

    var request = new Requestor("http://localhost:1234");
    request.Get("/old/path");

    request.LastResponse.Status;               // 302
    request.LastResponse.Headers["Location"];  // "/new/path"
    request.LastResponse.Body;                 // "Redirecting ..."

    request.FollowRedirect(); // will Get("/new/path");

    request.LastResponse.Status; // 200
    request.LastResponse.Body;   // "Welcome to the new path!"

If you want to automatically follow redirects:

    Requestor.Global.AutoRedirect = true;    // global

    myRequestor.AutoRedirect = true;         // just for this instance

### Faking Requests

If you're using Requestor to make calls against a REST API, it's likely that you'll want to be able to test your code *without* making any real HTTP requests.

Requestor has a built-in mechanism for providing mock responses that will be returned instead of making real requests.

If you have a reference to the Requestor that your code uses, you can:

    // Let's assume that your requestor's RootUrl is http://google.com

    myRequestor.FakeResponse("GET", "http://google.com/foo", new Response {
        Status  = 200,
        Body    = "You requested http://google.com/foo",
        Headers = new Dictionary<string,string>()
    });

    // Now, whenever you make a request that matches, the Response you provided will be returned instead
    myRequestor.Get("/foo"); // <--- this will not make a real request, it will just return your provided response

    // If you request a url that doesn't match, however, it will make the real request
    myRequestor.Get("/foo?q=someQuery"); // <--- this will make a real request

If you don't have a reference to the Requestor instance, that's fine.  You can call `Requestor.Global.FakeResponse()` instead and 
your response will be used for *all* Requestor instances.

`FakeResponse` can take any `Requestoring.IResponse`.  If you use a `new Response()` (our default implementaiton), it has useful defaults 
so you don't have to specify Status and Body and Headers if you don't want to

    Requestor.Global.FakeResponse("GET", "http://www.google.com", new Response());

    var response = myRequestor.Get("http://www.google.com");
    
    // response.Status  will be 200
    // response.Body    will be ""
    // response.Headers will be an empty Dictionary<string,string>

#### Loading saved responses

An easy way to record HTTP responses and then use them as fake responses in Requestor is to use curl:

    $ curl -i http://google.com/ > google_home_page

This saves the Status, Headers, and Body of the request.

You can easily load this file into a Response with `Response.FromHttpResponse`:

    Requestor.Global.FakeResponse("GET", "http://www.google.com/", Response.FromHttpResponse(ReadFile("google_home_page")));

    new Requestor().Get("http://www.google.com/"); // gives you the full response, including headers

#### Limiting the number of times your response will be used

If you want to make sure that your fake response is only used once (or N number of times):

    >> myRequestor.Get("/");
    => "Hello from the real web site"

    >> myRequestor.FakeResponseOnce("GET", "http://www.google.com/", new Response("Faked!"));

    >> myRequestor.Get("/").Body
    >> "Faked!"

    >> myRequestor.Get("/");
    => "Hello from the real web site"   // <--- the fake response was only used once

To have your rake response returned N times, you can call `FakeResponse(5, ...)`

By default, calling `FakeResponse()` without a number will allow your response to be returned indefinitely.

If you want to clear all of the fake responses (for instance, before each of your tests):

    Requestor.Global.FakeResponses.Clear(); // clears all globally registered responses

    myRequestor.FakeResponses.Clear(); // clears responses for this requestor

#### Disabling Real Requests

When testing, it's often useful to disable real HTTP requests entirely:

    Requestor.Global.DisableRealRequests(); // disable requests for all Requestor instances

    myRequestor.DisableRealRequests(); // will only disable requests for this instance

    myRequestor.Get("http://www.google.com"); // if you haven't provided a FakeResponse for this, it'll throw an exception:

    Requestoring.RealRequestsDisabledException: Real requests are disabled. GET http://www.google.com
      at Requestoring.Requestor.Request (System.String method, System.String path, Requestoring.RequestInfo info) [0x00000] in <filename unknown>:0 

The message of the exception thrown included the Url that was being requested so you can easily register a new FakeResponse for that Url.

**NOTE**: `DisableRealRequests()` prevents the `Requestor.Implementation` (eg. `HttpRequestor`) from being called at *all* so it will work with any `IRequestor`

#### Manually Faking Requests

If the built-in mechanisms for faking requests don't work for you for some reason, the easiest way to fake requests for your tests is to implement your 
own `IRequestor` and use that, instead of the default `HttpRequestor`, for your tests.  It's a lot easier than it sounds.  `IRequestor` only has 1 method!

    public class MyMockWebApi : IRequestor {

        // This is the *only* method required for implementing your own IRequestor
        //
        // Let's say that your code makes calls to urls like http://www.somesite.com/dogs.json and you want to 
        // catch all of these and read dogs.json (or cats.json) off of the file system and return it, instead of making 
        // real requests to the website.
        //
        IResponse GetResponse(string verb, string url, IDictionary<string, string> postVariables, IDictionary<string, string> requestHeaders) {
            var jsonDirectory = Path.Combine(Directory.GetCurrentDirectory(), "example-json");
            var jsonFilePath  = Path.Combine(jsonDirectory, new Uri(url).AbsolutePath); // this would be something like example-json/dogs.json

            // when testing, it's useful to throw exceptions with useful messages when your mocking doesn't yet support something your code is trying to do
            if (! File.Exists(jsonFilePath))
                throw new Exception("Tried to {0} {1} and couldn't find json file to return: {2}", verb, url, jsonFilePath);

            return new Response {
                Body    = File.ReadAllText(jsonFilePath),
                Headers = new Dictionary<string,string> {{"Content-Type", "application/json"}};
            };
        }

    }

    [TestFixture]
    public class GettingDogs {

        [SetUp]
        public void BeforeEach() {
            // Override the Requestor implementation that your code uses so it uses your IRequestor instead of HttpRequestor
            Requestor.Global.Implementation = new MyMockWebApi();
        }

        [Test]
        public void ExampleTest() {
            // Assuming your application uses Requestor, it will return the sample json instead of doing real HTTP requests!
            var dogs = MyApplication.GetDogs();    
            dogs.Count.ShouldEqual(5);
            dogs.First.Name.ShouldEqual("Rover");
        }

        [Test]
        public void DifferentStyle() {
            // Reading from the file system is nice for large example data but, if you're dealing with little snippets of 
            // JSON then it might be useful to write your IRequestor to support something like this:

            MyMockWebApi.JsonSamples.Add("/cats.json", @"[{""Name"":""Mittens""},{""Name"":""Tom""}]");

            var cats = MyApplication.GetCats();
            cats.Count.ShouldEqual(2);
            cats.First.Name.ShouldEqual("Mittens");
            cats.Last.Name.ShouldEquel("Tom");
        }
    }

That's all there is to it!

If your tests use cookies, its recommended that your `IRequestor` implement `Requestoring.IHaveCookies`. 
If you don't, then calls to these Requestor methods won't work: `EnableCookies, DisableCookies, ResetCookies`

### DSL

If you're writing tests, it creates a lot of noise if you're constantly calling Get(), LastResponse, etc on an instance of Requestor.

It's much easier if you have your test's base class inherit from Requestor:

    [TestFixture]
    public class MyTest : Requestor {

        [SetUp]
        public void Setup() {
            Clear();                            // clears LastResponse, DefaultHeaders, FakeResponses, etc
            RootUrl = "http://localhost:1234";  // set the RootUrl property for our requests
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

To see more examples, you can [browse Requestor's Specs][specs]

License
-------

Requestor is released under the MIT license.


[merb]: http://www.merbivore.com/
[rack-test]: https://github.com/brynary/rack-test
[owin]: http://groups.google.com/group/net-http-abstractions
[rack]: http://rack.rubyforge.org/
[wsgi]: http://wsgi.org/wsgi/
[jsgi]: http://jackjs.org/
[mspec]: https://github.com/machine/machine.specifications
[specs]: http://github.com/remi/Requestor/tree/master/spec
[psgi]: http://plackperl.org/

[Download .dll]: http://github.com/remi/Requestor/raw/1.0.2.2/bin/Release/Requestor.dll
[Browse Source]: http://github.com/remi/Requestor/tree/1.0.2.2
