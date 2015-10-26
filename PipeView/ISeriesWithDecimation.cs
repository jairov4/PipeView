using System.Collections.Generic;
using System.Windows;
using TReal = System.Single;

namespace PipeView
{
	public interface ISeriesWithDecimation : ISeriesWithIncrementalLoading
	{
		IEnumerable<Rect> FindInArea(Rect area, TReal pixSizeX, TReal pixSizeY);
	}
}