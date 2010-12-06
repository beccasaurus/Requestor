using System;

namespace Requestor.Specs {

    public class Spec {
	public static string TestUrl = "http://localhost:3000";

	// Setup DSL so we can statically call methods on our Requestor instance from our specs
	public static Requestor R = new Requestor(TestUrl);
	public static IResponse Get(string path){                 return R.Get(path);        }
	public static IResponse Get(string path, object vars){    return R.Get(path, vars);  }
	public static IResponse Post(string path){                return R.Post(path);        }
	public static IResponse Post(string path, object vars){   return R.Post(path, vars);  }
	public static IResponse Put(string path){                 return R.Put(path);        }
	public static IResponse Put(string path, object vars){    return R.Put(path, vars);  }
	public static IResponse Delete(string path){              return R.Delete(path);        }
	public static IResponse Delete(string path, object vars){ return R.Delete(path, vars);  }
	public static IResponse FollowRedirect(){                 return R.FollowRedirect(); }
	public static IResponse LastResponse { get {              return R.LastResponse;     }}
    }

}
