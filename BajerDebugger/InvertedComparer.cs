using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BajerDebugger
{
	public class InvertedComparer : IComparer<DateTime> {
		public int Compare(DateTime x, DateTime y) {
			return y.CompareTo(x);
		}
	}
}
