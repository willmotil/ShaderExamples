using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public class MgTextureGenerator
    {

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

        public Texture2D GenerateTexture2DWithTopLeftDiscoloration(GraphicsDevice gd)
        {
            var cdata = new Color[250 * 250];
            for (int i = 0; i < 250; i++)
            {
                for (int j = 0; j < 250; j++)
                {
                        if (i < 50 && j < 50)
                            cdata[i * 250 + j] = new Color(120, 120, 120, 255);
                        else
                            cdata[i * 250 + j] = new Color(i, j,( i+j) /2, 255);
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
            var c = new Vector2(radius -1, 0.00f);
            //
            Color[] data = new Color[size * size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var p = new Vector2(x, y);
                    var dist = Vector2.Distance(center, p);
                    var coeff = dist / radius;
                    var curvepoint = MgHelpers.GetPointAtTimeOn2ndDegreePolynominalCurve(a, b, c, coeff);

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
            float sliderControl = 4.00f; // increase beyond 1.0f that gives a more opaque interior
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
                    var curvepoint = MgHelpers.BiCubicSubdivision(a, b, c, d, coeff).ToVector2();

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
                    var curvepoint = MgHelpers.GetPointAtTimeOn2ndDegreePolynominalCurve(a, b, c, dist);

                    data[x + y * 100] = new Color((byte)(curvepoint.Y * 255), (byte)(curvepoint.Y * 255), (byte)(curvepoint.Y * 255), (byte)(curvepoint.Y * 255));
                }
            }
            Texture2D tex = new Texture2D(device, 100, 100);
            tex.SetData<Color>(data);
            return tex;
        }

    }
}
