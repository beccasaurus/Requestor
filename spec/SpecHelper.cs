using System;
using System.Reflection;
using NUnit.Framework;
using Requestoring;

// Playing around with making it easier to define SetUp and TearDown ... we'll move this logic into a base class that you can 
// use for your specs to get this type of functionality ...
namespace Requestoring.Specs {

	public delegate void Before(Spec s = null);

	public class Spec : Requestor {

		Before GetBeforeEach() {
			var field = this.GetType().GetField("each", BindingFlags.NonPublic | BindingFlags.Instance);
			if (field == null)
				return null;
			else
				return field.GetValue(this) as Before;
		}

		void RunBeforeEach() {
			var before = GetBeforeEach();
			if (before != null)
				before(this);
		}

		[SetUp]
		public void BeforeEach() {
			Reset();
			HttpRequestor.MethodVariable = null;
			RootUrl = "http://localhost:3000";
			RunBeforeEach();
		}
	}
}
