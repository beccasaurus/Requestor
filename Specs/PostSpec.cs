using System;
using System.Collections.Generic;
using Machine.Specifications;
using Requestoring;

namespace Requestoring.Specs {

    [Subject(typeof(Requestor))]
    public class Post : Spec {

		It can_post =()=> {
			Post("/info").Body.ShouldContain("You did: POST /info");
		};

		It can_post_a_variable =()=> {
			Post("/info").Body.ShouldNotContain("foo ... bar");
			Post("/info", new { foo="bar" }).Body.ShouldContain("POST Variable: foo = bar");
		};

		It can_post_a_string =()=> {
			var json = "{\"Name\":\"Lander\",\"Breed\":\"APBT\"}";
			Post("/info", json).Body.ShouldContain("POST Variable: " + json + " = "); // data shows up as a key with no value on the server
		};

		It can_post_a_string_with_a_particular_content_type =()=> {
			Post("/info", new { PostData = "some data" });
			LastResponse.Body.ShouldContain("Header: CONTENT_TYPE = application/x-www-form-urlencoded"); // default

			Post("/info", new { PostData = "some data", Headers = new { ContentType = "application/json" }});
			LastResponse.Body.ShouldNotContain("Header: CONTENT_TYPE = application/x-www-form-urlencoded");
			LastResponse.Body.ShouldContain(   "Header: CONTENT_TYPE = application/json");
		};

		It can_add_post_data_before_doing_post =()=> {
			AddPostData("this", "that");

			Post("/info", new { more = "data" });

			LastResponse.Body.ShouldContain("POST Variable: this = that");
			LastResponse.Body.ShouldContain("POST Variable: more = data");
		};

		It can_set_post_data_to_string_before_doing_post =()=> {
			var json = "{\"Name\":\"Lander\",\"Breed\":\"APBT\"}";

			SetPostData(json);

			Post("/info").Body.ShouldContain("POST Variable: " + json + " = "); // data shows up as a key with no value on the server
		};

		It can_post_multiple_variables =()=> {
			Post("/info", new { foo="bar"             }).Body.ShouldNotContain("POST Variable: hi = there");
			Post("/info", new { foo="bar", hi="there" }).Body.ShouldContain(   "POST Variable: hi = there");
			Post("/info", new { foo="bar", hi="there" }).Body.ShouldContain(   "POST Variable: foo = bar");
		};

		It can_post_variable_with_nonalphanumeric_characters_in_the_name_using_a_Dictionary =()=> {
			Post("/info", new Dictionary<string,string>{{"foo-bar", "hello"}, {"!NEAT*", "w00t"}}).Body.ShouldContain("POST Variable: foo-bar = hello");
			Post("/info", new Dictionary<string,string>{{"foo-bar", "hello"}, {"!NEAT*", "w00t"}}).Body.ShouldContain("POST Variable: !NEAT* = w00t");
		};

		It can_post_variable_with_nonalphanumeric_characters_in_the_name_using_Vars_which_is_a_Dictionary_alias =()=> {
			Post("/info", new Vars {{"foo-bar", "hello"}, {"!NEAT*", "w00t"}}).Body.ShouldContain("POST Variable: foo-bar = hello");
			Post("/info", new Vars {{"foo-bar", "hello"}, {"!NEAT*", "w00t"}}).Body.ShouldContain("POST Variable: !NEAT* = w00t");
		};

		It can_post_and_supply_query_strings =()=> {
			Post("/info", new { foo="bar", QueryStrings = new { hi="there" } });

			LastResponse.Body.ShouldContain("POST Variable: foo = bar");
			LastResponse.Body.ShouldContain("QueryString: hi = there");
			LastResponse.Body.ShouldNotContain("POST Variable: hi = there");
			LastResponse.Body.ShouldNotContain("QueryString: foo = bar");
		};

		It can_post_and_supply_query_strings_and_custom_headers =()=> {
			Post("/info", new { 
				foo = "bar", 
				QueryStrings = new { hi = "there"      }, 
				Headers      = new { CUSTOM = "header" }
			});

			var body = LastResponse.Body;

			body.ShouldContain(   "QueryString: hi = there");
			body.ShouldNotContain("QueryString: foo = bar");
			body.ShouldNotContain("QueryString: HTTP_CUSTOM = header");

			body.ShouldNotContain("POST Variable: hi = there");
			body.ShouldContain(   "POST Variable: foo = bar");
			body.ShouldNotContain("POST Variable: HTTP_CUSTOM = header");

			body.ShouldNotContain("Header: hi = there");
			body.ShouldNotContain("Header: foo = bar");
			body.ShouldContain(   "Header: HTTP_CUSTOM = header");
		};
    }
}
