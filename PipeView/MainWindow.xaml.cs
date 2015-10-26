using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Abt.Controls.SciChart;
using TReal=System.Single;

namespace PipeView
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			ThemeManager.SetTheme(this, "BrightSpark");
		}

		private void btnLoadData_OnClick(object sender, RoutedEventArgs e)
		{
			var size = int.Parse(txtSizeBox.Text);
			var dataset = GenerateDataStream(size);
			CreateSeries(dataset);
			UpdateStatus();
		}

		protected void UpdateStatus()
		{
			Dispatcher.Invoke(() => txtStatus.Text = $"Displaying {chart.RenderableSeries.Sum(s => s.DataSeries.Count)} featues");
		}

		/// <summary>
		/// Process the series to build the chart data structure.
		/// </summary>
		/// <param name="dataset">The dataset.</param>
		/// <returns>Data series</returns>
		protected virtual void CreateSeries(DataStream dataset)
		{
			var xI = dataset.FieldNames.IndexOf("x");
			var yI = dataset.FieldNames.IndexOf("y");
			var wI = dataset.FieldNames.IndexOf("w");
			var hI = dataset.FieldNames.IndexOf("h");
			
			var x = new List<TReal>();
			var y = new List<TReal>();
			var w = new List<TReal>();
			var h = new List<TReal>();

			//var atts = new List<object>();
			//var nameIndices = Enumerable.Range(0, dataset.FieldNames.Count).Except(new[] { xI, yI, wI, hI }).ToArray();
			//var names = nameIndices.Select(i => dataset.FieldNames[i]).ToArray();
			//var types = nameIndices.Select(i => dataset.FieldTypes[i]).ToArray();

			foreach (var dataStreamValue in dataset.Stream)
			{
				//atts.AddRange(from c in nameIndices select dataStreamValue.Values[c]);
				x.Add((TReal)dataStreamValue.Values[xI]);
				y.Add((TReal)dataStreamValue.Values[yI]);
				w.Add((TReal)dataStreamValue.Values[wI]);
				h.Add((TReal)dataStreamValue.Values[hI]);
			}

			var series = new Series1(x, y, w, h);
			//var series = new Series2(x, y, w, h, names, types, atts);

			var adapter = new RectangleRenderableSeries(series);
			chart.RenderableSeries.Add(adapter);
		}

		/// <summary>
		/// Generates the data stream like service can deliver.
		/// </summary>
		/// <param name="length">The length.</param>
		/// <returns>data stream</returns>
		private DataStream GenerateDataStream(int length)
		{
			var names = new[] { "x", "y", "w", "h", "type" };
			var types = new[] { typeof(TReal), typeof(TReal), typeof(TReal), typeof(TReal), typeof(string) };
			var stream = new DataStream(names, types, EnumerateDataStreamValues(length));
			return stream;
		}

		private IEnumerable<DataStreamValue> EnumerateDataStreamValues(int length)
		{
			var rnd = new Random();
			var types = new[] {"METAL_LOSS", "CRACKING", "UNKNOWN"};
			for (var i = 0; i < length; i++)
			{
				var x = rnd.NextDouble()*250000.0;
				var y = rnd.NextDouble()*2*Math.PI;
				var w = rnd.NextDouble()*3.0;
				var h = rnd.NextDouble()*Math.PI/4;
				var type = types[rnd.Next(types.Length)];
				var value = new DataStreamValue(new object[] { (TReal)x, (TReal)y, (TReal)w, (TReal)h, type});
				yield return value;
			}
		}

		private void btnClear_OnClick(object sender, RoutedEventArgs e)
		{
			chart.RenderableSeries.Clear();
			UpdateStatus();
		}
	}
}
