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
    //

    public class PrimitiveSphere
    {
        // constants
        public const int USAGE_CUBE_UNDER_CCW = 0;
        public const int USAGE_CUBE_UNDER_CW = 1;
        public const int USAGE_SKYSPHERE_UNDER_CCW = 2;
        public const int USAGE_SKYSPHERE_UNDER_CW = 3;

        public bool showOutput = false;
        public bool IsSkyBox { get { return usage > 1; } }
        public bool IsWindingCcw { get { return (usage == 0 || usage == 2); } }

        public VertexPositionNormalTextureTangentWeights[] vertices;
        public int[] indices;
        public TextureCube textureCube;

        private Matrix transform = Matrix.Identity;
        private Matrix orientation = Matrix.Identity;
        private float worldscale = 1f;
        public Matrix WorldTransformation { get { return transform; } }
        public float Scale { get { return worldscale; } set { worldscale = value; Transform(); } }
        public Vector3 Position { get { return orientation.Translation; } set { orientation.Translation = value; Transform(); } }
        private void Transform()
        {
            transform = Matrix.Identity * Matrix.CreateScale(worldscale) * orientation;
        }
        public Matrix SetWorldTransformation(Vector3 position, Vector3 forward, Vector3 up, float scale)
        {
            worldscale = scale;
            orientation = Matrix.CreateWorld(position, forward, up);
            transform = Matrix.Identity * Matrix.CreateScale(scale) * orientation;
            return transform;
        }


        #region private stuff

        private int usage = 0;
        private bool windVerticesClockwise = false;
        private bool invert = false;

        // This matches the definitions for cubefaces but ... it doesn't matter cause the uv data just comes from normal direction on the texture cube.
        private static Matrix matrixPositiveX = Matrix.CreateWorld(Vector3.Zero, new Vector3(1.0f, 0, 0), Vector3.Up);  // 0  
        private static Matrix matrixNegativeX = Matrix.CreateWorld(Vector3.Zero, new Vector3(-1.0f, 0, 0), Vector3.Up);  // 1
        private static Matrix matrixPositiveY = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 1.0f, 0), Vector3.Backward); //2
        private static Matrix matrixNegativeY = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, -1.0f, 0), Vector3.Forward);  //3
        private static Matrix matrixPositiveZ = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 0, 1.0f), Vector3.Up);   //4
        private static Matrix matrixNegativeZ = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 0, -1.0f), Vector3.Up);   // 5

        #endregion

        public PrimitiveSphere(int subdivisionWidth, int subdividsionHeight, float scale, int Usage_Options, bool invert, bool flatFaces)
        {
            CreatePrimitiveSphere(subdivisionWidth, subdividsionHeight, scale, Usage_Options, invert, flatFaces, null, 1f);
        }

        public PrimitiveSphere(int subdivisionWidth, int subdividsionHeight, float scale, int Usage_Options, bool invert, bool flatFaces, Texture2D heightMap, float dataScalar)
        {
            CreatePrimitiveSphere(subdivisionWidth, subdividsionHeight, scale, Usage_Options, invert, flatFaces, heightMap, dataScalar);
        }

        public void CreatePrimitiveSphere(int subdivisionWidth, int subdividsionHeight, float worldscale, int Usage_Options, bool invert, bool flatFaces, Texture2D heightMap, float dataScalar)
        {
            List<VertexPositionNormalTextureTangentWeights> cubesFaceMeshVertList = new List<VertexPositionNormalTextureTangentWeights>();
            List<int> cubeFaceMeshIndexList = new List<int>();

            this.usage = Usage_Options;
            this.invert = invert;
            Scale = worldscale;

            switch (usage)
            {
                case USAGE_CUBE_UNDER_CCW:
                    windVerticesClockwise = false;
                    break;
                case USAGE_SKYSPHERE_UNDER_CCW:
                    windVerticesClockwise = true;
                    break;
                case USAGE_CUBE_UNDER_CW:
                    windVerticesClockwise = true;
                    break;
                case USAGE_SKYSPHERE_UNDER_CW:
                    windVerticesClockwise = false;
                    break;
            }

            if (subdivisionWidth < 2)
                subdivisionWidth = 2;
            if (subdividsionHeight < 2)
                subdividsionHeight = 2;

            var quadsPerFace = ((subdivisionWidth - 1) * (subdividsionHeight - 1));
            var quadsTotal = quadsPerFace * 6;
            System.Console.WriteLine($"\n Expected ...   quads per face {quadsPerFace}   quadsTotal {quadsTotal} ");

            CreateIndices(cubeFaceMeshIndexList, subdivisionWidth, subdividsionHeight);

            CreateInitialVertices( cubesFaceMeshVertList, subdivisionWidth, subdividsionHeight, flatFaces, heightMap, dataScalar);

            CreateTangents( cubesFaceMeshVertList, cubeFaceMeshIndexList, subdivisionWidth, subdividsionHeight);

            vertices = cubesFaceMeshVertList.ToArray();
            indices = cubeFaceMeshIndexList.ToArray();
        }

        private void CreateIndices(List<int> cubeFaceMeshIndexLists, int subdivisionWidth, int subdividsionHeight)
        {
            int faceOffset = 0;
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
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

                        if (windVerticesClockwise)
                        {
                            cubeFaceMeshIndexLists.Add(tl);
                            cubeFaceMeshIndexLists.Add(tr);
                            cubeFaceMeshIndexLists.Add(br);

                            cubeFaceMeshIndexLists.Add(br);
                            cubeFaceMeshIndexLists.Add(bl);
                            cubeFaceMeshIndexLists.Add(tl);
                        }
                        else
                        {
                            cubeFaceMeshIndexLists.Add(tl);
                            cubeFaceMeshIndexLists.Add(bl);
                            cubeFaceMeshIndexLists.Add(br);

                            cubeFaceMeshIndexLists.Add(br);
                            cubeFaceMeshIndexLists.Add(tr);
                            cubeFaceMeshIndexLists.Add(tl);
                        }

                        //if (showOutput)
                        //    Output(faceIndex, cubeFaceMeshIndexLists, tl, bl, br, tr);
                    }
                }
            }
        }

        private void CreateInitialVertices(List<VertexPositionNormalTextureTangentWeights> cubesFaceMeshVertsLists, int subdivisionWidth, int subdividsionHeight, bool flatFaces, Texture2D heightMap, float dataScalar)
        {
            bool hasHeightMapdata = false;
            Color[] heightdata = new Color[0];
            if (heightMap != null)
            {
                hasHeightMapdata = true;
                heightdata = GetSphericalTextureHeightMapData(heightMap);
            }

            var tl = new Vector2(-1, -1);
            var br = new Vector2(1, 1);

            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                for (int y = 0; y < subdividsionHeight; y++)
                {
                    Vector2 uv;
                    uv.Y = (float)(y) / (float)(subdividsionHeight - 1);
                    for (int x = 0; x < subdivisionWidth; x++)
                    {
                        uv.X = (float)(x) / (float)(subdivisionWidth - 1);
                        var tmp = Interpolate(tl, br, uv, false);
                        var p = new Vector3(tmp.X , tmp.Y , 1f);
                        VertexPositionNormalTextureTangentWeights vert;
                        if (hasHeightMapdata)
                            vert = GetVerticeDisplacementMapped(p, faceIndex, flatFaces, uv, heightdata, dataScalar, heightMap.Width, heightMap.Height);
                        else
                            vert = GetVertice(p, faceIndex, flatFaces, uv);
                        cubesFaceMeshVertsLists.Add(vert);

                        if (showOutput)
                            System.Console.WriteLine("new vertice: " + vert);
                    }
                }
            }
        }

        private void CreateTangents( List<VertexPositionNormalTextureTangentWeights> cubesFaceMeshVertLists, List<int> cubeFaceMeshIndexLists, int subdivisionWidth, int subdividsionHeight)
        {
            int faceOffset = 0;
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                faceOffset = faceIndex * (subdividsionHeight * subdivisionWidth);
                for (int y = 0; y < subdividsionHeight-1; y++)
                {
                    for (int x = 0; x < subdivisionWidth; x++)
                    {
                        var faceVerticeOffset = subdivisionWidth * y + x + faceOffset;
                        int tl = faceVerticeOffset;
                        int bl = faceVerticeOffset + subdivisionWidth;
                        var vTL = cubesFaceMeshVertLists[tl];
                        var vBL = cubesFaceMeshVertLists[bl];
                        Vector3 t = Vector3.Normalize(vBL.Position - vTL.Position);
                        vTL.Tangent += t;
                        vBL.Tangent += t;
                        cubesFaceMeshVertLists[tl] = vTL;
                        cubesFaceMeshVertLists[bl] = vBL;
                    }
                }
            }

            // smooth out tangents and normals. vector addition normals and tangents normalized.
            for (int curvert = 0; curvert < cubesFaceMeshVertLists.Count; curvert++)
            {
                var v = cubesFaceMeshVertLists[curvert];
                v.Normal = Vector3.Normalize(v.Normal);
                v.Tangent = Vector3.Normalize(v.Tangent);
                //  Negating here due to a bug in monogames storage of texel data in DX for TextureCubes this might be as bad or worse then handling things on the shader directly.
                //v.Normal = -Vector3.Normalize(v.Normal); //v.Tangent = -Vector3.Normalize(v.Tangent);
                cubesFaceMeshVertLists[curvert] = v;
            }
        }

        private VertexPositionNormalTextureTangentWeights GetVertice(Vector3 pos, int faceIndex, bool flatFaces, Vector2 uv)
        {
            var v2 = Vector3.Transform(pos, GetWorldFaceMatrix(faceIndex));
            var n = Vector3.Normalize(v2);
            var normal = FlatFaceOrDirectional(pos, faceIndex, flatFaces);
            return new VertexPositionNormalTextureTangentWeights(n, normal , uv, Vector3.Zero, Color.Transparent, new Color(1,0,0,0) );
        }

        private VertexPositionNormalTextureTangentWeights GetVerticeDisplacementMapped(Vector3 pos, int faceIndex, bool flatFaces, Vector2 uv, Color[] heightData, float dataScalar, int width, int height)
        {
            var v = Vector3.Normalize(Vector3.Transform(pos, GetWorldFaceMatrix(faceIndex)));
            var suv = CubeMapNormalTo2dEquaRectangularMapUvCoordinatesAlt(v);
            int x = (int)(suv.X * width);
            int y = (int)(suv.Y * height);
            int index = y * width + x;
            float avg = ((float)(heightData[index].R / 255f) + (float)(heightData[index].G / 255f) + (float)(heightData[index].B / 255f)) / 3f;
            v = v * (avg * dataScalar + 1f);
            var normal = FlatFaceOrDirectional(v, faceIndex, flatFaces);
            return new VertexPositionNormalTextureTangentWeights(v, normal, uv, Vector3.Zero, Color.Transparent, new Color(1, 0, 0, 0));
        }


        // TODO maybe getting rid of this will help simplify things  besides i already called the get world face matrix why am i doing it again here ?  copy pasting my old code and then adding crap on.
        private Vector3 FlatFaceOrDirectional(Vector3 v, int faceIndex, bool flatFaces)
        {
            if (flatFaces)
                v = new Vector3(0, 0, 1f);
            v = Vector3.Normalize(v);
            return Vector3.Transform(v, GetWorldFaceMatrix(faceIndex));
        }

        public static Matrix GetWorldFaceMatrix(int faceIndex)
        {
            switch (faceIndex)
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
        private Vector2 Interpolate(Vector2 A, Vector2 B, Vector2 t, bool reverse)
        {
            if (reverse)
                return ((A - B) * t) + B;
            else
                return ((B - A) * t) + A;
        }

        private Color[] GetSphericalTextureHeightMapData(Texture2D texturemap)
        {
            var data = new Color[texturemap.Width * texturemap.Height];
            texturemap.GetData<Color>(data);
            return data;
        }

        public void DrawPrimitiveSphere(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3, VertexPositionNormalTextureTangentWeights.VertexDeclaration);
            }
        }

        public void DrawPrimitiveSphereFace(GraphicsDevice gd, Effect effect, int cubeFaceToRender)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, cubeFaceToRender * 6, 2, VertexPositionNormalTexture.VertexDeclaration);
            }
        }

    }
}

//#region extra

//public void Output(int faceIndex, List<int> meshIndexes, int tl, int bl, int br, int tr)
//{
//    System.Console.WriteLine();
//    System.Console.WriteLine("t0  face" + faceIndex + " cubeFaceMeshIndexLists [" + tl + "] " + "  vert " + meshIndexes[tl]);
//    System.Console.WriteLine("t0  face" + faceIndex + " cubeFaceMeshIndexLists [" + bl + "] " + "  vert " + meshIndexes[bl]);
//    System.Console.WriteLine("t0  face" + faceIndex + " cubeFaceMeshIndexLists [" + br + "] " + "  vert " + meshIndexes[br]);

//    System.Console.WriteLine();
//    System.Console.WriteLine("t1  face" + faceIndex + " cubeFaceMeshIndexLists [" + br + "] " + "  vert " + meshIndexes[br]);
//    System.Console.WriteLine("t1  face" + faceIndex + " cubeFaceMeshIndexLists [" + tr + "] " + "  vert " + meshIndexes[tr]);
//    System.Console.WriteLine("t1  face" + faceIndex + " cubeFaceMeshIndexLists [" + tl + "] " + "  vert " + meshIndexes[tl]);
//}

//bool HasInvalidValues(Vector3 v)
//{
//    //    System.Diagnostics.Debug.Assert(HasInvalidValues(v.Tangent), $"Tangent Inf or Nan  PrimitiveSphere.CreateTangents() vertice {curvert}");
//    return IsNan(v) || IsInfinity(v);
//}
//bool IsNan(Vector3 v)
//{
//    return float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z);
//}
//bool IsInfinity(Vector3 v)
//{
//    return float.IsInfinity(v.X) || float.IsInfinity(v.Y) || float.IsInfinity(v.Z);
//}

//float NormalIdentity(Vector3 v)
//{
//    var d = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
//    if (d > .9999f && d < 1.0001f)
//        return 1.0f;
//    else
//        return d;
//}

//// https://en.wikipedia.org/wiki/List_of_common_coordinate_transformations

//private Vector2 NormalTo2dSphericalUvCoordinates(Vector3 normal)
//{
//    Vector2 uv = new Vector2(Atan2Xna(-normal.Z, normal.X, true), (float)Math.Asin(normal.Y));
//    Vector2 INVERT_ATAN = new Vector2(0.1591f, 0.3183f);
//    uv = uv * INVERT_ATAN + new Vector2(0.5f, 0.5f);
//    return uv;
//}

//private Vector2 CubeMapNormalTo2dEquaRectangularMapUvCoordinatesAlt(Vector3 v)
//{
//    float pi = 3.141592653589793f;
//    Vector3 n = Vector3.Normalize(v);
//    float lon = (float)System.Math.Atan2(-n.Z, n.X);
//    float lat = (float)Math.Acos(-n.Y);  // or +y
//    Vector2 sphereCoords = new Vector2(lon, lat) * (1.0f / pi);
//    return new Vector2(sphereCoords.X * 0.5f + 0.5f, 1.0f - sphereCoords.Y);
//}

//public static float Atan2Xna(float difx, float dify, bool SpriteBatchAtan2)
//{
//    if (SpriteBatchAtan2)
//        return (float)System.Math.Atan2(difx, dify) * -1f;
//    else
//        return (float)System.Math.Atan2(difx, dify);
//}

//#endregion

//// further the z must also be adjusted as the data is stored across faces inverted.
//switch (usage)
//{
//    case USAGE_CUBE_UNDER_CCW:
//        if (invert)
//        { }
//        else
//        {
//            //v.Normal = -v.Normal;
//            //v.Normal.Z = -v.Normal.Z; v.Tangent.Z = -v.Tangent.Z; 
//        }
//        break;
//    case USAGE_SKYSPHERE_UNDER_CCW:
//        if (invert)
//        { 
//            //v.Normal.Z = -v.Normal.Z; v.Tangent.Z = -v.Tangent.Z; 
//        }
//        else
//        { }
//        break;
//    case USAGE_CUBE_UNDER_CW:
//        if (invert)
//        { }
//        else
//        { 
//            //v.Normal.Z = -v.Normal.Z; v.Tangent.Z = -v.Tangent.Z; 
//        }
//        break;
//    case USAGE_SKYSPHERE_UNDER_CW:
//        if (invert)
//        { 
//            //v.Normal.Z = -v.Normal.Z; v.Tangent.Z = -v.Tangent.Z; 
//        }
//        else
//        { }
//        break;
//}

/*

// DX This is with ccw  triangles outgoing normals and upward tangents in the negative u direction.
//// outward cube
////float3x3 m = float3x3 (
////    -1, 0, 0,
////    0, -1, 0,
////    0, 0, +1
////    );
float4 PS_RenderCcwCube(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	//N = float3(-N.x, -N.y, N.z); // outward cube.
	N = float3(N.x, N.y, -N.z);

	float4 envMapColor = texCUBElod(CubeMapSampler, float4 (N , 0));
	//clip(envMapColor.a - .01f); // just straight clip super low alpha.
	return float4(envMapColor.rgb, 1.0f);
}

// DX This is with ccw  triangles outgoing normals and upward tangents in the negative u direction.
float4 PS_RenderCcwSkybox(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	//N = -N;   // inward skybox.

	float4 envMapColor = texCUBElod(CubeMapSampler, float4 (N , 0));
	//clip(envMapColor.a - .01f); // just straight clip super low alpha.
	return float4(envMapColor.rgb, 1.0f);
}

*/