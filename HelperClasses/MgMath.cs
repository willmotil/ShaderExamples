using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public static class MgMath
    {

        public static Vector2 GetPointAtTimeOn2ndDegreePolynominalCurve(Vector2 A, Vector2 B, Vector2 C, float t)
        {
            float i = 1.0f - t;
            float plotX = 0;
            float plotY = 0;
            plotX = (float)(A.X * 1 * (i * i) + B.X * 2 * (i * t) + C.X * 1 * (t * t));
            plotY = (float)(A.Y * 1 * (i * i) + B.Y * 2 * (i * t) + C.Y * 1 * (t * t));
            return new Vector2(plotX, plotY);
        }

        public static float Atan2Xna(float difx, float dify, bool useSpriteBatchAtan2)
        {
            if (useSpriteBatchAtan2)
                return (float)System.Math.Atan2(difx, dify) * -1f;
            else
                return (float)System.Math.Atan2(difx, dify);
        }

        public static float Power(int baseVal, int exponentVal)
        {
            float result = 0;
            for (float exponent = exponentVal; exponent > 0; exponent--)
            {
                result = result * baseVal;
            }
            return result;
        }
    }
}
