using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace Requestoring {

	/// <summary>
	/// Short alias to ...
	/// </summary>
	/// <remarks>
	/// Allows us to write <c>new Vars { {"Key","Value"}, {"Key2","Value2"} }</c>.
	///
	/// It's not pretty but it's WAY better than having to write ...
	/// </remarks>
	public class Vars : Dictionary<string, string> {}
}
