using System;
using System.Collections.Generic;
using Machine.Specifications;
using Requestoring;

namespace Requestoring.Specs {

    // RequestInfo should ideally only be used internally, but we made it public so ...
    //  - we can test it
    //  - folks can use it if they really want to
    [Subject(typeof(Requestor.RequestInfo))]
    public class RequestInfoSpec : Spec {

	It can_add_QueryStrings_by_default =()=> {
	    var info = new Requestor.RequestInfo(new { Foo = "Bar" }, "QueryStrings");
	    info.Headers.Count.ShouldEqual(0);
	    info.PostData.Count.ShouldEqual(0);
	    info.QueryStrings.Count.ShouldEqual(1);
	    info.QueryStrings["Foo"].ShouldEqual("Bar");
	};

	It can_add_QueryStrings_using_Dictionary =()=> {
	    var info = new Requestor.RequestInfo(new Dictionary<string,string>{{"Foo", "Bar"}}, "QueryStrings");
	    info.Headers.Count.ShouldEqual(0);
	    info.PostData.Count.ShouldEqual(0);
	    info.QueryStrings.Count.ShouldEqual(1);
	    info.QueryStrings["Foo"].ShouldEqual("Bar");
	};

	It can_add_QueryStrings_using_Vars_which_is_a_Dictionary_alias =()=> {
	    var info = new Requestor.RequestInfo(new Vars{{"Foo", "Bar"}}, "QueryStrings");
	    info.Headers.Count.ShouldEqual(0);
	    info.PostData.Count.ShouldEqual(0);
	    info.QueryStrings.Count.ShouldEqual(1);
	    info.QueryStrings["Foo"].ShouldEqual("Bar");
	};

	It can_add_PostData_by_default =()=> {
	    var info = new Requestor.RequestInfo(new { Foo = "Bar" }, "PostData");
	    info.Headers.Count.ShouldEqual(0);
	    info.QueryStrings.Count.ShouldEqual(0);
	    info.PostData.Count.ShouldEqual(1);
	    info.PostData["Foo"].ShouldEqual("Bar");
	};

	It can_add_QueryStrings_explicitly =()=> {
	    var info = new Requestor.RequestInfo(new { QueryStrings = new { Foo = "Bar" } }, "PostData");
	    info.Headers.Count.ShouldEqual(0);
	    info.PostData.Count.ShouldEqual(0);
	    info.QueryStrings.Count.ShouldEqual(1);
	    info.QueryStrings["Foo"].ShouldEqual("Bar");
	};
	
	It can_add_QueryStrings_and_PostData_explicitly =()=> {
	    var info = new Requestor.RequestInfo(new { QueryStrings = new { Foo = "Bar" }, PostData = new { Hi = "There" } }, "PostData");
	    info.Headers.Count.ShouldEqual(0);
	    info.PostData.Count.ShouldEqual(1);
	    info.PostData["Hi"].ShouldEqual("There");
	    info.QueryStrings.Count.ShouldEqual(1);
	    info.QueryStrings["Foo"].ShouldEqual("Bar");
	};

	It can_add_QueryStrings_and_PostData_explicitly_using_dictionaries =()=> {
	    var info = new Requestor.RequestInfo(new { QueryStrings = new Dictionary<string,string>{{"Foo","Bar"}}, PostData = new Dictionary<string,string>{{"Hi","There"}} }, "PostData");
	    info.Headers.Count.ShouldEqual(0);
	    info.PostData.Count.ShouldEqual(1);
	    info.PostData["Hi"].ShouldEqual("There");
	    info.QueryStrings.Count.ShouldEqual(1);
	    info.QueryStrings["Foo"].ShouldEqual("Bar");
	};

	It can_add_QueryStrings_and_PostData_explicitly_using_Vars_which_is_a_Dictionary_alias =()=> {
	    var info = new Requestor.RequestInfo(new { QueryStrings = new Vars{{"Foo","Bar"}}, PostData = new Vars{{"Hi","There"}} }, "PostData");
	    info.Headers.Count.ShouldEqual(0);
	    info.PostData.Count.ShouldEqual(1);
	    info.PostData["Hi"].ShouldEqual("There");
	    info.QueryStrings.Count.ShouldEqual(1);
	    info.QueryStrings["Foo"].ShouldEqual("Bar");
	};

	It can_add_PostData_by_default_and_QueryStrings_explicitly =()=> {
	    var info = new Requestor.RequestInfo(new { Hi = "There", QueryStrings = new { Foo = "Bar" }}, "PostData");
	    info.Headers.Count.ShouldEqual(0);
	    info.PostData.Count.ShouldEqual(1);
	    info.PostData["Hi"].ShouldEqual("There");
	    info.QueryStrings.Count.ShouldEqual(1);
	    info.QueryStrings["Foo"].ShouldEqual("Bar");
	};

	It can_add_Headers_by_default_and_QueryStrings_explicitly =()=> {
	    var info = new Requestor.RequestInfo(new { Hi = "There", QueryStrings = new { Foo = "Bar" }}, "Headers");
	    info.PostData.Count.ShouldEqual(0);
	    info.Headers.Count.ShouldEqual(1);
	    info.Headers["Hi"].ShouldEqual("There");
	    info.QueryStrings.Count.ShouldEqual(1);
	    info.QueryStrings["Foo"].ShouldEqual("Bar");
	};
    }
}
