using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Abt.Controls.SciChart.ChartModifiers;

namespace PipeView
{
	class TooltipModifier : ChartModifierBase
	{
		private Border tooltipControl;
		private TextBlock textBlock;

		public override void OnAttached()
		{
			base.OnAttached();
			tooltipControl = new Border()
			{
				Background = new SolidColorBrush(Color.FromRgb(241, 242, 247)),
				BorderBrush = new SolidColorBrush(Color.FromRgb(118,118,118)),
				BorderThickness = new Thickness(1.0),
				Padding = new Thickness(8)
			};

			textBlock = new TextBlock()
			{
				Foreground = new SolidColorBrush(Color.FromRgb(87, 87, 87)),
				FontFamily = new FontFamily("Segoe UI Light"),
				FontSize = 15
			};
			
			tooltipControl.Child = textBlock;
		}

		private Point TransformMousePoint(Point p)
		{
			var xcalc = ParentSurface.XAxis.GetCurrentCoordinateCalculator();
			var ycalc = ParentSurface.YAxis.GetCurrentCoordinateCalculator();
			return new Point(xcalc.GetDataValue(p.X), ycalc.GetDataValue(p.Y));
		}

		public override void OnModifierMouseMove(ModifierMouseArgs e)
		{
			e.Handled = true;

			if (!ModifierSurface.Children.Contains(tooltipControl))
			{
				ModifierSurface.Children.Add(tooltipControl);
			}

			var p = TransformMousePoint(GetPointRelativeTo(e.MousePoint, ParentSurface.RenderSurface));

			var seriesCol = ParentSurface.RenderableSeries;
			foreach (var gseries in seriesCol)
			{
				var rseries = gseries as RectangleRenderableSeries2;
				var series = rseries?.DataSeries as ISeriesWithAttributes;
				var i = series?.FindNearest(p);
				if (i == null)
				{
					continue;
				}
				textBlock.Visibility = Visibility.Visible;
				textBlock.Text = $"{series.AttributeNames[0]?.ToUpper()}: {series.GetAttribute(i.Value, 0)}\nX: {series.XValues[i.Value]:F1}\nY: {series.YValues[i.Value]:F2}";

				var mp = GetPointRelativeTo(e.MousePoint, ModifierSurface);

				Canvas.SetLeft(tooltipControl, mp.X);
				Canvas.SetTop(tooltipControl, mp.Y);
				return;
			}
			textBlock.Visibility = Visibility.Hidden;
		}
	}
}
