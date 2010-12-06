using System;
using Machine.Specifications;

namespace Requestor.Specs {

    public class Spec : Requestor.Static {
	public static string TestUrl = "http://localhost:3000";

	Establish context =()=>
	    Requestor.Static.Instance = new Requestor(TestUrl);
    }
}
