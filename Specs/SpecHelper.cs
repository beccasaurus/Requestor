using System;
using Machine.Specifications;

namespace Requestoring.Specs {

    public class Spec : Requestor.Static {
	public static string TestUrl = "http://localhost:3000";

	Establish context =()=> Instance = new Requestor(TestUrl);
    }
}
