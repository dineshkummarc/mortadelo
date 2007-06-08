using System.Collections;
using NUnit.Framework;

namespace Mortadelo {
	public class StringPool {
		public StringPool ()
		{
			hash = new Hashtable ();
		}

		public string AddString (string str)
		{
			if (hash.ContainsKey (str))
				return (string) hash[str];

			hash[str] = str;
			return (string) hash[str];
		}

		Hashtable hash;
	}

	[TestFixture]
	public class StringPoolTest {
		[Test]
		public void Pool ()
		{
			StringPool pool;
			string a, b, c, d;

			pool = new StringPool ();

			a = pool.AddString ("hello");
			b = pool.AddString ("hello");
			c = pool.AddString ("world");
			d = pool.AddString ("world");

			Assert.AreEqual (a, b, "Unique string 1");
			Assert.AreEqual (c, d, "Unique string 2");
		}
	}
}
