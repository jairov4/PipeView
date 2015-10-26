using System.Collections.Generic;
using System.Windows;
using TReal = System.Single;

namespace PipeView
{
	public interface ISeries
	{
		IEnumerable<int> FindInArea(Rect area);
		IEnumerable<int> FindInPoint(Point point);
		int? FindNearest(Point point);

		IReadOnlyList<TReal> XValues { get; }
		IReadOnlyList<TReal> YValues { get; }
		IReadOnlyList<TReal> HeightValues { get; }
		IReadOnlyList<TReal> WidthValues { get; }

		VisibleRange XRange { get; }
		VisibleRange YRange { get; }
		
		int Count { get; }
	}
}