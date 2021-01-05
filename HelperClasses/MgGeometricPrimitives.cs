using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// Assortment hodge podge of primitives ive made over time. 
// I really need to line these all up so they work the same way and use the same vertex structures.

namespace Microsoft.Xna.Framework
{

    /// <summary>
    /// This mesh will allow 2 types of constructors one that will take a point array from the user.
    /// The other will allow the user to simply define the size of a mesh x y and it will be created not particularly useful.
    /// However it could be used as a basic tilemapping example typically model meshes use indexing. 
    /// This makes shared vertice normals possibel so you don't end up with flat shading.
    /// </summary>
    public class MeshNonIndexed
    {
        VertexPositionTextureNormalTangent[] vertices;
        int w;
        int h;
        public bool invertTexAddressU = false;
        public bool invertTexAddressV = false;

        /// <summary>
        /// This method takes the R value for height data mapping it to the range of 0 to 1 for the z value.
        /// The G B A elements are unused as of yet.
        /// </summary>
        public VertexPositionTextureNormalTangent[] CreateMesh(Texture2D textureHeightMapData, Rectangle modelRectangle,float depthScalar, bool flipNormalDirection, bool reverseMapAddressU, bool reverseMapAddressV, bool reverseTexAddressU, bool reverseTexAddressV)
        {
            invertTexAddressU = reverseTexAddressU;
            invertTexAddressV = reverseTexAddressV;
            w = textureHeightMapData.Width;
            h = textureHeightMapData.Height;
            //well use the height data via get data.
            Color[] data = new Color[w * h];
            textureHeightMapData.GetData<Color>(data);

            List<VertexPositionTextureNormalTangent> vertlist = new List<VertexPositionTextureNormalTangent>();

            var tl = new Vector2(0, 0);
            var tr = new Vector2(1, 0);
            var bl = new Vector2(0, 1);
            var br = new Vector2(1, 1);

            // initial calculation and fill in the struct with dummy variables , we will have to calculate the normals and tangents later.
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    int mx = reverseMapAddressU ? (w - 1) - x :  x;
                    int my = reverseMapAddressV ? (h - 1) - y :  y;
                    Vector2 transMapAddr = Vector2.One;
                    if (reverseMapAddressU)
                        transMapAddr.X = -1;
                    if (reverseMapAddressV)
                        transMapAddr.Y = -1;
                    var tlz = data[GetIndex(new Vector2(mx, my) + tl * transMapAddr)].R / 255f * depthScalar;
                    var trz = data[GetIndex(new Vector2(mx, my) + tr * transMapAddr)].R / 255f * depthScalar;
                    var blz = data[GetIndex(new Vector2(mx, my) + bl * transMapAddr)].R / 255f * depthScalar;
                    var brz = data[GetIndex(new Vector2(mx, my) + br * transMapAddr)].R / 255f * depthScalar;
                    //int mx = reverseMapAddressU ? (w - 2) - x :  x;
                    //int my = reverseMapAddressV ? (h - 2) - y :  y;

                    var uvXy = new Vector2(x, y);
                    // t0
                    vertlist.Add(new VertexPositionTextureNormalTangent(new Vector3(x, y, tlz) + tl.ToVector3(), uvFromXy(uvXy + tl, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                    vertlist.Add(new VertexPositionTextureNormalTangent(new Vector3(x, y, blz) + bl.ToVector3(), uvFromXy(uvXy + bl, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                    vertlist.Add(new VertexPositionTextureNormalTangent(new Vector3(x, y, trz) + tr.ToVector3(), uvFromXy(uvXy + tr, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                    // t1
                    vertlist.Add(new VertexPositionTextureNormalTangent(new Vector3(x, y, trz) + tr.ToVector3(), uvFromXy(uvXy + tr, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                    vertlist.Add(new VertexPositionTextureNormalTangent(new Vector3(x, y, blz) + bl.ToVector3(), uvFromXy(uvXy + bl, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                    vertlist.Add(new VertexPositionTextureNormalTangent(new Vector3(x, y, brz) + br.ToVector3(), uvFromXy(uvXy + br, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                }
            }
            RePositionVerticesInModelSpace(ref vertlist, modelRectangle);
            DetermineQuadNormals(ref vertlist, flipNormalDirection);
            DetermineQuadTangents(ref vertlist, flipNormalDirection);
            vertices = vertlist.ToArray();
            return vertices;
        }

        

        public VertexPositionTextureNormalTangent[] CreateMesh(Vector3[] positionArray, int verticesWidth, int verticesHeight, bool flipNormalDirection, bool reverseU, bool reverseV)
        {
            invertTexAddressU = reverseU;
            invertTexAddressV = reverseV;
            w = verticesWidth;
            h = verticesHeight;
            List<VertexPositionTextureNormalTangent> vertlist = new List<VertexPositionTextureNormalTangent>();

            // initial calculation and fill in the struct with dummy variables.
            // the normals and tangents must be calculated to be proper well do that later.
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    var uvXy = new Vector2(x, y);
                    // we do a index calculation and get the position.
                    var index2d = new Vector2(x, y);
                    var tl = index2d + new Vector2(0, 0);   var p0 = positionArray[GetIndex(tl)];
                    var tr = index2d + new Vector2(1, 0);   var p1 = positionArray[GetIndex(tr)];
                    var bl = index2d + new Vector2(0, 1);   var p2 = positionArray[GetIndex(bl)];
                    var br = index2d + new Vector2(1, 1);   var p3 = positionArray[GetIndex(br)];
                    // t0
                    vertlist.Add(new VertexPositionTextureNormalTangent(p0, uvFromXy(uvXy + tl, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                    vertlist.Add(new VertexPositionTextureNormalTangent(p0, uvFromXy(uvXy + bl, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                    vertlist.Add(new VertexPositionTextureNormalTangent(p0, uvFromXy(uvXy + tr, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                    // t1
                    vertlist.Add(new VertexPositionTextureNormalTangent(p0, uvFromXy(uvXy + tr, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                    vertlist.Add(new VertexPositionTextureNormalTangent(p0, uvFromXy(uvXy + bl, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                    vertlist.Add(new VertexPositionTextureNormalTangent(p0, uvFromXy(uvXy + br, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                }
            }
            DetermineQuadNormals(ref vertlist, flipNormalDirection);
            DetermineQuadTangents(ref vertlist, flipNormalDirection);
            vertices = vertlist.ToArray();
            return vertices;
        }

        public VertexPositionTextureNormalTangent[] CreateMesh(Rectangle modelRectangle, int verticesWidth, int verticesHeight, bool flipNormalDirection, bool reverseU, bool reverseV)
        {
            invertTexAddressU = reverseU;
            invertTexAddressV = reverseV;
            w = verticesWidth;
            h = verticesHeight;
            List<VertexPositionTextureNormalTangent> vertlist = new List<VertexPositionTextureNormalTangent>();

            var tl = new Vector2(0, 0);
            var tr = new Vector2(1, 0);
            var bl = new Vector2(0, 1);
            var br = new Vector2(1, 1);

            // initial calculation and fill in the struct with dummy variables , we will have to calculate the normals and tangents later.
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    var uvXy = new Vector2(x, y);
                    // t0
                    vertlist.Add(new VertexPositionTextureNormalTangent(new Vector3(x, y, 0) + tl.ToVector3(), uvFromXy(uvXy + tl, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1),  new Vector3(0, -1, 0)));
                    vertlist.Add(new VertexPositionTextureNormalTangent(new Vector3(x, y, 0) + bl.ToVector3(), uvFromXy(uvXy + bl, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                    vertlist.Add(new VertexPositionTextureNormalTangent(new Vector3(x, y, 0) + tr.ToVector3(), uvFromXy(uvXy + tr, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                    // t1
                    vertlist.Add(new VertexPositionTextureNormalTangent(new Vector3(x, y, 0) + tr.ToVector3(), uvFromXy(uvXy + tr, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                    vertlist.Add(new VertexPositionTextureNormalTangent(new Vector3(x, y, 0) + bl.ToVector3(), uvFromXy(uvXy + bl, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1),  new Vector3(0, -1, 0)));
                    vertlist.Add(new VertexPositionTextureNormalTangent(new Vector3(x, y, 0) + br.ToVector3(), uvFromXy(uvXy + br, invertTexAddressU, invertTexAddressV), new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
                }
            }
            RePositionVerticesInModelSpace(ref vertlist, modelRectangle);
            DetermineQuadNormals(ref vertlist, flipNormalDirection);
            DetermineQuadTangents(ref vertlist, flipNormalDirection);
            vertices = vertlist.ToArray();
            return vertices;
        }

        private void RePositionVerticesInModelSpace(ref List<VertexPositionTextureNormalTangent> vertlist, Rectangle modelSpaceRectangle)
        {
            // resize to the world rectangle this is still in object space. 
            // so the rectangle might want to be like -100,100  and have a size of 200,200 to center it in local object space.
            var loc = modelSpaceRectangle.Location.ToVector2();
            var size = modelSpaceRectangle.Size.ToVector2();
            for (int i = 0; i < vertlist.Count; i += 1)
            {
                var v = vertlist[i];
                var z = v.Position.Z;
                var ratio = (v.Position / new Vector3(w - 1, h - 1, 1));
                v.Position = ratio * size.ToVector3(1f) + loc.ToVector3(0);
                v.Position.Z = z;
                vertlist[i] = v;
            }
        }

        // flat normals.
        public void DetermineQuadNormals(ref List<VertexPositionTextureNormalTangent> vertlist, bool flipDirection)
        {
            // generate the normals
            for (int i = 0; i < vertlist.Count; i += 6)
            {
                var n0 = Vector3.Normalize(CrossProduct3d(vertlist[i + 0].Position, vertlist[i + 1].Position, vertlist[i + 2].Position));
                if (flipDirection)
                {
                    n0 = -n0;
                    //n1 = -n1;
                }
                //t0
                vertlist[i + 0] = SetNormal(vertlist[i + 0], n0);
                vertlist[i + 1] = SetNormal(vertlist[i + 1], n0);
                vertlist[i + 2] = SetNormal(vertlist[i + 2], n0);
                // t1
                vertlist[i + 3] = SetNormal(vertlist[i + 3], n0);
                vertlist[i + 4] = SetNormal(vertlist[i + 4], n0);
                vertlist[i + 5] = SetNormal(vertlist[i + 5], n0);
            }
        }

        public void DetermineQuadTangents(ref List<VertexPositionTextureNormalTangent> vertlist, bool flipDirection)
        {
            // generate the tangents
            for (int i = 0; i < vertlist.Count; i += 6)
            {
                var tn0 = Vector3.Normalize(vertlist[i + 0].Position - vertlist[i + 1].Position);
                //var tn1 = Vector3.Normalize(vertlist[i + 3].Position - vertlist[i + 5].Position);
                if (flipDirection)
                {
                    tn0 = -tn0;
                    //tn1 = -tn1;
                }
                //t0
                vertlist[i + 0] = SetTangent(vertlist[i + 0], tn0);
                vertlist[i + 1] = SetTangent(vertlist[i + 1], tn0);
                vertlist[i + 2] = SetTangent(vertlist[i + 2], tn0);
                // t1
                vertlist[i + 3] = SetTangent(vertlist[i + 3], tn0);
                vertlist[i + 4] = SetTangent(vertlist[i + 4], tn0);
                vertlist[i + 5] = SetTangent(vertlist[i + 5], tn0);
            }
        }

        public VertexPositionTextureNormalTangent SetPosition(VertexPositionTextureNormalTangent v, Vector3 n)
        {
            v.Position = n;
            return v;
        }
        public VertexPositionTextureNormalTangent SetUvCoordinates(VertexPositionTextureNormalTangent v, Vector2 n)
        {
            v.TextureCoordinate = n;
            return v;
        }
        public VertexPositionTextureNormalTangent SetNormal(VertexPositionTextureNormalTangent v, Vector3 n)
        {
            v.Normal = n;
            return v;
        }
        public VertexPositionTextureNormalTangent SetTangent(VertexPositionTextureNormalTangent v, Vector3 n)
        {
            v.Tangent = n;
            return v;
        }

        public static Vector3 Cross(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 A = new Vector3(
                (b.X - a.X),
                (b.Y - a.Y),
                (b.Z - a.Z)
                );
            Vector3 B = new Vector3(
                (c.X - a.X),
                (c.Y - a.Y),
                (c.Z - a.Z)
                );
            return new Vector3(
                A.Y * B.Z - B.Y * A.Z,
                -(A.X * B.Z - B.X * A.Z),
                A.X * B.Y - B.X * A.Y
                );
        }

        // hum this version of mine has a error too many of them all over.
        public Vector3 CrossProduct3d(Vector3 a, Vector3 b, Vector3 c)
        {
            return new Vector3
                (
                ((b.Y - a.Y) * (c.Z - b.Z)) - ((c.Y - b.Y) * (b.Z - a.Z)),
                ((b.Z - a.Z) * (c.X - b.X)) - ((c.Z - b.Z) * (b.X - a.X)),
                ((b.X - a.X) * (c.Y - b.Y)) - ((c.X - b.X) * (b.Y - a.Y))
                );
        }
        Vector2 uvFromXy(Vector2 v, bool reverseU, bool reverseV)
        {
            return uvFromXy(v.X, v.Y, reverseU, reverseV);
        }
        Vector2 uvFromXy(float x, float y, bool reverseU, bool reverseV)
        {
            var uv = new Vector2(x / (float)(w - 1), y / (float)(h - 1));
            if (reverseU)
                uv.X = 1f - uv.X;
            if (reverseV)
                uv.Y = 1f - uv.Y;
            return uv;
        }
        int GetIndex(Vector2 p)
        {
            return (int)p.X + (int)p.Y * w;
        }
        int GetIndex(int x, int y)
        {
            return x + y * w;
        }

        public BasicEffect BasicEffectSettingsForThisPrimitive(GraphicsDevice gd, BasicEffect effect, Texture2D texture)
        {
            effect.VertexColorEnabled = false;
            effect.TextureEnabled = true;
            effect.EnableDefaultLighting();
            effect.LightingEnabled = true;
            effect.PreferPerPixelLighting = true;
            effect.Texture = texture;
            return effect;
        }

        public void Draw(GraphicsDevice gd, BasicEffect effect, Texture2D texture)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3, VertexPositionTextureNormalTangent.VertexDeclaration);
            }
        }

        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3, VertexPositionNormalTexture.VertexDeclaration);
            }
        }

    }

    public class Quad
    {
        public VertexPositionNormalTexture[] vertices;
        public int[] indices;
        public bool changeToXz = true;

        float z = 0.0f;
        float adjustmentX = 0f; // .5
        float adjustmentY = 0f; // -.5
        float scale = 2f; // scale 2 and matrix identity passed straight thru is litterally orthographic

        public Quad()
        {
            CreateQuad();
        }
        public Quad(float scale)
        {
            this.scale = scale;
            CreateQuad();
        }
        public Quad(float scale, float z, float adjustmentX, float adjustmentY)
        {
            this.scale = scale;
            this.adjustmentX = adjustmentX;
            this.adjustmentY = adjustmentY;
            this.z = z;
            CreateQuad();
        }

        private void CreateQuad()
        {
            //    
            //    //    0          2 
            //    //   LT ------ RT
            //    //   |          |  
            //    //   |1         |3 
            //    //   LB ------ RB
            //
            indices = new int[6];
            indices[0] = 0; indices[1] = 1; indices[2] = 2;
            indices[3] = 2; indices[4] = 1; indices[5] = 3;


            vertices = new VertexPositionNormalTexture[4];
            vertices[0].Position = new Vector3((adjustmentX - 0.5f) * scale, (adjustmentY - 0.5f) * scale, z);
            vertices[0].Normal = Vector3.Backward;
            //vertices_Quad[0].Color = Color.White;
            vertices[0].TextureCoordinate = new Vector2(0f, 1f);

            vertices[1].Position = new Vector3((adjustmentX + 0.5f) * scale, (adjustmentY - 0.5f) * scale, z);
            vertices[1].Normal = Vector3.Backward;
            //vertices_Quad[1].Color = Color.White;
            vertices[1].TextureCoordinate = new Vector2(0f, 0f);

            vertices[2].Position = new Vector3((adjustmentX - 0.5f) * scale, (adjustmentY + 0.5f) * scale, z);
            vertices[2].Normal = Vector3.Backward;
            //vertices_Quad[2].Color = Color.White;
            vertices[2].TextureCoordinate = new Vector2(1f, 1f);

            vertices[3].Position = new Vector3((adjustmentX + 0.5f) * scale, (adjustmentY + 0.5f) * scale, z);
            vertices[3].Normal = Vector3.Backward;
            //vertices_Quad[3].Color = Color.White;
            vertices[3].TextureCoordinate = new Vector2(1f, 0f);

            if (changeToXz)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 n = vertices[i].Position;
                    vertices[i].Position = new Vector3(n.X, n.Z, n.Y);
                }
            }

            // ok finding a bi tanget or what ever its called a perpendicular is straightforward.
            // provided im using a quad. even if im not i can use some simple averages on the triangle and figure out the up and or right.
            //Vector3 parrallelPlaneNormalUp = vertices[0].Position - vertices[1].Position;
            // this is for all the vertices ^^^ as long as the quad is flat to itself which it should be.
            //
            //vertices[0].Tangent = Vector3.Normalize(vertices[0].Position - vertices[1].Position);
            //vertices[1].Tangent = Vector3.Normalize(vertices[0].Position - vertices[1].Position);
            //vertices[2].Tangent = Vector3.Normalize(vertices[0].Position - vertices[1].Position);
            //vertices[3].Tangent = Vector3.Normalize(vertices[0].Position - vertices[1].Position);

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

    public class GridPlanes3D
    {
        public Grid3d gridForward;
        public Grid3d gridRight;
        public Grid3d gridUp;

        /// <summary>
        /// Draws 3 3d grids, linewith should be very small like .001
        /// </summary>
        public GridPlanes3D(int x, int y, float lineWidth, Color xAxisColor, Color yAxisColor, Color zAxisColor )
        {
            gridForward = new Grid3d(x, y, lineWidth, true, 0, zAxisColor);
            gridRight = new Grid3d(x, y, lineWidth, true, 1, xAxisColor);
            gridUp = new Grid3d(x, y, lineWidth, true, 2, yAxisColor);
        }

        /// <summary>
        /// Draws this world grid with basic effect.
        /// </summary>
        public void DrawWithBasicEffect(GraphicsDevice gd, BasicEffect effect, Matrix world, float scale, Texture2D forwardTexture, Texture2D upTexture, Texture2D rightTexture)
        {
            // Draw a 3d full orientation grid
            var ms = effect.World;

            effect.World = Matrix.CreateScale(scale) * world;
            bool isLighting = effect.LightingEnabled;
            effect.LightingEnabled = false;
            effect.VertexColorEnabled = true;

            effect.Texture = upTexture;
            gridForward.Draw(gd, effect);

            effect.Texture = forwardTexture;
            gridRight.Draw(gd, effect);

            effect.Texture = rightTexture;
            gridUp.Draw(gd, effect);

            // restore.
            if (isLighting)
                effect.LightingEnabled = true;
            effect.World = ms;
        }

        public void DrawWithBasicEffect(GraphicsDevice gd, BasicEffect effect, Matrix world, float scale, Texture2D texture, bool drawZgrid, bool drawYgrid, bool drawXgrid)
        {
            var ms = effect.World;

            effect.World = Matrix.CreateScale(scale) * world;
            bool isLighting = effect.LightingEnabled;
            effect.LightingEnabled = false;
            effect.VertexColorEnabled = true;
            effect.Texture = texture;

            if (drawZgrid)
                gridForward.Draw(gd, effect);
            if (drawYgrid)
                gridUp.Draw(gd, effect);
            if (drawXgrid)
                gridRight.Draw(gd, effect);

            // restore.
            if (isLighting)
                effect.LightingEnabled = true;
            effect.World = ms;
        }

        /// <summary>
        /// The method expects that the shader can accept a parameter named TextureA.
        /// </summary>
        public void Draw(GraphicsDevice gd, Effect effect, Texture2D forwardTexture, Texture2D upTexture, Texture2D rightTexture)
        {
            // Draw a 3d full orientation grid
            effect.Parameters["TextureA"].SetValue(upTexture);
            gridForward.Draw(gd, effect);
            effect.Parameters["TextureA"].SetValue(forwardTexture);
            gridRight.Draw(gd, effect);
            effect.Parameters["TextureA"].SetValue(rightTexture);
            gridUp.Draw(gd, effect);
        }

        public void Draw(GraphicsDevice gd, Effect effect, bool drawZgrid, bool drawYgrid, bool drawXgrid)
        {
            if (drawZgrid)
                gridForward.Draw(gd, effect);
            if (drawYgrid)
                gridUp.Draw(gd, effect);
            if (drawXgrid)
                gridRight.Draw(gd, effect);
        }
    }

    public class Grid3d
    {
        int width;
        int height;
        public VertexPositionColorTexture[] vertices;
        public int[] indices;

        /// <summary>
        /// Creates a grid for 3d modelspace.
        /// The Width Height is doubled into negative and positive.
        /// linesize should be a very small value less then 1;
        /// flip options range from 0 to 2
        /// </summary>
        public Grid3d(int rows, int columns, float lineSize, bool centered, int flipOption , Color color)
        {
            rows *= 2;
            columns *= 2;
            Vector3 centerOffset = Vector3.Zero;
            if (centered)
                centerOffset = new Vector3(-.5f, -.5f, 0f);
            width = rows;
            height = columns;
            int len = width * 4 + height * 4;
            float xratio = 1f / width;
            float yratio = 1f / height;
            vertices = new VertexPositionColorTexture[len];
            indices = new int[(width * 6 + height * 6) * 2];
            int vIndex = 0;
            int iIndex = 0;

            for (int x = 0; x < width; x++)
            {
                int svIndex = vIndex;
                Vector3 xpos = new Vector3(xratio * x, 0f, 0f);
                vertices[vIndex] = new VertexPositionColorTexture(
                    new Vector3(0f, 0f, 0f) + xpos + centerOffset,
                    color,
                    new Vector2(0f, 0f)
                    );
                vIndex++;
                vertices[vIndex] = new VertexPositionColorTexture(
                    new Vector3(0f, 1f, 0f) + xpos + centerOffset,
                    color,
                    new Vector2(0f, 1f)
                    );
                vIndex++;
                vertices[vIndex] = new VertexPositionColorTexture(
                    new Vector3(lineSize, 0f, 0f) + xpos + centerOffset,
                    color,
                    new Vector2(1f, 0f)
                    );
                vIndex++;
                vertices[vIndex] = new VertexPositionColorTexture(
                    new Vector3(lineSize, 1f, 0f) + xpos + centerOffset,
                    color,
                    new Vector2(1f, 1f)
                    );
                vIndex++;
                // triangle 1
                indices[iIndex + 0] = svIndex + 0; indices[iIndex + 1] = svIndex + 1; indices[iIndex + 2] = svIndex + 2;
                // triangle 2
                indices[iIndex + 3] = svIndex + 2; indices[iIndex + 4] = svIndex + 1; indices[iIndex + 5] = svIndex + 3;
                // triangle 3 backface
                indices[iIndex + 0] = svIndex + 2; indices[iIndex + 1] = svIndex + 1; indices[iIndex + 2] = svIndex + 0;
                // triangle 4 backface
                indices[iIndex + 3] = svIndex + 3; indices[iIndex + 4] = svIndex + 2; indices[iIndex + 5] = svIndex + 1;
                iIndex += 6 * 2;
            }

            for (int y = 0; y < height; y++)
            {
                int svIndex = vIndex;
                Vector3 ypos = new Vector3(0f, yratio * y, 0f);
                vertices[vIndex] = new VertexPositionColorTexture(new Vector3(0f, 0f, 0f) + ypos + centerOffset, color, new Vector2(0f, 0f)); vIndex++;
                vertices[vIndex] = new VertexPositionColorTexture(new Vector3(0f, lineSize, 0f) + ypos + centerOffset, color, new Vector2(0f, 1f)); vIndex++;
                vertices[vIndex] = new VertexPositionColorTexture(new Vector3(1f, 0f, 0f) + ypos + centerOffset, color, new Vector2(1f, 0f)); vIndex++;
                vertices[vIndex] = new VertexPositionColorTexture(new Vector3(1f, lineSize, 0f) + ypos + centerOffset, color, new Vector2(1f, 1f)); vIndex++;
                // triangle 1
                indices[iIndex + 0] = svIndex + 0; indices[iIndex + 1] = svIndex + 1; indices[iIndex + 2] = svIndex + 2;
                // triangle 2
                indices[iIndex + 3] = svIndex + 2; indices[iIndex + 4] = svIndex + 1; indices[iIndex + 5] = svIndex + 3;
                // triangle 3 backface
                indices[iIndex + 0] = svIndex + 2; indices[iIndex + 1] = svIndex + 1; indices[iIndex + 2] = svIndex + 0;
                // triangle 4 backface
                indices[iIndex + 3] = svIndex + 3; indices[iIndex + 4] = svIndex + 2; indices[iIndex + 5] = svIndex + 1;
                iIndex += 6 * 2;
            }
            Flip(flipOption);
        }

        void Flip(int flipOption)
        {
            if (flipOption == 1)
            {
                int index = 0;
                for (int x = 0; x < width; x++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var p = vertices[index].Position;
                        vertices[index].Position = new Vector3(0f, p.X, p.Y);
                        index++;
                    }
                }
                for (int y = 0; y < height; y++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var p = vertices[index].Position;
                        vertices[index].Position = new Vector3(0f, p.X, p.Y);
                        index++;
                    }
                }
            }
            if (flipOption == 2)
            {
                int index = 0;
                for (int x = 0; x < width; x++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var p = vertices[index].Position;
                        vertices[index].Position = new Vector3(p.Y, 0f, p.X);
                        index++;
                    }
                }
                for (int y = 0; y < height; y++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var p = vertices[index].Position;
                        vertices[index].Position = new Vector3(p.Y, 0f, p.X);
                        index++;
                    }
                }
            }
        }

        public void Draw(GraphicsDevice gd, BasicEffect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionColorTexture.VertexDeclaration);
            }
        }

        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionColorTexture.VertexDeclaration);
            }
        }
    }

    public class OrientationLines
    {
        VertexPositionColor[] vertices;
        int[] indices;

        public OrientationLines()
        {
            CreateOrientationLines(.1f, 1.0f);
        }
        public OrientationLines(float linewidth, float lineDistance)
        {
            CreateOrientationLines(linewidth, lineDistance);
        }

        public void CreateOrientationLines(float linewidth, float lineDistance)
        {
            var center = new Vector3(0, 0, 0);
            var scaledup = Vector3.Up * linewidth;
            var scaledforward = Vector3.Forward * linewidth;
            var forward = Vector3.Forward * lineDistance;
            var right = Vector3.Right * lineDistance;
            var up = Vector3.Up * lineDistance;

            var r = new Color(1.0f, 0.0f, 0.0f, .8f);
            var g = new Color(0.0f, 1.0f, 0.0f, .8f);
            var b = new Color(0.0f, 0.0f, 1.0f, .8f);

            vertices = new VertexPositionColor[9];
            indices = new int[18];

            // forward
            vertices[0].Position = forward; vertices[0].Color = g;
            vertices[1].Position = scaledup; vertices[1].Color = g;
            vertices[2].Position = center; vertices[2].Color = g;

            indices[0] = 0; indices[1] = 1; indices[2] = 2;
            indices[3] = 0; indices[4] = 2; indices[5] = 1;

            // right
            vertices[4].Position = right; vertices[3].Color = b;
            vertices[5].Position = scaledup; vertices[4].Color = b;
            vertices[6].Position = center; vertices[5].Color = b;

            indices[6] = 3; indices[7] = 4; indices[8] = 5;
            indices[9] = 3; indices[10] = 5; indices[11] = 4;

            // up square
            vertices[8].Position = up; vertices[6].Color = r;
            vertices[9].Position = center; vertices[7].Color = r;
            vertices[10].Position = scaledforward; vertices[8].Color = r;

            indices[12] = 6; indices[13] = 7; indices[14] = 8;
            indices[15] = 6; indices[16] = 8; indices[17] = 7;
        }
        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionColor.VertexDeclaration);
            }
        }
    }

    public class NavOrientation3d
    {
        CircleNav3d navCircle3dA;
        CircleNav3d navCircle3dB;
        CircleNav3d navCircle3dC;

        public NavOrientation3d()
        {
            navCircle3dA = new CircleNav3d(30, .05f, 24, 6, 0);
            navCircle3dB = new CircleNav3d(30, .05f, 24, 24, 1);
            navCircle3dC = new CircleNav3d(30, .05f, 24, 24, 2);
        }
        public NavOrientation3d(int segments, int navSegments, int largeSegmentModulator, float lineThickness0to1)
        {
            navCircle3dA = new CircleNav3d(segments, lineThickness0to1, navSegments, largeSegmentModulator, 0);
            navCircle3dB = new CircleNav3d(segments, lineThickness0to1, navSegments, largeSegmentModulator, 1);
            navCircle3dC = new CircleNav3d(segments, lineThickness0to1, navSegments, largeSegmentModulator, 2);
        }
        public NavOrientation3d(int segments, int navSegments, int largeSegmentModulator, float lineThickness0to1, float navSize0to1)
        {
            navCircle3dA = new CircleNav3d(segments, navSegments, largeSegmentModulator, lineThickness0to1, navSize0to1, true, 0);
            navCircle3dB = new CircleNav3d(segments, navSegments, largeSegmentModulator, lineThickness0to1, navSize0to1, true, 1);
            navCircle3dC = new CircleNav3d(segments, navSegments, largeSegmentModulator, lineThickness0to1, navSize0to1, true, 2);
        }

        public void DrawNavOrientation3DWithBasicEffect(GraphicsDevice gd, BasicEffect beffect, Matrix world, Texture2D ta, Texture2D tb, Texture2D tc)
        {
            bool isLighting = beffect.LightingEnabled;
            beffect.LightingEnabled = false;
            gd.RasterizerState = new RasterizerState() { FillMode = FillMode.Solid, CullMode = CullMode.None };
            beffect.World = world;
            beffect.Texture = ta;
            navCircle3dA.Draw(gd, beffect);
            beffect.Texture = tb;
            navCircle3dB.Draw(gd, beffect);
            beffect.Texture = tc;
            navCircle3dC.Draw(gd, beffect);
            beffect.LightingEnabled = isLighting;
        }
        public void DrawNavOrientation3DWithBasicEffect(GraphicsDevice gd, BasicEffect beffect, Vector3 position, float scale, Texture2D ta, Texture2D tb, Texture2D tc)
        {
            Matrix world = Matrix.CreateScale(scale) * Matrix.Identity;
            world.Translation = position;
            bool isLighting = beffect.LightingEnabled;
            beffect.LightingEnabled = false;
            gd.RasterizerState = new RasterizerState() { FillMode = FillMode.Solid, CullMode = CullMode.None };
            beffect.World = world;
            beffect.Texture = ta;
            navCircle3dA.Draw(gd, beffect);
            beffect.Texture = tb;
            navCircle3dB.Draw(gd, beffect);
            beffect.Texture = tc;
            navCircle3dC.Draw(gd, beffect);
            beffect.LightingEnabled = isLighting;
        }

        public void DrawNavOrientation3DToTargetWithBasicEffect(GraphicsDevice gd, BasicEffect beffect, Vector3 position, float scale, Matrix lookAtTargetsMatrix, Texture2D ta, Texture2D tb, Texture2D tc)
        {
            var totarget = lookAtTargetsMatrix.Translation - position;
            Matrix target = Matrix.CreateScale(scale * .6f) * Matrix.CreateWorld(position, -totarget, lookAtTargetsMatrix.Up);
            Matrix world = Matrix.CreateScale(scale) * Matrix.Identity;
            world.Translation = position;
            bool isLighting = beffect.LightingEnabled;
            beffect.LightingEnabled = false;
            gd.RasterizerState = new RasterizerState() { FillMode = FillMode.Solid, CullMode = CullMode.None };
            beffect.World = world;
            beffect.Texture = ta;
            navCircle3dA.Draw(gd, beffect);
            beffect.World = target;
            beffect.Texture = tb;
            navCircle3dB.Draw(gd, beffect);
            beffect.World = world;
            beffect.Texture = tc;
            navCircle3dC.Draw(gd, beffect);
            beffect.LightingEnabled = isLighting;
        }
    }

    public class CircleNav3d
    {
        bool centered = true;
        float navSmallLargeRatio = .40f;
        float zerodegreelinethickener = 4.0f;

        public VertexPositionTexture[] vertices;
        public int[] indices;
        static int OrientationOptionRightUpForward
        {
            get;
            set;
        }

        public CircleNav3d(int segments)
        {
            CreateCircleNav3d(segments, 4, 2, .01f, .35f, true, 2);
        }
        /// <summary>
        /// Create a circle default orientation is 2 forward
        /// </summary>
        public CircleNav3d(int segments, float lineSize)
        {
            CreateCircleNav3d(segments, 4, 2, lineSize, .35f, true, 2);
        }
        public CircleNav3d(int segments, float lineSize, int navsegments, int largeSegmentModulator)
        {
            CreateCircleNav3d(segments, navsegments, largeSegmentModulator, lineSize, .35f, true, 2);
        }
        public CircleNav3d(int segments, float lineSize, int navsegments, int largeSegmentModulator, int orientation012)
        {
            CreateCircleNav3d(segments, navsegments, largeSegmentModulator, lineSize, .35f, true, orientation012);
        }
        /// <summary>
        /// Create a circle default orientation is 2 forward
        /// </summary>
        public CircleNav3d(int segments, int navsegments, int largeSegmentModulator, float lineSize0to1, float navSize0to1, bool centerIt, int orientation012)
        {
            CreateCircleNav3d(segments, navsegments, largeSegmentModulator, lineSize0to1, navSize0to1, centerIt, orientation012);
        }

        /// <summary>
        /// Create a circle default orientation is 2 forward
        /// </summary>
        public void CreateCircleNav3d(int segments, int navSegments, int largeSegmentModulator, float lineSize0to1, float navSize0to1, bool centerIt, int orientation012)
        {
            OrientationOptionRightUpForward = orientation012;
            int circlesegmentVertexs = segments * 2;
            int circlesegmentIndices = segments * 6;
            int navsegmentVertexs = navSegments * 4;
            int navsegmentIndices = navSegments * 6;
            vertices = new VertexPositionTexture[circlesegmentVertexs + navsegmentVertexs];
            indices = new int[circlesegmentIndices + navsegmentIndices];

            centered = centerIt;
            float centering = .5f;
            if (centered)
                centering = 0.0f;
            float offset = 1f - lineSize0to1;
            float pi2 = (float)(Math.PI * 2d);
            float steppercentage = 1f / (float)(segments);
            float u = 0f, radians = 0f, x = 0f, y = 0f;
            int index = 0, v_index = 0, i_index = 0;
            for (index = 0; index < segments; index++)
            {
                u = (float)(index) * steppercentage;
                radians = u * pi2;
                x = ((float)(Math.Sin(radians)) * .5f) + centering;
                y = ((float)(Math.Cos(radians)) * .5f) + centering;
                vertices[v_index + 0] = new VertexPositionTexture(ReOrient(new Vector3(x, y, 0)), new Vector2(u, 0f));
                vertices[v_index + 1] = new VertexPositionTexture(ReOrient(new Vector3(x * offset, y * offset, 0)), new Vector2(u, 1f));
                if (index < segments - 1)
                {
                    indices[i_index + 0] = v_index + 0; indices[i_index + 1] = v_index + 1; indices[i_index + 2] = v_index + 2;
                    indices[i_index + 3] = v_index + 2; indices[i_index + 4] = v_index + 1; indices[i_index + 5] = v_index + 3;
                }
                else
                {
                    // connect the last one directly to the front
                    indices[i_index + 0] = v_index + 0; indices[i_index + 1] = v_index + 1; indices[i_index + 2] = 0;
                    indices[i_index + 3] = 0; indices[i_index + 4] = v_index + 1; indices[i_index + 5] = 1;
                }
                v_index += 2;
                i_index += 6;
            }
            //CreateNavLines
            var offsetOuter = offset;
            var offsetInner = 1f - (navSize0to1);
            steppercentage = 1f / (float)(navSegments);
            for (index = 0; index < navSegments; index++)
            {
                if (index % largeSegmentModulator == 0)
                    offsetInner = 1f - (navSize0to1);
                else
                    offsetInner = 1f - (navSize0to1 * navSmallLargeRatio);
                // first set of vertices
                u = (float)(index) * steppercentage;
                radians = u * pi2;
                x = ((float)(Math.Sin(radians)) * .5f) + centering;
                y = ((float)(Math.Cos(radians)) * .5f) + centering;
                vertices[v_index + 0] = new VertexPositionTexture(ReOrient(new Vector3(x * offsetOuter, y * offsetOuter, 0)), new Vector2(u, 0f));
                vertices[v_index + 1] = new VertexPositionTexture(ReOrient(new Vector3(x * offsetInner, y * offsetInner, 0)), new Vector2(u, 1f));
                // second set of vertices
                u = (float)(index + lineSize0to1) * steppercentage;
                // just make the 0 line slightly larger as its special.
                if (index == 0)
                    u = (float)(index + lineSize0to1 * zerodegreelinethickener) * steppercentage;
                radians = u * pi2;
                x = ((float)(Math.Sin(radians)) * .5f) + centering;
                y = ((float)(Math.Cos(radians)) * .5f) + centering;
                vertices[v_index + 2] = new VertexPositionTexture(ReOrient(new Vector3(x * offsetOuter, y * offsetOuter, 0)), new Vector2(u, 0f));
                vertices[v_index + 3] = new VertexPositionTexture(ReOrient(new Vector3(x * offsetInner, y * offsetInner, 0)), new Vector2(u, 1f));
                // indices
                indices[i_index + 0] = v_index + 0; indices[i_index + 1] = v_index + 1; indices[i_index + 2] = v_index + 2;
                indices[i_index + 3] = v_index + 2; indices[i_index + 4] = v_index + 1; indices[i_index + 5] = v_index + 3;
                v_index += 4;
                i_index += 6;
            }

        }

        Vector3 ReOrient(Vector3 v)
        {
            if (OrientationOptionRightUpForward == 1)
                v = new Vector3(v.Z, v.X, v.Y);
            if (OrientationOptionRightUpForward == 2)
                v = new Vector3(v.X, v.Z, v.Y);
            return v;
        }

        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionTexture.VertexDeclaration);
            }
        }
    }

    public class Line
    {
        VertexPositionColor[] vertices;
        int[] indices;

        public Vector3 camUp = Vector3.Up;

        public Line(float linewidth, Color c, Vector3 start, Vector3 end)
        {
            CreateLine(linewidth, c, start, end);
        }

        private void CreateLine(float linewidth, Color c, Vector3 start, Vector3 end)
        {
            var a = end - start;
            a.Normalize();
            var b = Vector3.Up;
            float n = Vector3.Dot(a, b);
            if (n * n > .95f)
                b = Vector3.Right;
            var su = Vector3.Cross(a, b);
            var sr = Vector3.Cross(a, su);
            var offsetup = su * linewidth;
            var offsetright = sr * linewidth;

            Vector3 s0 = start + offsetright - offsetup;
            Vector3 s1 = start - offsetright - offsetup;
            Vector3 s2 = start + offsetup;

            Vector3 e0 = end + offsetright - offsetup;
            Vector3 e1 = end - offsetright - offsetup;
            Vector3 e2 = end + offsetup;

            var cs = c * .4f;
            cs.A = c.A;

            vertices = new VertexPositionColor[12];
            indices = new int[18];

            int v = 0;
            int i = 0;
            // q1
            vertices[v].Position = s0; vertices[v].Color = cs; v++;
            vertices[v].Position = s1; vertices[v].Color = cs; v++;
            vertices[v].Position = e0; vertices[v].Color = c; v++;
            vertices[v].Position = e1; vertices[v].Color = c; v++;

            var vi = v - 4;
            indices[i + 0] = vi + 0; indices[i + 1] = vi + 1; indices[i + 2] = vi + 2;
            indices[i + 3] = vi + 2; indices[i + 4] = vi + 1; indices[i + 5] = vi + 3;
            i += 6;

            // q2
            vertices[v].Position = s1; vertices[v].Color = cs; v++;
            vertices[v].Position = s2; vertices[v].Color = cs; v++;
            vertices[v].Position = e1; vertices[v].Color = c; v++;
            vertices[v].Position = e2; vertices[v].Color = c; v++;

            vi = v - 4;
            indices[i + 0] = vi + 0; indices[i + 1] = vi + 1; indices[i + 2] = vi + 2;
            indices[i + 3] = vi + 2; indices[i + 4] = vi + 1; indices[i + 5] = vi + 3;
            i += 6;

            //q3
            vertices[v].Position = s2; vertices[v].Color = cs; v++;
            vertices[v].Position = s0; vertices[v].Color = cs; v++;
            vertices[v].Position = e2; vertices[v].Color = c; v++;
            vertices[v].Position = e0; vertices[v].Color = c; v++;

            vi = v - 4;
            indices[i + 0] = vi + 0; indices[i + 1] = vi + 1; indices[i + 2] = vi + 2;
            indices[i + 3] = vi + 2; indices[i + 4] = vi + 1; indices[i + 5] = vi + 3;
            i += 6;
        }
        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionColor.VertexDeclaration);
            }
        }
    }

    public class LinePCT
    {
        VertexPositionColorTexture[] vertices;
        int[] indices;

        public Vector3 camUp = Vector3.Up;

        public LinePCT(float linewidth, Color c, Vector3 start, Vector3 end)
        {
            CreateLine(linewidth, c, c, start, end);
        }
        public LinePCT(float linewidth, Color colorStart, Color colorEnd, Vector3 start, Vector3 end)
        {
            CreateLine(linewidth, colorStart, colorEnd, start, end);
        }

        private void CreateLine(float linewidth, Color cs, Color ce, Vector3 start, Vector3 end)
        {
            var a = end - start;
            a.Normalize();
            var b = Vector3.Up;
            float n = Vector3.Dot(a, b);
            if (n * n > .95f)
                b = Vector3.Right;
            var su = Vector3.Cross(a, b);
            var sr = Vector3.Cross(a, su);
            var offsetup = su * linewidth;
            var offsetright = sr * linewidth;

            Vector3 s0 = start + offsetright - offsetup;
            Vector3 s1 = start - offsetright - offsetup;
            Vector3 s2 = start + offsetup;

            Vector3 e0 = end + offsetright - offsetup;
            Vector3 e1 = end - offsetright - offsetup;
            Vector3 e2 = end + offsetup;

            Vector2 uv0 = new Vector2(0f, 1f);
            Vector2 uv1 = new Vector2(0f, 0f);
            Vector2 uv2 = new Vector2(1f, 0f);
            Vector2 uv3 = new Vector2(1f, 1f);

            vertices = new VertexPositionColorTexture[12];
            indices = new int[18];

            int v = 0;
            int i = 0;
            // q1
            vertices[v].Position = s0; vertices[v].Color = cs; vertices[v].TextureCoordinate = uv0; v++;
            vertices[v].Position = s1; vertices[v].Color = cs; vertices[v].TextureCoordinate = uv1; v++;
            vertices[v].Position = e0; vertices[v].Color = ce; vertices[v].TextureCoordinate = uv2; v++;
            vertices[v].Position = e1; vertices[v].Color = ce; vertices[v].TextureCoordinate = uv3; v++;

            var vi = v - 4;
            indices[i + 0] = vi + 0; indices[i + 1] = vi + 1; indices[i + 2] = vi + 2;
            indices[i + 3] = vi + 2; indices[i + 4] = vi + 1; indices[i + 5] = vi + 3;
            i += 6;

            // q2
            vertices[v].Position = s1; vertices[v].Color = cs; vertices[v].TextureCoordinate = uv0; v++;
            vertices[v].Position = s2; vertices[v].Color = cs; vertices[v].TextureCoordinate = uv1; v++;
            vertices[v].Position = e1; vertices[v].Color = ce; vertices[v].TextureCoordinate = uv2; v++;
            vertices[v].Position = e2; vertices[v].Color = ce; vertices[v].TextureCoordinate = uv3; v++;

            vi = v - 4;
            indices[i + 0] = vi + 0; indices[i + 1] = vi + 1; indices[i + 2] = vi + 2;
            indices[i + 3] = vi + 2; indices[i + 4] = vi + 1; indices[i + 5] = vi + 3;
            i += 6;

            //q3
            vertices[v].Position = s2; vertices[v].Color = cs; vertices[v].TextureCoordinate = uv0; v++;
            vertices[v].Position = s0; vertices[v].Color = cs; vertices[v].TextureCoordinate = uv1; v++;
            vertices[v].Position = e2; vertices[v].Color = ce; vertices[v].TextureCoordinate = uv2; v++;
            vertices[v].Position = e0; vertices[v].Color = ce; vertices[v].TextureCoordinate = uv3; v++;

            vi = v - 4;
            indices[i + 0] = vi + 0; indices[i + 1] = vi + 1; indices[i + 2] = vi + 2;
            indices[i + 3] = vi + 2; indices[i + 4] = vi + 1; indices[i + 5] = vi + 3;
            i += 6;
        }
        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionColorTexture.VertexDeclaration);
            }
        }
    }

    public class Circle3d
    {
        bool centered = true;
        public VertexPositionTexture[] vertices;
        public int[] indices;

        static int OrientationOptionRightUpForward
        {
            get;
            set;
        }

        public Circle3d(int segments)
        {
            CreateCircle(segments, .01f, true, 2);
        }
        /// <summary>
        /// Create a circle default orientation is 2 forward
        /// </summary>
        public Circle3d(int segments, float lineSize)
        {
            CreateCircle(segments, lineSize, true, 2);
        }
        /// <summary>
        /// Create a circle default orientation is 2 forward
        /// </summary>
        public Circle3d(int segments, float lineSize0to1, bool centerIt, int orientation012)
        {
            CreateCircle(segments, lineSize0to1, centerIt, orientation012);
        }
        /// <summary>
        /// Create a circle default orientation is 2 forward
        /// </summary>
        public void CreateCircle(int segments, float lineSize0to1, bool centerIt, int orientation012)
        {
            centered = centerIt;
            float centering = .5f;
            if (centered)
                centering = 0.0f;
            float offset = 1f - lineSize0to1;
            vertices = new VertexPositionTexture[segments * 2];
            indices = new int[segments * 6];
            float pi2 = (float)(Math.PI * 2d);
            float mult = 1f / (float)(segments);
            int index = 0;
            int v_index = 0;
            int i_index = 0;
            for (index = 0; index < segments; index++)
            {
                var u = (float)(index) * mult;
                double radians = u * pi2;
                float x = ((float)(Math.Sin(radians)) * .5f) + centering;
                float y = ((float)(Math.Cos(radians)) * .5f) + centering;
                vertices[v_index + 0] = new VertexPositionTexture(ReOrient(new Vector3(x, y, 0)), new Vector2(u, 0f));
                vertices[v_index + 1] = new VertexPositionTexture(ReOrient(new Vector3(x * offset, y * offset, 0)), new Vector2(u, 1f));
                if (index < segments - 1)
                {
                    indices[i_index + 0] = v_index + 0; indices[i_index + 1] = v_index + 1; indices[i_index + 2] = v_index + 2;
                    indices[i_index + 3] = v_index + 2; indices[i_index + 4] = v_index + 1; indices[i_index + 5] = v_index + 3;
                }
                else
                {
                    // connect the last one directly to the front
                    indices[i_index + 0] = v_index + 0; indices[i_index + 1] = v_index + 1; indices[i_index + 2] = 0;
                    indices[i_index + 3] = 0; indices[i_index + 4] = v_index + 1; indices[i_index + 5] = 1;
                }
                v_index += 2;
                i_index += 6;
            }
        }
        Vector3 ReOrient(Vector3 v)
        {
            if (OrientationOptionRightUpForward == 1)
                v = new Vector3(v.Z, v.X, v.Y);
            if (OrientationOptionRightUpForward == 2)
                v = new Vector3(v.X, v.Z, v.Y);
            return v;
        }
        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionTexture.VertexDeclaration);
            }
        }
    }

    public class Prism
    {
        #region Variables
        /// <summary>
        /// Number of prism faces
        /// </summary>
        private int prismSides;

        /// <summary>
        /// Height of the prism
        /// </summary>
        private float prismHeight;

        /// <summary>
        /// Diameter of the prism
        /// </summary>
        private float prismRadius;


        private bool invertNormals = false;

        ///// <summary>
        ///// Placeholder for the texture on the sides
        ///// </summary>
        //private Texture2D prismSideTexture;

        ///// <summary>
        ///// prism BasicEffect
        ///// </summary>
        //public BasicEffect effect;

        ///// <summary>
        ///// The World Matrix somewhat redundant being here.
        ///// Now if anything we should provide accessors to set the effect view and projection.
        ///// </summary>
        //public Matrix worldMatrix = Matrix.Identity;
        #endregion

        // Requisite for draw user indexed primitives. 
        private VertexPositionNormalTexture[] vertices;
        private int[] indices;

        // Requisite for draw primitives.
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        /// <summary>
        /// Build the prism
        /// </summary>
        public void CreatePrism(GraphicsDevice gd, int sides, float height, float radius, bool invertTheNormals)
        {
            invertNormals = invertTheNormals;
            prismSides = sides;
            prismHeight = height;
            prismRadius = radius;
            if (sides < 3)
                prismSides = 3;
            // you might want decimals and you can probably do this with a scaling matrix in your own vertex shader.
            if (height < 1f)
                prismHeight = 1f;
            if (radius < 1f)
                prismRadius = 1f;
            vertices = GetPrismVertices(radius, height, sides);

            //vertexBuffer = new VertexBuffer(gd, VertexPositionNormalTexture.VertexDeclaration, nverts.Length, BufferUsage.None);
            //vertexBuffer.SetData(nverts);
            //gd.SetVertexBuffer(vertexBuffer);

            // set up the index buffer
            indices = new int[sides * 3 * 2];

            int offset = 0;
            // first set
            for (int i = 2; i < vertices.Length; i++)
            {

                int i0 = offset + 0;
                int i1 = offset + 1;
                int i2 = offset + 2;
                offset += 3;

                int v0 = (int)(0); // vertice [0] holds the up prism point.
                int v1 = (int)(i); // each side has 2 points other then top or bottom.
                int v2 = (int)(i + 1); // we know all our side points are from 2 to the end.
                                           //
                                           // now towards the end of this loop.
                                           // well wrap that second side vertice around back to vertice [2]
                                           //
                if (v2 >= vertices.Length)
                {
                    v2 = 2;
                }
                // we can control our initial culling order.
                // i.e. the way vertices use backface or frontface culling right here.
                // So here ill set it to use counter clockwise winding (ccw)
                indices[i0] = v0;
                indices[i1] = v1;
                indices[i2] = v2;
            }
            // second set
            for (int i = 2; i < vertices.Length; i++)
            {
                int i0 = offset + 0;
                int i1 = offset + 1;
                int i2 = offset + 2;
                offset += 3;

                int v0 = (int)(1); // vertice [1] holds the down prism point
                int v1 = (int)(i);
                int v2 = (int)(i + 1);
                if (v2 >= vertices.Length)
                {
                    v2 = 2;
                }
                // reverse the input ordering to keep the winding counter clockwise
                indices[i0] = v1;
                indices[i1] = v0;
                indices[i2] = v2;
            }

            vertices = CreateSmoothNormals(vertices, indices, invertNormals);

            vertexBuffer = new VertexBuffer(gd, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.None);
            vertexBuffer.SetData(vertices);
            gd.SetVertexBuffer(vertexBuffer);

            indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, offset + 1, BufferUsage.None);
            indexBuffer.SetData(indices);
            gd.Indices = indexBuffer;
        }
        /// <summary>
        /// Returns all the vertices the first two indices are the top then bottom points.
        /// Followed by all the other vertices points.
        /// </summary>
        private VertexPositionNormalTexture[] GetPrismVertices(float radius, float height, float nPositions)
        {
            VertexPositionNormalTexture[] result = new VertexPositionNormalTexture[(int)(nPositions) + 2];

            float degrees = 0;
            float radians = 0f;
            float x;
            float z;
            float textureU = .5f;
            float textureV = .5f;
            var n = Vector3.Zero;
            result[0] = new VertexPositionNormalTexture(Vector3.Up * height, n ,new Vector2(textureU, textureV));
            result[1] = new VertexPositionNormalTexture(Vector3.Down * height,n ,  new Vector2(textureU, textureV));
            for (int i = 0; i < nPositions; i++)
            {
                radians = ((i / (float)nPositions) * 6.28318530717f);
                float sin = (float)(Math.Sin(radians));
                float cos = (float)(Math.Cos(radians));
                x = radius * sin;
                z = radius * cos;
                
                var ss = Sign(sin);
                var sc = Sign(cos);
                 
                Vector2 uv = GetOuterSquareVector(sin, cos) * .5f + new Vector2(.5f, .5f);

                result[i + 2] = new VertexPositionNormalTexture(new Vector3(x, 0f, z), n, uv);
            }
            return result;
        }

        VertexPositionNormalTexture[] CreateSmoothNormals(VertexPositionNormalTexture[] vertices, int[] indexs, bool invertNormalsOnCreation)
        {
            // For each vertice we must calculate the surrounding triangles normals, average them and set the normal.
            int tvertmultiplier = 3;
            int triangles = (int)(indexs.Length / tvertmultiplier);
            for (int currentTestedVerticeIndex = 0; currentTestedVerticeIndex < vertices.Length; currentTestedVerticeIndex++)
            {
                Vector3 sum = Vector3.Zero;
                float total = 0;
                for (int t = 0; t < triangles; t++)
                {
                    int tvstart = t * tvertmultiplier;
                    int tindex0 = tvstart + 0;
                    int tindex1 = tvstart + 1;
                    int tindex2 = tvstart + 2;
                    var vindex0 = indexs[tindex0];
                    var vindex1 = indexs[tindex1];
                    var vindex2 = indexs[tindex2];
                    if (vindex0 == currentTestedVerticeIndex || vindex1 == currentTestedVerticeIndex || vindex2 == currentTestedVerticeIndex)
                    {
                        var n0 = (vertices[vindex1].Position - vertices[vindex0].Position) * 10f; // * 10 math artifact avoidance.
                        var n1 = (vertices[vindex2].Position - vertices[vindex1].Position) * 10f;
                        var cnorm = Vector3.Cross(n0, n1);
                        sum += cnorm;
                        total += 1;
                    }
                }
                if (total > 0)
                {
                    var averagednormal = sum / total;
                    averagednormal.Normalize();
                    if (invertNormalsOnCreation)
                        averagednormal = -averagednormal;
                    vertices[currentTestedVerticeIndex].Normal = averagednormal;
                }
            }
            return vertices;
        }

        public Vector2 GetOuterSquareVector(float sin, float cos)
        {
            var ss = (sin < 0) ? -1f : 1f;
            var sc = (cos < 0) ? -1f : 1f;
            var asin = sin * sin;
            var acos = cos * cos;
            if (asin > acos) //  x is higher
                return new Vector2(ss, acos * sc * 2f); // re-signed acosine
            else // x is lower
                return new Vector2(asin * ss * 2f, sc); // re-signed asin
        }

        public float Sign(float n)
        {
            if (n < 0)
               return -1f;
            else
                return 1f;
        }
        public float Abs(float n)
        {
            if (n < 0)
                n = -n;
            return n;
        }

        public BasicEffect BasicEffectSettingsForThisPrimitive(GraphicsDevice gd, BasicEffect effect, Texture2D texture)
        {
            effect.VertexColorEnabled = false;
            effect.TextureEnabled = true;
            effect.EnableDefaultLighting();
            effect.LightingEnabled = true;
            effect.PreferPerPixelLighting = true;
            effect.Texture = texture;
            return effect;
        }

        public void DrawWithBasicEffect(GraphicsDevice device, BasicEffect effect, Texture2D texture)
        {
            //float aspectRatio = (float)device.Viewport.Width / device.Viewport.Height;
            //effect
            //effect.LightingEnabled = false;
            //effect.VertexColorEnabled = false;
            //effect.TextureEnabled = true;
            //effect.Texture = texture;

            effect.CurrentTechnique.Passes[0].Apply();
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3, VertexPositionNormalTexture.VertexDeclaration);
        }

        //public void Draw(GraphicsDevice device, Matrix world, Matrix view, Matrix projection, bool useingUserIndexedPrims)
        //{

        //    int triangleCount = nIndexs.Length / 3;

        //    //World Matrix
        //    effect.World = world;
        //    effect.View = view;
        //    effect.Projection = projection;

        //    effect.CurrentTechnique.Passes[0].Apply();

        //    if (useingUserIndexedPrims)
        //    {
        //        // With DrawUserIndexedPrimitives we can work with the arrays themselves by passing them each frame.
        //        device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, nverts, 0, nverts.Length, nIndexs, 0, triangleCount, VertexPositionTexture.VertexDeclaration);
        //    }
        //    else
        //    {
        //        // set buffers on device
        //        device.Indices = indexBuffer;
        //        device.SetVertexBuffer(vertexBuffer);

        //        // this way actually uses these buffers that we already set onto the device.
        //        device.DrawPrimitives(PrimitiveType.TriangleList, 0, triangleCount);
        //    }
        //}

    }


    /// <summary>
    /// This is what id like to be using in general for our primitives.
    /// </summary>
    public struct VertexPositionTextureNormalTangent : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector3 Normal;
        public Vector3 Tangent;

        public VertexPositionTextureNormalTangent(Vector3 position, Vector2 uvcoordinates, Vector3 normal, Vector3 tangent)
        {
            Position = position;
            TextureCoordinate = uvcoordinates;
            Normal = normal;
            Tangent = tangent;
        }

        public static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
              new VertexElement(VertexElementByteOffset.PositionStartOffset(), VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector2(), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 1)
        );
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }

    /// <summary>
    /// basically a semi wide spectrum vertice structure minus weights.
    /// It's faily dumb to pass color into the data structure when you can just pass a single shading color if you wanted or litterally texture everything.
    /// However this does let us use spritebatch shaders that want color in them better.
    /// </summary>
    public struct VertexPositionColorTextureNormalTangent : IVertexType
    {
        public Vector3 Position;
        public Vector4 Color;
        public Vector2 TextureCoordinate;
        public Vector3 Normal;
        public Vector3 Tangent;

        public static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
              new VertexElement(VertexElementByteOffset.PositionStartOffset(), VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector4(), VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector2(), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 1)
        );
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }

    /// <summary>
    /// basically a wide spectrum vertice structure.
    /// </summary>
    public struct VertexPositionColorTextureNormalTangentBiTangentWeights : IVertexType
    {
        public Vector3 Position;
        public Vector4 Color;
        public Vector2 TextureCoordinate;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 BiTangent;
        public Vector4 BlendIndices;
        public Vector4 BlendWeights;

        public static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
              new VertexElement(VertexElementByteOffset.PositionStartOffset(), VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
              //new VertexElement(VertexElementByteOffset.OffsetColor(), VertexElementFormat.Color, VertexElementUsage.Color, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector4(), VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector2(), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 1),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 2),
              new VertexElement(VertexElementByteOffset.OffsetVector4(), VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector4(), VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0)
        );
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }

    /// <summary>
    /// This is a helper struct for tallying byte offsets
    /// </summary>
    public struct VertexElementByteOffset
    {
        public static int currentByteSize = 0;
        //[STAThread]
        public static int PositionStartOffset() { currentByteSize = 0; var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
        public static int Offset(int n) { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        public static int Offset(float n) { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Vector2 n) { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Color n) { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Vector3 n) { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Vector4 n) { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }

        public static int OffsetInt() { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        public static int OffsetFloat() { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
        public static int OffsetColor() { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        public static int OffsetVector2() { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
        public static int OffsetVector3() { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
        public static int OffsetVector4() { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }
    }
}
