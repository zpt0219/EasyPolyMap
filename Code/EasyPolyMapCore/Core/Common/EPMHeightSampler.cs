using System.Collections;
using System.Collections.Generic;

namespace EasyPolyMap.Core
{
	public class EPMHeightSampler
	{
		public enum CombineMode
		{
			Add=0,
			Subtract,
			Replace,
			ReplaceHighest,
			ReplaceLowest,
			AddWithLimitation,
			SubtractWithLimitation,
		}

		public enum CurveMode
		{
			Flat,
			ReverseLinear,
			ReverseSineQuarter,
			CosineHalf,
			CosineQuarter,
		}

		private static void SetHeightByCombineMode(EPMPoint point, double height, CombineMode mode,double limitation)
		{
			switch (mode)
			{
				case CombineMode.Add:
					point.g_height += height;
					break;
				case CombineMode.Subtract:
					point.g_height -= height;
					break;
				case CombineMode.Replace:
					point.g_height = height;
					break;
				case CombineMode.ReplaceHighest:
					if (point.g_height <= height)
						point.g_height = height;
					break;
				case CombineMode.ReplaceLowest:
					if (point.g_height >= height)
						point.g_height = height;
					break;
				case CombineMode.AddWithLimitation:
					if (point.g_height + height < limitation)
						point.g_height += height;
					else point.g_height = limitation;
					break;
				case CombineMode.SubtractWithLimitation:
					if (point.g_height - height > limitation)
						point.g_height -= height;
					else point.g_height = limitation;
					break;

			}
		}

		private static double GetHeightByCurveMode(double ratio,CurveMode mode)
		{
			double ret = 1;
			//All the curves should start from point (0,1) and ends with point (1,0).
			switch(mode)
			{
				case CurveMode.Flat:
					return 1;
				case CurveMode.ReverseLinear: //normally, Linear starts from point(0,0) and ends with point (1,1),so we reverse it.
					ret = 1 - ratio;
					break;
				case CurveMode.ReverseSineQuarter:  //normally, SineQuater starts from point(0,0) and ends with point (1,1),so we reverse it.
					ret = 1 - System.Math.Sin(ratio * System.Math.PI / 2);
					break;
				case CurveMode.CosineHalf:  //normally, ConsineHalf starts from point (0,1) and ends with point (1,-1), so we scale it.
					ret = System.Math.Cos(ratio * System.Math.PI) / 2 + 0.5f;
					break;
				case CurveMode.CosineQuarter://Consine Quarter perfectly starts from point(0,1) and ends with point(1,0). just get the origin value.
					ret = System.Math.Cos(ratio * System.Math.PI / 2);
					break;
			}
			if (ret > 1) return 1;
			if (ret < 0) return 0;
			return ret;
		}

		public static bool UserDefineSampler(DoubleCallback<EPMPoint> userDefineFunc ,List<EPMPoint> pointList, CombineMode mode = CombineMode.Add,double limitation=0)
		{
			for (int i = 0; i <pointList.Count; i++)
			{
				EPMPoint p = pointList[i];
				double y = userDefineFunc(p);
				SetHeightByCombineMode(p, y, mode,limitation);
			}
			return true;
		}


		public static void DistanceToPoint(double height,Vector2d center,double radius,List<EPMPoint> pointList,CurveMode curveMode=CurveMode.ReverseLinear,CombineMode combineMode=CombineMode.Add,double limitation=0)
		{
			for (int i = 0; i < pointList.Count; i++)
			{
				EPMPoint p = pointList[i];
				double distance = center.Distance(p.pos2d);
				if (distance < radius)
				{
					double y=GetHeightByCurveMode(distance / radius, curveMode)* height;
					SetHeightByCombineMode(p, y, combineMode, limitation);
				}
			}
		}

		public static void DistanceToBorderOfCircle(double height, Vector2d center, double radius,double influenceDistance, List<EPMPoint> pointList, CurveMode curveMode = CurveMode.ReverseLinear, CombineMode combineMode = CombineMode.Add, double limitation = 0)
		{
			for (int i = 0; i < pointList.Count; i++)
			{
				EPMPoint p = pointList[i];
				double distance = center * p.pos2d - radius;
				if (distance>=0&&distance < influenceDistance)
				{
					double y = GetHeightByCurveMode(distance / influenceDistance, curveMode) * height;
					SetHeightByCombineMode(p, y, combineMode, limitation);
				}
			}
		}

		public static void DistanceWithinCircle(double height, Vector2d center, double radius, double influenceDistance, List<EPMPoint> pointList, CurveMode curveMode = CurveMode.ReverseLinear, CombineMode combineMode = CombineMode.Add, double limitation = 0)
		{
			for (int i = 0; i < pointList.Count; i++)
			{
				EPMPoint p = pointList[i];
				double distance = radius-center*p.pos2d;
				if (distance >= 0 && distance < influenceDistance)
				{
					double y = GetHeightByCurveMode(distance / influenceDistance, curveMode) * height;
					SetHeightByCombineMode(p, y, combineMode, limitation);
				}
			}
		}

		public static void DistanceToLine(double height,Vector2d lineStart,Vector2d lineEnd,double radius,List<EPMPoint> pointList,CurveMode curveMode=CurveMode.ReverseLinear,CombineMode combineMode=CombineMode.Add,double limitation=0)
		{
			Vector2d dir = lineEnd - lineStart;
			double lineLength = dir.Length();
			dir.Normalize();
			Vector2d normal = new Vector2d(-dir.y, dir.x);
			for (int i = 0; i < pointList.Count; i++)
			{
				EPMPoint p = pointList[i];
				Vector2d v2 = p.pos2d - lineStart;
				double dot = v2 * dir;
				double distance = 0;
				if (dot < 0 || dot > lineLength)
				{
					//Check the distance to LineStart point and LineEndPoint
					distance = System.Math.Min(v2.Length(), (p.pos2d - lineEnd).Length());
				}
				else
				{
					//check the distance to line
					distance = System.Math.Abs(v2 * normal);
				}

				if (distance < radius)
				{
					double y = GetHeightByCurveMode(distance / radius, curveMode) * height;
					SetHeightByCombineMode(p, y, combineMode, limitation);
				}
			}
		}

		public static void DistanceToBorderOfRectangle(double height, Vector2d rectangleCenterLineStart, Vector2d rectangleCenterLineEnd, double rectangleHalfWitdth, double influenceDistance, List<EPMPoint> pointList, CurveMode curveMode = CurveMode.ReverseLinear, CombineMode combineMode = CombineMode.Add, double limitation = 0)
		{
			Vector2d dir = rectangleCenterLineEnd - rectangleCenterLineStart;
			double lineLength = dir.Length();
			dir.Normalize();
			Vector2d normal = new Vector2d(-dir.y, dir.x);
			Vector2d corner1, corner2, corner3, corner4;
			corner1 = rectangleCenterLineStart + normal * rectangleHalfWitdth;
			corner2 = rectangleCenterLineStart - normal * rectangleHalfWitdth;
			corner3 = rectangleCenterLineEnd + normal * rectangleHalfWitdth;
			corner4 = rectangleCenterLineEnd - normal * rectangleHalfWitdth;

			for (int i = 0; i < pointList.Count; i++)
			{
				EPMPoint p = pointList[i];
				Vector2d v2 = p.pos2d - rectangleCenterLineStart;
				double hlength = v2 * dir;
				double vlength = v2 * normal;
				double distance = 0;
				if (hlength >= 0 && hlength <= lineLength && System.Math.Abs(vlength) <= rectangleHalfWitdth)
				{
					//The point is in the rectangle, ignore it.
					continue;
				}
				else
				{
					if (hlength >= 0 && hlength <= lineLength)
					{
						distance = System.Math.Abs(vlength - rectangleHalfWitdth);
					}
					else if (System.Math.Abs(vlength) <= rectangleHalfWitdth)
					{
						if (hlength > 0) distance = hlength - lineLength;
						else distance = -hlength;
					}
					else
					{
						v2 = p.pos2d;
						distance = EPMGlobal.Min((v2 - corner1).Length(), (v2 - corner2).Length(), (v2 - corner3).Length(), (v2 - corner4).Length());
					}

					if (distance <= influenceDistance)
					{
						double y = GetHeightByCurveMode(distance / influenceDistance, curveMode) * height;
						SetHeightByCombineMode(p, y, combineMode, limitation);
					}
				}
			}
		}


		public static void DistanceWithinRectangle(double height, Vector2d rectangleCenterLineStart, Vector2d rectangleCenterLineEnd, double rectangleHalfWitdth, double influenceDistance, List<EPMPoint> pointList, CurveMode curveMode = CurveMode.ReverseLinear, CombineMode combineMode = CombineMode.Add, double limitation = 0)
		{
			Vector2d dir = rectangleCenterLineEnd - rectangleCenterLineStart;
			double lineLength = dir.Length();
			dir.Normalize();
			Vector2d normal = new Vector2d(-dir.y, dir.x);

			for (int i = 0; i < pointList.Count; i++)
			{
				EPMPoint p = pointList[i];
				Vector2d v2 = p.pos2d - rectangleCenterLineStart;
				double hlength = v2 * dir;
				double vlength = v2 * normal;
				double distance = 0;
				if (hlength >= 0 && hlength <= lineLength && System.Math.Abs(vlength) <= rectangleHalfWitdth)
				{
					//The point is in the rectangle
					distance = EPMGlobal.Min(rectangleHalfWitdth - System.Math.Abs(vlength), hlength, lineLength - hlength);
					if (distance <= influenceDistance)
					{
						double y = GetHeightByCurveMode(distance / influenceDistance, curveMode) * height;
						SetHeightByCombineMode(p, y, combineMode, limitation);
					}
				}
				else
				{
					//The point is outside the rectangle, ignore it.
					continue;
				}
			}
		}

		//Customed depth style,the height will be up and down along its direction.
		public static void DirectionalDistance(double height, Vector2d direction,double cycleLength, List<EPMPoint> pointList, double upLimit = double.MaxValue,double downLimit=double.MinValue)
		{
			direction.Normalize();
			double twoPI = System.Math.PI * 2;
			for (int i = 0; i < pointList.Count; i++)
			{
				Vector2d v2 = pointList[i].pos2d;
				double dot = v2 * direction;
				double offset = System.Math.Sin(dot / cycleLength * twoPI) * height;
				double y = pointList[i].posY + offset;
				if (y > upLimit) y = upLimit;
				if (y < downLimit) y = downLimit;
				pointList[i].posY = y;
			}
		}
	}
}

