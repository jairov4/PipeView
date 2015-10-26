using System;
using TReal = System.Single;

namespace PipeView
{
	public struct VisibleRange
	{
		public VisibleRange(TReal a, TReal b)
		{
			Max = Math.Max(a, b);
			Min = Math.Min(a, b);
		}

		public TReal Min { get; private set; }
		public TReal Max { get; private set; }

		public TReal Diff => Max - Min;

		public override string ToString()
		{
			return $"{Min}-{Max} (dist:{Diff})";
		}
	}
}