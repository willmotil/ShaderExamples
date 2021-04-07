using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples //Microsoft.Xna.Framework
{
    //
    // We want this to be nice and clean i put this off for like a year and didn't take the time to redo it.
    // So take a little time to ...
    // Make it clean simple extendable and reusable into the future.
    //
    // https://cpetry.github.io/NormalMap-Online/  make a normal map.
    //
    // http://graphics.cs.cmu.edu/nsp/course/15-462/Spring04/slides/09-texture.pdf
    // https://www.katjaas.nl/transpose/transpose.html more matrix stuff waves complexs and fouriers.
    // http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-13-normal-mapping/

    public class ProtoTypePrimitiveSphere
    {
        public bool showOutput = false;
        // ...
        public static Matrix matrixNegativeX = Matrix.CreateWorld(Vector3.Zero, new Vector3(-1.0f, 0, 0), Vector3.Up);
        public static Matrix matrixNegativeZ = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 0, -1.0f), Vector3.Up);
        public static Matrix matrixPositiveX = Matrix.CreateWorld(Vector3.Zero, new Vector3(1.0f, 0, 0), Vector3.Up);
        public static Matrix matrixPositiveZ = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 0, 1.0f), Vector3.Up);
        public static Matrix matrixPositiveY = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 1.0f, 0), Vector3.Backward);
        public static Matrix matrixNegativeY = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, -1.0f, 0), Vector3.Forward);

        public VertexPositionNormalTextureTangentWeights[] cubesFaceVertices;
        public int[] cubesFacesIndices;

        //public PrimitiveSphere()
        //{
        //    CreatePrimitiveSphere(0, 0, 1f, false, true, true, null, 1f, false, false);
        //}

        public ProtoTypePrimitiveSphere(int subdivisionWidth, int subdividsionHeight, float scale, bool clockwise, bool invert, bool flatFaces)
        {
            CreatePrimitiveSphere(subdivisionWidth, subdividsionHeight, scale, clockwise, invert, flatFaces, null, 1f, false, false);
        }

        public ProtoTypePrimitiveSphere(int subdivisionWidth, int subdividsionHeight, float scale, bool clockwise, bool invert, bool flatFaces, Texture2D heightMap, float dataScalar)
        {
            CreatePrimitiveSphere(subdivisionWidth, subdividsionHeight, scale, clockwise, invert, flatFaces, heightMap, dataScalar, false, false);
        }

        public ProtoTypePrimitiveSphere(int subdivisionWidth, int subdividsionHeight, float scale, bool clockwise, bool invert, bool flatFaces, bool negateNormalDirection, bool negateTangentDirection)
        {
            CreatePrimitiveSphere(subdivisionWidth, subdividsionHeight, scale, clockwise, invert, flatFaces, null, 1f, negateNormalDirection, negateTangentDirection);
        }

        public ProtoTypePrimitiveSphere(int subdivisionWidth, int subdividsionHeight, float scale, bool clockwise, bool invert, bool flatFaces, Texture2D heightMap, float dataScalar, bool negateNormalDirection, bool negateTangentDirection)
        {
            CreatePrimitiveSphere(subdivisionWidth, subdividsionHeight, scale, clockwise, invert, flatFaces, heightMap, dataScalar, negateNormalDirection, negateTangentDirection);
        }

        public void CreatePrimitiveSphere(int subdivisionWidth, int subdividsionHeight, float scale, bool clockwise, bool invert, bool flatFaces, Texture2D heightMap, float dataScalar, bool negateNormalDirection, bool negateTangentDirection)
        {
            List<VertexPositionNormalTextureTangentWeights> cubesFaceMeshVertList = new List<VertexPositionNormalTextureTangentWeights>();
            List<int> cubeFaceMeshIndexList = new List<int>();

            if (subdivisionWidth < 2)
                subdivisionWidth = 2;
            if (subdividsionHeight < 2)
                subdividsionHeight = 2;

            var quadsPerFace = ((subdivisionWidth - 1) * (subdividsionHeight - 1));
            var quadsTotal = quadsPerFace * 6;
            System.Console.WriteLine($"\n Expected ...   quads per face {quadsPerFace}   quadsTotal {quadsTotal} ");

            CreateInitialVertices( cubesFaceMeshVertList, subdivisionWidth, subdividsionHeight, scale, clockwise, invert, flatFaces, heightMap, dataScalar);

            CreateIndices( cubeFaceMeshIndexList, subdivisionWidth, subdividsionHeight);

            CreateTangents( cubesFaceMeshVertList, cubeFaceMeshIndexList, subdivisionWidth, subdividsionHeight, negateNormalDirection, negateTangentDirection);

            cubesFaceVertices = cubesFaceMeshVertList.ToArray();
            cubesFacesIndices = cubeFaceMeshIndexList.ToArray();
        }

        bool HasInvalidValues(Vector3 v)
        {
            return IsNan(v) || IsInfinity(v);
        }
        bool IsNan(Vector3 v)
        {
            return float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z);
        }
        bool IsInfinity(Vector3 v)
        {
            return float.IsInfinity(v.X) || float.IsInfinity(v.Y) || float.IsInfinity(v.Z);
        }

        float NormalIdentity(Vector3 v)
        {
            var d = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
            if (d > .9999f && d < 1.0001f)
                return 1.0f;
            else
                return d;
        }

        private void CreateInitialVertices(List<VertexPositionNormalTextureTangentWeights> cubesFaceMeshVertsLists, int subdivisionWidth, int subdividsionHeight, float scale, bool clockwise, bool invert, bool flatFaces, Texture2D heightMap, float dataScalar)
        {
            bool hasHeightMapdata = false;
            Color[] heightdata = new Color[0];
            if (heightMap != null)
            {
                hasHeightMapdata = true;
                heightdata = GetSphericalTextureHeightMapData(heightMap);
            }

            float depth = scale;
            if (invert)
                depth = -depth;

            float left = -1f;
            float right = +1f;
            float top = -1f;
            float bottom = 1f;

            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                //if (showOutput)
                //    System.Console.WriteLine("\n  faceIndex: " + faceIndex);
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
                        VertexPositionNormalTextureTangentWeights v0;
                        if (hasHeightMapdata)
                            v0 = GetVertice(p0, faceIndex, flatFaces, depth, uv0, heightdata, dataScalar, heightMap.Width, heightMap.Height);
                        else
                            v0 = GetVertice(p0, faceIndex, flatFaces, depth, uv0);

                        if (showOutput)
                            System.Console.WriteLine("v0: " + v0);

                        cubesFaceMeshVertsLists.Add(v0);
                    }
                }
            }
            //System.Console.WriteLine($"\n  totalVertices.Count: {cubesFaceMeshVertsLists.Count} ");
        }

        private void CreateIndices(List<int> cubeFaceMeshIndexLists, int subdivisionWidth, int subdividsionHeight)
        {
            int faceOffset = 0;
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                //if (showOutput)
                //    System.Console.WriteLine("\n  faceIndex: " + faceIndex);
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
                            Output(faceIndex, cubeFaceMeshIndexLists, tl, bl, br, tr);
                    }
                }
            }
            //System.Console.WriteLine($"\n  cubeFaceMeshIndexLists: {cubeFaceMeshIndexLists.Count}    count/6: {cubeFaceMeshIndexLists.Count/6}");
        }

        private void CreateTangents( List<VertexPositionNormalTextureTangentWeights> cubesFaceMeshVertLists, List<int> cubeFaceMeshIndexLists, int subdivisionWidth, int subdividsionHeight, bool negateNormalDirection, bool negateTangentDirection)
        {
            int faceOffset = 0;
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                //Console.WriteLine($"faceIndex {faceIndex}");
                faceOffset = faceIndex * (subdividsionHeight * subdivisionWidth);
                for (int y = 0; y < subdividsionHeight-1; y++)
                {
                    //Console.WriteLine($"faceIndex {faceIndex} Y index {y}");
                    for (int x = 0; x < subdivisionWidth; x++)
                    {
                        var faceVerticeOffset = subdivisionWidth * y + x + faceOffset;
                        var tl = faceVerticeOffset;
                        var bl = faceVerticeOffset + subdivisionWidth;

                        var vTL = cubesFaceMeshVertLists[tl];
                        var vBL = cubesFaceMeshVertLists[bl];

                        var t = Vector3.Normalize(vBL.Position - vTL.Position);

                        vTL.Tangent += t;
                        vBL.Tangent += t;

                        cubesFaceMeshVertLists[tl] = vTL;
                        cubesFaceMeshVertLists[bl] = vBL;

                        //Console.WriteLine($"OO y [{y}] x [{x}]  faceOffset {faceOffset}  faceVerticeOffset {faceVerticeOffset}   tl {tl} bl {bl}    tangent    vTL{cubesFaceMeshVertLists[tl].Tangent} {NormalIdentity(cubesFaceMeshVertLists[tl].Tangent)}       vBL{cubesFaceMeshVertLists[bl].Tangent}  {NormalIdentity(cubesFaceMeshVertLists[bl].Tangent)}   ");
                    }
                }
            }


            //if (HasInvalidValues(vBL.Tangent) || HasInvalidValues(vTL.Tangent) || vBL.Tangent == Vector3.Zero || vTL.Tangent == Vector3.Zero)
            //{
            //    Console.WriteLine($"faceVerticeOffset {faceVerticeOffset} vTL.Tangent{vTL.Tangent} vBL.Tangent{vBL.Tangent}  ");
            //}

            // smooth out tangents and normals.
            // vector addition normals and tangents normalized.
            for (int curvert = 0; curvert < cubesFaceMeshVertLists.Count; curvert++)
            {
                var v = cubesFaceMeshVertLists[curvert];

                if (negateNormalDirection)
                    v.Normal = -(Vector3.Normalize(v.Normal));
                else
                    v.Normal = Vector3.Normalize(v.Normal);

                if (negateTangentDirection)
                    v.Tangent = -(Vector3.Normalize(v.Tangent));
                else
                    v.Tangent = (Vector3.Normalize(v.Tangent));

                //var bitan = Vector3.Cross(v.Normal, v.Tangent);
                //v.Tangent = Vector3.Cross(v.Normal, bitan);

                if (HasInvalidValues(v.Tangent) || v.Tangent == Vector3.Zero)
                    System.Diagnostics.Debug.Assert(HasInvalidValues(v.Tangent), $"Tnagent Inf or Nan  PrimitiveSphere.CreateTangents() vertice {curvert}");

                cubesFaceMeshVertLists[curvert] = v;
            }

            faceOffset = 0;
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                //Console.WriteLine($"faceIndex {faceIndex}");
                faceOffset = faceIndex * (subdividsionHeight * subdivisionWidth);
                for (int y = 0; y < subdividsionHeight; y++)
                {
                    //Console.WriteLine($"faceIndex {faceIndex} Y {y}");
                    for (int x = 0; x < subdivisionWidth; x++)
                    {
                        var faceVerticeOffset = subdivisionWidth * y + x + faceOffset;

                        var v = cubesFaceMeshVertLists[faceVerticeOffset];

                        //if (HasInvalidValues(v.Tangent) || v.Tangent == Vector3.Zero)
                        //    Console.WriteLine($"XXXXX  y [{y}] x [{x}]  current vert {faceVerticeOffset}      v.tan  {v.Tangent}  {NormalIdentity(v.Tangent)} ");
                        //else
                        //    Console.WriteLine($">>       y [{y}] x [{x}]  current vert {faceVerticeOffset}     v.tan  {v.Tangent}  {NormalIdentity(v.Tangent)}");
                    }
                }
            }

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

        private VertexPositionNormalTextureTangentWeights GetVertice(Vector3 v, int faceIndex, bool flatFaces, float depth, Vector2 uv)
        {
            var v2 = Vector3.Transform(v, GetWorldFaceMatrix(faceIndex));
            var n = Vector3.Normalize(v2);
            v2 = n * depth;
            return new VertexPositionNormalTextureTangentWeights(v2, FlatFaceOrDirectional(v, faceIndex, flatFaces, depth), uv, Vector3.Zero, Color.Transparent, new Color(1,0,0,0) );
        }

        private VertexPositionNormalTextureTangentWeights GetVertice(Vector3 v, int faceIndex, bool flatFaces, float depth, Vector2 uv, Color[] heightData, float dataScalar, int width, int height)
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
            return new VertexPositionNormalTextureTangentWeights(v2, FlatFaceOrDirectional(v2, faceIndex, flatFaces, depth), uv, Vector3.Zero, Color.Transparent, new Color(1, 0, 0, 0));
        }

        private Vector3 FlatFaceOrDirectional(Vector3 v, int faceIndex, bool flatFaces, float depth)
        {
            if (flatFaces)
                v = new Vector3(0, 0, depth);
            v = Vector3.Normalize(v);
            return Vector3.Transform(v, GetWorldFaceMatrix(faceIndex));
        }

        private Color[] GetSphericalTextureHeightMapData(Texture2D texturemap)
        {
            var data = new Color[texturemap.Width * texturemap.Height];
            texturemap.GetData<Color>(data);
            return data;
        }

        // https://en.wikipedia.org/wiki/List_of_common_coordinate_transformations

        private Vector2 NormalTo2dSphericalUvCoordinates(Vector3 normal)
        {
            Vector2 uv = new Vector2(Atan2Xna(-normal.Z, normal.X, true), (float)Math.Asin(normal.Y));
            Vector2 INVERT_ATAN = new Vector2(0.1591f, 0.3183f);
            uv = uv * INVERT_ATAN + new Vector2(0.5f, 0.5f);
            return uv;
        }

        private Vector2 CubeMapNormalTo2dEquaRectangularMapUvCoordinatesAlt(Vector3 v)
        {
            float pi = 3.141592653589793f;
            Vector3 n = Vector3.Normalize(v);
            float lon = (float)System.Math.Atan2(-n.Z, n.X);
            float lat = (float)Math.Acos(-n.Y);  // or +y
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

        public void Output(int faceIndex, List<int> meshIndexes, int tl, int bl, int br, int tr)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("t0  face" + faceIndex + " cubeFaceMeshIndexLists [" + tl + "] " + "  vert " + meshIndexes[tl]);
            System.Console.WriteLine("t0  face" + faceIndex + " cubeFaceMeshIndexLists [" + bl + "] " + "  vert " + meshIndexes[bl]);
            System.Console.WriteLine("t0  face" + faceIndex + " cubeFaceMeshIndexLists [" + br + "] " + "  vert " + meshIndexes[br]);

            System.Console.WriteLine();
            System.Console.WriteLine("t1  face" + faceIndex + " cubeFaceMeshIndexLists [" + br + "] " + "  vert " + meshIndexes[br]);
            System.Console.WriteLine("t1  face" + faceIndex + " cubeFaceMeshIndexLists [" + tr + "] " + "  vert " + meshIndexes[tr]);
            System.Console.WriteLine("t1  face" + faceIndex + " cubeFaceMeshIndexLists [" + tl + "] " + "  vert " + meshIndexes[tl]);
        }

        public void DrawPrimitiveSphere(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, cubesFaceVertices, 0, cubesFaceVertices.Length, cubesFacesIndices, 0, cubesFacesIndices.Length / 3, VertexPositionNormalTextureTangentWeights.VertexDeclaration);
            }
        }

        public void DrawPrimitiveSphere(GraphicsDevice gd, Effect effect, TextureCube cubeTexture)
        {
            effect.Parameters["CubeMap"].SetValue(cubeTexture);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, cubesFaceVertices, 0, cubesFaceVertices.Length, cubesFacesIndices, 0, cubesFacesIndices.Length / 3, VertexPositionNormalTextureTangentWeights.VertexDeclaration);
            }
        }

        public void DrawPrimitiveSphereFace(GraphicsDevice gd, Effect effect, TextureCube cubeTexture, int cubeFaceToRender)
        {
            effect.Parameters["CubeMap"].SetValue(cubeTexture);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, cubesFaceVertices, cubeFaceToRender * 6, 2, VertexPositionNormalTextureTangentWeights.VertexDeclaration);
            }
        }

    }
}
