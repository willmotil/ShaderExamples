

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

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
        private static float Range = 28.0f;
        private Vector4[] _cameraWayPoints = new Vector4[]
        {
            new Vector4(-Range, 0, 0, 1f), new Vector4(0, 0, -Range, 1f), new Vector4(Range, 0, 0, 1f), new Vector4(0, 0, Range , 1f)
        };
        private bool _useDemoWaypoints = false;
        private Vector3 _targetLookAt = new Vector3(0, 0, 0);

        private Matrix _projectionBuildSkyCubeMatrix;


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
            Window.AllowAltF4 = true;
            Window.ClientSizeChanged += ClientResize;
            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 800;
            graphics.ApplyChanges();
        }
        public void ClientResize(object sender, EventArgs e)
        {
            //SetupTheCameras();
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
            mesh.GetMesh(50, 50);

            SetTempCam();
            SetupTheCameras();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            _cameraCinematic.Update(_targetLookAt, gameTime);

            msg =
            $"\n Camera.World.Translation: \n  { _cameraCinematic.World.Translation.X.ToString("N3") } { _cameraCinematic.World.Translation.Y.ToString("N3") } { _cameraCinematic.World.Translation.Z.ToString("N3") }" +
            $"\n Camera.Forward: \n  { _cameraCinematic.Forward.X.ToString("N3") } { _cameraCinematic.Forward.Y.ToString("N3") } { _cameraCinematic.Forward.Z.ToString("N3") }" +
            $"\n Up: \n { _cameraCinematic.Up.X.ToString("N3") } { _cameraCinematic.Up.Y.ToString("N3") } { _cameraCinematic.Up.Z.ToString("N3") } " +
            $"\n Camera IsSpriteBatchStyled {_cameraCinematic.IsSpriteBatchStyled}"
            ;

            base.Update(gameTime);
        }

        public void SetupTheCameras()
        {
            // a 90 degree field of view is needed for the projection matrix.
            var f90 = 90.0f * (3.14159265358f / 180f);
            _cameraCinematic = new DemoCamera(GraphicsDevice, spriteBatch, null, new Vector3(0, 0, 0), Vector3.Forward, Vector3.Up, 0.01f, 10000f, f90, true, false, false);
            _cameraCinematic.WayPointCycleDurationInTotalSeconds = 30f;
            _cameraCinematic.MovementSpeedPerSecond = 8f;
            _cameraCinematic.SetWayPoints(_cameraWayPoints, true, true, 30);
            _cameraCinematic.UseForwardPathLook = false;
            _cameraCinematic.UseWayPointMotion = false;

            Orthographic(GraphicsDevice);
        }

        public void Orthographic(GraphicsDevice device)
        {
            float forwardDepthDirection = 1f;
            _basicEffect = new BasicEffect(GraphicsDevice);
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.TextureEnabled = true;
            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = Matrix.Invert(Matrix.CreateWorld(new Vector3(0, 0, 0), new Vector3(0, 0, 1), Vector3.Down));
            _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, -device.Viewport.Height, 0, forwardDepthDirection * 0, forwardDepthDirection * 1f);
        }

        public void SetTempCam()
        {
            proj = Matrix.CreatePerspectiveFieldOfView(1f, GraphicsDevice.Viewport.AspectRatio, .01f, 1000f);
            view = Matrix.CreateLookAt(new Vector3(0, 0, +1), new Vector3(0, 0, .01f), new Vector3(0, 0, -1f));
            world = Matrix.Identity; //Matrix.CreateWorld(new Vector3(0, 0, 0), Vector3.Forward, new Vector3(0, 0, -1f));
        }


        public void DrawMesh(GameTime gameTime)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            meshEffect.CurrentTechnique = meshEffect.Techniques["QuadDraw"];
            meshEffect.Parameters["SpriteTexture"].SetValue(texture);
            //meshEffect.Parameters["World"].SetValue(world);
            //meshEffect.Parameters["View"].SetValue(view);
            //meshEffect.Parameters["Projection"].SetValue(proj);
            meshEffect.Parameters["World"].SetValue(_cameraCinematic.World);
            meshEffect.Parameters["View"].SetValue(_cameraCinematic.View);
            meshEffect.Parameters["Projection"].SetValue(_cameraCinematic.Projection);
            mesh.Draw(GraphicsDevice, meshEffect);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            DrawMesh(gameTime);

            //spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            //spriteBatch.Draw(texture, new Rectangle(0, 0, 300, 300), Color.White);
            //spriteBatch.End();

            //spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            //spriteBatch.DrawString(font, $" press space to alter image  ", new Vector2(10, 10), Color.Black);
            //spriteBatch.End();

            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, _basicEffect, null);

            _cameraCinematic.DrawCurveThruWayPointsWithSpriteBatch(1.5f, new Vector3(GraphicsDevice.Viewport.Bounds.Right - 100, 1, GraphicsDevice.Viewport.Bounds.Bottom - 100), 1, gameTime);

            spriteBatch.DrawString(font, msg, new Vector2(10, 210), Color.Moccasin);

            spriteBatch.End();
        }

        public bool IsPressedWithDelay(Keys key, GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(key) && IsUnDelayed(gameTime))
                return true;
            else
                return false;
        }

        float delay = 0f;
        bool IsUnDelayed(GameTime gametime)
        {
            if (delay < 0)
            {
                delay = .25f;
                return true;
            }
            else
            {
                delay -= (float)gametime.ElapsedGameTime.TotalSeconds;
                return false;
            }
        }
    }

    public class MeshSimple
    {
        VertexPositionNormalTexture[] vertices;
        int w;
        int h;
        public VertexPositionNormalTexture[] GetMesh(int width, int height)
        {
            w = width;
            h = height;
            List<VertexPositionNormalTexture> vertlist = new List<VertexPositionNormalTexture>();
            var itl = GetIndex(0, 0);
            var itr = GetIndex(1, 0);
            var ibl = GetIndex(0, 1);
            var ibr = GetIndex(1, 1);
            var tl = new Vector2(0, 0);
            var tr = new Vector2(1, 0);
            var bl = new Vector2(0, 1);
            var br = new Vector2(1, 1);
            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
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
            //for (int i = 0; i < vertlist.Count; i += 6)
            //{
            //    var tn0 = Vector3.Cross( Vector3.Normalize(vertlist[i + 1].Position - vertlist[i].Position), Vector3.Normalize(vertlist[i + 2].Position - vertlist[i].Position));
            //    vertlist[i + 0] = new VertexPositionNormalTexture(vertlist[i + 0].Position, tn0, vertlist[i + 0].TextureCoordinate);
            //    vertlist[i + 1] = new VertexPositionNormalTexture(vertlist[i + 1].Position, tn0, vertlist[i + 1].TextureCoordinate);
            //    vertlist[i + 2] = new VertexPositionNormalTexture(vertlist[i + 2].Position, tn0, vertlist[i + 2].TextureCoordinate);
            //    var tn1 = Vector3.Cross(Vector3.Normalize(vertlist[i + 1].Position - vertlist[i+2].Position), Vector3.Normalize(vertlist[i + 3].Position - vertlist[i+2].Position));
            //    vertlist[i + 3] = new VertexPositionNormalTexture(vertlist[i + 3].Position, tn1, vertlist[i + 3].TextureCoordinate);
            //    vertlist[i + 4] = new VertexPositionNormalTexture(vertlist[i + 4].Position, tn1, vertlist[i + 4].TextureCoordinate);
            //    vertlist[i + 5] = new VertexPositionNormalTexture(vertlist[i + 5].Position, tn1, vertlist[i + 5].TextureCoordinate);
            //}
            vertices = vertlist.ToArray();
            return vertices;
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
    public static class Ext
    {
        public static Vector3 ToVector3(this Vector2 v)
        {
            return new Vector3(v.X, v.Y, 0);
        }
    }
}
