using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PipeView
{
	public class TreeNode
	{
		public TreeNode Parent { get; set; }

		public List<TreeNode> Nodes { get; }

		public List<int> Data { get; }

		public int Height { get; set; }

		public bool IsLeaf => Nodes.Count == 0;

		public VisibleRange VerticalRange { get; set; }

		public VisibleRange HorizontalRange { get; set; }

		public Rect Rect => new Rect(HorizontalRange.Min, VerticalRange.Min, HorizontalRange.Diff, VerticalRange.Diff);

		public bool IsRoot => Parent == null;
		public bool HasData => Data.Count != 0 || Nodes.Count != 0;

		public IEnumerable<int> EnumerateAll()
		{
			var r = Enumerable.Empty<int>();
			r = r.Concat(Data);
			r = r.Concat(Nodes.SelectMany(x => x.EnumerateAll()));
			return r;
		}

		public IEnumerable<TreeNode> EnumerateLeafs()
		{
			if (IsLeaf)
			{
				yield return this;
				yield break;
			}

			var remaining = new List<TreeNode>(Nodes);
			for (int i = 0; i < remaining.Count; i++)
			{
				var n = remaining[i];
				if (n.IsLeaf) yield return n;
				else remaining.AddRange(n.Nodes);
			}
		}

		public TreeNode()
		{
			Data = new List<int>();
			Nodes = new List<TreeNode>();
		}
	}
}