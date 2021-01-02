﻿

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;


//   Unfinished prototype not yet vetted / working.

namespace ShaderExamples
{
    public class Game1_Mesh : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D texture;
        BasicEffect _basicEffect;
        Effect meshEffect;

        private DemoCamera _cameraCinematic;
        private bool _useDemoWaypoints = false;
        private bool _useDesignatedTarget = false;
        private Vector3 _targetLookAt = new Vector3(0, 0, 0);

        private static float Range = 28.0f;
        private static float weight = 1f;
        private Vector4[] _cameraWayPoints = new Vector4[]
        {
            new Vector4(-Range, 0, 0, weight), new Vector4(0, 0, -Range, weight), new Vector4(Range, 0, 0, weight), new Vector4(0, 0, Range , weight)
        };



        MeshSimple mesh = new MeshSimple();
        Matrix proj;
        Matrix view;
        Matrix world;


        string msg = "";

        public Game1_Mesh()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.Title = " ex Game1_Mesht.";
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += UserResizedWindow;
            Window.AllowAltF4 = true;
            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 800;
            graphics.ApplyChanges();
        }
        public void UserResizedWindow(object sender, EventArgs e)
        {
            //Re Setup Cameras;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = @"Content";
            meshEffect = Content.Load<Effect>("MeshDrawEffect");

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");

            mesh = new MeshSimple();
            mesh.GetMesh(new Rectangle( -100, -100, 200,200) , 50, 50, true);

            SetupMyCamera();
            SetUpOrthographicBasicEffect(GraphicsDevice);
            SetUpDirectCameraMatrices();
        }

        public void SetupMyCamera()
        {
            // a 90 degree field of view is needed for the projection matrix.
            var f90 = 90.0f * (3.14159265358f / 180f);
            _cameraCinematic = new DemoCamera(GraphicsDevice, spriteBatch, null, new Vector3(0, 0, 0), Vector3.Forward, Vector3.Up, 0.01f, 10000f, f90, true, false, false);
            _cameraCinematic.WayPointCycleDurationInTotalSeconds = 30f;
            _cameraCinematic.MovementSpeedPerSecond = 8f;
            _cameraCinematic.SetWayPoints(_cameraWayPoints, true, true, 30);
            _cameraCinematic.UseForwardPathLook = false;
            _cameraCinematic.UseWayPointMotion = true;
        }
        public void SetUpOrthographicBasicEffect(GraphicsDevice device)
        {
            float forwardDepthDirection = 1f;
            _basicEffect = new BasicEffect(GraphicsDevice);
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.TextureEnabled = true;
            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = Matrix.Invert(Matrix.CreateWorld(new Vector3(0, 0, 0), new Vector3(0, 0, 1), Vector3.Down));
            _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, -device.Viewport.Height, 0, forwardDepthDirection * 0, forwardDepthDirection * 1f);
        }
        public void SetUpDirectCameraMatrices()
        {
            proj = Matrix.CreatePerspectiveFieldOfView(1f, GraphicsDevice.Viewport.AspectRatio, .01f, 1000f);
            view = Matrix.CreateLookAt(new Vector3(0, 0, +1), new Vector3(0, 0, .01f), new Vector3(0, 0, -1f));
            world = Matrix.CreateWorld(new Vector3(0, 0, 0), Vector3.Forward, new Vector3(0, 0, -1f));
        }

        protected override void UnloadContent(){  }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            if (Keys.F1.IsKeyPressedWithDelay(gameTime))
                _cameraCinematic.UseForwardPathLook = !_cameraCinematic.UseForwardPathLook;

            if (Keys.F2.IsKeyPressedWithDelay(gameTime))
                _cameraCinematic.UseWayPointMotion = !_cameraCinematic.UseWayPointMotion;

            if (Keys.F3.IsKeyPressedWithDelay(gameTime))
                _useDesignatedTarget = !_useDesignatedTarget;

            if(_useDesignatedTarget)
                _cameraCinematic.Update(_targetLookAt, gameTime);
            else
                _cameraCinematic.Update(gameTime);

            ComposeMessege();

            base.Update(gameTime);
        }

        public void ComposeMessege()
        {
            string msg2 = "";
            if (_useDesignatedTarget)
                msg2 += $"\n Camera  _cameraCinematic.TargetPosition: {_cameraCinematic.TargetPosition}";


            msg =
           $"\n Camera.World.Translation: \n  { _cameraCinematic.World.Translation.X.ToString("N3") } { _cameraCinematic.World.Translation.Y.ToString("N3") } { _cameraCinematic.World.Translation.Z.ToString("N3") }" +
           $"\n Camera.Forward: \n  { _cameraCinematic.Forward.X.ToString("N3") } { _cameraCinematic.Forward.Y.ToString("N3") } { _cameraCinematic.Forward.Z.ToString("N3") }" +
           $"\n Up: \n { _cameraCinematic.Up.X.ToString("N3") } { _cameraCinematic.Up.Y.ToString("N3") } { _cameraCinematic.Up.Z.ToString("N3") } " +
           $"\n Camera IsSpriteBatchStyled: {_cameraCinematic.IsSpriteBatchStyled}" +
           $"\n Camera F1 UseForwardPathLook: {_cameraCinematic.UseForwardPathLook}" +
           $"\n Camera F2 UseWayPointMotion: {_cameraCinematic.UseWayPointMotion}" +
           $"\n Camera F3 UseDesignatedTarget: {_useDesignatedTarget + msg2}"
           ;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            DrawMesh(gameTime);
            DrawSpriteBatches(gameTime);
            base.Draw(gameTime);
        }

        public void DrawMesh(GameTime gameTime)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            meshEffect.CurrentTechnique = meshEffect.Techniques["QuadDraw"];
            meshEffect.Parameters["SpriteTexture"].SetValue(texture);
            meshEffect.Parameters["World"].SetValue(_cameraCinematic.World);
            meshEffect.Parameters["View"].SetValue(_cameraCinematic.View);
            meshEffect.Parameters["Projection"].SetValue(_cameraCinematic.Projection);
            //meshEffect.Parameters["World"].SetValue(world);
            //meshEffect.Parameters["View"].SetValue(view);
            //meshEffect.Parameters["Projection"].SetValue(proj);
            mesh.Draw(GraphicsDevice, meshEffect);
        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
            //spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            //spriteBatch.Draw(texture, new Rectangle(0, 0, 300, 300), Color.White);
            //spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            _cameraCinematic.DrawCurveThruWayPointsWithSpriteBatch(1.5f, new Vector3(GraphicsDevice.Viewport.Bounds.Right - 100, 1, GraphicsDevice.Viewport.Bounds.Bottom - 100), 1, gameTime);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, _basicEffect, null);
            spriteBatch.DrawString(font, msg, new Vector2(10, 10), Color.Moccasin);
            spriteBatch.End();
        }
    }

    public class MeshSimple
    {
        VertexPositionNormalTexture[] vertices;
        int w;
        int h;
        public VertexPositionNormalTexture[] GetMesh(Rectangle modelRectangle, int verticesWidth, int verticesHeight, bool flipNormalDirection)
        {
            w = verticesWidth;
            h = verticesHeight;
            List<VertexPositionNormalTexture> vertlist = new List<VertexPositionNormalTexture>();
            var tl = new Vector2(0, 0);
            var tr = new Vector2(1, 0);
            var bl = new Vector2(0, 1);
            var br = new Vector2(1, 1);
            // initial calculation
            for (int y = 0; y < w - 1; y++)
            {
                for (int x = 0; x < h - 1; x++)
                {
                    // t0
                    vertlist.Add(new VertexPositionNormalTexture(new Vector3(x, y, 0) + tl.ToVector3(), new Vector3(0, 0, -1), uv(x, y) + tl));
                    vertlist.Add(new VertexPositionNormalTexture(new Vector3(x, y, 0) + bl.ToVector3(), new Vector3(0, 0, -1), uv(x, y) + bl));
                    vertlist.Add(new VertexPositionNormalTexture(new Vector3(x, y, 0) + tr.ToVector3(), new Vector3(0, 0, -1), uv(x, y) + tr));
                    // t1
                    vertlist.Add(new VertexPositionNormalTexture(new Vector3(x, y, 0) + tr.ToVector3(), new Vector3(0, 0, -1), uv(x, y) + tr));
                    vertlist.Add(new VertexPositionNormalTexture(new Vector3(x, y, 0) + bl.ToVector3(), new Vector3(0, 0, -1), uv(x, y) + bl));
                    vertlist.Add(new VertexPositionNormalTexture(new Vector3(x, y, 0) + br.ToVector3(), new Vector3(0, 0, -1), uv(x, y) + br));
                }
            }
            RePositionVerticesInModelSpace(ref vertlist, modelRectangle);
            DetermineQuadNormals(ref vertlist, flipNormalDirection);
            vertices = vertlist.ToArray();
            return vertices;
        }

        private void RePositionVerticesInModelSpace(ref List<VertexPositionNormalTexture> vertlist, Rectangle modelSpaceRectangle)
        {
            // resize to the world rectangle this is still in object space. 
            // so the rectangle might want to be like -100,100  and have a size of 200,200 to center it in local object space.
            var loc = modelSpaceRectangle.Location.ToVector2();
            var size = modelSpaceRectangle.Size.ToVector2();
            for (int i = 0; i < vertlist.Count; i += 6)
            {
                var v = vertlist[i];
                v.Position = (v.Position / new Vector3(w, h, 1)) * size.ToVector3() + loc.ToVector3();
                vertlist[i] = v;
            }
        }

        public void DetermineQuadNormals(ref List<VertexPositionNormalTexture> vertlist, bool flipDirection)
        {
            // generate the normals
            for (int i = 0; i < vertlist.Count; i += 6)
            {
                var tn0 = Vector3.Normalize( CrossProduct3d(Vector3.Normalize(vertlist[i + 0].Position), Vector3.Normalize(vertlist[i + 1].Position), Vector3.Normalize(vertlist[i + 2].Position)) );
                var tn1 = Vector3.Normalize(CrossProduct3d(Vector3.Normalize(vertlist[i + 3].Position), Vector3.Normalize(vertlist[i + 4].Position), Vector3.Normalize(vertlist[i + 5].Position)) );
                if (flipDirection)
                {
                    tn0 = -tn0;
                    tn0 = -tn0;
                }
                // t0
                vertlist[i + 0] = new VertexPositionNormalTexture(vertlist[i + 0].Position, tn0, vertlist[i + 0].TextureCoordinate);
                vertlist[i + 1] = new VertexPositionNormalTexture(vertlist[i + 1].Position, tn0, vertlist[i + 1].TextureCoordinate);
                vertlist[i + 2] = new VertexPositionNormalTexture(vertlist[i + 2].Position, tn0, vertlist[i + 2].TextureCoordinate);
                // t1
                vertlist[i + 3] = new VertexPositionNormalTexture(vertlist[i + 3].Position, tn1, vertlist[i + 3].TextureCoordinate);
                vertlist[i + 4] = new VertexPositionNormalTexture(vertlist[i + 4].Position, tn1, vertlist[i + 4].TextureCoordinate);
                vertlist[i + 5] = new VertexPositionNormalTexture(vertlist[i + 5].Position, tn1, vertlist[i + 5].TextureCoordinate);
            }
        }

        public static Vector3 CrossProduct3d(Vector3 a, Vector3 b, Vector3 c)
        {
            return new Vector3
                (
                ((b.Y - a.Y) * (c.Z - b.Z)) - ((c.Y - b.Y) * (b.Z - a.Z)),
                ((b.Z - a.Z) * (c.X - b.X)) - ((c.Z - b.Z) * (b.X - a.X)),
                ((b.X - a.X) * (c.Y - b.Y)) - ((c.X - b.X) * (b.Y - a.Y))
                );
        }

        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length /3, VertexPositionNormalTexture.VertexDeclaration);
            }
        }


        int GetIndex(int x, int y)
        {
            return x + y * w;
        }

        Vector2 uv(Vector2 v)
        {
            return new Vector2((float)v.X / (float)w, (float)v.Y / (float)h);
        }
        Vector2 uv(int x, int y)
        {
            return new Vector2((float)x / (float)w, (float)y / (float)h);
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

        /// <summary>
        /// Placeholder for the texture on the sides
        /// </summary>
        private Texture2D prismSideTexture;

        /// <summary>
        /// prism BasicEffect
        /// </summary>
        public BasicEffect effect;

        /// <summary>
        /// The World Matrix somewhat redundant being here.
        /// Now if anything we should provide accessors to set the effect view and projection.
        /// </summary>
        private Matrix worldMatrix;
        #endregion

        // Requisite for draw user indexed primitives. 
        private VertexPositionTexture[] nverts;
        private short[] nIndexs;
        // Requisite for draw primitives.
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        /// <summary>
        /// Creates and initializes a prism class object at load time. 
        /// Returns it as desired by the users specifications.
        /// this method is static so that you call it like so... Prism p = Prism.Load(..) .
        /// </summary>
        public static Prism Load(GraphicsDevice device, int nSides, float height, float radius, Texture2D sideTexture)
        {
            var t = new Prism();
            t.prismSides = nSides;
            t.prismHeight = height;
            t.prismRadius = radius;
            t.prismSideTexture = sideTexture;
            if (nSides < 3)
                t.prismSides = 3;
            // you might want decimals and you can probably do this with a scaling matrix in your own vertex shader.
            if (height < 1f)
                t.prismHeight = 1f;
            if (radius < 1f)
                t.prismRadius = 1f;

            // __________________________________
            // moved this all to this load method
            //
            // All common stuff set up the effect initially.
            //
            t.effect = new BasicEffect(device);
            t.effect.LightingEnabled = false;
            t.effect.TextureEnabled = true;
            t.effect.Texture = t.prismSideTexture;

            // The game itself is really responsible for this not some arbitrary game object.
            if (t.worldMatrix == null) { t.worldMatrix = Matrix.Identity; }
            float aspectRatio = (float)device.Viewport.Width / device.Viewport.Height;
            t.effect.View = Matrix.CreateLookAt(new Vector3(0f, 4f, 0f), Vector3.Zero, Vector3.Up);
            t.effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10.0f);

            //
            // build the prism
            //
            t.BuildPrism(device, t.prismSides, t.prismHeight, (int)(t.prismRadius));

            // i made this a static load to sort of be like blah forget the constructor.
            // so now its time to return the new prism object.
            return t;
        }

        /// <summary>
        /// Build the prism
        /// </summary>
        private void BuildPrism(GraphicsDevice gd, int sides, float height, float radius)
        {
            //
            // Get the vertices into a vertex array.
            // Note drawuserindexed primitives can use this.
            // However its not really using the vertex buffer this way.
            //
            nverts = GetPrismVertices(radius, height, sides);
            //
            // Send a vertex buffer to the device.
            // create the buffer, set the vertice array to that buffer, send the buffer to the device. 
            //
            vertexBuffer = new VertexBuffer(gd, VertexPositionTexture.VertexDeclaration, nverts.Length, BufferUsage.None);
            vertexBuffer.SetData(nverts);
            gd.SetVertexBuffer(vertexBuffer);

            //
            // set up the index buffer
            //
            nIndexs = new short[sides * 3 * 2];

            int offset = 0;
            // first set
            for (int i = 2; i < nverts.Length; i++)
            {

                int i0 = offset + 0;
                int i1 = offset + 1;
                int i2 = offset + 2;
                offset += 3;

                short v0 = (short)(0); // vertice [0] holds the up prism point.
                short v1 = (short)(i); // each side has 2 points other then top or bottom.
                short v2 = (short)(i + 1); // we know all our side points are from 2 to the end.
                                           //
                                           // now towards the end of this loop.
                                           // well wrap that second side vertice around back to vertice [2]
                                           //
                if (v2 >= nverts.Length)
                {
                    v2 = 2;
                }
                // we can control our initial culling order.
                // i.e. the way vertices use backface or frontface culling right here.
                // So here ill set it to use counter clockwise winding (ccw)
                nIndexs[i0] = v0;
                nIndexs[i1] = v1;
                nIndexs[i2] = v2;
            }
            // second set
            for (int i = 2; i < nverts.Length; i++)
            {
                int i0 = offset + 0;
                int i1 = offset + 1;
                int i2 = offset + 2;
                offset += 3;

                short v0 = (short)(1); // vertice [1] holds the down prism point
                short v1 = (short)(i);
                short v2 = (short)(i + 1);
                if (v2 >= nverts.Length)
                {
                    v2 = 2;
                }
                // reverse the input ordering to keep the winding counter clockwise
                nIndexs[i0] = v1;
                nIndexs[i1] = v2;
                nIndexs[i2] = v0;
            }

            indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, offset + 1, BufferUsage.None);
            indexBuffer.SetData(nIndexs);
            gd.Indices = indexBuffer;
        }
        /// <summary>
        /// Returns all the vertices the first two indices are the top then bottom points.
        /// Followed by all the other vertices points.
        /// </summary>
        public VertexPositionTexture[] GetPrismVertices(float radius, float height, float nPositions)
        {
            VertexPositionTexture[] result = new VertexPositionTexture[(int)(nPositions) + 2];

            float degrees = 0;
            float radians = 0f;
            float x;
            float z;
            float textureU = .5f;
            float textureV = 0f;
            result[0] = new VertexPositionTexture(Vector3.Up * height, new Vector2(textureU, textureV));
            textureV = 1f;
            result[1] = new VertexPositionTexture(Vector3.Down * height, new Vector2(textureU, textureV));
            textureV = .5f;
            for (int i = 0; i < nPositions; i++)
            {
                degrees = i * (360 / nPositions);
                radians = (degrees * ((float)Math.PI / 180));
                float sin = (float)(Math.Sin(radians));
                float cos = (float)(Math.Cos(radians));
                x = radius * sin;
                z = radius * cos;
                textureU = (i) / (nPositions - 1);
                result[i + 2] = new VertexPositionTexture(new Vector3(x, 0f, z), new Vector2(textureU, textureV));
            }
            return result;
        }

        public void Draw(GraphicsDevice device)
        {
            float aspectRatio = (float)device.Viewport.Width / device.Viewport.Height;
            effect.World = worldMatrix;
            effect.View = Matrix.CreateLookAt(new Vector3(0f, 0f, 1f), Vector3.Zero, Vector3.Up);
            effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(65.0f), aspectRatio, .5f, 1000.0f);

            Draw(device, effect.World, effect.View, effect.Projection, true);
        }
        public void Draw(GraphicsDevice device, Matrix world, Matrix view, Matrix projection, bool useingUserIndexedPrims)
        {

            int triangleCount = nIndexs.Length / 3;

            //World Matrix
            effect.World = world;
            effect.View = view;
            effect.Projection = projection;

            effect.CurrentTechnique.Passes[0].Apply();

            if (useingUserIndexedPrims)
            {
                // With DrawUserIndexedPrimitives we can work with the arrays themselves by passing them each frame.
                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, nverts, 0, nverts.Length, nIndexs, 0, triangleCount, VertexPositionTexture.VertexDeclaration);
            }
            else
            {
                // set buffers on device
                device.Indices = indexBuffer;
                device.SetVertexBuffer(vertexBuffer);

                // this way actually uses these buffers that we already set onto the device.
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, triangleCount);
            }
        }

    }


    public static class Ext
    {
        public static Vector3 ToVector3(this Vector2 v, float z)
        {
            return new Vector3(v.X, v.Y, z);
        }
        public static Vector3 ToVector3(this Vector2 v)
        {
            return new Vector3(v.X, v.Y, 0);
        }
    }
}
