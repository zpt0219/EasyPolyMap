using System.Collections;
using System.Collections.Generic;

namespace EasyPolyMap.Core
{
	public class EPMShape
	{
        public List<EPMPoint> g_pointList=new List<EPMPoint>();
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

		public List<EPMEdge> g_edgeList = new List<EPMEdge>();
        public List<int> g_sortedIndexList=new List<int>();  //Sorted Index List is easy for hashing and comparing.
        protected long m_longHash = -1;
        //useful flag for avoiding duplicate
        public bool g_visited = false;
		public int g_indexInList = -1;
		public EPMPoint.PointType g_shapeType = EPMPoint.PointType.Ground;
		public bool g_isTypeDetermined = false;

		public List<EPMShape> g_sharePointNeighbors = new List<EPMShape>();
		public List<EPMShape> g_shareEdgeNeighbors = new List<EPMShape>();

		protected virtual void SortPointsIndex()
		{
			g_sortedIndexList.Clear();
			for (int i=0;i<g_pointList.Count;i++)
			{
				g_sortedIndexList.Add(g_pointList[i].g_indexInList);
			}
            g_sortedIndexList.Sort();
		}

		public virtual bool HasPoint(EPMPoint p)
		{
			for(int i=0;i<g_pointList.Count;i++)
			{
				if (g_pointList[i] == p) return true;
			}
			return false;
		}

		public virtual bool EqualTo(EPMShape shape)
		{
			if (GetLongHash() != shape.GetLongHash()) return false;
			if (g_sortedIndexList.Count != shape.g_sortedIndexList.Count) return false;
			for (int i = 0; i < g_sortedIndexList.Count; i++)
			{
				if (g_sortedIndexList[i] != shape.g_sortedIndexList[i]) return false;
			}
			return true;
		}

		public long GetLongHash()
		{
			if(m_longHash<0)
			{
				SortPointsIndex();
				int shift = 63 / g_pointList.Count;
				m_longHash = 0;
				for (int i = 0; i < g_sortedIndexList.Count; i++)
				{
					m_longHash = (m_longHash << shift) + g_sortedIndexList[i];
				}
			}
			return m_longHash;
		}

		//Every vertex of the shape should be different with each other. 
		public virtual bool IsValid()
		{
			for(int i=0;i<g_pointList.Count;i++)
			{
				for(int j=i+1;j<g_pointList.Count;j++)
				{
					if (g_pointList[i].g_indexInList == g_pointList[j].g_indexInList) return false;
				}
			}
			return true;
		}

		//public static bool operator == (EPMShape shape1,EPMShape shape2)
  //      {
  //          return shape1.Equals(shape2);
  //      }

  //      public static bool operator !=(EPMShape shape1, EPMShape shape2)
  //      {
  //          return !shape1.Equals(shape2);
  //      }

		public virtual bool HasPointType(EPMPoint.PointType type,int LeastNumber=1)
		{
            for(int i=0;i<g_pointList.Count;i++)
            {
				if (g_pointList[i].HasType(type))
				{
					LeastNumber--;
					if (LeastNumber <= 0) return true;
				}
            }
			return false;
		}

		public virtual bool HasPointType(int typeEnum,int LeastNumber=1)
		{
			for(int i = 0; i< g_pointList.Count;i++)
			{
				if(g_pointList[i].HasType(typeEnum))
				{
					LeastNumber--;
					if (LeastNumber <= 0) return true;
				}
			}
			return false;
		}

		public bool HasShapeType(EPMPoint.PointType type)
		{
			return HasShapeType(EPMPoint.GenerateTypesInt(false, type));
		}

		public bool HasShapeType(int typeEnums)
		{
			return ((int)g_shapeType & typeEnums) != 0;
		}

		public virtual void TryDetermineType()
		{

		}

		public void SetType(EPMPoint.PointType type)
		{
			g_shapeType = type;
			g_isTypeDetermined = true;
		}

		//How many points does the input triangle share with this triangle;
		public virtual List<int> GetCommonPointIndexList(EPMShape s2)
		{
			List<int> commonIndexList = new List<int>();
			int p1, p2;
			p1 = p2 = 0;
			while(p1<g_sortedIndexList.Count&&p2<s2.g_sortedIndexList.Count)
			{
				if (g_sortedIndexList[p1] < s2.g_sortedIndexList[p2]) p1++;
				else if (g_sortedIndexList[p1] > s2.g_sortedIndexList[p2]) p2++;
				else
				{
					commonIndexList.Add(g_sortedIndexList[p1]);
					p1++;
					p2++;
				}
			}
			return commonIndexList;
		}

		public virtual List<int> GetUnCommonPointIndexList(EPMShape s2,bool countS2In=false)
		{
			List<int> unCommonIndexList = new List<int>();
			int p1, p2;
			p1 = p2 = 0;
			while (p1 < g_sortedIndexList.Count && p2 < s2.g_sortedIndexList.Count)
			{
				if (g_sortedIndexList[p1] < s2.g_sortedIndexList[p2])
				{
					unCommonIndexList.Add(g_sortedIndexList[p1]);
					p1++;
				}
				else if(g_sortedIndexList[p1]>s2.g_sortedIndexList[p2])
				{
					if(countS2In) unCommonIndexList.Add(s2.g_sortedIndexList[p2]);
					p2++;
				}
				else
				{
					p1++;
					p2++;
				}
			}
			return unCommonIndexList;
		}

		//remove same shapes in neighbor shape list,init the shareEdge shape list.
		public virtual void TidyNeighbors()
		{
			for (int i = g_sharePointNeighbors.Count - 1; i >= 0; i--)
			{
				if (g_sharePointNeighbors[i] == this) g_sharePointNeighbors.RemoveAt(i);
				else
				{
					for (int j = i - 1; j >= 0; j--)
					{
						if (g_sharePointNeighbors[i] == g_sharePointNeighbors[j])
						{
							g_sharePointNeighbors.RemoveAt(i);
							break;
						}
					}
				}
			}

			g_shareEdgeNeighbors.Clear();
			for (int i = 0; i < g_sharePointNeighbors.Count; i++)
			{
				if (g_sharePointNeighbors[i].GetCommonPointIndexList(this).Count > 1)
				{
					g_shareEdgeNeighbors.Add(g_sharePointNeighbors[i]);
				}
			}
		}

		public double GetSignedArea()
		{
			Vector2d v, nv;
			double signedArea = 0;

			for (int i = 0; i < g_pointList.Count; i++)
			{
				int next = (i == g_pointList.Count - 1 ? 0:i + 1);
				v = g_pointList[i].pos2d;
				nv = g_pointList[next].pos2d;
				signedArea += v.x * nv.y - nv.x * v.y;
			}
			return signedArea/2;
		}
	}
}

