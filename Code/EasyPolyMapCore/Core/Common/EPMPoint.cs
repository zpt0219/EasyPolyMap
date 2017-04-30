using System.Collections;
using System.Collections.Generic;

namespace EasyPolyMap.Core
{
	public class EPMPoint
	{
		public enum PointType
		{
			Ground=1,
			Mountain=2,
			MountainSummit=4,
			Hill=8,
			Road=16,
			RoadSide=32,
			River=64,
			RiverSide=128,
			Sand=256,
			Soil=512,
			Ocean=1024,
			OceanSide=2048,
		}

		public static int groundEnums = -1;

		//The default point type is ground.
		public PointType g_type = PointType.Ground;
		//the point position
		private Vector2d m_pos2 = new Vector2d();
		private double m_posy = 0;

		public double posX
		{
			get { return m_pos2.x; }
			set { m_pos2.x = value; }
		}
		public double posY
		{
			get{ return m_posy; }
			set{ m_posy = value;}
		}
		public double posZ
		{
			get { return m_pos2.y; }
			set { m_pos2.y = value; }
		}

		public Vector3d pos3d
		{
			get
			{
				return new Vector3d(m_pos2.x, m_posy, m_pos2.y);
			}
			set
			{
				m_pos2.x = value.x;
				m_posy = value.y;
				m_pos2.y = value.z;
			}
		}
		
		public Vector2d pos2d
		{
			get
			{
				return m_pos2;
			}
			set
			{
				m_pos2.x = value.x;
				m_pos2.y = value.y;
			}
		}
		//Since the height we add to position need to be calculated and processed,we save the calculation height here, then use applyHeight() to add this height to real positon.
		public double g_height
		{
			set
			{
				if(value>EPMPoint.HighestHeight)
				{
					EPMPoint.HighestHeight = value;
				}
				else if(value<EPMPoint.LowestHeight)
				{
					EPMPoint.LowestHeight = value;
				}
				m_height = value;
			}
			get
			{
				return m_height;
			}
		}
		private double m_height;
		//Used when uniform the height to a given range.
		public static double LowestHeight = 0;
		public static double HighestHeight = 0;

		//The index of this point in generated point list.
		public int g_indexInList = -1;

		//The neighbor point of this point, each neighbor point must have a edge with this point
		public List<EPMPoint> g_neighbors = new List<EPMPoint>();

		public EPMPoint()
		{
			g_type = PointType.Ground;
			pos3d = new Vector3d();
			g_indexInList = -1;
		}

		public EPMPoint(PointType type,Vector2d pos)
		{
			g_type = type;
			pos2d = pos;
		}

		public EPMPoint(PointType type,Vector3d pos)
		{
			g_type = type;
			pos3d = pos;
		}

		//Remove duplicate neighbors.
		public void TidyNeighbors()
		{
			for(int i = g_neighbors.Count-1; i >= 0; i--)
			{
				if (g_neighbors[i].g_indexInList == g_indexInList) g_neighbors.RemoveAt(i);
				else
				{
					for(int j=i-1;j>=0;j--)
					{
						if(g_neighbors[j].g_indexInList==g_neighbors[i].g_indexInList)
						{
							g_neighbors.RemoveAt(i);
							break;
						}
					}
				}
			}
		}

		public PointType GetPointType()
		{
			return g_type;
		}

		public void SetType(PointType type)
		{
			g_type = type;
		}

		public bool HasType(PointType type)
		{
			return g_type == type;
		}

		public bool HasType(int typeNum)
		{
			return ((int)g_type & typeNum) != 0;
		}

		//Since the types are pre-defined as 1,2,4,8,..., we could use int to present the combination of types.
		public static int GenerateTypesInt(bool SubtractMode,params PointType[] values)
		{
			int ret = 0;
			for(int i=0;i<values.Length;i++)
			{
				ret |= (int)values[i];
			}

			if (SubtractMode) ret ^= 0x7fffffff;
			return ret;
		}

		public static int GetGroundEnums()
		{
			if (groundEnums > 0) return groundEnums;
			groundEnums = GenerateTypesInt(false, PointType.Ground, PointType.Sand, PointType.Soil);
			return groundEnums;
		}

		public void ApplyHeight()
		{
			m_posy += g_height;
			g_height = 0;
		}

		public void ApplyHeight(Vector2d sampleToRange)
		{
			double distance = EPMPoint.HighestHeight - EPMPoint.LowestHeight;
			if (distance == 0) return;
			double ratio = (g_height - EPMPoint.LowestHeight) / distance;
			m_posy += sampleToRange.x + ratio * (sampleToRange.y - sampleToRange.x);
			g_height = 0;
		}
	}

}
