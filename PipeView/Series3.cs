using System;
using System.Collections.Generic;
using System.Linq;
using TReal = System.Single;

namespace PipeView
{
	public class Series3 : Series2, ISeriesWithIncrementalLoading
	{
		private static readonly TReal[] EmptyRealArray = new TReal[0];
		private static readonly object[] EmptyObjectArray = new object[0];

		protected readonly List<object> attributeValues;
		protected readonly List<TReal> x, y, w, h;
		private VisibleRange xRange;
		private VisibleRange yRange;

		public Series3(IReadOnlyList<string> names, IReadOnlyList<Type> types)
			: this(0, names, types)
		{
		}

		public Series3(int capacity, IReadOnlyList<string> names, IReadOnlyList<Type> types)
			: base(EmptyRealArray, EmptyRealArray, EmptyRealArray, EmptyRealArray, names, types, EmptyObjectArray)
		{
			x = new List<TReal>(capacity);
			y = new List<TReal>(capacity);
			w = new List<TReal>(capacity);
			h = new List<TReal>(capacity);
			attributeValues = new List<object>(capacity*names.Count);
		}

		public override IReadOnlyList<object> AttributeValues => attributeValues;
		public override IReadOnlyList<TReal> XValues => x;
		public override IReadOnlyList<TReal> YValues => y;
		public override IReadOnlyList<TReal> WidthValues => w;
		public override IReadOnlyList<TReal> HeightValues => h;

		public virtual void AppendCore(ICollection<TReal> x, ICollection<TReal> y, ICollection<TReal> w, ICollection<TReal> h, ICollection<object> atts)
		{
			this.x.AddRange(x);
			this.y.AddRange(y);
			this.w.AddRange(w);
			this.h.AddRange(h);
			this.attributeValues.AddRange(atts);

			var min = Math.Min(x.Min(), XRange.Min);
			var max = Math.Max(x.Zip(w, (a, b) => a + b).Max(), XRange.Max);
			xRange = new VisibleRange(min, max);

			min = Math.Min(y.Min(), YRange.Min);
			max = Math.Max(y.Zip(h, (a, b) => a + b).Max(), YRange.Max);
			yRange = new VisibleRange(min, max);
		}

		public void Append(ICollection<TReal> x, ICollection<TReal> y, ICollection<TReal> w, ICollection<TReal> h, ICollection<object> atts)
		{
			AppendingChunk?.Invoke();
			AppendCore(x, y, w, h, atts);
			AppendedChunk?.Invoke();
		}

		public override VisibleRange XRange => xRange;
		public override VisibleRange YRange => yRange;

		public event Action AppendingChunk;
		public event Action AppendedChunk;
	}
}