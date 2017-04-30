using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace EasyPolyMap.Core
{
	//Provide the Basic terrain functions and data. Delaunay terrain and voronoi terrain are based on this class.
	public class EPMBaseTerrain
	{
		//the originPointList are points random generated initially
		protected List<EPMPoint> m_vertexList = new List<EPMPoint>();

		//the startPoint (left-bottom corner) and endPoint (right-up corner) of the terrain.
		//notice that the terrain is like a 2D picture, you have to run height sampler to generate height infomation for each point of terrain.
		public Vector2d g_startPoint;
		public Vector2d g_endPoint;

		//All the originPoint generated must within at list one of the valid region. Useful when creating Island scene, the surrending ocean should not have any other kind of points except ocean points.
		//if it is null, the region is the same as the terrain size.
		private List<List<Vector2d>> m_validRegions = null;
		//Save the normal info of valid regions. No need to calculate them again and again.
		private List<List<Vector2d>> m_validRegionNormals = null;

		public double getXLength() { return g_endPoint.x - g_startPoint.x; }
		//In unity, z-axis is the forward direction
		public double getZlength() { return g_endPoint.y - g_startPoint.y; }

		//System.random can provide double random number.
		System.Random m_random = null;

		public EPMBaseTerrain()
		{
			m_random = new System.Random(0);
		}

		public virtual void InitGenerating(string name,int seed,Vector2d startPoint,Vector2d endPoint,List<List<Vector2d>> validRegions=null)
		{
			m_random = new System.Random(seed);

			g_startPoint = startPoint;
			g_endPoint = endPoint;

			m_vertexList.Clear();

			m_validRegions = validRegions;
			if(m_validRegions!=null)
			{
				m_validRegionNormals = new List<List<Vector2d>>();
				for(int i=0;i<m_validRegions.Count;i++)
				{
					List<Vector2d> normals = new List<Vector2d>();
					for(int j=1;j<m_validRegions[i].Count;j++)
					{
						Vector2d dir = m_validRegions[i][j] - m_validRegions[i][j - 1];
						dir.Normalize();
						normals.Add(new Vector2d(-dir.y, dir.x));
					}
					m_validRegionNormals.Add(normals);
				}
			}
		}

		public double RandomDouble(double min,double max)
		{
			return m_random.NextDouble() * (max - min) + min;
		}

		public int RandomInt(int min,int max)
		{
			return m_random.Next(min, max);
		}

		public void GeneratingBase(int number,EPMPoint.PointType type)
		{

			m_vertexList.Add(new EPMPoint(type, g_startPoint));
			m_vertexList.Add(new EPMPoint(type, new Vector2d(g_endPoint.x, g_startPoint.y)));
			m_vertexList.Add(new EPMPoint(type, g_endPoint));
			m_vertexList.Add(new EPMPoint(type, new Vector2d(g_startPoint.x, g_endPoint.y)));

			for (int i = 0; i < number; i++)
			{
				Vector2d v2 = new Vector2d(RandomDouble(g_startPoint.x, g_endPoint.x), RandomDouble(g_startPoint.y, g_endPoint.y));
				EPMPoint p = new EPMPoint(type, new Vector3d(v2.x, 0, v2.y));
				m_vertexList.Add(p);
			}

			GeneratingPoints_Line((int)(getXLength() / 5), type, g_startPoint, new Vector2d(g_endPoint.x, g_startPoint.y), 0, true);
			GeneratingPoints_Line((int)(getZlength() / 5), type, new Vector2d(g_endPoint.x, g_startPoint.y), g_endPoint, 0, true);
			GeneratingPoints_Line((int)(getXLength() / 5), type, g_endPoint, new Vector2d(g_startPoint.x, g_endPoint.y), 0, true);
			GeneratingPoints_Line((int)(getZlength() / 5), type, new Vector2d(g_startPoint.x, g_endPoint.y), g_startPoint, 0, true);
		}

		public void GeneratingPoints_Square(int number, EPMPoint.PointType type, Vector2d startPos,Vector2d endPos)
		{
			for (int i = 0; i < number; i++)
			{
				Vector2d v2 = new Vector2d(RandomDouble(startPos.x, endPos.x), RandomDouble(startPos.y, endPos.y));
				if(IsPointInValidRange(v2))
				{
					EPMPoint p = new EPMPoint(type, new Vector3d(v2.x, 0, v2.y));
					m_vertexList.Add(p);
				}
			}
		}

		public void GeneratingPoints_Circle(int number,EPMPoint.PointType type, Vector2d center, double radius)
		{
			double SquaredRadius = radius * radius;
			for(int i=0;i<number;i++)
			{
				Vector2d v2 = new Vector2d(center.x+RandomDouble(-radius,radius),center.y+RandomDouble(-radius,radius));
				if(v2.SquaredDistance(center)>SquaredRadius)
				{
					i--;
				}
				else
				{
					if(IsPointInValidRange(v2))
					{
						EPMPoint p = new EPMPoint(type, new Vector3d(v2.x, 0, v2.y));
						m_vertexList.Add(p);
					}
				}
			}
		}

		public void GeneratingPoints_Ring(int number,EPMPoint.PointType type,Vector2d center,double minRadius,double maxRadius, bool uniformly=true)
		{
			double uniDegree = System.Math.PI * 2 / number;
			double c1 = (maxRadius - minRadius) / 3 + minRadius;
			double c2 = maxRadius-(maxRadius - minRadius) / 3;
			for(int i=0;i<number;)
			{
				Vector2d v2 = new Vector2d();
				if(uniformly)
				{
					double radius = RandomDouble(minRadius, c1);
					v2.x = System.Math.Cos(uniDegree*i) * radius+center.x;
					v2.y = System.Math.Sin(uniDegree*i) * radius+center.y;

					if(IsPointInValidRange(v2))
					{
						EPMPoint p = new EPMPoint(type, new Vector3d(v2.x, 0, v2.y));
						m_vertexList.Add(p);
					}

					radius = RandomDouble(c2, maxRadius);
					v2.x = System.Math.Cos(uniDegree * (i+1)) * radius+center.x;
					v2.y = System.Math.Sin(uniDegree * (i+1)) * radius+center.y;

					if (IsPointInValidRange(v2))
					{
						EPMPoint p = new EPMPoint(type, new Vector3d(v2.x, 0, v2.y));
						m_vertexList.Add(p);
					}
					i += 2;
				}
				else
				{
					double radius = RandomDouble(minRadius, maxRadius);
					double degree = (double)RandomDouble(0, 2 * System.Math.PI);
					v2.x = System.Math.Cos(degree) * radius + center.x;
					v2.y = System.Math.Sin(degree) * radius + center.y;

					if (IsPointInValidRange(v2))
					{
						EPMPoint p = new EPMPoint(type, new Vector3d(v2.x, 0, v2.y));
						m_vertexList.Add(p);
					}
					i++;
				}
			}
		}


		public void GeneratingPoints_Line(int number,EPMPoint.PointType type,Vector2d lineStart,Vector2d lineEnd,double maxDistance, bool uniformly=true)
		{
			Vector2d dir = lineEnd - lineStart;
			double lineLength = dir.Length();
			double uniLength = lineLength / number;
			dir.Normalize();
			Vector2d normal = new Vector2d(-dir.y, dir.x);
			for (int i=0;i<number;i++)
			{
				Vector2d v2 = new Vector2d();
				if(uniformly)
				{
					v2.x = RandomDouble(i * uniLength, (i + 1) * uniLength);
				}
				else
				{
					v2.x = RandomDouble(0, lineLength);
				}

				v2.y = RandomDouble(-maxDistance, maxDistance);

				
				v2 = lineStart + v2.x * dir + v2.y * normal;
				if(IsPointInValidRange(v2))
				{
					EPMPoint p = new EPMPoint(type,new Vector3d(v2.x,0,v2.y));
					m_vertexList.Add(p);
				}
				
			}
		}

		public void DeletePoints_Line(Vector2d lineStart, Vector2d lineEnd,double maxDistance, int deleteTypes=0x7fffffff)
		{
			Vector2d dir = lineEnd - lineStart;
			double lineLength = dir.Length();
			dir.Normalize();
			Vector2d normal = new Vector2d(-dir.y, dir.x);
			for (int i=m_vertexList.Count-1;i>=0;i--)
			{
				if (!m_vertexList[i].HasType(deleteTypes)) continue;
				Vector2d v2 = m_vertexList[i].pos2d - lineStart;
				double dot = v2 * dir;
				if (dot < 0 || dot > lineLength) continue;

				double distance = System.Math.Abs(normal*v2);
				if (distance<=maxDistance)
				{
					m_vertexList.RemoveAt(i);
				}
			}
		}

		public List<EPMPoint> ExtractPoints_Line(Vector2d lineStart, Vector2d lineEnd, double maxDistance, List<EPMPoint> waitingProcessList = null,int extractTypes=0x7fffffff)
		{
			List<EPMPoint> ret = new List<EPMPoint>();
			if (waitingProcessList == null) waitingProcessList = m_vertexList;
			Vector2d dir = lineEnd - lineStart;
			double lineLength = dir.Length();
			dir.Normalize();
			Vector2d normal = new Vector2d(-dir.y, dir.x);
			for (int i = waitingProcessList.Count - 1; i >= 0; i--)
			{
				if (!waitingProcessList[i].HasType(extractTypes)) continue;
				Vector2d v2 = waitingProcessList[i].pos2d - lineStart;
				double dot = v2*dir;
				if (dot < 0 || dot > lineLength) continue;

				double distance = System.Math.Abs(normal*v2);
				if (distance <= maxDistance)
				{
					ret.Add(waitingProcessList[i]);
				}
			}
			return ret;
		}

		public void DeletePoints_Circle(Vector2d origin,double radius,int deleteTypes=0x7fffffff)
		{
			double squaredRadius = radius * radius;
			for (int i = m_vertexList.Count - 1; i >= 0; i--)
			{
				if (!m_vertexList[i].HasType(deleteTypes)) continue;
				double squaredDistance = origin.SquaredDistance(m_vertexList[i].pos2d);
				if (squaredDistance<= squaredRadius)
				{
					m_vertexList.RemoveAt(i);
				}
			}
		}

		public List<EPMPoint> ExtractPoints_Circle(Vector2d origin, double radius, List<EPMPoint> waitingProcessList = null,int extractTypes=0x7fffffff)
		{
			double squaredRadius = radius * radius;
			List<EPMPoint> ret = new List<EPMPoint>();
			if (waitingProcessList == null) waitingProcessList = m_vertexList;
			for (int i = waitingProcessList.Count - 1; i >= 0; i--)
			{
				if (!waitingProcessList[i].HasType(extractTypes)) continue;
				double squaredDistance =origin.Distance(waitingProcessList[i].pos2d);
				if (squaredDistance <= squaredRadius)
				{
					ret.Add(waitingProcessList[i]);
				}
			}
			return ret;
		}

		public void DeletePoints_Square(Vector2d leftBottom, Vector2d upRight,int deleteTypes=0x7fffffff)
		{
			for(int i= m_vertexList.Count-1;i>=0;i--)
			{
				if (!m_vertexList[i].HasType(deleteTypes)) continue;
				Vector2d v2 = m_vertexList[i].pos2d;
				if (v2.x>=leftBottom.x&& v2.y>=leftBottom.y&& v2.x<=upRight.x&& v2.y<=upRight.y)
				{
					m_vertexList.RemoveAt(i);
				}
			}
		}

		public List<EPMPoint> ExtractPoints_Square(Vector2d leftBottom,Vector2d upRight,List<EPMPoint> waitingProcessList=null,int extractTypes=0x7fffffff)
		{
			List<EPMPoint> ret = new List<EPMPoint>();
			if (waitingProcessList == null) waitingProcessList = m_vertexList;
			for (int i = waitingProcessList.Count - 1; i >= 0; i--)
			{
				if (!waitingProcessList[i].HasType(extractTypes)) continue;
				Vector2d v2 = m_vertexList[i].pos2d;
				if (v2.x >= leftBottom.x && v2.y >= leftBottom.y && v2.x <= upRight.x && v2.y <= upRight.y)
				{
					ret.Add(waitingProcessList[i]);
				}
			}
			return ret;
		}

		public List<EPMPoint> ExtractPoints_ByTypes(List<EPMPoint> waitingProcessList = null,int extractTypes=0x7fffffff)
		{
			List<EPMPoint> ret = new List<EPMPoint>();
			if (waitingProcessList == null) waitingProcessList = m_vertexList;
			for (int i=0;i< waitingProcessList.Count;i++)
			{
				if (!waitingProcessList[i].HasType(extractTypes)) continue;
				ret.Add(m_vertexList[i]);
			}
			return ret;
		}

		public List<EPMPoint> ExtractPoints_ByRegion(List<Vector2d> regionPoints,List<EPMPoint> waitingProcessList=null,int extractTypes=0x7fffffff)
		{
			List<EPMPoint> ret = new List<EPMPoint>();
			//A region must closed
			if (!IsRegionValid(regionPoints)) return ret;

			//Pre compute the normal for each edge.
			List<Vector2d> normalList = new List<Vector2d>();
			for(int i=1;i<regionPoints.Count;i++)
			{
				Vector2d dir = regionPoints[i] - regionPoints[i - 1];
				dir.Normalize();
				normalList.Add(new Vector2d(-dir.y, dir.x));
			}

			if (waitingProcessList == null) waitingProcessList = m_vertexList;

			for(int i=0;i<waitingProcessList.Count;i++)
			{
				if (!waitingProcessList[i].HasType(extractTypes)) continue;
				Vector2d pos = waitingProcessList[i].pos2d;
				int count = 0;
				for (int k = 0; k < regionPoints.Count-1; k++)
				{
					if (regionPoints[k].y <= pos.y)
					{
						if (regionPoints[k+1].y > pos.y)
						{
							if (normalList[k]*(pos-regionPoints[k]) > 0)
							{
								count++;
							}
						}
					}
					else
					{
						if (regionPoints[k+1].y <= pos.y)
						{
							if (normalList[k]*(pos - regionPoints[k]) < 0)
							{
								count--;
							}
						}
					}
				}
				if(count!=0)
				{
					ret.Add(waitingProcessList[i]);
				}
			}
			return ret;
		}

		private bool IsPointInValidRange(Vector2d point)
		{
			if (point.x >= g_startPoint.x && point.y >= g_startPoint.y && point.x <= g_endPoint.x && point.y <= g_endPoint.y)
				return isPointInValidRegions(point);
			return false;
		}

		private bool IsRegionValid(List<Vector2d> region)
		{
			if (region != null && region.Count >= 3 && region[0].x == region[region.Count - 1].x && region[0].y == region[region.Count - 1].y) return true;
			return false;
		}

		private bool isPointInValidRegions(Vector2d point)
		{
			if (m_validRegions == null||m_validRegions.Count==0) return true;
			for(int i=0;i<m_validRegions.Count;i++)
			{
				List<Vector2d> region = m_validRegions[i];
				if (!IsRegionValid(region)) continue;

				List<Vector2d> regionNormal = m_validRegionNormals[i];

				int count = 0;
				for (int k = 0; k < region.Count - 1; k++)
				{
					if (region[k].y <= point.y)
					{
						if (region[k + 1].y > point.y)
						{
							if (regionNormal[k]*(point - region[k]) > 0)
							{
								count++;
							}
						}
					}
					else
					{
						if (region[k + 1].y <= point.y)
						{
							if (regionNormal[k]*(point - region[k]) < 0)
							{
								count--;
							}
						}
					}
				}
				if (count != 0)
				{
					return true;
				}
			}
			return false;
		}

		//The order of the each point in waitingProcessList should not change if user didn't change it manually. All the APIs provided do not violate the order.
		public List<EPMPoint> ExtractPoints_NotInGivenList(List<EPMPoint> waitingProcessList = null,bool needSort=false)
		{
			//If input list is null or empty, return all the vertex.
			if (waitingProcessList == null||waitingProcessList.Count==0) return m_vertexList;

			List<EPMPoint> ret = new List<EPMPoint>();

			if(needSort)
			{
				waitingProcessList.Sort(delegate (EPMPoint comparePoint1, EPMPoint comparePoint2)
				{
					if (comparePoint1.g_indexInList > comparePoint2.g_indexInList) return 1;
					else if (comparePoint1.g_indexInList == comparePoint2.g_indexInList) return 0;
					else return -1;
				});
			}

			int waitCount = 0;
			int verCount = 0;
			while(waitCount<waitingProcessList.Count&&verCount<m_vertexList.Count)
			{
				if (m_vertexList[verCount].g_indexInList < waitingProcessList[waitCount].g_indexInList)
				{
					ret.Add(m_vertexList[verCount]);
					verCount++;
				}
				else if (m_vertexList[verCount].g_indexInList == waitingProcessList[waitCount].g_indexInList)
				{
					verCount++;
					waitCount++;
				}
				else
				{
					waitCount++;
				}
			}

			for (int i=verCount;i<m_vertexList.Count;i++)
			{
				ret.Add(m_vertexList[i]);
			}
			return ret;
		}

		public List<EPMPoint> ExtractPoints_All()
		{
			return m_vertexList;
		}

		public virtual void StartGenerating()
		{
			//"Please use the Inherited Class's Method";
		}

	}
}

