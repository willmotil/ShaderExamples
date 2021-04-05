using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    //
    // there we go or not the spacing is off needs fixed.
    //
    // 
    // todo's
    // ensure partitions work and then displace meshes.
    // nasa map height data.
    //
    // make a mesh on the list per face need to pass width height
    // extending normals tangents and uv's is all par for the course no problems there.
    // however besides the parameter methods that i need to pass.
    // i need to be able to pass image data for nasa map data i want to add that in.    
    //
    // We want this to be nice and clean i put this off for like a year and didn't take the time to redo it.
    // So take a little time to ...
    // Make it clean simple extendable and reusable into the future.
    //
    // https://github.com/cpt-max/MonoGame-Shader-Samples

    public class PrimitiveSphere
    {
        public bool showOutput = false;
        // ...
        public static Matrix matrixNegativeX = Matrix.CreateWorld(Vector3.Zero, new Vector3(-1.0f, 0, 0), Vector3.Up);
        public static Matrix matrixNegativeZ = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 0, -1.0f), Vector3.Up);
        public static Matrix matrixPositiveX = Matrix.CreateWorld(Vector3.Zero, new Vector3(1.0f, 0, 0), Vector3.Up);
        public static Matrix matrixPositiveZ = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 0, 1.0f), Vector3.Up);
        public static Matrix matrixPositiveY = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 1.0f, 0), Vector3.Backward);
        public static Matrix matrixNegativeY = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, -1.0f, 0), Vector3.Forward);

        public VertexPositionNormalTexture[] cubesFaceVertices;
        public int[] cubesFacesIndices;

        public PrimitiveSphere()
        {
            CreatePrimitiveSphere(0, 0, 1f, false, true, true, null, 1f);
        }

        public PrimitiveSphere(int subdivisionWidth, int subdividsionHeight, float scale, bool clockwise, bool invert, bool flatFaces)
        {
            CreatePrimitiveSphere(subdivisionWidth, subdividsionHeight, scale, clockwise, invert, flatFaces, null, 1f);
        }

        public PrimitiveSphere(int subdivisionWidth, int subdividsionHeight, float scale, bool clockwise, bool invert, bool flatFaces, Texture2D heightMap, float dataScalar)
        {
            CreatePrimitiveSphere(subdivisionWidth, subdividsionHeight, scale, clockwise, invert, flatFaces, heightMap, dataScalar);
        }

        public void CreatePrimitiveSphere(int subdivisionWidth, int subdividsionHeight, float scale, bool clockwise, bool invert, bool flatFaces, Texture2D heightMap, float dataScalar)
        {
            bool hasHeightMapdata = false;
            Color[] heightdata = new Color[0];
            if (heightMap != null)
            {
                hasHeightMapdata = true;
                heightdata = GetSphericalTextureHeightMapData(heightMap);
            }

            List<VertexPositionNormalTexture> cubesFaceMeshLists = new List<VertexPositionNormalTexture>();
            List<int> cubeFaceMeshIndexLists = new List<int>();

            if (subdivisionWidth < 2)
                subdivisionWidth = 2;
            if (subdividsionHeight < 2)
                subdividsionHeight = 2;

            float depth = scale;
            if (invert)
                depth = -depth;

            float left = -1f;
            float right = +1f;
            float top = -1f;
            float bottom = 1f;

            int v = 0;
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                if (showOutput)
                    System.Console.WriteLine("\n  faceIndex: " + faceIndex);
                for (int y = 0; y < subdividsionHeight; y++)
                {
                    float perY = (float)(y) / (float)(subdividsionHeight - 1);
                    for (int x = 0; x < subdivisionWidth; x++)
                    {
                        float perX = (float)(x) / (float)(subdivisionWidth - 1);

                        float X = Interpolate(left, right, perX, false);
                        float Y = Interpolate(top, bottom, perY, false);

                        var p0 = new Vector3(X * scale, Y * scale, depth);
                        var uv0 = new Vector2(perX, perY);
                        VertexPositionNormalTexture v0;
                        if (hasHeightMapdata)
                            v0 = GetVertice(p0, faceIndex, flatFaces, depth, uv0, heightdata, dataScalar, heightMap.Width, heightMap.Height);
                        else
                            v0 = GetVertice(p0, faceIndex, flatFaces, depth, uv0);

                        if (showOutput)
                            System.Console.WriteLine("v0: " + v0);

                        cubesFaceMeshLists.Add(v0);
                        v += 1;
                    }
                }
                if (showOutput)
                    System.Console.WriteLine(" faceIndex: " + faceIndex + " v " + v);
            }

            int faceOffset = 0;
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                if (showOutput)
                    System.Console.WriteLine("\n  faceIndex: " + faceIndex);
                faceOffset = faceIndex * (subdividsionHeight * subdivisionWidth);

                for (int y = 0; y < subdividsionHeight - 1; y++)
                {
                    for (int x = 0; x < subdivisionWidth - 1; x++)
                    {
                        var faceVerticeOffset = subdivisionWidth * y + x + faceOffset;
                        var stride = subdivisionWidth;
                        var tl = faceVerticeOffset;
                        var bl = faceVerticeOffset + stride;
                        var br = faceVerticeOffset + stride + 1;
                        var tr = faceVerticeOffset + 1;

                        cubeFaceMeshIndexLists.Add(tl);
                        cubeFaceMeshIndexLists.Add(bl);
                        cubeFaceMeshIndexLists.Add(br);

                        cubeFaceMeshIndexLists.Add(br);
                        cubeFaceMeshIndexLists.Add(tr);
                        cubeFaceMeshIndexLists.Add(tl);

                        if (showOutput)
                        {
                            System.Console.WriteLine();
                            System.Console.WriteLine("t0  face" + faceIndex + " cubeFaceMeshIndexLists [" + tl + "] " + "  vert " + cubesFaceMeshLists[tl]);
                            System.Console.WriteLine("t0  face" + faceIndex + " cubeFaceMeshIndexLists [" + bl + "] " + "  vert " + cubesFaceMeshLists[bl]);
                            System.Console.WriteLine("t0  face" + faceIndex + " cubeFaceMeshIndexLists [" + br + "] " + "  vert " + cubesFaceMeshLists[br]);

                            System.Console.WriteLine();
                            System.Console.WriteLine("t1  face" + faceIndex + " cubeFaceMeshIndexLists [" + br + "] " + "  vert " + cubesFaceMeshLists[br]);
                            System.Console.WriteLine("t1  face" + faceIndex + " cubeFaceMeshIndexLists [" + tr + "] " + "  vert " + cubesFaceMeshLists[tr]);
                            System.Console.WriteLine("t1  face" + faceIndex + " cubeFaceMeshIndexLists [" + tl + "] " + "  vert " + cubesFaceMeshLists[tl]);
                        }
                    }
                }
            }
            cubesFaceVertices = cubesFaceMeshLists.ToArray();
            cubesFacesIndices = cubeFaceMeshIndexLists.ToArray();
        }

        public static Matrix GetWorldFaceMatrix(int i)
        {
            switch (i)
            {
                case (int)CubeMapFace.PositiveX: // 0 FACE_RIGHT
                    return matrixPositiveX;
                case (int)CubeMapFace.NegativeX: // 1 FACE_LEFT
                    return matrixNegativeX;
                case (int)CubeMapFace.PositiveY: // 2 FACE_TOP
                    return matrixPositiveY;
                case (int)CubeMapFace.NegativeY: // 3 FACE_BOTTOM
                    return matrixNegativeY;
                case (int)CubeMapFace.PositiveZ: // 4 FACE_BACK
                    return matrixPositiveZ;
                case (int)CubeMapFace.NegativeZ: // 5 FACE_FORWARD
                    return matrixNegativeZ;
                default:
                    return matrixNegativeZ;
            }
        }

        private float Interpolate(float A, float B, float t)
        {
            return ((B - A) * t) + A;
        }
        private float Interpolate(float A, float B, float t, bool reverse)
        {
            if (reverse)
                return ((A - B) * t) + B;
            else
                return ((B - A) * t) + A;
        }

        private VertexPositionNormalTexture GetVertice(Vector3 v, int faceIndex, bool flatFaces, float depth, Vector2 uv)
        {
            var v2 = Vector3.Transform(v, GetWorldFaceMatrix(faceIndex));
            var n = Vector3.Normalize(v2);
            v2 = n * depth;
            return new VertexPositionNormalTexture(v2, FlatFaceOrDirectional(v, faceIndex, flatFaces, depth), uv);
        }

        private VertexPositionNormalTexture GetVertice(Vector3 v, int faceIndex, bool flatFaces, float depth, Vector2 uv, Color[] heightData, float dataScalar, int width, int height)
        {
            var v2 = Vector3.Transform(v, GetWorldFaceMatrix(faceIndex));
            var n = Vector3.Normalize(v2);
            var suv = CubeMapNormalTo2dEquaRectangularMapUvCoordinatesAlt(n);
            int x = (int)(suv.X * width);
            int y = (int)(suv.Y * height);
            int index = y * width + x;
            float avg = ((float)(heightData[index].R / 255f) + (float)(heightData[index].G / 255f) + (float)(heightData[index].B / 255f)) / 3f;
            var mapDepth = (avg) * dataScalar;

            //mapDepth = 1.0f;

            v2 = n * (depth + mapDepth);
            return new VertexPositionNormalTexture(v2, FlatFaceOrDirectional(v2, faceIndex, flatFaces, depth), uv);
        }

        private Vector3 FlatFaceOrDirectional(Vector3 v, int faceIndex, bool flatFaces, float depth)
        {
            if (flatFaces == true)
                v = new Vector3(0, 0, depth);
            v = Vector3.Normalize(v);
            return Vector3.Transform(v, GetWorldFaceMatrix(faceIndex));
        }

        public Color[] GetSphericalTextureHeightMapData(Texture2D texturemap)
        {
            var data = new Color[texturemap.Width * texturemap.Height];
            texturemap.GetData<Color>(data);
            return data;
        }

        // https://en.wikipedia.org/wiki/List_of_common_coordinate_transformations

        Vector2 NormalTo2dSphericalUvCoordinates(Vector3 normal)
        {
            Vector2 uv = new Vector2(Atan2Xna(-normal.Z, normal.X, true), (float)Math.Asin(normal.Y));
            Vector2 INVERT_ATAN = new Vector2(0.1591f, 0.3183f);
            uv = uv * INVERT_ATAN + new Vector2(0.5f, 0.5f);
            return uv;
        }

        //Vector2 CubeMapNormalTo2dEquaRectangularMapUvCoordinatesAlt(Vector3 v)
        //{
        //    float pi = 3.141592653589793f;
        //    Vector3 n = Vector3.Normalize(v);
        //    float lon = (float)System.Math.Atan2(-n.Z, n.X);
        //    float lat = (float)Math.Acos(n.Y);
        //    Vector2 sphereCoords = new Vector2(lon, lat) * (1.0f / pi);
        //    return new Vector2(sphereCoords.X * 0.5f + 0.5f, sphereCoords.Y);
        //}

        Vector2 CubeMapNormalTo2dEquaRectangularMapUvCoordinatesAlt(Vector3 v)
        {
            float pi = 3.141592653589793f;
            Vector3 n = Vector3.Normalize(v);
            float lon = (float)System.Math.Atan2(-n.Z, n.X);
            float lat = (float)Math.Acos(-n.Y);
            Vector2 sphereCoords = new Vector2(lon, lat) * (1.0f / pi);
            return new Vector2(sphereCoords.X * 0.5f + 0.5f, 1.0f - sphereCoords.Y);
        }

        public static float Atan2Xna(float difx, float dify, bool SpriteBatchAtan2)
        {
            if (SpriteBatchAtan2)
                return (float)System.Math.Atan2(difx, dify) * -1f;
            else
                return (float)System.Math.Atan2(difx, dify);
        }

        public void DrawPrimitiveSphere(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, cubesFaceVertices, 0, cubesFaceVertices.Length, cubesFacesIndices, 0, cubesFacesIndices.Length / 3, VertexPositionNormalTexture.VertexDeclaration);
            }
        }

        public void DrawPrimitiveSphere(GraphicsDevice gd, Effect effect, TextureCube cubeTexture)
        {
            effect.Parameters["CubeMap"].SetValue(cubeTexture);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, cubesFaceVertices, 0, cubesFaceVertices.Length, cubesFacesIndices, 0, cubesFacesIndices.Length / 3, VertexPositionNormalTexture.VertexDeclaration);
            }
        }

        public void DrawPrimitiveSphereFace(GraphicsDevice gd, Effect effect, TextureCube cubeTexture, int cubeFaceToRender)
        {
            effect.Parameters["CubeMap"].SetValue(cubeTexture);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, cubesFaceVertices, cubeFaceToRender * 6, 2, VertexPositionNormalTexture.VertexDeclaration);
            }
        }

    }
}
