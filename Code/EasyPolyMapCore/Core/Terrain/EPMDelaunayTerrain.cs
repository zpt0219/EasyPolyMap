using System.Collections;
using System.Collections.Generic;

namespace EasyPolyMap.Core
{
	public class EPMDelaunayTerrain : EPMBaseTerrain
	{
		//ToDo. It is quite common to iterate all the triangles, iterate a list is quite faster than iterate dictionary.
		public List<EPMTriangle> g_triangleList = new List<EPMTriangle>();

		//For each generated point index, save the triangles who use this point as their vertex.
		List<EPMTriangle>[] m_pointIndexToTriangleList = null;

		public EPMDelaunayTerrain()
		{

		}

		public override void InitGenerating(string name, int seed, Vector2d startPoint, Vector2d endPoint, List<List<Vector2d>> validRegion=null)
		{
			base.InitGenerating(name, seed, startPoint, endPoint, validRegion);
			g_triangleList.Clear();
		}

		public void Test()
		{
			
		}

		private void AddTriangleByPointIndex(int index,EPMTriangle t)
		{
			if(m_pointIndexToTriangleList[index] == null)
			{
				m_pointIndexToTriangleList[index] = new List<EPMTriangle>();
			}
			m_pointIndexToTriangleList[index].Add(t);
		}

		public override void StartGenerating()
		{
			g_triangleList = EPMDelaunayAlgorithm.DoDelaunay(m_vertexList, g_startPoint, g_endPoint);
			m_pointIndexToTriangleList = new List<EPMTriangle>[m_vertexList.Count];

			for (int i = 0; i < g_triangleList.Count; i++)
			{
				EPMTriangle t = g_triangleList[i];
				t.TryDetermineType();
				AddTriangleByPointIndex(t[0].g_indexInList, t);
				AddTriangleByPointIndex(t[1].g_indexInList, t);
				AddTriangleByPointIndex(t[2].g_indexInList, t);
			}
			CalculateTriangleNeighbor();
		}

		//For each triangle, find its neighbors, note that there are 2 types of neighbors, sharedEdge neighbors and sharedPoint neighbors. the TidyNeighbor() function would do the classification.
		private void CalculateTriangleNeighbor()
		{
			for(int i=0;i<g_triangleList.Count;i++)
			{
				EPMTriangle t = g_triangleList[i];
				List<EPMPoint> vertices = t.g_pointList;

				for(int j=0;j<vertices.Count;j++)
				{
					List<EPMTriangle> shared = m_pointIndexToTriangleList[vertices[j].g_indexInList];
					for(int k=0;k<shared.Count;k++)
					{
						t.g_sharePointNeighbors.Add(shared[k]);
					}
				}
				t.TidyNeighbors();
			}
		}

		//It is not accurate, but it can extract most triangles and it is simple&fast
		public List<EPMTriangle> ExtractTriangles_IntersectLine(Vector2d lineStart, Vector2d lineEnd, List<EPMTriangle> waitingProcessList = null)
		{
			List<EPMTriangle> extractList = new List<EPMTriangle>();
			if (waitingProcessList == null) waitingProcessList = g_triangleList;
			Vector2d dir = lineEnd - lineStart;
			double lineLength = dir.Length();
			dir.Normalize();
			Vector2d normal = new Vector2d(-dir.y, dir.x);
			for (int i = 0; i < waitingProcessList.Count; i++)
			{
				EPMTriangle t = waitingProcessList[i];
				List<EPMPoint> plist = t.g_pointList;
				int side = 0;
				for (int j = 0; j < plist.Count; j++)
				{
					Vector2d v2 = plist[j].pos2d - lineStart;
					double dot = v2 * dir;
					if (dot < 0 || dot > lineLength)
					{
						continue;
					}

					double distance = v2 * normal;
					if (side == 0) side = distance > 0 ? 1 : -1;
					else if(side*distance<0)
					{
						extractList.Add(t);
						break;
					}
				}
			}
			return extractList;
		}

		public List<EPMTriangle> ExtractTriangles_PointsInLineWithDistance(Vector2d lineStart, Vector2d lineEnd, double halfWidth,int pointCount = 1, List<EPMTriangle> waitingProcessList = null)
		{
			List<EPMTriangle> extractList = new List<EPMTriangle>();
			if (waitingProcessList == null) waitingProcessList = g_triangleList;
			Vector2d dir = lineEnd - lineStart;
			double lineLength = dir.Length();
			dir.Normalize();
			Vector2d normal = new Vector2d(-dir.y, dir.x);
			for (int i = 0; i < waitingProcessList.Count; i++)
			{
				EPMTriangle t = waitingProcessList[i];
				List<EPMPoint> plist = t.g_pointList;

				int count = 0;
				for (int j = 0; j < plist.Count; j++)
				{
					Vector2d v2 = plist[j].pos2d - lineStart;
					double dot = v2*dir;
					double distance = v2 * normal;
					if (dot >= 0 && dot <= lineLength && System.Math.Abs(distance) <= halfWidth)
					{
						count++;
						if(count>=pointCount)
						{
							extractList.Add(t);
							break;
						}
					}
				}
			}
			return extractList;
		}

		public List<EPMTriangle> ExtractTriangles_PointsInCircle(Vector2d origin, double radius,int pointCount=1,List<EPMTriangle> waitingProcessList = null)
		{
			List<EPMTriangle> extractList = new List<EPMTriangle>();
			if (waitingProcessList == null) waitingProcessList = g_triangleList;
			for (int i = 0; i < waitingProcessList.Count; i++)
			{
				EPMTriangle t = waitingProcessList[i];
				List<EPMPoint> plist = t.g_pointList;
				int count = 0;
				for (int j = 0; j < plist.Count; j++)
				{
					Vector2d v2 = plist[j].pos2d - origin;
					double distance = v2.Length();
					if (distance <= radius)
					{
						count++;
						if(count>=pointCount)
						{
							extractList.Add(t);
							break;
						}
					}
				}
			}
			return extractList;
		}


		public List<EPMTriangle> ExtractTriangles_ByTypes(List<EPMTriangle> waitingProcessList = null, int extractTypes = 0x7fffffff)
		{
			List<EPMTriangle> extractList = new List<EPMTriangle>();
			if (waitingProcessList == null) waitingProcessList = g_triangleList;
			for (int i = 0; i < waitingProcessList.Count; i++)
			{
				if (waitingProcessList[i].HasShapeType(extractTypes))
				{
					extractList.Add(waitingProcessList[i]);
				}
			}
			return extractList;
		}


		public List<EPMTriangle> ExtractTriangles_ByVertex(List<EPMPoint> pointList, List<EPMTriangle> waitingProcessList = null)
		{
			List<EPMTriangle> extractList = new List<EPMTriangle>(); //Save the triangles both extracted and in waitingProcessList
			List<EPMTriangle> tempList = new List<EPMTriangle>();   //Since we need to check whether or not the triangle is in waitingProcessList, use this list to save all extracted triangles.
			if (waitingProcessList == null) waitingProcessList = g_triangleList;
			for (int i = 0; i < pointList.Count; i++)
			{
				List<EPMTriangle> l=m_pointIndexToTriangleList[pointList[i].g_indexInList];
				if(l==null)
				{
					continue;
				}
				for(int j=0;j<l.Count;j++)
				{
					if (l[j].g_visited) continue;
					l[j].g_visited = true;  //Use the visit sign to avoid adding same triangles.
					tempList.Add(l[j]);
				}
			}

			for(int i=0;i<waitingProcessList.Count;i++)
			{
				if(waitingProcessList[i].g_visited)
				{
					extractList.Add(waitingProcessList[i]);
				}
			}

			//Clear the visit sign
			for(int i=0;i<tempList.Count;i++)
			{
				tempList[i].g_visited = false;
			}

			return extractList;
		}
	}
}

