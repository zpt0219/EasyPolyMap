//A refined version of Delaunay Triangulation Algorithm

using System.Collections;
using System.Collections.Generic;

namespace EasyPolyMap.Core
{
	public class  EPMLinkedList<T>
	{
		private class Node
		{
			public T value = default(T);
			public Node pre = null;
			public Node next = null;
		};

		Node current = null;
		Node head = null;
		Node tail = null;

		public EPMLinkedList()
		{
			head = new Node();
			tail = new Node();
			head.next = tail;
			tail.pre = head;
		}

		public void Push_Back(T obj)
		{
			Node t = new Node();
			t.value = obj;
			t.pre = tail.pre;
			t.next = tail;
			tail.pre.next = t;
			tail.pre = t;
		}

		public T Pop_Back()
		{
			Node t = tail.pre;
			tail.pre = t.pre;
			t.pre.next = tail;
			return t.value;
		}

		public void Push_Front(T obj)
		{
			Node t = new Node();
			t.value = obj;
			t.next = head.next;
			t.pre = head;
			head.next.pre = t;
			head.next = t;
		}

		public T Pop_Front()
		{
			Node t = head.next;
			head.next = t.next;
			t.next.pre = head;
			return t.value;
		}

		public void ResetCurrentToHead()
		{
			current = head;
		}

		public void ResetCurrentToTail()
		{
			current = tail;
		}

		public T DeleteCurrentAndMoveForward()
		{
			current.pre.next = current.next;
			current.next.pre = current.pre;

			return MoveForward();
		}

		public T DeleteCurrentAndMoveBack()
		{
			current.pre.next = current.next;
			current.next.pre = current.pre;

			return MoveBack();
		}

		public T MoveForward()
		{
			current = current.next;
			if (current != tail)
			{
				return current.value;
			}
			else return default(T);
		}

		public T MoveBack()
		{
			current = current.pre;
			if (current != head)
			{
				return current.value;
			}
			else return default(T);
		}
	}

	public class EPMDelaunayAlgorithm
	{
		//The EPMTriangle class contains many information the generating algorithm doesn't use, since the algorithm generates many temporary triangles, I implement a lite triangle version here.
		private class EPMLiteTriangle
		{
			public EPMPoint[] g_pointList = new EPMPoint[3];
			public Vector2d g_outerCircleOrigin;
			public double g_outerCicleRadius;
			public bool g_visited = false;

			public EPMPoint this[int index]
			{
				get
				{
					return g_pointList[index];
				}
				set
				{
					g_pointList[index] = value;
				}
			}
			public EPMLiteTriangle(EPMPoint p1, EPMPoint p2, EPMPoint p3)
			{
				g_pointList[0] = p1;
				g_pointList[1] = p2;
				g_pointList[2] = p3;
			}

			public void CalculateOuterCircle()
			{
				g_outerCircleOrigin = EPMGlobal.GetOuterCircleOrigin(g_pointList[0].pos2d, g_pointList[1].pos2d, g_pointList[2].pos2d);
				g_outerCicleRadius = g_outerCircleOrigin.Distance(g_pointList[0].pos2d);
			}

			public bool HasPoint(EPMPoint p)
			{
				for (int i = 0; i < g_pointList.Length; i++)
				{
					if (g_pointList[i] == p) return true;
				}
				return false;
			}
		}

		//The divideYCount is set by experiment....
		public static List<EPMTriangle> DoDelaunay(List<EPMPoint> pointList, Vector2d regionStart, Vector2d regionEnd, int divideYCount = 12)
		{
			//Shift point's the postion, Treat regionStart as Origin Point;
			if (regionStart.x != 0 || regionStart.y != 0)
			{
				for (int i = 0; i < pointList.Count; i++)
				{
					pointList[i].pos2d -= regionStart;
				}
			}
			regionEnd -= regionStart;

			//Make a super triangle, All the points are in this super triangle.
			EPMPoint p1, p2, p3;
			double regionWidth = regionEnd.x;
			double regionHeight = regionEnd.y;
			p1 = new EPMPoint(EPMPoint.PointType.Ground, new Vector3d(-regionHeight, 0, -100));
			p2 = new EPMPoint(EPMPoint.PointType.Ground, new Vector3d(regionEnd.x + regionHeight, 0, p1.posZ));
			double tempRatio = regionHeight / (regionHeight + regionWidth / 2);
			double p3y = (regionHeight + 100f) / tempRatio - 100f;
			p3y += regionHeight;
			p3 = new EPMPoint(EPMPoint.PointType.Ground, new Vector3d((regionEnd.x) / 2, 0, p3y));

			p1.g_indexInList = pointList.Count;
			p2.g_indexInList = p1.g_indexInList + 1;
			p3.g_indexInList = p2.g_indexInList + 1;
			EPMLiteTriangle superTriangle = new EPMLiteTriangle(p1, p2, p3);
			superTriangle.CalculateOuterCircle();


			//triangleList saves the determined triangle.
			List<EPMLiteTriangle> triangleList = new List<EPMLiteTriangle>();
			//tempTriangleList save all the uncertain triangles, put the triangle into different groups according by its outerCircle's coverage on Y-Axis.
			EPMLinkedList<EPMLiteTriangle>[] tempTriangleList = new EPMLinkedList<EPMLiteTriangle>[divideYCount];
			//the pointer to current section of tempTriangleList.
			EPMLinkedList<EPMLiteTriangle> currentSection = null;
			//Use Dictionary to quickly remove duplicate edge. In the test, the edgeDic can at most contains less than 100 edges. But I still can't figure out a way to defeat C#'s dictionary....
			Dictionary<long, EPMEdge> tempEdgeDic = new Dictionary<long, EPMEdge>();

			//Sort the input points by x coordinate.
			pointList.Sort(delegate (EPMPoint comparePoint1, EPMPoint comparePoint2)
			{
				if (comparePoint1.posX > comparePoint2.posX) return 1;
				else if (comparePoint1.posX == comparePoint2.posX) return 0;
				else return -1;
			});

			//reset the index saved in each point,index is used by edges to generate hash. Hash could accelerate comparation.
			for (int i = 0; i < pointList.Count; i++)
			{
				pointList[i].g_indexInList = i;
			}

			//Put the super triangle into tempList, Super triangle absolutely cover all groups of tempTriangleList.
			for (int i = 0; i < tempTriangleList.Length; i++)
			{
				tempTriangleList[i] = new EPMLinkedList<EPMLiteTriangle>();
				tempTriangleList[i].Push_Back(superTriangle);
			}

			double YSEGMENT = regionHeight / tempTriangleList.Length;


			//Begin Algorithm.
			for (int i = 0; i < pointList.Count; i++)
			{
				tempEdgeDic.Clear();
				EPMPoint cp = pointList[i];
				int section = (int)((cp.posZ) / YSEGMENT);
				if (section >= tempTriangleList.Length) section = tempTriangleList.Length - 1;
				currentSection = tempTriangleList[section];
				currentSection.ResetCurrentToHead();
				EPMLiteTriangle ct = currentSection.MoveForward();
				while (ct != null)
				{
					if (ct.g_visited)
					{
						ct = currentSection.DeleteCurrentAndMoveForward();
						continue;
					}

					Vector2d origin = ct.g_outerCircleOrigin;
					double radius = ct.g_outerCicleRadius;

					if (cp.posX - origin.x > radius)
					{
						triangleList.Add(ct);
						ct.g_visited = true;
						ct = currentSection.DeleteCurrentAndMoveForward();
						continue;
					}
					else if (cp.pos2d.SquaredDistance(origin) > radius * radius)
					{
						ct = currentSection.MoveForward();
						continue;
					}
					else
					{
						TryAddEdge(ref tempEdgeDic, new EPMEdge(ct[0], ct[1]));
						TryAddEdge(ref tempEdgeDic, new EPMEdge(ct[1], ct[2]));
						TryAddEdge(ref tempEdgeDic, new EPMEdge(ct[2], ct[0]));

						ct.g_visited = true;
						ct = currentSection.DeleteCurrentAndMoveForward();
					}
				}

				foreach (EPMEdge e in tempEdgeDic.Values)
				{
					if (e.g_visited) continue;
					EPMLiteTriangle tt = new EPMLiteTriangle(cp, e.g_startPoint, e.g_endPoint);
					tt.CalculateOuterCircle();

					int lowerBound = (int)((tt.g_outerCircleOrigin.y - tt.g_outerCicleRadius) / YSEGMENT);
					if (lowerBound < 0) lowerBound = 0;
					int upperBound = (int)((tt.g_outerCircleOrigin.y + tt.g_outerCicleRadius) / YSEGMENT);
					if (upperBound >= tempTriangleList.Length) upperBound = tempTriangleList.Length - 1;

					for (int k = lowerBound; k <= upperBound; k++)
					{
						tempTriangleList[k].Push_Back(tt);
					}
				}

			}

			for (int i = 0; i < tempTriangleList.Length; i++)
			{
				currentSection = tempTriangleList[i];
				currentSection.ResetCurrentToHead();
				for (EPMLiteTriangle ct = currentSection.MoveForward(); ct != null; ct = currentSection.MoveForward())
				{
					if (ct.g_visited == false) triangleList.Add(ct);
				}
			}

			List<EPMTriangle> buffer = new List<EPMTriangle>();
			for (int i = triangleList.Count - 1; i >= 0; i--)
			{
				EPMLiteTriangle lt = triangleList[i];
				if (lt.HasPoint(p1) || lt.HasPoint(p2) || lt.HasPoint(p3))
				{

				}
				else
				{
					buffer.Add(new EPMTriangle(lt[0],lt[1],lt[2]));
					lt[0].g_neighbors.Add(lt[1]);
					lt[0].g_neighbors.Add(lt[2]);
					lt[1].g_neighbors.Add(lt[0]);
					lt[1].g_neighbors.Add(lt[2]);
					lt[2].g_neighbors.Add(lt[0]);
					lt[2].g_neighbors.Add(lt[1]);
				}
			}

			for(int i=0;i<pointList.Count;i++)
			{
				pointList[i].TidyNeighbors();
			}

			//Shift point's position Back.
			if (regionStart.x != 0 || regionStart.y != 0)
			{
				for (int i = 0; i < pointList.Count; i++)
				{
					pointList[i].pos2d += regionStart;
				}
			}

			return buffer;
		}

		private static void TryAddEdge(ref Dictionary<long,EPMEdge> edgeDic,EPMEdge edge)
		{
			EPMEdge t = null;
			if(edgeDic.TryGetValue(edge.LongHash(),out t))
			{
				t.g_visited = true;
			}
			else
			{
				edgeDic.Add(edge.LongHash(), edge);
			}
		}
	}
}
