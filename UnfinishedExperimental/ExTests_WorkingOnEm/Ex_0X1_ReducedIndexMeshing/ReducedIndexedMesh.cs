using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples  
{

    /// <summary>
    /// This class allows us to create a mesh that can be drawn using shaders and custom vertex data.
    /// </summary>
    public class ReducedIndexedMesh
    {
        public static bool ShowMinimalBasicOutput { get; set; } = true;
        public static bool ShowOutput { get; set; } = false;

        public bool IsWindingCcw { get; private set; } = false;
        public static int AveragingOption { get; set; } = AVG_OPTION_USE_NON_ALPHA_AS_ONE;

        public const int AVG_OPTION_USE_PREMULT_NON_ALPHA_AS_ONE = 4;
        public const int AVG_OPTION_USE_NON_ALPHA_AS_ONE = 3;
        public const int AVG_OPTION_USE_HIGHEST_RGB = 2;
        public const int AVG_OPTION_USE_AVERAGE_RGB = 1;
        public const int AVG_OPTION_USE_RED = 0;

        public VertexPositionNormalTextureTangentWeights[] vertices;
        public int[] indices;
        public int subdivisionWidth = 2;
        public int subdivisionHeight = 2;

        public Texture2D DiffuseTexture { get; set; }
        public Texture2D NormalMapTexture { get; set; }

        Color[] heightColorArray;
        Vector3 defaultNormal = new Vector3(0, 0, 1);


        private Matrix transform = Matrix.Identity;
        private Matrix orientation = Matrix.Identity;
        private Vector3 worldscale = new Vector3(1, 1, 1);
        public Matrix WorldTransformation { get { return transform; } }
        public Vector3 Scale { get { return worldscale; } set { worldscale = value; Transform(); } }
        public Vector3 Position { get { return orientation.Translation; } set { orientation.Translation = value; Transform(); } }
        public Vector3 Center { get { return Scale / 2f; } }
        public Matrix SetWorldTransformation(Vector3 position, Vector3 forward, Vector3 up)
        {
            orientation = Matrix.CreateWorld(position, forward, up);
            Transform();
            return transform;
        }
        public Matrix SetWorldTransformation(Vector3 position, Vector3 forward, Vector3 up, Vector3 scale)
        {
            worldscale = scale;
            orientation = Matrix.CreateWorld(position, forward, up);
            transform = Matrix.Identity * Matrix.CreateScale(scale) * orientation;
            return transform;
        }
        private void Transform()
        {
            transform = Matrix.Identity * Matrix.CreateScale(worldscale) * orientation;
        }


        public ReducedIndexedMesh()
        {
            int w = 2;
            int h = 2;
            heightColorArray = new Color[w * h];
            for (int i = 0; i < w * h; i++)
            {
                heightColorArray[i].R = 0;
                heightColorArray[i].A = 0;
            }
            CreatePrimitiveMesh(heightColorArray, 2, Vector3.Zero, false, false, false);
            heightColorArray = new Color[0];
        }

        /// <param name="scale"> scale should either be 1 or the size of the mesh.</param>
        public ReducedIndexedMesh(int subdivisionWidth, int subdividsionHeight, Vector3 scale, bool windingCounterClockwise, bool negateNormalDirection, bool negateTangentDirection)
        {
            heightColorArray = new Color[subdivisionWidth * subdividsionHeight];
            for (int i = 0; i < subdivisionWidth * subdividsionHeight; i++)
            {
                heightColorArray[i].R = 0;
                heightColorArray[i].A = 0;
            }
            CreatePrimitiveMesh(heightColorArray, subdivisionWidth, scale, windingCounterClockwise, negateNormalDirection, negateTangentDirection);
            heightColorArray = new Color[0];
        }

        /// <param name="scale">  scale should either be 1 or the size of the mesh.</param>
        public ReducedIndexedMesh(float[] heightArray, int strideWidth, bool windingCounterClockwise)
        {
            heightColorArray = new Color[heightArray.Length];
            for (int i = 0; i < heightArray.Length; i++)
            {
                heightColorArray[i].R = GetAvgHeightFromFloatAsByte(heightArray[i]);
                heightColorArray[i].A = GetAvgHeightFromFloatAsByte(heightArray[i]);
            }
            CreatePrimitiveMesh(heightColorArray, strideWidth, Vector3.Zero, windingCounterClockwise, false, false);
            heightColorArray = new Color[0];
        }

        /// <param name="scale">  scale should either be 1 or the size of the mesh.</param>
        public ReducedIndexedMesh(float[] heightArray, int strideWidth, Vector3 scale, bool windingCounterClockwise, bool negateNormalDirection, bool negateTangentDirection)
        {
            heightColorArray = new Color[heightArray.Length];
            for (int i = 0; i < heightArray.Length; i++)
            {
                heightColorArray[i].R = GetAvgHeightFromFloatAsByte(heightArray[i]);
                heightColorArray[i].A = GetAvgHeightFromFloatAsByte(heightArray[i]);
            }
            CreatePrimitiveMesh(heightColorArray, strideWidth, scale, windingCounterClockwise, negateNormalDirection, negateTangentDirection);
            heightColorArray = new Color[0];
        }

        /// <param name="scale">  scale should either be 1 or the size of the mesh.</param>
        public ReducedIndexedMesh(Texture2D heightTexture, Vector3 scale, bool windingCounterClockwise, bool negateNormalDirection, bool negateTangentDirection)
        {
            Color[] heightColorArray = new Color[heightTexture.Width * heightTexture.Height];
            heightTexture.GetData<Color>(heightColorArray);
            CreatePrimitiveMesh(heightColorArray, heightTexture.Width, scale, windingCounterClockwise, negateNormalDirection, negateTangentDirection);
            //heightColorArray = new Color[0];
        }

        /// <param name="scale"> scale id like to get rid of it and just use the transform scaling but... normals and stuff in these examples rely on it being set early. </param>
        public void CreatePrimitiveMesh(Color[] heightColorArray, int strideWidth, Vector3 scale, bool windingCounterClockwise, bool negateNormalDirection, bool negateTangentDirection)
        {
            List<VertexPositionNormalTextureTangentWeights> VertexLists = new List<VertexPositionNormalTextureTangentWeights>();
            List<int> IndexLists = new List<int>();

            IsWindingCcw = windingCounterClockwise;

            subdivisionWidth = strideWidth;
            subdivisionHeight = (int)(heightColorArray.Length / strideWidth);

            if (subdivisionWidth < 2)
                subdivisionWidth = 2;
            if (subdivisionHeight < 2)
                subdivisionHeight = 2;

            float left = -1f;
            float right = +1f;
            float top = -1f;
            float bottom = +1f;

            // add vertices.
            int vertCounter = 0;
            for (int y = 0; y < subdivisionHeight; y++)
            {
                float stepV = (float)(y) / (float)(subdivisionHeight - 1);
                for (int x = 0; x < subdivisionWidth; x++)
                {
                    float stepU = (float)(x) / (float)(subdivisionWidth - 1);

                    float val = GetHeightFromColorAsUnitLengthValue(heightColorArray[GetIndex(x, y, strideWidth)], AveragingOption);
                    float hval = -val;

                    float X = Interpolate(left, right, stepU);
                    float Y = Interpolate(top, bottom, stepV);

                    var p0 = new Vector3(stepU, stepV, hval) * scale;
                    var uv0 = new Vector2(stepU, stepV);

                    VertexLists.Add(GetInitialVertice(p0, uv0));
                    vertCounter += 1;
                }
            }

            // add indices.
            DetermineIndices(IndexLists, subdivisionWidth, subdivisionHeight);

            // calculate normals.
            NormalsAddToVertices(ref VertexLists, ref IndexLists);

            // calculate tangents.
            TangentsAddToVertices(ref VertexLists, ref IndexLists);

            // normalize vectors.
            SmoothNormalsAndTangents(VertexLists, negateNormalDirection, negateTangentDirection);

            if (ShowMinimalBasicOutput)
                Console.WriteLine($"\n new PrimitiveIndexedMesh(...);  strideWidth: {strideWidth}   Winding CCW: {IsWindingCcw}   Vertices: {VertexLists.Count}  Indices: {IndexLists.Count}    Quads: {(IndexLists.Count / 6)}    Triangles: { (IndexLists.Count / 3) }");
            if (ShowOutput)
                ConsoleOutput(VertexLists, IndexLists);

            vertices = VertexLists.ToArray();
            indices = IndexLists.ToArray();

            TurnInto3Dmesh();
        }

        public bool InRange(int index, int len)
        {
            return (index > 0 && index < len) ? true : false;
        }

        public struct Temp
        {
            public int verticeIndex;
            public int x, y;
            public bool isVerticeEmptySpace;
            public bool isVerticeEdge;
            public bool isInnerVertice;
            public bool isInnerVerticeCoplanar;
            public Vector3 position;
            public float GetZ() { return position.Z; }
        }
        public List<Temp> parsedVertexs = new List<Temp>();
        public void TurnInto3Dmesh()
        {
            for(int i = 0; i < vertices.Length;i++)
            {
                int x, y;
                GetIndexXy(i, subdivisionWidth, out x, out y);
                var n = new Temp();
                n.verticeIndex = i;
                n.x = x;
                n.y = y;
                n.position = vertices[i].Position;
                n.isInnerVertice = true;
                parsedVertexs.Add(n);
            }
            
            // Alrighty lets talk about the order of operations.
            // each vertices has 4 surrounding vertices in the general case 3 for sides and 2 for corners.
            // since we already have a built list with uv's and normals and all that.

            // the easiest way to do this is to completely discard the indexs lists well end up rebuilding them from scratch.
            // and by process determine first what the status is of a vertice marking if we skip it for consideration due to it being a empty or coplanar.
            // there is a fill distinction here that determines if a vertice can be ajoined to another.
            // if the vertice is a edge 
            // and its closest possible connections that are value
            // 


        }

        public void MarkAllDirectlyExcludedVertices()
        {
            for (int y = 0;y < subdivisionHeight; y++)
            {
                for (int x = 0;x< subdivisionWidth; x++)
                {
                    int index = GetIndex(x, y, subdivisionWidth);
                    var v = parsedVertexs[index];
                    var z = v.GetZ();
                    float threashold = .001f;
                    if(z > -threashold && z < threashold)
                    {
                        v.isVerticeEmptySpace = true;
                        v.isInnerVertice = false;
                        parsedVertexs[index] = v;
                    }
                }
            }
        }

        public void MarkEdgeVertices()
        {
            int len = subdivisionWidth * subdivisionHeight;
            for (int y = 0; y < subdivisionHeight; y++)
            {
                for (int x = 0; x < subdivisionWidth; x++)
                {
                    int index = GetIndex(x, y, subdivisionWidth);
                    var v = parsedVertexs[index];

                    if (v.isVerticeEmptySpace == false)
                    {

                        int upIndex = GetIndex(x, y - 1, subdivisionWidth);
                        int downIndex = GetIndex(x, y + 1, subdivisionWidth);
                        int leftIndex = GetIndex(x - 1, y, subdivisionWidth);
                        int rightIndex = GetIndex(x + 1, y, subdivisionWidth);

                        if (InRange(upIndex, len) && InRange(downIndex, len) && InRange(leftIndex, len) && InRange(rightIndex, len))
                        {
                            var vup = parsedVertexs[upIndex];
                            var vdown = parsedVertexs[downIndex];
                            var vleft = parsedVertexs[leftIndex];
                            var vright = parsedVertexs[rightIndex];
                            if (vup.isVerticeEmptySpace || vdown.isVerticeEmptySpace || vleft.isVerticeEmptySpace || vright.isVerticeEmptySpace)
                            {
                                v.isVerticeEdge = true;
                                v.isInnerVertice = false;
                            }
                        }
                        else
                        {
                            v.isVerticeEdge = true;
                            v.isInnerVertice = false;
                        }
                    }
                }
            }
        }

        public void MarkInnerCoplanarVertices()
        {
            int len = subdivisionWidth * subdivisionHeight;
            for (int y = 0; y < subdivisionHeight; y++)
            {
                for (int x = 0; x < subdivisionWidth; x++)
                {
                    int index = GetIndex(x, y, subdivisionWidth);
                    var v = parsedVertexs[index];

                    if (v.isInnerVertice)
                    {
                        int upIndex = GetIndex(x, y - 1, subdivisionWidth);
                        int downIndex = GetIndex(x, y + 1, subdivisionWidth);
                        int leftIndex = GetIndex(x - 1, y, subdivisionWidth);
                        int rightIndex = GetIndex(x + 1, y, subdivisionWidth);
                        int upLeftIndex = GetIndex(x-1, y - 1, subdivisionWidth);
                        int downLeftIndex = GetIndex(x-1, y + 1, subdivisionWidth);
                        int upRightIndex = GetIndex(x + 1, y - 1, subdivisionWidth);
                        int downRightIndex = GetIndex(x + 1, y + 1, subdivisionWidth);

                        // here we must do the following ...
                        // out of bounds indexs do not fail the test and are ignored.
                        // isVerticeEmptySpace do not fail the test and are ignored.
                        // other neighboring inner values do not fail the test unless the z value is different.
                        // edges do not fail the test unless ...
                        //  1) the z value is different for nearby vertices.
                        //  2) their is only one  ( ? or more then 2 connecting edges nearby ? probably not) 
                        //  3) there is only 2 vertices nearby but they are not aligned opposite the vertex in question.
                        //  4) this is gonna definately going to be handled by a seperate method.

                        //if (InRange(upIndex, len) && InRange(downIndex, len) && InRange(leftIndex, len) && InRange(rightIndex, len))
                        //{
                        //    var vup = parsedVertexs[upIndex];
                        //    var vdown = parsedVertexs[downIndex];
                        //    var vleft = parsedVertexs[leftIndex];
                        //    var vright = parsedVertexs[rightIndex];

                        //}
                    }

                }
            }
        }

        public void DetermineIndices(List<int> IndexLists, int subdivisionWidth, int subdividsionHeight)
        {
            int quadIndice = 0;
            for (int y = 0; y < subdividsionHeight - 1; y++)
            {
                for (int x = 0; x < subdivisionWidth - 1; x++)
                {
                    var stride = subdivisionWidth;
                    var verticeOffset = stride * y + x;

                    var tl = verticeOffset;
                    var tr = verticeOffset + 1;
                    var br = verticeOffset + stride + 1;
                    var bl = verticeOffset + stride;

                    AddQuadIndexes(tl, tr, br, bl, ref IndexLists);
                    quadIndice += 6;
                }
            }
        }

        /// <summary>
        /// CW
        /// 
        /// triangle 0
        /// tl[0]=0 , tr[1]=1 , br[2]=2   
        /// 
        /// triangle 1
        /// br[3]=2 , bl[4]=3 , tl[5]=0
        /// 
        /// left u=0   top v=0
        ///      -x            -y
        /// 
        /// 0        1
        /// tl        tr
        /// ______
        /// |\        |
        /// |   \t0  |      n +z
        /// | t1 \   |    /
        /// |       \ |  /
        /// ----------/
        /// bl      br
        /// 3        2
        /// 
        /// </summary>
        public void AddQuadIndexes(int tl, int tr, int br, int bl, ref List<int> IndexLists)
        {
            if (IsWindingCcw)
            {
                // ccw
                IndexLists.Add(tl);
                IndexLists.Add(bl);
                IndexLists.Add(br);
                IndexLists.Add(br);
                IndexLists.Add(tr);
                IndexLists.Add(tl);
            }
            else
            {
                //cw
                IndexLists.Add(tl);
                IndexLists.Add(tr);
                IndexLists.Add(br);
                IndexLists.Add(br);
                IndexLists.Add(bl);
                IndexLists.Add(tl);
            }
        }

        /// <summary>
        /// aligns with the order that the function AddQuadIndexes adds vertexs to the index list depending on the winding built with.
        /// </summary>
        public void GetQuadVerticeIndexes(int startIndice, List<int> IndexLists, out int TL_index, out int TR_index, out int BR_index, out int BL_index)
        {
            if (IsWindingCcw)
            {
                //ccw
                TL_index = IndexLists[startIndice + 0];
                TR_index = IndexLists[startIndice + 4];
                BR_index = IndexLists[startIndice + 2];
                BL_index = IndexLists[startIndice + 1];
            }
            else
            {
                //cw
                TL_index = IndexLists[startIndice + 0];
                TR_index = IndexLists[startIndice + 1];
                BR_index = IndexLists[startIndice + 2];
                BL_index = IndexLists[startIndice + 4];
            }
        }

        public void NormalsAddToVertices(ref List<VertexPositionNormalTextureTangentWeights> VertexLists, ref List<int> IndexLists)
        {
            for (int k = 0; k < IndexLists.Count; k += 6)
            {
                int startIndice = k;

                int tl, bl, tr, br;
                GetQuadVerticeIndexes(startIndice, IndexLists, out tl, out tr, out br, out bl);

                var TL = VertexLists[tl];
                var BL = VertexLists[bl];
                var TR = VertexLists[tr];
                var BR = VertexLists[br];

                var d0 = BL.Position - TL.Position;
                var d1 = TR.Position - TL.Position;
                var n = Vector3.Cross(d0, d1);

                TL.Normal += n;
                BL.Normal += n;
                BR.Normal += n;

                d0 = TR.Position - BR.Position;
                d1 = BL.Position - BR.Position;
                n = Vector3.Cross(d0, d1);

                TL.Normal += n;
                TR.Normal += n;
                BR.Normal += n;

                VertexLists[tl] = TL;
                VertexLists[bl] = BL;
                VertexLists[tr] = TR;
                VertexLists[br] = BR;
            }
        }

        public void TangentsAddToVertices(ref List<VertexPositionNormalTextureTangentWeights> VertexLists, ref List<int> IndexLists)
        {
            for (int k = 0; k < IndexLists.Count; k += 6)
            {
                int startIndice = k;

                int tl, bl, tr, br;
                GetQuadVerticeIndexes(startIndice, IndexLists, out tl, out tr, out br, out bl);

                var TL = VertexLists[tl];
                var BL = VertexLists[bl];
                var TR = VertexLists[tr];
                var BR = VertexLists[br];

                // bottom to top direction for tangent ?
                var t0 = (BL.Position - TL.Position);
                var t1 = (BR.Position - TR.Position);
                TL.Tangent += t0;
                BL.Tangent += t0;
                TR.Tangent += t1;
                BR.Tangent += t1;

                VertexLists[tl] = TL;
                VertexLists[bl] = BL;
                VertexLists[tr] = TR;
                VertexLists[br] = BR;
            }
        }

        public void SmoothNormalsAndTangents(List<VertexPositionNormalTextureTangentWeights> VertexLists, bool negateNormalDirection, bool negateTangentDirection)
        {
            // vector addition normals and tangents normalized.
            for (int i = 0; i < VertexLists.Count; i++)
            {
                var v = VertexLists[i];

                v.Normal = Vector3.Normalize(v.Normal);
                v.Tangent = Vector3.Normalize(v.Tangent);

                var bitan = Vector3.Cross(v.Normal, v.Tangent);
                v.Tangent = Vector3.Cross(bitan, v.Normal);

                //if (IsWindingCcw)
                //{
                //    v.Normal = -v.Normal;
                //    v.Tangent = -v.Tangent;
                //}

                //if (negateNormalDirection)
                //    v.Normal = -v.Normal;
                //if (negateTangentDirection)
                //    v.Tangent = -v.Tangent;

                VertexLists[i] = v;
            }
        }
    
        public int GetIndex(int x, int y, int stride)
        {
            return x + y * stride;
        }

        public void GetIndexXy(int Index, int stride, out int x, out int y)
        {
            y = (int)(Index / stride);
            x = Index - (y * stride);
        }

        private float Interpolate(float A, float B, float t)
        {
            return ((B - A) * t) + A;
        }

        private VertexPositionNormalTextureTangentWeights GetInitialVertice(Vector3 v, Vector2 uv)
        {
            return new VertexPositionNormalTextureTangentWeights(v, defaultNormal, uv, Vector3.Zero, new Color(1, 0, 0, 0), new Color(1, 0, 0, 0));
        }

        private byte GetAvgHeightFromFloatAsByte(float v)
        {
            return (byte)(v * 255f);
        }

        private float GetHeightFromColorAsUnitLengthValue(Color c, int option)
        {
            float result = 0;
            float r = (c.R / 255f);
            float g = (c.G / 255f);
            float b = (c.B / 255f);
            float a = (c.A / 255f);
            switch (option)
            {
                case AVG_OPTION_USE_NON_ALPHA_AS_ONE:
                    result = a > .01f ? 1f : 0f;
                    break;
                case AVG_OPTION_USE_PREMULT_NON_ALPHA_AS_ONE:
                    result = r;
                    result = g > result ? g : result;
                    result = b > result ? b : result;
                    result = result > .01f ? 1.0f : 0.0f;
                    break;
                case AVG_OPTION_USE_HIGHEST_RGB:
                    result = r;
                    result = g > result ? g : result;
                    result = b > result ? b : result;
                    break;
                case AVG_OPTION_USE_AVERAGE_RGB:
                    result = (((r + g + b) / 3f) * a);
                    break;
                case AVG_OPTION_USE_RED:
                    result = r;
                    break;
                default:
                    result = a;
                    break;
            }
            return result;
        }

        public void ConsoleOutput(List<VertexPositionNormalTextureTangentWeights> MeshLists, List<int> IndexLists)
        {
            for (int k = 0; k < IndexLists.Count; k += 6)
            {
                System.Console.WriteLine();
                int T0_Index_0 = k + 0;
                int T0_Index_1 = k + 1;
                int T0_Index_2 = k + 2;
                int T1_Index_0 = k + 3;
                int T1_Index_1 = k + 4;
                int T1_Index_2 = k + 5;

                int T0_VIndex_0 = IndexLists[T0_Index_0];
                int T0_VIndex_1 = IndexLists[T0_Index_1];
                int T0_VIndex_2 = IndexLists[T0_Index_2];
                int T1_VIndex_0 = IndexLists[T1_Index_0];
                int T1_VIndex_1 = IndexLists[T1_Index_1];
                int T1_VIndex_2 = IndexLists[T1_Index_2];

                System.Console.WriteLine("quad " + k / 6);
                System.Console.WriteLine("t0   TL  IndexLists [" + T0_Index_0 + "] " + "  vert  [" + T0_VIndex_0 + "] Pos: " + MeshLists[T0_VIndex_0].Position + " Norm: " + MeshLists[T0_VIndex_0].Normal + " Tangent: " + MeshLists[T0_VIndex_0].Tangent);
                System.Console.WriteLine("t0   BL  IndexLists [" + T0_Index_1 + "] " + "  vert  [" + T0_VIndex_1 + "] Pos: " + MeshLists[T0_VIndex_1].Position + " Norm: " + MeshLists[T0_VIndex_1].Normal + " Tangent: " + MeshLists[T0_VIndex_1].Tangent);
                System.Console.WriteLine("t0   BR  IndexLists [" + T0_Index_2 + "] " + "  vert  [" + T0_VIndex_2 + "] Pos: " + MeshLists[T0_VIndex_2].Position + " Norm: " + MeshLists[T0_VIndex_2].Normal + " Tangent: " + MeshLists[T0_VIndex_2].Tangent);

                System.Console.WriteLine();
                System.Console.WriteLine("t1   BR  IndexLists [" + T1_Index_0 + "] " + "  vert  [" + T1_VIndex_0 + "] Pos: " + MeshLists[T1_VIndex_0].Position + " Norm: " + MeshLists[T1_VIndex_0].Normal + " Tangent: " + MeshLists[T1_VIndex_0].Tangent);
                System.Console.WriteLine("t1   TR  IndexLists [" + T1_Index_1 + "] " + "  vert  [" + T1_VIndex_1 + "] Pos: " + MeshLists[T1_VIndex_1].Position + " Norm: " + MeshLists[T1_VIndex_1].Normal + " Tangent: " + MeshLists[T1_VIndex_1].Tangent);
                System.Console.WriteLine("t1   TL  IndexLists [" + T1_Index_2 + "] " + "  vert  [" + T1_VIndex_2 + "] Pos: " + MeshLists[T1_VIndex_2].Position + " Norm: " + MeshLists[T1_VIndex_2].Normal + " Tangent: " + MeshLists[T1_VIndex_2].Tangent);
            }
        }

        public void DrawPrimitive(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3, VertexPositionNormalTextureTangentWeights.VertexDeclaration);
            }
        }
    }
}
