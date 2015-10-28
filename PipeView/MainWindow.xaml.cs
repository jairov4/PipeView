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
		}

		private void btnLoadData_OnClick(object sender, RoutedEventArgs e)
		{
			var size = int.Parse(txtSizeBox.Text);
			var dataset = GenerateDataStream(size);
			progressBar.Minimum = 0;
			progressBar.Maximum = size;
			brd.Visibility = Visibility.Visible;
			loadingSeries = CreateSeries(dataset);
			UpdateStatus();
		}

		private ISeries loadingSeries;

		protected void UpdateStatus()
		{
			Dispatcher.Invoke(() =>
			{
				txtLoadingStatus.Text = $"Loaded {loadingSeries.Count:N0} featues";
				progressBar.Value = loadingSeries.Count;
			});
		}

		/// <summary>
		/// Process the series to build the chart data structure.
		/// </summary>
		/// <param name="dataset">The dataset.</param>
		/// <returns>Data series</returns>
		protected virtual ISeries CreateSeries(DataStream dataset)
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
			brd.Visibility = Visibility.Collapsed;

			return series;
		}

		/// <summary>
		/// Generates the data stream like service can deliver.
		/// </summary>
		/// <param name="length">The length.</param>
		/// <returns>data stream</returns>
		private DataStream GenerateDataStream(int length)
		{
			var adt = 20;
			var names = new[] { "x", "y", "w", "h", "type" };
			var types = new[] { typeof(TReal), typeof(TReal), typeof(TReal), typeof(TReal), typeof(string) };
			Array.Resize(ref names, names.Length+adt);
			Array.Resize(ref types, names.Length + adt);
			for (int i = 5; i < names.Length; i++)
			{
				names[i] = "Field" + i;
				types[i] = typeof (double);
			}
			var stream = new DataStream(names, types, EnumerateDataStreamValues(length, adt));
			return stream;
		}

		private IEnumerable<DataStreamValue> EnumerateDataStreamValues(int length, int adt)
		{
			var rnd = new Random();
			var types = new[] {"METAL_LOSS", "CRACKING", "UNKNOWN"};

			var innerValues = new List<object>(adt+5);
			for (var i = 0; i < length; i++)
			{
				var x = rnd.NextDouble()*250000.0;
				var y = rnd.NextDouble()*2*Math.PI;
				var w = rnd.NextDouble()*3.0;
				var h = rnd.NextDouble()*Math.PI/10;
				var type = types[rnd.Next(types.Length)];

				innerValues.Clear();
				innerValues.Add((TReal)x);
				innerValues.Add((TReal)y);
				innerValues.Add((TReal)w);
				innerValues.Add((TReal)h);
				innerValues.Add(type);
				for (int j = 0; j < adt; j++)
				{
					innerValues.Add(rnd.NextDouble() * 10);
				}
				var value = new DataStreamValue(innerValues.ToArray());
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
