using System;
using Abt.Controls.SciChart.Visuals;
using Abt.Controls.SciChart.Visuals.RenderableSeries;

namespace PipeView
{
	/// <summary>
	/// With incremental loading support
	/// </summary>
	public class RectangleRenderableSeries2 : RectangleRenderableSeries
	{
		public RectangleRenderableSeries2(ISeriesWithIncrementalLoading series) : base(series)
		{
			((CustomRenderableSeries) this).DataSeries = new DummyDataSeries2(series);
		}

		class DummyDataSeries2 : DummyDataSeries
		{
			public DummyDataSeries2(ISeriesWithIncrementalLoading series) : base(series)
			{
				series.AppendedChunk += SeriesOnAppendedChunk;
			}
			
			private void SeriesOnAppendedChunk()
			{
				RaiseDataSeriesChanged();
			}
		}
	}
}