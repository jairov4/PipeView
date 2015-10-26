using System;
using System.Collections.Generic;
using static PipeView.CodeContracts;
using TReal = System.Single;

namespace PipeView
{
	public class Series2 : Series1, ISeriesWithAttributes
	{
		public virtual IReadOnlyList<string> AttributeNames { get; }
		public virtual IReadOnlyList<object> AttributeValues { get; }
		public virtual IReadOnlyList<Type> AttributeTypes { get; }

		public object GetAttribute(int index, int attributeIndex)
		{
			Requires(index < Count);
			Requires(attributeIndex < AttributeNames.Count);
			var idx = index*AttributeNames.Count + attributeIndex;
			return AttributeValues[idx];
		}

		public Series2(IReadOnlyList<float> xValues, IReadOnlyList<float> yValues, IReadOnlyList<float> widthValues, IReadOnlyList<float> heightValues, IReadOnlyList<string> attributeNames, IReadOnlyList<Type> attributeTypes, IReadOnlyList<object> attributeValues) 
			: base(xValues, yValues, heightValues, widthValues)
		{
			RequiresNotNull(attributeNames, attributeTypes, attributeValues);
			Requires(attributeNames.Count == attributeTypes.Count);
			Requires(attributeValues.Count * attributeNames.Count == xValues.Count);

			AttributeNames = attributeNames;
			AttributeTypes = attributeTypes;
			AttributeValues = attributeValues;
		}
		
	}
}