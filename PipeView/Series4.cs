using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TReal = System.Single;

namespace PipeView
{
	public class Series4 : Series3, ISeriesWithDecimation
	{
		private TreeNode rootNode = new TreeNode() {Height = 1};

		public override void AppendCore(ICollection<TReal> x, ICollection<TReal> y, ICollection<TReal> w, ICollection<TReal> h, ICollection<object> atts)
		{
			AppendSingle(x, y, w, h, atts);
			//AppendBulk(x, y, w, h, atts);
		}

		private void AppendSingle(ICollection<TReal> x, ICollection<TReal> y, ICollection<TReal> w, ICollection<TReal> h, ICollection<object> atts)
		{
			var idx = Count;
			base.AppendCore(x, y, w, h, atts);
			for (var i = idx; i < Count; i++)
			{
				//var rect = new Rect(XValues[i], YValues[i], WidthValues[i], HeightValues[i]);
				//var nodes = FindNodes(rect, false);
				//AddPoint(nodes, i, rect);
				Insert(i, rootNode.Height - 1);
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
				foreach (var child in node.Nodes)
				{
					var r = child.Rect;
					var area = child.HorizontalRange.Diff*child.VerticalRange.Diff;
					r.Union(bbox);
					var enlargement = r.Width*r.Height - area;
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

		public void AppendBulk(ICollection<TReal> x, ICollection<TReal> y, ICollection<TReal> w, ICollection<TReal> h, ICollection<object> atts)
		{
			var idx = Count;
			base.AppendCore(x, y, w, h, atts);
			var minx = TReal.MaxValue;
			var maxx = TReal.MinValue;
			var miny = TReal.MaxValue;
			var maxy = TReal.MinValue;
			for(var i = idx; i < Count; i++)
			{
				minx = Math.Min(minx, XValues[i]);
				maxx = Math.Max(maxx, XValues[i] + WidthValues[i]);
				miny = Math.Min(miny, YValues[i]);
				maxy = Math.Max(maxy, YValues[i] + HeightValues[i]);
			}

			var width = maxx - minx;
			var height = maxy - miny;

			var added = Count - idx;
			var densityX = width / added;
			var densityY = height / added;

			var blockSizeXTarget = 128*densityX;
			var blocksPerRow = 4;
			TReal blockSizeX= width / blocksPerRow;
			while (blockSizeX > blockSizeXTarget)
			{
				blocksPerRow *= 2;
				blockSizeX = width / blocksPerRow;
			}
			
			var blockSizeYTarget = 128*densityY;
			var blocksPerCol = 4;
			TReal blockSizeY = height / blocksPerCol;
			while (blockSizeY > blockSizeYTarget)
			{
				blocksPerCol *= 2;
				blockSizeY = height / blocksPerCol;
			}

			var leaves = new Dictionary<int, TreeNode>();
			for (var i = idx; i < Count; i++)
			{
				var xv = XValues[i] - minx;
				var yv = YValues[i] - miny;
				var bx = (int) (xv/blockSizeX);
				var by = (int) (yv/blockSizeY);
				var bid = bx*blocksPerCol + by;
				TreeNode block;
				if (!leaves.TryGetValue(bid, out block))
				{
					block = new TreeNode();
					leaves.Add(bid, block);
				}
				block.Data.Add(i);
			}

			var l = new List<TreeNode>(leaves.Values);
			foreach (var treeNode in l)
			{
				RecomputeBoundingBox(treeNode);
			}
			l.Sort((r, v) => r.HorizontalRange.Min < v.HorizontalRange.Min ? -1 : 1);
			
			var group = new Dictionary<int, TreeNode>();
			while (blocksPerCol > 0 && blocksPerRow > 0)
			{
				// itera sobre el rango original
				for (int bx = 0; bx < blocksPerRow; bx += 2)
				{
					for (int by = 0; by < blocksPerCol; by += 2)
					{
						var newNode = new TreeNode();
						// crea un id en un rango nuevo
						var blockId = bx/2*blocksPerCol/2 + by/2;
						
						for (int i = 0; i < 2; i++)
						{
							for (int j = 0; i < 2; i++)
							{
								if (bx + i >= blocksPerRow) continue;
								if (by + j >= blocksPerCol) continue;
								// crea id en el rango original
								var bid = (bx + i)*blocksPerCol + (by + j);
								TreeNode n;
								if (leaves.TryGetValue(idx, out n))
								{
									newNode.Nodes.Add(n);
									n.Parent = newNode;
									RecomputeBoundingBox(n);
								}
							}
						}

						if (newNode.HasData)
						{
							group.Add(blockId, newNode);
						}
					}
				}
				var tmp = leaves;
				leaves = group;
				group = tmp;
				group.Clear();

				blocksPerCol /= 2;
				blocksPerRow /= 2;
			}

			var root = new TreeNode();
			RecomputeBoundingBox(root);
			root.Nodes.AddRange(leaves.Values);
			foreach (var value in leaves.Values)
			{
				value.Parent = root;
				RecomputeBoundingBox(value);
			}

			rootNode = root;
		}

		private void AddPoint(IEnumerable<TreeNode> nodes, int i, Rect rect)
		{
			var node = rootNode;
			foreach (var quadTreeNode in nodes)
			{
				node = quadTreeNode;
				if (quadTreeNode.IsLeaf)
				{
					break;
				}
			}

			if (!node.IsLeaf)
			{
				node = SearchLeafToExtend(node.EnumerateLeafs(), rect);
			}

			if (node.Data.Count >= MaxItemsPerNode)
			{
				var newNode = Split(node);
				node = SearchLeafToExtend(new[] {newNode, node}, rect);
			}

			if (!node.Rect.Contains(rect))
			{
				Extend(node, rect);
			}

			node.Data.Add(i);
		}
		
		private TreeNode Split(TreeNode found)
		{
			const int MinItemsPerNode = 4;

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
					xmax = Math.Max(xmax, XValues[i] + WidthValues[i]);
					xmin = Math.Min(xmin, XValues[i]);
					ymax = Math.Max(ymax, YValues[i] + HeightValues[i]);
					ymin = Math.Min(ymin, YValues[i]);
				}
			}
			else
			{
				end = Math.Min(end, node.Nodes.Count);
				start = Math.Min(start, node.Nodes.Count);
				for (var j = start; j < end; j++)
				{
					var child = node.Nodes[j];
					xmin = Math.Min(xmin, child.HorizontalRange.Min);
					xmax = Math.Max(xmax, child.HorizontalRange.Max);
					ymin = Math.Min(ymin, child.VerticalRange.Min);
					ymax = Math.Max(ymax, child.VerticalRange.Max);
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
	}
}