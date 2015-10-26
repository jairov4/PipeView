using System;
using System.Collections.Generic;

namespace PipeView
{
	public interface ISeriesWithAttributes : ISeries
	{
		IReadOnlyList<string> AttributeNames { get; }
		IReadOnlyList<object> AttributeValues { get; }
		IReadOnlyList<Type> AttributeTypes { get; }

		object GetAttribute(int index, int attributeIndex);
	}
}