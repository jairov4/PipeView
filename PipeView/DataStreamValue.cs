using System.Collections.Generic;

namespace PipeView
{
	public struct DataStreamValue
	{
		public DataStreamValue(IReadOnlyList<object> values)
		{
			Values = values;
		}

		public  IReadOnlyList<object> Values { get; }
	}
}