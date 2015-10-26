using System;
using System.Collections.Generic;
using TReal = System.Single;

namespace PipeView
{
	public interface ISeriesWithIncrementalLoading : ISeriesWithAttributes
	{
		void Append(ICollection<TReal> x, ICollection<TReal> y, ICollection<TReal> w, ICollection<TReal> h,
			ICollection<object> atts);

		event Action AppendingChunk;
		event Action AppendedChunk;
	}
}