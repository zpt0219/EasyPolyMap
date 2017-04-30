using System.Collections;
using System.Collections.Generic;

namespace EasyPolyMap.Core
{
	//Since LitJson can't recognize float, use this Vector2d and Vector3d function to save the vector data. Also, it provides more accuracy.
	public class Vector2d
	{
		public double x;
		public double y;

		public int FloorX()
		{
			int i = (int)x;
			if (i > x) i--;
			return i;
		}

		public int FloorY()
		{
			int i = (int)y;
			if (i > y) i--;
			return i;
		}

		public Vector2d()
		{
			x = 0;
			y = 0;
		}

		public Vector2d(double x_, double y_)
		{
			x = x_;
			y = y_;
		}

		public double SquaredDistance(Vector2d v2)
		{
			double t1 = x - v2.x;
			double t2 = y - v2.y;
			return t1 * t1 + t2 * t2;
		}

		public double Distance(Vector2d v2)
		{
			return System.Math.Sqrt(SquaredDistance(v2));
		}

		public static Vector2d operator + (Vector2d vec1,Vector2d vec2)
		{
			return new Vector2d(vec1.x + vec2.x, vec1.y + vec2.y);
		}

		public static Vector2d operator - (Vector2d vec1,Vector2d vec2)
		{
			return new Vector2d(vec1.x - vec2.x, vec1.y - vec2.y);
		}

		public static Vector2d operator * (Vector2d vec1,double ratio)
		{
			return new Vector2d(vec1.x * ratio, vec1.y * ratio);
		}

		public static Vector2d operator * (double ratio,Vector2d vec1)
		{
			return vec1 * ratio;
		}

		public static double operator * (Vector2d vec1,Vector2d vec2)
		{
			return vec1.x * vec2.x + vec1.y * vec2.y;
		}

		public static Vector2d operator / (Vector2d vec1,double ratio)
		{
			return new Vector2d(vec1.x / ratio, vec1.y / ratio);
		}

		public static Vector2d operator - (Vector2d vec)
		{
			return new Vector2d(-vec.x, -vec.y);
		}

		public double Dot(Vector2d v2)
		{
			return x * v2.x + y * v2.y;
		}

		public double Length()
		{
			return System.Math.Sqrt(x * x + y * y);
		}

		public double SquaredLength()
		{
			return x * x + y * y;
		}

		public void Normalize()
		{
			double length = Length();
			if(length<EPMGlobal.EPSILON&&length>-EPMGlobal.EPSILON)
			{
				x = y = 0;
			}
			else
			{
				x = x / length;
				y = y / length;
			}
		}
	}

	public class Vector3d
	{
		public double x;
		public double y;
		public double z;

		public Vector3d()
		{
			x = 0;
			y = 0;
			z = 0;
		}

		public Vector3d(double _x, double _y, double _z)
		{
			x = _x;
			y = _y;
			z = _z;
		}

		public int IntX()
		{
			return x > 0 ? ((int)(x * 2) + 1) / 2 : ((int)(x * 2) - 1) / 2;
		}

		public int IntY()
		{
			return y > 0 ? ((int)(y * 2) + 1) / 2 : ((int)(y * 2) - 1) / 2;
		}

		public int IntZ()
		{
			return z > 0 ? ((int)(z * 2) + 1) / 2 : ((int)(z * 2) - 1) / 2;
		}
	}
}