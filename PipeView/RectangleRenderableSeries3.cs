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
			var pixTakeCare = 3;
			var pixSizeX = (TReal)Math.Abs(xcal.GetDataValue(pixTakeCare) - xcal.GetDataValue(0.0));
			var pixSizeY = (TReal)Math.Abs(ycal.GetDataValue(0.0) - ycal.GetDataValue(pixTakeCare));
			var found = DataSeries.FindInArea(area, pixSizeX, pixSizeY);
			var brush = renderContext.CreateBrush(new SolidColorBrush(Colors.BlueViolet));
			var pen = renderContext.CreatePen(Colors.Black, false, 1);
			foreach (var rect in found)
			{
				DrawRectangle(renderContext, pen, brush, xcal, ycal, (TReal)rect.X, (TReal)rect.Y, (TReal)rect.Width, (TReal)rect.Height);
			}
			brush.Dispose();
			pen.Dispose();

			var sdbg = DataSeries as Series4;
			//sdbg.DrawNodes(renderContext, xcal, ycal, area, pixSizeX, pixSizeY);
		}
	}
}