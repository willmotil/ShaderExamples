using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples //.ExampleSupportClasses.ClassUtilitysExtensions
{

    /// <summary>
    /// This class creates renderable lines to aid in visualizing vertex normals or tangents.
    /// </summary>
    public class VisualizationNormals
    {
        public VertexPositionNormalTexture[] vertices;
        public int[] indices;
        public Texture2D texture;

        public BasicEffect basicEffect;

        public VisualizationNormals() { }

        public void SetUpBasicEffect(GraphicsDevice device, Texture2D texture, Matrix view, Matrix proj)
        {
            basicEffect = new BasicEffect(device);
            basicEffect.VertexColorEnabled = false;
            basicEffect.TextureEnabled = true;
            //basicEffect.LightingEnabled = true;
            //basicEffect.EnableDefaultLighting();
            //basicEffect.AmbientLightColor = new Vector3(1.0f,1.0f,1.0f);
            World = Matrix.Identity;
            basicEffect.View = view;
            basicEffect.Projection = proj;
            basicEffect.Texture = texture;
        }

        public Matrix World { set { basicEffect.World = value; } get { return basicEffect.World; } }
        public Matrix View { set { basicEffect.View = value; } get { return basicEffect.View; } }
        public Matrix Projection { set { basicEffect.Projection = value; } get { return basicEffect.Projection; } }
        public Texture2D Texture { set { basicEffect.Texture = value; } get { return basicEffect.Texture; } }

        public void CreateVisualNormalsForPrimitiveMesh(VertexPositionNormalTexture[] inVertices, int[] inIndices, Texture2D t, float thickness, float scale)
        {
            texture = t;
            int len = inVertices.Length;

            // we will make a tubular line
            List<VertexPositionNormalTexture> nverts = new List<VertexPositionNormalTexture>();
            List<int> nindices = new List<int>();

            // well define the number of sides of the tube
            int sides = 4;
            // the number of vertices per line
            int lineVerts = 4 * 2;
            // the number of indices per line
            int lineIndices = sides * 6;

            // for each vertice in the model
            for (int j = 0; j < len; j++)
            {
                int startvert = j * lineVerts;
                int startindices = j * lineIndices;

                var p = inVertices[j].Position;
                var n = inVertices[j].Normal;
                var p2 = n * scale + p;

                int index = 0;
                float radMult = 6.28f / sides;
                for (int k = 0; k < sides; k++)
                {
                    float rads = (float)(k) * radMult;
                    var m = Matrix.CreateFromAxisAngle(n, rads);
                    var mright = m.Right;
                    var np = p + m.Right * thickness;
                    var np2 = p2 + m.Right * thickness;

                    var v0 = new VertexPositionNormalTexture() { Position = np, Normal = n, TextureCoordinate = new Vector2((float)(k) / (float)(sides - 1), 0f) };
                    var v1 = new VertexPositionNormalTexture() { Position = np2, Normal = n, TextureCoordinate = new Vector2((float)(k) / (float)(sides - 1), 1f) };
                    nverts.Add(v0);
                    nverts.Add(v1);

                    index += 2;
                }
            }

            // build the indices and line them up to the vertices.
            for (int j = 0; j < len; j++)
            {
                int startvert = j * lineVerts;
                int startindices = j * lineIndices;
                for (int quadindex = 0; quadindex < sides; quadindex++)
                {
                    int offsetVertice = quadindex * 2 + startvert;
                    //int offsetIndice = quadindex * 6;

                    if (quadindex != sides - 1)
                    {
                        nindices.Add(offsetVertice + 0);
                        nindices.Add(offsetVertice + 1);
                        nindices.Add(offsetVertice + 2);

                        nindices.Add(offsetVertice + 2);
                        nindices.Add(offsetVertice + 1);
                        nindices.Add(offsetVertice + 3);
                    }
                    else // the last face wraps around well sort of manually attach this.
                    {
                        nindices.Add(offsetVertice + 0);
                        nindices.Add(offsetVertice + 1);
                        nindices.Add(startvert + 0);

                        nindices.Add(startvert + 0);
                        nindices.Add(offsetVertice + 1);
                        nindices.Add(startvert + 1);
                    }
                }
            }

            this.vertices = nverts.ToArray();
            this.indices = nindices.ToArray();
        }

        public void Draw(GraphicsDevice gd)
        {
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionNormalTexture.VertexDeclaration);
            }
        }

        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionNormalTexture.VertexDeclaration);
            }
        }
    }

}
