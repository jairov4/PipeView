using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace PipeView
{
	public static class CodeContracts
	{
		[Conditional("DEBUG")]
		[Conditional("CODE_CONTRACTS")]
		public static void Requires(bool cond, string msg = "")
		{
			if (!cond)
			{
				throw new ArgumentException(msg);
			}
		}

		[Conditional("DEBUG")]
		[Conditional("CODE_CONTRACTS")]
		public static void RequiresNotNull(params object[] o)
		{
			if (o.Any(x => x == null))
			{
				throw new ArgumentNullException();
			}
		}
	}
}