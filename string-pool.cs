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
			if (str == null)
				return null;

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
			string a, b, c, d, e, f;

			pool = new StringPool ();

			a = pool.AddString ("hello");
			b = pool.AddString ("hello");
			c = pool.AddString ("world");
			d = pool.AddString ("world");
			e = pool.AddString (null);
			f = pool.AddString (null);

			Assert.AreEqual (a, b, "Unique string 1");
			Assert.AreEqual (c, d, "Unique string 2");
			Assert.AreEqual (e, f, "Null string 1");
			Assert.AreEqual (e, null, "Null string 2");
		}
	}
}
