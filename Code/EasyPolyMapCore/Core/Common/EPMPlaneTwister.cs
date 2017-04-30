using System.Collections;
using System.Collections.Generic;

namespace EasyPolyMap.Core
{
	public class EPMPlaneTwister
	{
		public static void SineShifter(List<EPMPoint> plist,Vector2d direction,double cycleLength,double amplifier)
		{
			direction.Normalize();
			Vector2d normal = new Vector2d(-direction.y, direction.x);
			double twoPI = System.Math.PI * 2;
			for (int i = 0; i < plist.Count; i++)
			{
				double dot = plist[i].pos2d * direction;
				double offset = System.Math.Sin(dot / cycleLength * twoPI) * amplifier;
				plist[i].pos2d += new Vector2d(normal.x * offset, normal.y * offset);
			}
		}

		public static void SineExpander(List<EPMPoint> plist,Vector2d direction,double cycleLength,double amplifier)
		{
			direction.Normalize();
			double twoPI = System.Math.PI * 2;
			for (int i = 0; i < plist.Count; i++)
			{
				double dot = plist[i].pos2d * direction;
				double offset = System.Math.Sin(dot / cycleLength * twoPI) * amplifier;
				plist[i].pos2d += new Vector2d(direction.x * offset, direction.y * offset);
			}
		}
	}
}

