using System.Collections;
using System.Collections.Generic;

namespace EasyPolyMap.Core
{
	public class EPMEdge
	{
		public EPMPoint g_startPoint, g_endPoint;
		private long m_longHash = -1;
		//useful flag for avoiding duplicate
		public bool g_visited = false;

		public EPMEdge(EPMPoint p1,EPMPoint p2)
		{
			g_startPoint = p1;
			g_endPoint = p2;
			g_visited = false;

			if(g_startPoint.g_indexInList<g_endPoint.g_indexInList)
			{
				m_longHash = g_startPoint.g_indexInList + (((long)g_endPoint.g_indexInList) << 31);
			}
			else
			{
				m_longHash = g_endPoint.g_indexInList + (((long)g_startPoint.g_indexInList) << 31);
			}
		}

		public bool EqualTo(EPMEdge edge2)
		{
			//If hash dismatch, they must not equal.
			if (m_longHash != edge2.m_longHash) return false;
			if (g_startPoint.g_indexInList == edge2.g_startPoint.g_indexInList && g_endPoint.g_indexInList == edge2.g_endPoint.g_indexInList) return true;
			if (g_startPoint.g_indexInList == edge2.g_endPoint.g_indexInList && g_endPoint.g_indexInList == edge2.g_startPoint.g_indexInList) return true;
			return false;
		}

		public long LongHash()
		{
			return m_longHash;
		}
	}
}


