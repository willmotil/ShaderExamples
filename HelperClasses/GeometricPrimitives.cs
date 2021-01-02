using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShaderExamples
{

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

        private void CreateOrientationLines(float linewidth, float lineDistance)
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

}
