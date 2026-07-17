using UnityEngine;

namespace Cadi.Scripts.Utility.Extensions
{
	public static class ColorExtensions
	{
		public static Color AsTransparent(this Color c)
		{
			c.a = 0f;
			return c;
		}

		public static Color AsOpaque(this Color c)
		{
			c.a = 1f;
			return c;
		}

		public static Color WithRed(this Color c, float r)
		{
			c.r = r;
			return c;
		}

		public static Color WithGreen(this Color c, float g)
		{
			c.g = g;
			return c;
		}

		public static Color WithBlue(this Color c, float b)
		{
			c.b = b;
			return c;
		}

		public static Color WithAlpha(this Color c, float a)
		{
			c.a = a;
			return c;
		}
	}
}
