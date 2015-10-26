using System;
using System.Windows;
using System.Windows.Media;
using Abt.Controls.SciChart.Rendering.Common;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using TReal = System.Single;

namespace PipeView
{
	public class RectangleRenderableSeries3 : RectangleRenderableSeries2
	{
		public new ISeriesWithDecimation DataSeries { get; }

		public RectangleRenderableSeries3(ISeriesWithDecimation series) : base(series)
		{
			this.DataSeries = series;
		}

		protected override void Draw(IRenderContext2D renderContext, IRenderPassData renderPassData)
		{
			var xcal = renderPassData.XCoordinateCalculator;
			var ycal = renderPassData.YCoordinateCalculator;
			var surf = GetParentSurface();
			var xrange = surf.XAxis.VisibleRange.AsDoubleRange();
			var yrange = surf.YAxis.VisibleRange.AsDoubleRange();
			var area = new Rect(xrange.Min, yrange.Min, xrange.Diff, yrange.Diff);
			var pixSizeX = Math.Abs(xcal.GetDataValue(1.0) - xcal.GetDataValue(0.0));
			var pixSizeY = Math.Abs(ycal.GetDataValue(0.0) - ycal.GetDataValue(1.0));
			var found = DataSeries.FindInArea(area, (TReal)pixSizeX, (TReal)pixSizeY);
			var brush = renderContext.CreateBrush(new SolidColorBrush(Colors.BlueViolet));
			var pen = renderContext.CreatePen(Colors.Black, false, 1);
			foreach (var rect in found)
			{
				DrawRectangle(renderContext, pen, brush, xcal, ycal, (TReal)rect.X, (TReal)rect.Y, (TReal)rect.Width, (TReal)rect.Height);
			}
			brush.Dispose();
			pen.Dispose();
		}
	}
}