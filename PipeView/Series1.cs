using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static PipeView.CodeContracts;
using TReal = System.Single;

namespace PipeView
{
	public class Series1 : ISeries
	{
		public Series1(IReadOnlyList<TReal> xValues, IReadOnlyList<TReal> yValues, IReadOnlyList<TReal> widthValues, IReadOnlyList<TReal> heightValues)
		{
			RequiresNotNull(xValues, yValues, heightValues, widthValues);
			Requires(xValues.Count == yValues.Count);
			Requires(xValues.Count == heightValues.Count);
			Requires(xValues.Count == widthValues.Count);

			XValues = xValues;
			YValues = yValues;
			HeightValues = heightValues;
			WidthValues = widthValues;

			if (xValues.Count > 0)
			{
				XRange = new VisibleRange(xValues.Min(), xValues.Max());
				YRange = new VisibleRange(yValues.Min(), yValues.Max());
			}
			else
			{
				XRange = new VisibleRange(0, 0);
				YRange = new VisibleRange(0, 0);
			}
		}
		
		public virtual IEnumerable<int> FindInArea(Rect area)
		{
			for (var i = 0; i < Count; i++)
			{
				var x = XValues[i];
				var y = YValues[i];
				var w = WidthValues[i];
				var h = HeightValues[i];

				var rect = new Rect(x, y, w, h);
				if (area.IntersectsWith(rect))
				{
					yield return i;
				}
			}
		}

		public virtual IEnumerable<int> FindInPoint(Point point)
		{
			for (var i = 0; i < Count; i++)
			{
				var x = XValues[i];
				var y = YValues[i];
				var w = WidthValues[i];
				var h = HeightValues[i];

				var rect = new Rect(x, y, w, h);
				if (rect.Contains(point))
				{
					yield return i;
				}
			}
		}

		public virtual int? FindNearest(Point point)
		{
			int? j = null;
			var k = double.MaxValue;
			for (var i = 0; i < Count; i++)
			{
				var x = XValues[i] + WidthValues[i]/2;
				var y = YValues[i]+ HeightValues[i]/2;
				var xx = Math.Abs(x - point.X);
				var yy = Math.Abs(y - point.Y);
				var d = xx*xx + yy*yy;
				if (d < k)
				{
					k = d;
					j = i;
				}
			}
			return j;
		}

		public virtual IReadOnlyList<TReal> XValues { get; }
		public virtual IReadOnlyList<TReal> YValues { get; }
		public virtual IReadOnlyList<TReal> HeightValues { get; }
		public virtual IReadOnlyList<TReal> WidthValues { get; }

		public virtual VisibleRange XRange { get; }
		public virtual VisibleRange YRange { get; }

		public int Count => XValues.Count;
	}
}