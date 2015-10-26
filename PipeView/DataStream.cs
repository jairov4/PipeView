using System;
using System.Collections;
using System.Collections.Generic;

namespace PipeView
{
	public class DataStream
	{
		public DataStream(IReadOnlyList<string> fieldNames, IReadOnlyList<Type> fieldTypes, IEnumerable<DataStreamValue> stream)
		{
			FieldNames = fieldNames;
			FieldTypes = fieldTypes;
			Stream = stream;
		}

		public IReadOnlyList<string> FieldNames { get; }
		public IReadOnlyList<Type> FieldTypes { get; }
		public IEnumerable<DataStreamValue> Stream { get; }
	}
}