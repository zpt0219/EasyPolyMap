using System.Collections;
using System.Collections.Generic;

namespace EasyPolyMap.Core
{
	public delegate void Callback();
	public delegate void Callback<T>(T arg1);
	public delegate void Callback<T, U>(T arg1, U arg2);
	public delegate void Callback<T, U, V>(T arg1, U arg2, V arg3);
	public delegate void Callback<T, U, V, M>(T arg1, U arg2, V arg3, M arg4);
	public delegate bool BoolCallback();
	public delegate bool BoolCallback<T>(T arg1);
	public delegate double DoubleCallback();
	public delegate double DoubleCallback<T>(T arg1);

	public class EPMGlobal
	{
		public static double EPSILON=10e-9;

		public static double Min(params double[] args)
		{
			double ret = args[0];
			for(int i=1;i<args.Length;i++)
			{
				if (ret > args[i]) ret = args[i];
			}
			return ret;
		}

		public static double Max(params double[] args)
		{
			double ret = args[0];
			for (int i = 1; i < args.Length; i++)
			{
				if (ret < args[i]) ret = args[i];
			}
			return ret;
		}

		public static Vector2d GetOuterCircleOrigin(Vector2d v1,Vector2d v2,Vector2d v3)
		{
			double t1 = v1.SquaredLength();
			double t2 = v2.SquaredLength();
			double t3 = v3.SquaredLength();
			double temp = v1.x * v2.y + v2.x * v3.y + v3.x * v1.y - v1.x * v3.y - v2.x * v1.y - v3.x * v2.y;
			double x = (t2 * v3.y + t1 * v2.y + t3 * v1.y - t2 * v1.y - t3 * v2.y - t1 * v3.y) / temp / 2;
			double y = (t3 * v2.x + t2 * v1.x + t1 * v3.x - t1 * v2.x - t2 * v3.x - t3 * v1.x) / temp / 2;
			return new Vector2d(x, y);
		}

	}

	public class EPMTimer
	{
		public long tickCount = 0;

		private long tempStart = 0;

		public EPMTimer()
		{

		}

		public void Reset()
		{
			tickCount = 0;
			tempStart = 0;
		}

		public void Start()
		{
			tempStart = System.DateTime.UtcNow.Ticks;
		}

		public void Stop()
		{
			tickCount += System.DateTime.UtcNow.Ticks - tempStart;
		}

		public long GetTick()
		{
			return tickCount;
		}

	}
}
