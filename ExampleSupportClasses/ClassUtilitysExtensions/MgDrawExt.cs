using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
    public static class MgDrawExt
    {
        static SpriteBatch spriteBatch;
        public static Texture2D dot;
        public static Texture2D dotRed;
        public static Texture2D dotGreen;
        public static Texture2D dotBlue;

        /// <summary>
        /// Flips atan direction to xna spritebatch rotational alignment defaults to true.
        /// </summary>
        public static bool SpriteBatchAtan2 = true;

        public static void Initialize(GraphicsDevice device, SpriteBatch spriteBatch)
        {
            MgDrawExt.spriteBatch = spriteBatch;
            if (MgDrawExt.dot == null)
                MgDrawExt.dot = CreateDotTexture(device, Color.White);
            if (MgDrawExt.dotRed == null)
                MgDrawExt.dotRed = CreateDotTexture(device, Color.Red);
            if (MgDrawExt.dotGreen == null)
                MgDrawExt.dotGreen = CreateDotTexture(device, Color.Green);
            if (MgDrawExt.dotBlue == null)
                MgDrawExt.dotBlue = CreateDotTexture(device, Color.Blue);
        }

        public static void DrawRectangleOutline(Rectangle r, int lineThickness, Color c)
        {
            DrawSquareBorder(r, lineThickness, c);
        }

        public static void DrawSquareBorder(Rectangle r, int lineThickness, Color c)
        {
            Rectangle TLtoR = new Rectangle(r.Left, r.Top, r.Width, lineThickness);
            Rectangle BLtoR = new Rectangle(r.Left, r.Bottom - lineThickness, r.Width, lineThickness);
            Rectangle LTtoB = new Rectangle(r.Left, r.Top, lineThickness, r.Height);
            Rectangle RTtoB = new Rectangle(r.Right - lineThickness, r.Top, lineThickness, r.Height);
            spriteBatch.Draw(dot, TLtoR, c);
            spriteBatch.Draw(dot, BLtoR, c);
            spriteBatch.Draw(dot, LTtoB, c);
            spriteBatch.Draw(dot, RTtoB, c);
        }

        public static void DrawCrossHair(Vector2 position, float radius, Color color)
        {
            MgDrawExt.DrawBasicLine(new Vector2(-radius, 0) + position, new Vector2(0 + radius, 0) + position, 1, color);
            MgDrawExt.DrawBasicLine(new Vector2(0, 0 - radius) + position, new Vector2(0, radius) + position, 1, color);
        }

        public static void DrawBasicLine(Vector2 s, Vector2 e, int thickness, Color linecolor)
        {
            spriteBatch.Draw(dot, new Rectangle((int)s.X, (int)s.Y, thickness, (int)Vector2.Distance(e, s)), new Rectangle(0, 0, 1, 1), linecolor, (float)Atan2Xna(e.X - s.X, e.Y - s.Y), Vector2.Zero, SpriteEffects.None, 0);
        }

        public static void DrawBasicPoint(Vector2 p, Color c)
        {
            spriteBatch.Draw(dot, new Rectangle((int)p.X, (int)p.Y, 2, 2), new Rectangle(0, 0, 1, 1), c, 0.0f, Vector2.One, SpriteEffects.None, 0);
        }

        public static void DrawBasicPoint(Vector2 p, Color c, int size)
        {
            int half = (int)(size / 2);
            spriteBatch.Draw(dot, new Rectangle((int)p.X, (int)p.Y, 1 + size, 1 + size), new Rectangle(0, 0, 1, 1), c, 0.0f, new Vector2(.5f, .5f), SpriteEffects.None, 0);
        }

        #region Extension methods of these types.

        public static void DrawRectangleOutline(this SpriteBatch spriteBatch, Rectangle r, int lineThickness, Color c)
        {
            Rectangle TLtoR = new Rectangle(r.Left, r.Top, r.Width, lineThickness);
            Rectangle BLtoR = new Rectangle(r.Left, r.Bottom - lineThickness, r.Width, lineThickness);
            Rectangle LTtoB = new Rectangle(r.Left, r.Top, lineThickness, r.Height);
            Rectangle RTtoB = new Rectangle(r.Right - lineThickness, r.Top, lineThickness, r.Height);
            spriteBatch.Draw(dot, TLtoR, c);
            spriteBatch.Draw(dot, BLtoR, c);
            spriteBatch.Draw(dot, LTtoB, c);
            spriteBatch.Draw(dot, RTtoB, c);
        }

        public static void DrawRectangleOutlineWithString(this SpriteBatch spriteBatch, SpriteFont font, Rectangle r, int lineThickness, Color outlineColor, string msg, Color msgColor)
        {
            Rectangle TLtoR = new Rectangle(r.Left, r.Top, r.Width, lineThickness);
            Rectangle BLtoR = new Rectangle(r.Left, r.Bottom - lineThickness, r.Width, lineThickness);
            Rectangle LTtoB = new Rectangle(r.Left, r.Top, lineThickness, r.Height);
            Rectangle RTtoB = new Rectangle(r.Right - lineThickness, r.Top, lineThickness, r.Height);
            spriteBatch.Draw(dot, TLtoR, outlineColor);
            spriteBatch.Draw(dot, BLtoR, outlineColor);
            spriteBatch.Draw(dot, LTtoB, outlineColor);
            spriteBatch.Draw(dot, RTtoB, outlineColor);
            spriteBatch.DrawString(font, msg, TLtoR.Location.ToVector2(), msgColor);
        }

        public static void DrawCrossHair(this SpriteBatch spriteBatch, Vector2 position, float radius, Color color)
        {
            MgDrawExt.DrawBasicLine(new Vector2(-radius, 0) + position, new Vector2(0 + radius, 0) + position, 1, color);
            MgDrawExt.DrawBasicLine(new Vector2(0, 0 - radius) + position, new Vector2(0, radius) + position, 1, color);
        }

        public static void DrawLineWithStringAtEnd(this SpriteBatch spriteBatch, SpriteFont font, string msg, Vector2 s, Vector2 e, int thickness, Color linecolor)
        {
            spriteBatch.Draw(dot, new Rectangle((int)s.X, (int)s.Y, thickness, (int)Vector2.Distance(e, s)), new Rectangle(0, 0, 1, 1), linecolor, (float)Atan2Xna(e.X - s.X, e.Y - s.Y), Vector2.Zero, SpriteEffects.None, 0);
            spriteBatch.DrawString(font, msg, e, linecolor);
        }

        public static void DrawLineWithPositionMsgAtEnd(this SpriteBatch spriteBatch, SpriteFont font, Vector2 s, Vector2 e, int thickness, Color linecolor)
        {
            spriteBatch.Draw(dot, new Rectangle((int)s.X, (int)s.Y, thickness, (int)Vector2.Distance(e, s)), new Rectangle(0, 0, 1, 1), linecolor, (float)Atan2Xna(e.X - s.X, e.Y - s.Y), Vector2.Zero, SpriteEffects.None, 0);
            spriteBatch.DrawString(font, $"{e.X.ToString("######0.000")} {e.Y.ToString("######0.000")}", e, linecolor);
        }

        public static void DrawBasicLine(this SpriteBatch spriteBatch, Vector2 s, Vector2 e, int thickness, Color linecolor)
        {
            spriteBatch.Draw(dot, new Rectangle((int)s.X, (int)s.Y, thickness, (int)Vector2.Distance(e, s)), new Rectangle(0, 0, 1, 1), linecolor, (float)Atan2Xna(e.X - s.X, e.Y - s.Y), Vector2.Zero, SpriteEffects.None, 0);
        }

        public static void DrawBasicPoint(this SpriteBatch spriteBatch, Vector2 p, Color c)
        {
            spriteBatch.Draw(dot, new Rectangle((int)p.X, (int)p.Y, 2, 2), new Rectangle(0, 0, 1, 1), c, 0.0f, Vector2.One, SpriteEffects.None, 0);
        }

        public static void DrawBasicPointWithMsg(this SpriteBatch spriteBatch, SpriteFont font, string msg, Vector2 p, Color c, int size)
        {
            int half = (int)(size / 2);
            spriteBatch.Draw(dot, new Rectangle((int)p.X, (int)p.Y, 1 + size, 1 + size), new Rectangle(0, 0, 1, 1), c, 0.0f, new Vector2(.5f, .5f), SpriteEffects.None, 0);
            spriteBatch.DrawString(font, msg, p, c);
        }

        public static void DrawBasicPoint(this SpriteBatch spriteBatch, Vector2 p, Color c, int size)
        {
            int half = (int)(size / 2);
            spriteBatch.Draw(dot, new Rectangle((int)p.X, (int)p.Y, 1 + size, 1 + size), new Rectangle(0, 0, 1, 1), c, 0.0f, new Vector2(.5f, .5f), SpriteEffects.None, 0);
        }

        /// <summary>
        /// Super inefficient just for tests.
        /// </summary>
        public static void DrawCircleOutline(this SpriteBatch spriteBatch, Vector2 center, int radius, int segments, int linethickness, Color c)
        {
            var coef = 1f / (float)(segments) * 6.2831530717f;
            Vector2 s = RotatePointAboutZaxis(new Vector2(0, radius), 0 * coef) + center;
            for (int curseg = 1; curseg < segments+1; curseg++)
            {
                var e = RotatePointAboutZaxis(new Vector2(0, radius), curseg * coef) + center;
                spriteBatch.DrawBasicLine(s, e, linethickness, c);
                s = e;
            }
        }

        #endregion

        #region TextureGenerationCalls

        public static Texture2D CreateDotTexture(GraphicsDevice device, Color color)
        {
            Color[] data = new Color[1] { color };
            Texture2D tex = new Texture2D(device, 1, 1);
            tex.SetData<Color>(data);
            return tex;
        }

        public static Texture2D CreateCheckerBoard(GraphicsDevice device, int w, int h, Color c0, Color c1)
        {
            Color[] data = new Color[w * h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int index = y * w + x;
                    Color c = c0;
                    if ((y % 2 == 0))
                    {
                        if ((x % 2 == 0))
                            c = c0;
                        else
                            c = c1;
                    }
                    else
                    {
                        if ((x % 2 == 0))
                            c = c1;
                        else
                            c = c0;
                    }
                    data[index] = c;
                }
            }
            Texture2D tex = new Texture2D(device, w, h);
            tex.SetData<Color>(data);
            return tex;
        }

        public static Texture2D CreateGrid(GraphicsDevice device, int w, int h, Color c0, Color c1, Color c2)
        {
            Color[] data = new Color[w * h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int index = y * w + x;
                    Color c = c2;
                    if ((y % 4 == 0))
                        c = c0;
                    if ((x % 4 == 0))
                        c = c1;
                    if ((x % 4 == 0) && (y % 4 == 0))
                    {
                        var r = (c0.R + c1.R) / 2;
                        var g = (c0.R + c1.R) / 2;
                        var b = (c0.R + c1.R) / 2;
                        c = new Color(r, g, b, 255);
                    }
                    data[index] = c;
                }
            }
            Texture2D tex = new Texture2D(device, w, h);
            tex.SetData<Color>(data);
            return tex;
        }

        public static Texture2D GenerateTexture2DWithTopLeftDiscoloration(GraphicsDevice gd)
        {
            var cdata = new Color[250 * 250];
            for (int i = 0; i < 250; i++)
            {
                for (int j = 0; j < 250; j++)
                {
                    if (i < 50 && j < 50)
                        cdata[i * 250 + j] = new Color(120, 120, 120, 255);
                    else
                        cdata[i * 250 + j] = new Color(i, j, (i + j) / 2, 255);
                }
            }
            Texture2D t = new Texture2D(gd, 250, 250);
            t.SetData(cdata);
            return t;
        }

        public static Texture2D GenerateAlphaStencilCircle(GraphicsDevice device, Color color)
        {
            float sliderControl = 2.00f; // increase beyond 1.0f that gives a more opaque interior
            int size = 200;
            var radius = 99.0f;
            var center = new Vector2(radius, radius);
            var a = new Vector2(0, 1.00f);
            var b = new Vector2((radius * sliderControl), 1.00f);
            var c = new Vector2(radius - 1, 0.00f);
            //
            Color[] data = new Color[size * size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var p = new Vector2(x, y);
                    var dist = Vector2.Distance(center, p);
                    var coeff = dist / radius;
                    var curvepoint = MgDrawExt.GetPointAtTimeOn2ndDegreePolynominalCurve(a, b, c, coeff);

                    if (coeff < 0f)
                        coeff = 0.0f;

                    if (curvepoint.Y < 0f)
                        curvepoint.Y = 0.0f;

                    data[x + y * size] = new Color((byte)(curvepoint.Y * 255), (byte)(curvepoint.Y * 255), (byte)(curvepoint.Y * 255), (byte)(curvepoint.Y * 255));
                }
            }
            Texture2D tex = new Texture2D(device, size, size);
            tex.SetData<Color>(data);
            return tex;
        }

        public static Texture2D GenerateSplinedAlphaStencilCircle(GraphicsDevice device, Color color)
        {
            //float sliderControl = 4.00f; // increase beyond 1.0f that gives a more opaque interior
            int size = 200;
            var radius = 99.0f;
            var center = new Vector2(radius, radius);
            var a = new Vector3(0f, 1.00f, 0f);
            var b = new Vector3(radius, 1.00f, 0f);
            var c = new Vector3(radius, 1.00f, 0f);
            var d = new Vector3(radius, 0.00f, 0f);
            //
            Color[] data = new Color[size * size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var p = new Vector2(x, y);
                    var dist = Vector2.Distance(center, p);
                    var coeff = dist / radius;
                    var curvepoint = MgDrawExt.BiCubicSubdivision(a, b, c, d, coeff).ToVector2();

                    if (coeff < 0f)
                        coeff = 0.0f;

                    if (curvepoint.Y < 0f)
                        curvepoint.Y = 0.0f;

                    data[x + y * size] = new Color((byte)(curvepoint.Y * 255), (byte)(curvepoint.Y * 255), (byte)(curvepoint.Y * 255), (byte)(curvepoint.Y * 255));
                }
            }
            Texture2D tex = new Texture2D(device, size, size);
            tex.SetData<Color>(data);
            return tex;
        }

        public static Texture2D GenerateAlphaStencilCircleEdge(GraphicsDevice device, Color color, float percentageEdgeRadialPosition, float percentageRadialWidth, float strength0to1)
        {
            int size = 200;
            var radius = size / 2f;
            var center = new Vector2(radius, radius);
            //
            Color[] data = new Color[size * size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var p = new Vector2(x, y);
                    var dist = Vector2.Distance(center, p);
                    var coeff = dist / radius;

                    if (coeff < 0f)
                        coeff = 0.0f;

                    var diff = (coeff - percentageEdgeRadialPosition) / percentageRadialWidth;
                    var result = 0f;
                    if (diff < 0)
                        diff = -diff;
                    if (diff > 1f)
                        diff = 1f;

                    result = (1.0f - diff) * strength0to1;

                    data[x + y * size] = new Color((byte)(result * 255), (byte)(result * 255), (byte)(result * 255), (byte)(result * 255));
                }
            }
            Texture2D tex = new Texture2D(device, size, size);
            tex.SetData<Color>(data);
            return tex;
        }

        public static Texture2D GenerateFractal(GraphicsDevice device, Color color)
        {
            Color[] data = new Color[100 * 100];
            var center = new Vector2(50, 50);
            var a = new Vector2(0, 1.00f);
            var b = new Vector2(80, 0.90f);
            var c = new Vector2(90, 0.00f);
            for (int x = 0; x < 100; x++)
            {
                for (int y = 0; y < 100; y++)
                {
                    var p = new Vector2(x, y);
                    var dist = Vector2.Distance(center, p);
                    var curvepoint = MgDrawExt.GetPointAtTimeOn2ndDegreePolynominalCurve(a, b, c, dist);

                    data[x + y * 100] = new Color((byte)(curvepoint.Y * 255), (byte)(curvepoint.Y * 255), (byte)(curvepoint.Y * 255), (byte)(curvepoint.Y * 255));
                }
            }
            Texture2D tex = new Texture2D(device, 100, 100);
            tex.SetData<Color>(data);
            return tex;
        }


        public static float Atan2Xna(float difx, float dify)
        {
            if (SpriteBatchAtan2)
                return (float)System.Math.Atan2(difx, dify) * -1f;
            else
                return (float)System.Math.Atan2(difx, dify);
        }

        public static Vector2 GetPointAtTimeOn2ndDegreePolynominalCurve(Vector2 A, Vector2 B, Vector2 C, float t)
        {
            float i = 1.0f - t;
            float plotX = 0;
            float plotY = 0;
            plotX = (float)(A.X * 1 * (i * i) + B.X * 2 * (i * t) + C.X * 1 * (t * t));
            plotY = (float)(A.Y * 1 * (i * i) + B.Y * 2 * (i * t) + C.Y * 1 * (t * t));
            return new Vector2(plotX, plotY);
        }

        public static Vector3 BiCubicSubdivision(Vector3 a0, Vector3 a1, Vector3 a2, Vector3 a3, float time)
        {
            return (((((a3 - a2) * time + a2) - ((a2 - a1) * time + a1)) * time + ((a2 - a1) * time + a1)) - ((((a2 - a1) * time + a1) - ((a1 - a0) * time + a0)) * time + ((a1 - a0) * time + a0))) * time + ((((a2 - a1) * time + a1) - ((a1 - a0) * time + a0)) * time + ((a1 - a0) * time + a0));
        }

        public static Vector2 RotatePointAboutZaxis(this Vector2 p, double q)
        {
            //x' = x*cos s - y*sin s
            //y' = x*sin s + y*cos s 
            //z' = z
            return new Vector2
                (
                (float)(p.X * Math.Cos(q) - p.Y * Math.Sin(q)),
                (float)(p.X * Math.Sin(q) + p.Y * Math.Cos(q))
                );
        }

        public static Vector3 RotatePointAboutZaxis(this Vector3 p, double q)
        {
            //x' = x*cos s - y*sin s
            //y' = x*sin s + y*cos s 
            //z' = z
            return new Vector3
                (
                (float)(p.X * Math.Cos(q) - p.Y * Math.Sin(q)),
                (float)(p.X * Math.Sin(q) + p.Y * Math.Cos(q)),
                p.Z
                );
        }

        #endregion
    }

}
