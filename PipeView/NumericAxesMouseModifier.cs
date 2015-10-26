using System.Windows;
using System.Windows.Input;
using Abt.Controls.SciChart;
using Abt.Controls.SciChart.ChartModifiers;
using Abt.Controls.SciChart.Visuals;

namespace PipeView
{
	public class NumericAxesMouseModifier : ChartModifierBase
	{
		private Point TransformMousePoint(Point p)
		{
			var xcalc = ParentSurface.XAxis.GetCurrentCoordinateCalculator();
			var ycalc = ParentSurface.YAxis.GetCurrentCoordinateCalculator();
			return new Point(xcalc.GetDataValue(p.X), ycalc.GetDataValue(p.Y));
		}

		public override void OnModifierMouseWheel(ModifierMouseArgs e)
		{
			e.Handled = true;
			var usey = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
			var usex = true;
			var factor = 1 - e.Delta*0.001;

			using (ParentSurface.SuspendUpdates())
			{
				var p = TransformMousePoint(GetPointRelativeTo(e.MousePoint, ParentSurface.RenderSurface));
				if (usex)
				{
					var vr = ParentSurface.XAxis.VisibleRange.AsDoubleRange();
					var leftPart = p.X - vr.Min;
					var rightPart = vr.Max - p.X;
					leftPart *= factor;
					rightPart *= factor;
					ParentSurface.XAxis.VisibleRange = new DoubleRange(p.X - leftPart, p.X + rightPart);
				}
				if (usey)
				{
					var vr = ParentSurface.YAxis.VisibleRange.AsDoubleRange();
					var bottomPart = p.Y - vr.Min;
					var topPart = vr.Max - p.Y;
					bottomPart *= factor;
					topPart *= factor;
					ParentSurface.YAxis.VisibleRange = new DoubleRange(p.Y - bottomPart, p.Y + topPart);
				}
			}
		}

		public override void OnModifierDoubleClick(ModifierMouseArgs e)
		{
			if (!e.MouseButtons.HasFlag(MouseButtons.Left)) return;

			ParentSurface.ZoomExtents();
			e.Handled = true;
		}
	}
}