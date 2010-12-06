using System;
using System.Collections.Generic;
using Machine.Specifications;
using Requestor;

namespace Requestor.Specs {

    [Subject(typeof(Requestor))]
    public class Put : Spec {

	It can_put_using_default_put_variable;
	It can_put_using_custom_put_variable;

    }

    [Subject(typeof(Requestor))]
    public class Delete : Spec {

	It can_delete_using_default_delete_variable;
	It can_delete_using_custom_delete_variable;

    }
}
