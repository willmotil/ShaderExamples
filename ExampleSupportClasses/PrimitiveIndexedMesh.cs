using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples   //.HelperClasses.EffectClasses
{

    /// <summary>
    /// This class allows us to create a mesh that can be drawn using shaders and custom vertex data.
    /// </summary>
    public class PrimitiveIndexedMesh
    {
        public static bool ShowOutput { get; set; } = false;

        public static int AveragingOption { get; set; } = AVERAGING_OPTION_USE_NONALPHACONSISTANTLY;

        public const int AVERAGING_OPTION_USE_NONALPHACONSISTANTLY = 3;
        public const int AVERAGING_OPTION_USE_HIGHEST = 2;
        public const int AVERAGING_OPTION_USE_AVERAGE = 1;
        public const int AVERAGING_OPTION_USE_RED = 0;

        public bool Clockwise = false;

        public VertexPositionNormalTextureTangentWeights[] vertices;
        public int[] indices;

        public Texture2D DiffuseTexture { get; set; }
        public Texture2D NormalMapTexture { get; set; }

        Color[] heightColorArray;
        Vector3 defaultNormal = new Vector3(0, 0, 1);


        //private Matrix transform = Matrix.Identity;
        //private Matrix orientation = Matrix.Identity;
        //private float worldscale = 1f;
        //public Matrix SetWorldTransformation(Vector3 position, Vector3 forward, Vector3 up, float scale)
        //{
        //    worldscale = scale;
        //    orientation = Matrix.CreateWorld(position, forward, up);
        //    transform = Matrix.Identity * Matrix.CreateScale(scale) * orientation;
        //    return transform;
        //}
        //private void Transform()
        //{
        //    transform = Matrix.Identity * Matrix.CreateScale(worldscale) * orientation;
        //}
        //public Matrix WorldTransformation { get { return transform; } }
        //public float Scale { get { return worldscale; } set { worldscale = value; Transform(); } }
        //public Vector3 Position { get { return orientation.Translation; } set { orientation.Translation = value; Transform(); } }



        public PrimitiveIndexedMesh()
        {
            int w = 2;
            int h = 2;
            heightColorArray = new Color[w * h];
            for (int i = 0; i < w * h; i++)
            {
                heightColorArray[i].R = 0;
                heightColorArray[i].A = 0;
            }
            CreatePrimitiveMesh(heightColorArray, 2, Vector3.Zero, false, false);
            heightColorArray = new Color[0];
        }
        public PrimitiveIndexedMesh(int subdivisionWidth, int subdividsionHeight, Vector3 scale, bool negateNormalDirection, bool negateTangentDirection)
        {
            heightColorArray = new Color[subdivisionWidth * subdividsionHeight];
            for (int i = 0; i < subdivisionWidth * subdividsionHeight; i++)
            {
                heightColorArray[i].R = 0;
                heightColorArray[i].A = 0;
            }
            CreatePrimitiveMesh(heightColorArray, subdivisionWidth, scale, negateNormalDirection, negateTangentDirection);
            heightColorArray = new Color[0];
        }

        public PrimitiveIndexedMesh(float[] heightArray, int strideWidth)
        {
            heightColorArray = new Color[heightArray.Length];
            for (int i = 0; i < heightArray.Length; i++)
            {
                heightColorArray[i].R = GetAvgHeightFromFloatAsByte(heightArray[i]);
                heightColorArray[i].A = GetAvgHeightFromFloatAsByte(heightArray[i]);
            }
            CreatePrimitiveMesh(heightColorArray, strideWidth, Vector3.Zero, false, false);
            heightColorArray = new Color[0];
        }

        public PrimitiveIndexedMesh(float[] heightArray, int strideWidth, Vector3 scale, bool negateNormalDirection, bool negateTangentDirection)
        {
            heightColorArray = new Color[heightArray.Length];
            for (int i = 0; i < heightArray.Length; i++)
            {
                heightColorArray[i].R = GetAvgHeightFromFloatAsByte(heightArray[i]);
                heightColorArray[i].A = GetAvgHeightFromFloatAsByte(heightArray[i]);
            }
            CreatePrimitiveMesh(heightColorArray, strideWidth, scale, negateNormalDirection, negateTangentDirection);
            heightColorArray = new Color[0];
        }

        public PrimitiveIndexedMesh(Texture2D heightTexture, Vector3 scale, bool negateNormalDirection, bool negateTangentDirection)
        {
            Color[] heightColorArray = new Color[heightTexture.Width * heightTexture.Height];
            heightTexture.GetData<Color>(heightColorArray);
            CreatePrimitiveMesh(heightColorArray, heightTexture.Width, scale, negateNormalDirection, negateTangentDirection);
            heightColorArray = new Color[0];
        }

        public void CreatePrimitiveMesh(Color[] heighColorArray, int strideWidth, Vector3 scale, bool negateNormalDirection, bool negateTangentDirection)
        {
            List<VertexPositionNormalTextureTangentWeights> VertexLists = new List<VertexPositionNormalTextureTangentWeights>();
            List<int> IndexLists = new List<int>();

            int subdivisionWidth = strideWidth;
            int subdividsionHeight = (int)(heighColorArray.Length / strideWidth);

            if (subdivisionWidth < 2)
                subdivisionWidth = 2;
            if (subdividsionHeight < 2)
                subdividsionHeight = 2;

            float left = -1f;
            float right = +1f;
            float top = -1f;
            float bottom = +1f;

            // add vertices.
            int vertCounter = 0;
            for (int y = 0; y < subdividsionHeight; y++)
            {
                float stepV = (float)(y) / (float)(subdividsionHeight - 1);
                for (int x = 0; x < subdivisionWidth; x++)
                {
                    float stepU = (float)(x) / (float)(subdivisionWidth - 1);

                    float val = GetAvgHeightFromColorAsUnitLengthValue(heighColorArray[GetIndex(x, y, strideWidth)], AveragingOption);
                    float hval = -val;

                    float X = Interpolate(left, right, stepU);
                    float Y = Interpolate(top, bottom, stepV);

                    var p0 = new Vector3(stepU, stepV, hval) * scale;
                    var uv0 = new Vector2(stepU, stepV);

                    VertexLists.Add(GetInitialVertice(p0, uv0));
                    vertCounter += 1;
                }
            }

            // add indices
            int quadIndice = 0;
            for (int y = 0; y < subdividsionHeight - 1; y++)
            {
                for (int x = 0; x < subdivisionWidth - 1; x++)
                {
                    var stride = subdivisionWidth;
                    var verticeOffset = stride * y + x;

                    var tl = verticeOffset;
                    var tr = verticeOffset + 1;
                    var bl = verticeOffset + stride;
                    var br = verticeOffset + stride + 1;

                    AddQuadIndexes(tl, tr, bl, br, ref IndexLists);

                    quadIndice += 6;
                }
            }

            // calculate normals and tangents
            for (int n = 0; n < IndexLists.Count; n += 6)
            {
                CalcululateNormalsAddToVertices(n, ref VertexLists, ref IndexLists);
                CalcululateTangentsAddToVertices(n, ref VertexLists, ref IndexLists);
            }

            // vector addition normals and tangents normalized.
            for (int i = 0; i < VertexLists.Count; i++)
            {
                var v = VertexLists[i];
                if (negateNormalDirection)
                    v.Normal = -(Vector3.Normalize(v.Normal));
                else
                    v.Normal = Vector3.Normalize(v.Normal);

                if (negateTangentDirection)
                    v.Tangent = -(Vector3.Normalize(v.Tangent));
                else
                    v.Tangent = (Vector3.Normalize(v.Tangent));
                VertexLists[i] = v;
            }

            for (int n = 0; n < IndexLists.Count; n += 6)
                if (ShowOutput)
                    ConsoleOutput(n, VertexLists, IndexLists);

            vertices = VertexLists.ToArray();
            indices = IndexLists.ToArray();
        }

        /// <summary>
        /// CCW
        /// 
        /// tl[0] > bl[1] > tr[2] > br[3]
        /// 
        /// 0        2
        /// tl        tr
        /// |      /  |
        /// | t0/    |
        /// |  /  t1 |
        /// |/        |
        /// bl      br
        /// 1        3
        /// 
        /// triangle 0:   
        /// tl > bl > tr   0,1,2
        /// 
        /// triangle 1:    
        /// br > tr > bl   3,2,1
        /// </summary>
        public void AddQuadIndexes(int tl, int tr, int bl, int br, ref List<int> IndexLists)
        {
            if (Clockwise)
            {
                // tl > bl > tr   0,1,2
                IndexLists.Add(tl);
                IndexLists.Add(bl);
                IndexLists.Add(tr);

                // br > tr > bl   3,2,1
                IndexLists.Add(br);
                IndexLists.Add(tr);
                IndexLists.Add(bl);
            }else
            {
                //IndexLists.Add(tr);
                //IndexLists.Add(bl);
                //IndexLists.Add(tl);

                //IndexLists.Add(bl);
                //IndexLists.Add(tr);
                //IndexLists.Add(br);

                IndexLists.Add(bl);
                IndexLists.Add(tr);
                IndexLists.Add(br);

                IndexLists.Add(tr);
                IndexLists.Add(bl);
                IndexLists.Add(tl);
            }
        }

        public void CalcululateNormalsAddToVertices(int startIndice, ref List<VertexPositionNormalTextureTangentWeights> VertexLists, ref List<int> IndexLists)
        {
            // tl[0] > bl[1] > tr[2] > br[3]
            int tl, bl, tr, br;
            if (Clockwise)
            {
                tl = IndexLists[startIndice + 0];
                bl = IndexLists[startIndice + 1];
                tr = IndexLists[startIndice + 2];
                br = IndexLists[startIndice + 3];
            }
            else
            {
                tl = IndexLists[startIndice + 5];
                bl = IndexLists[startIndice + 4];
                tr = IndexLists[startIndice + 3];
                br = IndexLists[startIndice + 2];
            }

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

        public void CalcululateTangentsAddToVertices(int startIndice, ref List<VertexPositionNormalTextureTangentWeights> VertexLists, ref List<int> IndexLists)
        {
            int tl, bl, tr, br;
            if (Clockwise)
            {
                tl = IndexLists[startIndice + 0];
                bl = IndexLists[startIndice + 1];
                tr = IndexLists[startIndice + 2];
                br = IndexLists[startIndice + 3];
            }
            else
            {
                tl = IndexLists[startIndice + 5];
                bl = IndexLists[startIndice + 4];
                tr = IndexLists[startIndice + 3];
                br = IndexLists[startIndice + 2];
            }

            var TL = VertexLists[tl];
            var BL = VertexLists[bl];
            var TR = VertexLists[tr];
            var BR = VertexLists[br];

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

        public void ConsoleOutput(int k, List<VertexPositionNormalTextureTangentWeights> MeshLists, List<int> IndexLists)
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

        private float GetAvgHeightFromColorAsUnitLengthValue(Color c, int option)
        {
            float result = 0;
            float alphamult = (c.A / 255f);
            switch (option)
            {
                case AVERAGING_OPTION_USE_NONALPHACONSISTANTLY:
                    result = c.A > 0 ? 1f : 0f;
                    break;
                case AVERAGING_OPTION_USE_HIGHEST:
                    result = c.R;
                    result = c.G > result ? c.G : result;
                    result = c.B > result ? c.B : result;
                    result = (c.R / 255f) * alphamult;
                    break;
                case AVERAGING_OPTION_USE_AVERAGE:
                    result = ((((c.R + c.G + c.B) / 3f) / 255f) * alphamult);
                    break;
                case AVERAGING_OPTION_USE_RED:
                    result = (c.R / 255f);
                    break;
                default:
                    result = alphamult;
                    break;
            }
            return result;
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
