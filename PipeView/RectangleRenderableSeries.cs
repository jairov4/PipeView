using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Common.Extensions;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Numerics;
using Abt.Controls.SciChart.Numerics.CoordinateCalculators;
using Abt.Controls.SciChart.Numerics.PointResamplers;
using Abt.Controls.SciChart.Rendering.Common;
using Abt.Controls.SciChart.Visuals;
using Abt.Controls.SciChart.Visuals.RenderableSeries;

namespace PipeView
{
	public class RectangleRenderableSeries : CustomRenderableSeries
	{
		public new ISeries DataSeries { get; }

		public RectangleRenderableSeries(ISeries series)
		{
			ResamplingMode = ResamplingMode.None;
			DataSeries = series;
			IsVisible = true;
			base.DataSeries = new DummyDataSeries(series);
		}

		protected override void Draw(IRenderContext2D renderContext, IRenderPassData renderPassData)
		{
			var xcal = renderPassData.XCoordinateCalculator;
			var ycal = renderPassData.YCoordinateCalculator;
			var surf = GetParentSurface();
			var xrange = surf.XAxis.VisibleRange.AsDoubleRange();
			var yrange = surf.YAxis.VisibleRange.AsDoubleRange();
			var area = new Rect(xrange.Min, yrange.Min, xrange.Diff, yrange.Diff);
			var found = DataSeries.FindInArea(area);
			var brush = renderContext.CreateBrush(new SolidColorBrush(Colors.BlueViolet));
			var pen = renderContext.CreatePen(Colors.Black, false, 1);
			foreach (var index in found)
			{
				var xv = DataSeries.XValues[index];
				var yv = DataSeries.YValues[index];
				var wv = DataSeries.WidthValues[index];
				var hv = DataSeries.HeightValues[index];
				DrawRectangle(renderContext, pen, brush, xcal, ycal, xv, yv, wv, hv);
			}
			brush.Dispose();
			pen.Dispose();
		}

		protected static void DrawRectangle(IRenderContext2D renderContext, IPen2D pen, IBrush2D brush, ICoordinateCalculator<double> xcal,
			ICoordinateCalculator<double> ycal, float xv, float yv, float wv, float hv)
		{
			var x1 = xcal.GetCoordinate(xv);
			var y1 = ycal.GetCoordinate(yv);
			var x2 = xcal.GetCoordinate(xv + wv);
			var y2 = ycal.GetCoordinate(yv + hv);
			var pt1 = new Point(x1, y1);
			var pt2 = new Point(x2, y2);
			renderContext.FillRectangle(brush, pt1, pt2);
			renderContext.DrawQuad(pen, pt1, pt2);
		}

		protected class DummyDataSeries : IDataSeries
		{
			private readonly ISeries series;

			readonly IndexRange iRange = new IndexRange(0,0);
			readonly IPointSeries pSeries = new Point2DSeries(1);
			private readonly IList values = new List<double>() {0.0};

			public DummyDataSeries(ISeries series)
			{
				this.series = series;
			}

			public IUpdateSuspender SuspendUpdates()
			{
				return ParentSurface.SuspendUpdates();
			}

			public void ResumeUpdates(IUpdateSuspender suspender)
			{
				ParentSurface.ResumeUpdates(suspender);
			}

			public void DecrementSuspend()
			{
				ParentSurface.DecrementSuspend();
			}

			public bool IsSuspended { get { return ParentSurface.IsSuspended; } }

			public void Clear()
			{
				throw new NotSupportedException();
			}

			public IndexRange GetIndicesRange(IRange visibleRange)
			{
				return iRange;
			}

			public IPointSeries ToPointSeries(ResamplingMode resamplingMode, IndexRange pointRange, int viewportWidth, bool isCategoryAxis,
				bool? dataIsDisplayedAs2D, IRange visibleXRange, IPointResamplerFactory factory)
			{
				return pSeries;
			}

			public IPointSeries ToPointSeries(IList column, ResamplingMode resamplingMode, IndexRange pointRange, int viewportWidth,
				bool isCategoryAxis)
			{
				throw new NotSupportedException();
			}

			public IRange GetWindowedYRange(IRange xRange)
			{
				return xRange;
			}

			public IRange GetWindowedYRange(IndexRange xIndexRange)
			{
				throw new NotSupportedException();
			}

			public IRange GetWindowedYRange(IndexRange xIndexRange, bool getPositiveRange)
			{
				throw new NotSupportedException();
			}

			public IRange GetWindowedYRange(IRange xRange, bool getPositiveRange)
			{
				return xRange;
			}

			public int FindIndex(IComparable x, SearchMode searchMode = SearchMode.Exact)
			{
				throw new NotSupportedException();
			}

			public HitTestInfo ToHitTestInfo(int index)
			{
				throw new NotSupportedException();
			}

			public void InvalidateParentSurface(RangeMode rangeMode)
			{
				throw new NotSupportedException();
			}

			public int FindClosestPoint(IComparable x, IComparable y, double xyScaleRatio, double maxXDistance)
			{
				throw new NotSupportedException();
			}

			public int FindClosestLine(IComparable x, IComparable y, double xyScaleRatio, double xRadius, LineDrawMode drawNanAs)
			{
				throw new NotSupportedException();
			}

			public Type XType => typeof (double);

			public Type YType => typeof(double);

			public ISciChartSurface ParentSurface { get; set; }

			public bool IsAttached => true;

			public IRange XRange => new DoubleRange(series.XRange.Min, series.XRange.Max);

			public IRange YRange => new DoubleRange(series.YRange.Min, series.YRange.Max);

			public DataSeriesType DataSeriesType => DataSeriesType.Xy;

			public IList XValues => values;

			public IList YValues => values;

			public IComparable LatestYValue
			{
				get { throw new NotSupportedException(); }
			}

			public string SeriesName { get; set; }

			public IComparable YMin
			{
				get { throw new NotSupportedException(); }
			}
			public IComparable YMinPositive
			{
				get { throw new NotSupportedException(); }
			}
			public IComparable YMax
			{
				get { throw new NotSupportedException(); }
			}
			public IComparable XMin
			{
				get { throw new NotSupportedException(); }
			}
			public IComparable XMinPositive
			{
				get { throw new NotSupportedException(); }
			}
			public IComparable XMax
			{
				get { throw new NotSupportedException(); }
			}
			public bool IsFifo => false;
			public int? FifoCapacity { get; set; }
			public bool HasValues => series.Count > 0;
			public int Count => series.Count;
			public bool IsSorted => false;
			public object SyncRoot => this;
			public bool AcceptsUnsortedData { get; set; }

			public event EventHandler<DataSeriesChangedEventArgs> DataSeriesChanged;

			protected void RaiseDataSeriesChanged()
			{
				DataSeriesChanged?.Invoke(this, new DataSeriesChangedEventArgs(DataSeriesUpdate.DataChanged));
			}
		}
	}
}