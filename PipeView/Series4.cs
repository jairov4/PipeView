using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Abt.Controls.SciChart.Numerics.CoordinateCalculators;
using Abt.Controls.SciChart.Rendering.Common;
using TReal = System.Single;

namespace PipeView
{
	public class Series4 : Series3, ISeriesWithDecimation
	{
		private TreeNode rootNode = new TreeNode() {Height = 1};

		public override void AppendCore(ICollection<TReal> x, ICollection<TReal> y, ICollection<TReal> w, ICollection<TReal> h, ICollection<object> atts)
		{
			var idx = Count;
			base.AppendCore(x, y, w, h, atts);
			/*
			for (var i = idx; i < Count; i++)
			{
				Insert(i, rootNode.Height - 1);
			}
			*/
			BulkInsert(idx);
		}

		private void BulkInsert(int begin)
		{
			if (Count - begin < MinItemsPerNode)
			{
				for (var i = begin; i < Count; i++)
				{
					Insert(i, rootNode.Height - 1);
				}
				return;
			}

			var node = Build(begin, Count - 1, 0);
			
			if (!rootNode.HasData)
			{
				// save as is if tree is empty
				rootNode = node;
			}
			else if (this.rootNode.Height == node.Height)
			{
				// split root if trees have the same height
				var newNode = new TreeNode();
				newNode.Height = rootNode.Height + 1;
				newNode.Nodes.Add(rootNode);
				newNode.Nodes.Add(node);
				RecomputeBoundingBox(newNode);
				rootNode = newNode;
			}
			else
			{
				if (rootNode.Height < node.Height)
				{
					// swap trees if inserted one is bigger
					var tmpNode = rootNode;
					rootNode = node;
					node = tmpNode;
				}

				// insert the small tree into the large tree at appropriate level
				InsertNode(node, rootNode.Height - node.Height - 1);
			}
		}
		
		private TreeNode Build(int begin, int endInclusive, int height)
		{
			var N = endInclusive - begin + 1;
			var M = MaxItemsPerNode;
			var node = new TreeNode();

			if (N <= M)
			{
				node.Height = 1;
				node.Data.AddRange(Enumerable.Range(begin, N));
				RecomputeBoundingBox(node);
				return node;
			}

			if (height == 0)
			{
				// target height of the bulk-loaded tree
				height = (int)Math.Ceiling(Math.Log(N) / Math.Log(M));

				// target number of root entries to maximize storage utilization
				M = (int)Math.Ceiling(N / Math.Pow(M, height - 1));
			}

			node.Height = height;

			var N2 = N/M + (N % M > 0 ? 1 : 0);
			var N1 = N2*(int) Math.Ceiling(Math.Sqrt(M));
			MultiSelect(begin, endInclusive, N1, x);

			for (var i = begin; i <= endInclusive; i += N1)
			{
				var right2 = Math.Min(i + N1 - 1, endInclusive);
				MultiSelect(i, right2, N2, y);

				for (var j = i; j <= right2; j += N2)
				{
					var right3 = Math.Min(j + N2 - 1, right2);

					// pack each entry recursively
					var n = Build(j, right3, height - 1);
					node.Nodes.Add(n);
				}
			}

			RecomputeBoundingBox(node);
			return node;
		}

		private void MultiSelect(int left, int right, int n, IReadOnlyList<TReal> arr)
		{
			var stack = new Stack<int>();
			stack.Push(left);
			stack.Push(right);

			while (stack.Count > 0)
			{
				right = stack.Pop();
				left = stack.Pop();

				if (right - left <= n) continue;

				var rl = right - left;
				var n2 = n*2;
				var div = rl/n2 + (rl%n2 > 0 ? 1 : 0);

				var mid = left + div * n;
				Select(left, right, mid, arr);

				stack.Push(left);
				stack.Push(mid);
				stack.Push(mid);
				stack.Push(right);
			}
		}

		private void Select(int left, int right, int k, IReadOnlyList<TReal> arr)
		{
			while (right > left)
			{
				int i;
				if (right - left > 600)
				{
					var n = right - left + 1;
					i = k - left + 1;
					var z = Math.Log(n);
					var s = 0.5 * Math.Exp(2 * z / 3);
					var sd = 0.5 * Math.Sqrt(z * s * (n - s) / n) * Math.Sign(i - n / 2);
					var newLeft = (int)Math.Max(left, k - i * s / n + sd);
					var newRight = (int)Math.Min(right, k + (n - i) * s / n + sd);
					Select(newLeft, newRight, k, arr);
				}
				
				i = left;
				var j = right;
				var t = arr[k];

				MultiSelectSwapByIndices(left, k);
				
				if (arr[right] > t)
				{
					MultiSelectSwapByIndices(right, left);
				}

				while (i < j)
				{
					MultiSelectSwapByIndices(i, j);
					i++;
					j--;
					while (arr[i] < t) i++;
					while (arr[j] > t) j--;
				}

				if (Math.Abs(arr[left] - t) < 2048*TReal.Epsilon)
				{
					MultiSelectSwapByIndices(left, j);
				}
				else
				{
					j++;
					MultiSelectSwapByIndices(j, right);
				}

				if (j <= k) left = j + 1;
				if (k <= j) right = j - 1;
			}
		}

		private void MultiSelectSwapByIndices(int i, int j)
		{
			ItemsSwap(x, i, j);
			ItemsSwap(y, i, j);
			ItemsSwap(w, i, j);
			ItemsSwap(h, i, j);
			ItemsSwap(attributeValues, i, j);
		}

		void ItemsSwap<T>(IList<T> l, int a, int b)
		{
			var tmp = l[a];
			l[a] = l[b];
			l[b] = tmp;
		}
		
		private void InsertNode(TreeNode item, int level)
		{
			var path = new List<TreeNode>(level);
			var bbox = item.Rect;
			TreeNode node;
			ChooseSubTree(bbox, level, rootNode, path, out node);
			node.Nodes.Add(item);
			Extend(node, bbox);
			while (level >= 0)
			{
				var currentNode = path[level];
				if ((currentNode.IsLeaf && currentNode.Data.Count > MaxItemsPerNode) ||
					(!currentNode.IsLeaf && currentNode.Nodes.Count > MaxItemsPerNode))
				{
					var n = Split(currentNode);
					path.Add(n);
					level--;
				}
				else break;
			}

			foreach (var treeNode in path)
			{
				RecomputeBoundingBox(treeNode);
			}
		}

		private void Insert(int item, int level)
		{
			var path = new List<TreeNode>(level);
			var bbox = ComputeBoundingBox(item);
			TreeNode node;
			ChooseSubTree(bbox, level, rootNode, path, out node);
			node.Data.Add(item);
			Extend(node, bbox);
			while (level >= 0)
			{
				var currentNode = path[level];
				if ((currentNode.IsLeaf && currentNode.Data.Count > MaxItemsPerNode) ||
					(!currentNode.IsLeaf && currentNode.Nodes.Count > MaxItemsPerNode))
				{
					var n = Split(currentNode);
					path.Add(n);
					level--;
				}
				else break;
			}

			foreach (var treeNode in path)
			{
				RecomputeBoundingBox(treeNode);
			}
		}

		void ChooseSubTree(Rect bbox, int level, TreeNode node, List<TreeNode> path, out TreeNode item)
		{
			while (true)
			{
				path.Add(node);
				if (node.IsLeaf || (path.Count - 1 == level)) break;
				var minArea = TReal.MaxValue;
				var minElargement = TReal.MaxValue;
				
				TreeNode targetNode = null;
				var xAndW = (TReal)bbox.Right;
				var yAndH = (TReal)bbox.Bottom;
				var xx = (TReal) bbox.X;
				var yy = (TReal)bbox.Y;
				foreach (var child in node.Nodes)
				{
					var area = child.HorizontalRange.Diff*child.VerticalRange.Diff;
					//var r = child.Rect;
					//r.Union(bbox);
					//var enlargement = r.Width * r.Height - area;
					var enlargedArea = 
						(Max(xAndW, child.HorizontalRange.Max) - Min(xx, child.HorizontalRange.Min))*
						(Max(yAndH, child.VerticalRange.Max) - Min(yy, child.VerticalRange.Min));
					var enlargement = enlargedArea - area;
					if (enlargement < minElargement)
					{
						minElargement = (TReal) enlargement;
						minArea = area < minArea ? area : minArea;
						targetNode = child;
					}
					else if (minElargement == enlargement)
					{
						if (area < minArea)
						{
							minArea = area < minArea ? area : minArea;
							targetNode = child;
						}
					}
				}

				node = targetNode;
			}

			item = node;
		}

		Rect ComputeBoundingBox(int i)
		{
			var xv = XValues[i];
			var yv = YValues[i];
			var wv = WidthValues[i];
			var hv = HeightValues[i];
			return new Rect(xv, yv, wv, hv);
		}

		const int MinItemsPerNode = 6;

		private TreeNode Split(TreeNode found)
		{
			// choose split axis
			found.Data.Sort((i, j) => XValues[i] < XValues[j] ? -1 : 1);
			TReal xmargin = CalcAllDistributionsMargin(found, MinItemsPerNode, MaxItemsPerNode);

			found.Data.Sort((i, j) => YValues[i] < YValues[j] ? -1 : 1);
			TReal ymargin = CalcAllDistributionsMargin(found, MinItemsPerNode, MaxItemsPerNode);

			if (xmargin < ymargin)
			{
				found.Data.Sort((i, j) => XValues[i] < XValues[j] ? -1 : 1);
			}

			// choose split index
			var minOverlap = TReal.MaxValue;
			var minArea = TReal.MaxValue;
			var splitIndex = int.MaxValue;
			for (var i = MinItemsPerNode; i <= (MaxItemsPerNode - MinItemsPerNode); i++)
			{
				TReal xmax, xmin, ymax, ymin;
				ComputeBoundingBox(found, 0, i, out xmin, out xmax, out ymin, out ymax);
				var bbox1 = new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
				ComputeBoundingBox(found, i, MaxItemsPerNode, out xmin, out xmax, out ymin, out ymax);
				var bbox2 = new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
				var areaTotal = (TReal)(bbox1.Width * bbox1.Height + bbox2.Width * bbox2.Height);
				bbox1.Intersect(bbox2);
				if (!bbox1.IsEmpty)
				{
					var overlap = (TReal) (bbox1.Width*bbox1.Height);
					if (overlap < minOverlap)
					{
						minOverlap = overlap;
						splitIndex = i;
					}
				}
				else if (areaTotal < minArea)
				{
					minOverlap = 0;
					minArea = areaTotal;
					splitIndex = i;
				}
			}

			// setup parent
			var parent = found.Parent;
			if (parent == null)
			{
				parent = new TreeNode();
				parent.Height = found.Height + 1;
				found.Parent = parent;
				parent.Nodes.Add(found);
				rootNode = parent;
			}
			else if (parent.Nodes.Count >= MaxItemsPerNode)
			{
				parent = Split(parent);
			}

			// append new node
			var newNode = new TreeNode();
			newNode.Height = found.Height;
			if (found.IsLeaf)
			{
				newNode.Data.AddRange(found.Data.Skip(splitIndex));
				found.Data.RemoveRange(splitIndex, found.Data.Count - splitIndex);
			}
			else
			{
				newNode.Nodes.AddRange(found.Nodes.Skip(splitIndex));
				found.Nodes.RemoveRange(splitIndex, found.Nodes.Count - splitIndex);
			}
			newNode.Parent = parent;
			parent.Nodes.Add(newNode);
			
			RecomputeBoundingBox(found);
			RecomputeBoundingBox(newNode);
			RecomputeBoundingBox(parent);

			return newNode;
		}

		private TReal CalcAllDistributionsMargin(TreeNode found, int minItemsPerNode, int maxItemsPerNode)
		{
			TReal xmax, xmin, ymax, ymin;

			ComputeBoundingBox(found, 0, minItemsPerNode, out xmin, out xmax, out ymin, out ymax);
			var left = new Rect(xmin, ymin, xmax - xmin, ymax - ymin);

			ComputeBoundingBox(found, maxItemsPerNode - minItemsPerNode, maxItemsPerNode, out xmin, out xmax, out ymin, out ymax);
			var right = new Rect(xmin, ymin, xmax - xmin, ymax - ymin);

			Func<Rect, TReal> getMargin = x => (TReal) (x.Width*2 + x.Height*2);
			var margin = getMargin(left) + getMargin(right);

			for (var i = minItemsPerNode; i < (maxItemsPerNode-minItemsPerNode); i++)
			{
				ComputeBoundingBox(found, i, i + 1, out xmin, out xmax, out ymin, out ymax);
				var r = new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
				left.Union(r);
				margin += getMargin(left);
			}

			for (var i = (maxItemsPerNode - minItemsPerNode -1); i >= minItemsPerNode; i--)
			{
				ComputeBoundingBox(found, i, i + 1, out xmin, out xmax, out ymin, out ymax);
				var r = new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
				right.Union(r);
				margin += getMargin(right);
			}

			return margin;
		}

		private void RecomputeBoundingBox(TreeNode node)
		{
			if (!node.HasData) return;

			TReal xmax, xmin, ymax, ymin;
			ComputeBoundingBox(node, 0, int.MaxValue, out xmin, out xmax, out ymin, out ymax);

			node.HorizontalRange = new VisibleRange(xmin, xmax);
			node.VerticalRange = new VisibleRange(ymin, ymax);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static TReal Max(TReal a,TReal b)
		{
			return a < b ? b : a;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static TReal Min(TReal a, TReal b)
		{
			return a < b ? a : b;
		}

		private void ComputeBoundingBox(TreeNode node, int start,int end, out float xmin, out float xmax, out float ymin, out float ymax)
		{
			xmax = TReal.MinValue; xmin = TReal.MaxValue;
			ymax = TReal.MinValue; ymin = TReal.MaxValue;
			
			if (node.IsLeaf)
			{
				end = Math.Min(end, node.Data.Count);
				start = Math.Min(start, node.Data.Count);
				for (var j = start; j < end; j++)
				{
					var i = node.Data[j];
					xmax = Max(xmax, XValues[i] + WidthValues[i]);
					xmin = Min(xmin, XValues[i]);
					ymax = Max(ymax, YValues[i] + HeightValues[i]);
					ymin = Min(ymin, YValues[i]);
				}
			}
			else
			{
				end = Math.Min(end, node.Nodes.Count);
				start = Math.Min(start, node.Nodes.Count);
				for (var j = start; j < end; j++)
				{
					var child = node.Nodes[j];
					xmin = Min(xmin, child.HorizontalRange.Min);
					xmax = Max(xmax, child.HorizontalRange.Max);
					ymin = Min(ymin, child.VerticalRange.Min);
					ymax = Max(ymax, child.VerticalRange.Max);
				}
			}
		}

		private void Extend(TreeNode found, Rect rect)
		{
			while (found != null)
			{
				var r = rect;

				// solo confiar en rect cuando hay data
				if (found.HasData)
				{
					r.Union(found.Rect);
				}
				found.HorizontalRange = new VisibleRange((TReal) r.X, (TReal) (r.X + r.Width));
				found.VerticalRange = new VisibleRange((TReal) r.Y, (TReal) (r.Y + r.Height));
				found = found.Parent;
			}
		}

		private TreeNode SearchLeafToExtend(IEnumerable<TreeNode> enumerateLeafs, Rect rect)
		{
			var minDiff = double.MaxValue;
			TreeNode minAreaChangeNode = null;
			
			foreach (var leaf in enumerateLeafs)
			{
				var r = leaf.Rect;
				var areaBefore = r.Height*r.Width;
				r.Union(rect);
				var areaAfter = r.Height*r.Width;
				var diff = areaAfter - areaBefore;
				if (diff < minDiff)
				{
					minDiff = diff;
					minAreaChangeNode = leaf;
				}
			}

			if (minAreaChangeNode == null)
			{
				throw new ArgumentOutOfRangeException();
			}

			return minAreaChangeNode;
		}

		public int MaxItemsPerNode { get; private set; }

		private IEnumerable<TreeNode> FindNodes(Rect rect, bool leafsOnly)
		{
			var remaining = new List<TreeNode> {rootNode};
			for (var i = 0; i < remaining.Count; i++)
			{
				var node = remaining[i];
				if (!rect.IntersectsWith(node.Rect)) continue;
				remaining.AddRange(node.Nodes);
				if (node.IsLeaf || !leafsOnly)
				{
					yield return node;
				}
			}
		}

		private IEnumerable<TreeNode> FindNodes(Point p, bool leafsOnly)
		{
			var remaining = new List<TreeNode> { rootNode };
			for (var i = 0; i < remaining.Count; i++)
			{
				var node = remaining[i];
				if (!node.Rect.Contains(p)) continue;
				remaining.AddRange(node.Nodes);
				if (node.IsLeaf || !leafsOnly)
				{
					yield return node;
				}
			}
		}

		public override IEnumerable<int> FindInArea(Rect area)
		{
			var nodes = FindNodes(area, true);
			return from node in nodes
				   from i in node.Data
				   let x = XValues[i]
				   let y = YValues[i]
				   let w = WidthValues[i]
				   let h = HeightValues[i]
				   let p = new Rect(x,y,w,h)
				   where area.IntersectsWith(p)
				   select i;
		}

		private IEnumerable<TreeNode> FindNodesWithDecimation(Rect rect, TReal pixSizeX, TReal pixSizeY)
		{
			var remaining = new List<TreeNode> { rootNode };
			for (var i = 0; i < remaining.Count; i++)
			{
				var node = remaining[i];
				var nodeRect = node.Rect;
				if (!rect.IntersectsWith(nodeRect)) continue;
				if (nodeRect.Width < pixSizeX || nodeRect.Height < pixSizeY || node.IsLeaf)
				{
					// skip lower levels
					yield return node;
					continue;
				}
				remaining.AddRange(node.Nodes);
			}
		}

		public IEnumerable<Rect> FindInArea(Rect area, TReal pixSizeX, TReal pixSizeY)
		{
			var nodes = FindNodesWithDecimation(area, pixSizeX, pixSizeY);
			foreach (var node in nodes)
			{
				if (node.IsLeaf)
				{
					var r =
						from i in node.Data
						let x = XValues[i]
						let y = YValues[i]
						let w = WidthValues[i]
						let h = HeightValues[i]
						let p = new Rect(x, y, w, h)
						where area.IntersectsWith(p)
						select p;
					foreach (var rect in r)
					{
						yield return rect;
					}
				}
				else
				{
					yield return node.Rect;
				}
			}
		}

		private const int DefaultMaxItemsPerNode = 16;

		public Series4(IReadOnlyList<string> names, IReadOnlyList<Type> types) : this(DefaultMaxItemsPerNode, 0, names, types)
		{
		}

		public Series4(int maxItemsPerNode, int capacity, IReadOnlyList<string> names, IReadOnlyList<Type> types) : base(capacity, names, types)
		{
			MaxItemsPerNode = maxItemsPerNode;
		}

		public void DrawNodes(IRenderContext2D renderContext, ICoordinateCalculator<double> xc, ICoordinateCalculator<double> yc, Rect area, float pixSizeX, float pixSizeY)
		{
			var pen = renderContext.CreatePen(Colors.Brown, false, 1);
			var nodes = FindNodesWithDecimation(area, pixSizeX, pixSizeY);
			foreach (var node in nodes)
			{
				var p1 = new Point(xc.GetCoordinate(node.HorizontalRange.Min), yc.GetCoordinate(node.VerticalRange.Min));
				var p2 = new Point(xc.GetCoordinate(node.HorizontalRange.Max), yc.GetCoordinate(node.VerticalRange.Max));
				renderContext.DrawQuad(pen, p1, p2);
			}
			pen.Dispose();
		}

		public override int? FindNearest(Point point)
		{
			var nn = FindNodes(point, true);
			var minR = double.MaxValue;
			int? found = null;
			foreach (var n in nn)
			{
				foreach (var i in n.Data)
				{
					var x = XValues[i] + WidthValues[i]/2;
					var y = YValues[i] + HeightValues[i]/2;
					var dx = point.X - x;
					var dy = point.Y - y;
					var r = Math.Sqrt(dx*dx + dy*dy);
					if (r < minR)
					{
						minR = r;
						found = i;
					}
				}
			}
			return found;
		}
	}
}