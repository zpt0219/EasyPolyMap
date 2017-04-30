using System.Collections;
using System.Collections.Generic;

namespace EasyPolyMap.Core
{
	public class EPMTriangle :EPMShape
	{	
		public EPMTriangle(EPMPoint p1,EPMPoint p2,EPMPoint p3)
		{
			g_pointList.Clear();
			g_pointList.Add(p1);
			g_pointList.Add(p2);
			g_pointList.Add(p3);
		}

		public override bool EqualTo(EPMShape shape)
		{
			//For Triangle, the long hash can represent 1,000,000 different triangles, I think it is enough for mesh generating.
			if(GetLongHash()!=shape.GetLongHash())
			{
				return false;
			}
			return true;
		}

		//if 2 points are shared, they must share one edge
		public bool GetShareEdge(EPMTriangle t2,out int point1Index,out int point2Index)
		{
			point1Index = point2Index = -1;
			if (t2.g_sortedIndexList[0] == g_sortedIndexList[0] || t2.g_sortedIndexList[0] == g_sortedIndexList[1] || t2.g_sortedIndexList[0] == g_sortedIndexList[2])
			{
				point1Index = t2.g_sortedIndexList[0];
			}

			if (t2.g_sortedIndexList[1] == g_sortedIndexList[0] || t2.g_sortedIndexList[1] == g_sortedIndexList[1] || t2.g_sortedIndexList[1] == g_sortedIndexList[2])
			{
				if (point1Index == -1) point1Index = t2.g_sortedIndexList[1];
				else point2Index = t2.g_sortedIndexList[1];
			}

			if (t2.g_sortedIndexList[2] == g_sortedIndexList[0] || t2.g_sortedIndexList[2] == g_sortedIndexList[1] || t2.g_sortedIndexList[2] == g_sortedIndexList[2])
			{
				point2Index = t2.g_sortedIndexList[2];
			}
			return point2Index != -1;
		}

		public override void TryDetermineType()
		{
			EPMPoint.PointType t1, t2, t3;
			t1= g_pointList[0].g_type;
			t2= g_pointList[1].g_type;
			t3= g_pointList[2].g_type;

			int sign = EPMPoint.GenerateTypesInt(false, t1, t2, t3);

			//If one of the vertex is ocean point and not all the points are ocean point. It is a oceanside for sure
			if((sign&(int)EPMPoint.PointType.Ocean)!=0&&(sign-(int)EPMPoint.PointType.Ocean)!=0)
			{
				g_shapeType = EPMPoint.PointType.OceanSide;
				g_isTypeDetermined = true;
				return;
			}

			int grounds = EPMPoint.GetGroundEnums();
			//If one of the vertice's type is ground, set the tile type to this ground.
			if((sign&grounds)!=0)
			{
				if ((sign & (int)EPMPoint.PointType.Ground) != 0) g_shapeType = EPMPoint.PointType.Ground; //Ground has highest priority.
				else if ((sign & (int)EPMPoint.PointType.Sand) != 0) g_shapeType = EPMPoint.PointType.Sand;
				else if ((sign & (int)EPMPoint.PointType.Soil) != 0) g_shapeType = EPMPoint.PointType.Soil;
				else g_shapeType = EPMPoint.PointType.Ground;
				g_isTypeDetermined = true;
			}
			else
			{
				//Find the type that appears 3 times. Set the tile to this type
				if (t1 == t2 && t1 == t3)
				{
					g_shapeType = t1;
					g_isTypeDetermined = true;
				}
				else
				{
					//Vertices have different types, simply set the tiles type to ground, but leave the determine sign to false, the tile's type depend on its neighbors.
					g_shapeType = EPMPoint.PointType.Ground;
					g_isTypeDetermined = false;
				}
			}
		}
	}
}


