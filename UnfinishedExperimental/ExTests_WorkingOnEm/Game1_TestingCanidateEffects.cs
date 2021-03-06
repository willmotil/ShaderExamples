﻿
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.IO.Compression;

namespace ShaderExamples
{
    public class Game1_TestingCanidateEffects : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        MouseState ms;

        Texture2D texture;
        Effect effect;

        //const int MAXSAMPLES = 60;
        //int numberOfSamples = 8;

        float time = 0.0f;
        Vector2 center = new Vector2(.5f, .5f);
        Vector3 shockParams = new Vector3(10.0f, 0.8f, 0.1f);

        bool shockwaveClicks = false;

        //float2 center; // Mouse position
        //float time; // effect elapsed time
        //float3 shockParams; // 10.0, 0.8, 0.1

        public Game1_TestingCanidateEffects()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = " ex Testing Effect.";
            IsMouseVisible = true;

            Test();

            Test2();
        }

        string msgMisc = "";
        public void Test()
        {
            int a = 12;
            int b = 4;
            int tOR = a | b;
            int tAnd = a & b;
            int tXor = a ^ b;
            int tOnesCompliment = ~a;
            int tleft = a << b;
            int tright = a >> b;

            msgMisc =
                     $" a = \n  {ToBinaryString(a)}  \n  b = \n  {ToBinaryString(b)}    \n  : " +
                    $"\n  a {a} |  b {b} = OR \n  { ToBinaryString(tOR) }    if either bit is on then we get a 1" +
                    $"\n  a {a} &  b {b} = And \n  {ToBinaryString(tAnd)}    if both bits are on we get a 1" +
                    $"\n  a {a} ^  b {b} = Xor \n  {ToBinaryString(tXor)}    if either bit is on we get a 1" +
                    $"\n  ~ a {a} = OnesCompliment \n  {ToBinaryString(tOnesCompliment)}    if a bit is on we get a 0 if off we get a 1  ~ reverses the bits" +
                    $"\n  a {a} <<  b {b} = tleft \n  {ToBinaryString(tleft)}    Increasing values" +
                    $"\n  a {a} >>  b {b} = tright \n  {ToBinaryString(tright)}    Decreasing values" +
                    $"\n  "
                    ;

            msgMisc += $"\n"+ tOR.ToSpacedBinaryString();

            msgMisc += $"\n" + tOR.ToWellFormatedBinaryString();

            Console.WriteLine(msgMisc);
        }

        public static string ToBinaryString(int value)
        {
            string result = "";
            int j = 0;
            for(int bitIndexToTest = 0; bitIndexToTest < 32; bitIndexToTest ++)
            {
                if (IsIntBitOn(value, bitIndexToTest))
                    result += "1";
                else
                    result += "0";
                j++;
                if (j > 7)
                {
                    result += " ";
                    j = 0;
                }
            }
            return result;
        }

        public static bool IsIntBitOn(int inValue, int bitIndexToTest)
        {
            if ( (inValue & (1 << (bitIndexToTest))) > 0)
                return true;
            else
                return false;
        }



        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            MgDrawExt.Initialize(GraphicsDevice, spriteBatch);

            Content.RootDirectory = @"Content/Shaders";
            effect = Content.Load<Effect>("TestingCanidateEffects");

            // Change Directory.
            Content.RootDirectory = @"Content/Images";

            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");  // cutePuppy  ,  MG_Logo_Med_exCanvs.png

            // Change Directory.
            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");

            // Change Directory back.
            Content.RootDirectory = @"Content";

            MgExtensions.DelayTime = 0.09f;
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            MouseHelper.Update(GraphicsDevice.Viewport.Bounds.Size);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float secondsScalar = 1.5f;
            time += (float)gameTime.ElapsedGameTime.TotalSeconds * secondsScalar;
            float maxTime = 10.0f;

            ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed && gameTime.IsUnDelayed())
            {
                shockwaveClicks = true;
                time = .0f;
            }
            if (shockwaveClicks)
            {
                time = time.Clamp( 0, maxTime);
                if (time >= maxTime)
                    shockwaveClicks = false;
            }
            else
                time = 100.0f;

            center = (ms.Position.ToVector2() / GraphicsDevice.Viewport.Bounds.Size.ToVector2()); // - new Vector2(.5f,.5f) ;

            base.Update(gameTime);
        }

        //if (Keyboard.GetState().IsKeyDown(Keys.Up))
        //    time += .01f;
        //if (Keyboard.GetState().IsKeyDown(Keys.Down))
        //    time -= .01f;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            effect.CurrentTechnique = effect.Techniques["TestTechnique"];
            effect.Parameters["center"].SetValue(center);
            effect.Parameters["time"].SetValue(time);
            effect.Parameters["shockParams"].SetValue(shockParams);
            //effect.Parameters["textureSize"].SetValue(new Vector2(texture.Width, texture.Height));


            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, effect, null);
            spriteBatch.Draw(texture, GraphicsDevice.Viewport.Bounds, Color.Blue);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            spriteBatch.DrawString(font, $"Controls: left click, arrow keys \n radialScalar: {time.ToString("##0.000")} \n numberOfSamples: {shockParams} \n textureBlurUvOrigin: {center.ToString()} ", new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(font, $"\n " + msgMisc, new Vector2(210, 110), Color.White);
            spriteBatch.End();


            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);


            var msPos = new Vector2(100, 500); float incy = 20f; Color col = Color.Blue;
            spriteBatch.DrawString(font, $"\n IsLeftDown {MouseHelper.IsLeftDown}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n IsLeftClicked {MouseHelper.IsLeftClicked}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n IsLeftHeld {MouseHelper.IsLeftHeld}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n IsLeftJustReleased {MouseHelper.IsLeftJustReleased}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n IsLeftDragged {MouseHelper.IsLeftDragged}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n LeftDragRectangle {MouseHelper.LeftDragRectangle}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n LastLeftPressedAt {MouseHelper.LastLeftPressedAt}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n LastLeftDragReleased {MouseHelper.LastLeftDragReleased}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n GetLeftDragVector() {MouseHelper.GetLeftDragVector()}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawRectangleOutlineWithString(font, MouseHelper.LeftDragRectangle, 1, col, $" Left Dragged", col);


            msPos = new Vector2(500, 500); incy = 20f; col = Color.Aqua;
            spriteBatch.DrawString(font, $"\n IsRightDown {MouseHelper.IsRightDown}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n IsRightClicked {MouseHelper.IsRightClicked}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n IsRightHeld {MouseHelper.IsRightHeld}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n IsRightJustReleased {MouseHelper.IsRightJustReleased}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n IsRightDragged {MouseHelper.IsRightDragged}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n RightDragRectangle {MouseHelper.RightDragRectangle}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n LastRightPressedAt {MouseHelper.LastRightPressedAt}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n LastRightDragReleased {MouseHelper.LastRightDragReleased}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawString(font, $"\n GetRightDragVector() {MouseHelper.GetRightDragVector()}", msPos, col); msPos.Y += incy;
            spriteBatch.DrawRectangleOutlineWithString(font, MouseHelper.RightDragRectangle, 1, col, $" right Dragged", col);

            spriteBatch.DrawString(font, $"\n MouseInputHelper.Pos {MouseHelper.Pos}", MouseHelper.Pos.ToVector2(), col); msPos.Y += incy;

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /*
                 /// <summary>
        /// is left being pressed now
        /// </summary>
        public static bool IsLeftDown = false;
        /// <summary>
        /// is left just clicked
        /// </summary>
        public static bool IsLeftClicked = false;
        /// <summary>
        /// is left being held down now
        /// </summary>
        public static bool IsLeftHeld = false;
        /// <summary>
        /// is true only in one single frame is the mouse just released
        /// </summary>
        public static bool IsLeftJustReleased = false;
        /// <summary>
        /// has the left mouse been dragged
        /// </summary>
        public static bool IsLeftDragged = false;
        /// <summary>
        /// dragged rectangle.
        /// </summary>
        public static Rectangle LeftDragRectangle = Rectangle.Empty;
        /// <summary>
        /// left last position pressed while useing left mouse button
        /// </summary>
        public static Vector2 LastLeftPressedAt;
        /// <summary>
        /// left last position draged from before release while useing left mouse button
        /// </summary>
        public static Vector2 LastLeftDragReleased;
        /// <summary>
        /// Gets the direction and magnitude of left drag press to drag released
        /// </summary>
        public static Vector2 GetLeftDragVector()
        {
            return LastLeftDragReleased - LastLeftPressedAt;
        }
         
         */

        public void Test2()
        {
            Console.WriteLine("\n\n");

            byte[] data = new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 1, 0, 0, 1, 0, 0,
                0, 0, 1, 0, 0, 1, 0, 0,
                0, 0, 1, 0, 0, 1, 0, 0,
                0, 0, 1, 0, 0, 1, 0, 0,
                0, 0, 1, 1, 1, 1, 0, 0,
                0, 0, 1, 0, 0, 1, 0, 0,
                0, 0, 1, 0, 0, 1, 0, 0,
                0, 0, 1, 0, 0, 1, 0, 0,
                0, 0, 1, 0, 0, 1, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0
            };


            Console.WriteLine("data");
            foreach (var b in data)
                Console.Write(b);
            Console.WriteLine("\n\n");



            var compressedData = Compress(data);
            Console.WriteLine("compressed data");
            foreach (var b in compressedData)
                Console.Write(b);
            Console.WriteLine("\n\n");



            var decompressedData = DeCompress(compressedData);
            Console.WriteLine("decompressed Data");
            foreach (var b in decompressedData)
                Console.Write(b);
            Console.WriteLine("\n\n");
        }

        public static byte[] Compress(byte[] rawByteData)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream deflateStream = new GZipStream(ms, CompressionMode.Compress))
                {
                    deflateStream.Write(rawByteData, 0, rawByteData.Length);
                }
                return ms.ToArray();
            }
        }

        static byte[] DeCompress(byte[] gzipCompressedByteData)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(gzipCompressedByteData), CompressionMode.Decompress))
            {
                const int maxNumOfBytesToRead = 4096;
                byte[] buffer = new byte[maxNumOfBytesToRead];
                using (MemoryStream memory = new MemoryStream())
                {
                    int countOfBytesRead = 0;
                    do
                    {
                        countOfBytesRead = stream.Read(buffer, 0, maxNumOfBytesToRead);
                        if (countOfBytesRead > 0)
                        {
                            memory.Write(buffer, 0, countOfBytesRead);
                        }
                    }
                    while (countOfBytesRead > 0);
                    return memory.ToArray();
                }
            }
        }

    #region helper functions


    #endregion
}
}


//public class Primitive2dQuadBuffer
//{
//    // ccw winding.

//    List<VertexPositionNormalTexture> verticeList = new List<VertexPositionNormalTexture>();
//    VertexPositionNormalTexture[] vertices;
//    public void AddVertexRectangleToBuffer(GraphicsDevice gd, Rectangle r, float depth)
//    {
//        var normal = Vector3.Normalize(new Vector3(0, 0, depth));
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Top, depth) , normal, new Vector2(0f, 0f))); ;  // p1
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Bottom, depth) , normal, new Vector2(0f, 1f))); // p0
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Bottom, depth) , normal, new Vector2(1f, 1f)));// p3

//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Bottom, depth) , normal, new Vector2(1f, 1f)));// p3
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Top, depth) , normal, new Vector2(1f, 0f)));// p2
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Top, depth) , normal, new Vector2(0f, 0f))); // p1

//        vertices = verticeList.ToArray();
//    }

//    public void AlterVertexRectanglePositionInBuffer(GraphicsDevice gd, int index, Rectangle r, float depth)
//    {
//        // Triangle 1
//        vertices[index + 0].Position = new Vector3(r.Left, r.Top, depth);  // p1
//        vertices[index + 1].Position = new Vector3(r.Left, r.Bottom, depth); // p0
//        vertices[index + 2].Position = new Vector3(r.Right, r.Bottom, depth); // p3
//        // Triangle 2
//        vertices[index + 3].Position = new Vector3(r.Right, r.Bottom, depth);// p3
//        vertices[index + 4].Position = new Vector3(r.Right, r.Top, depth); // p2
//        vertices[index + 5].Position = new Vector3(r.Left, r.Top, depth); // p1
//    }

//    public void DrawQuadBuffer(GraphicsDevice device, Effect effect)
//    {
//        if (vertices != null)
//        {
//            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
//            {
//                pass.Apply();
//                int numberOfTriangles = vertices.Length / 3;
//                device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, numberOfTriangles);
//            }
//        }
//    }
//    public void DrawQuadRangeInBuffer(GraphicsDevice device, Effect effect, int startQuad, int quadDrawLength)
//    {
//        int startVertice = startQuad * 2 * 3;
//        int numberOfTriangles = quadDrawLength * 2;
//        if (vertices != null)
//        {
//            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
//            {
//                pass.Apply();
//                device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, startVertice, numberOfTriangles);
//            }
//        }
//    }
//}

//public class Primitive2dIndexedQuadBuffer
//{
//    // ccw winding.

//    List<VertexPositionNormalTexture> verticeList = new List<VertexPositionNormalTexture>();
//    List<int> indiceList = new List<int>();
//    VertexPositionNormalTexture[] vertices;
//    int[] indices;
//    public void AddVertexRectangleToBuffer(GraphicsDevice gd, Rectangle r, float depth)
//    {
//        int currentCount = verticeList.Count;
//        var normal = Vector3.Normalize(new Vector3(0, 0, depth));
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Top, depth), normal, new Vector2(0f, 0f)));  // p1
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Left, r.Bottom, depth), normal, new Vector2(0f, 1f))); // p0
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Bottom, depth), normal, new Vector2(1f, 1f)));// p3
//        verticeList.Add(new VertexPositionNormalTexture(new Vector3(r.Right, r.Top, depth), normal, new Vector2(1f, 0f)));// p4
//        vertices = verticeList.ToArray();

//        // triangle 0 indices.
//        indiceList.Add(currentCount + 0);
//        indiceList.Add(currentCount + 1);
//        indiceList.Add(currentCount + 3);
//        // triangle 1 indices.
//        indiceList.Add(currentCount + 1);
//        indiceList.Add(currentCount + 2);
//        indiceList.Add(currentCount + 3);
//        indices = indiceList.ToArray();
//    }

//    public void AlterVertexRectanglePositionInBuffer(GraphicsDevice gd, int index, Rectangle r, float depth)
//    {
//        vertices[index + 0].Position = new Vector3(r.Left, r.Top, depth);  // p1
//        vertices[index + 1].Position = new Vector3(r.Left, r.Bottom, depth); // p0
//        vertices[index + 2].Position = new Vector3(r.Right, r.Bottom, depth); // p3
//        vertices[index + 4].Position = new Vector3(r.Right, r.Top, depth); // p2
//    }

//    public void DrawQuadBuffer(GraphicsDevice device, Effect effect)
//    {
//        if (vertices != null)
//        {
//            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
//            {
//                pass.Apply();
//                int numberOfTriangles = vertices.Length / 3;
//                device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, numberOfTriangles);
//            }
//        }
//    }
//    public void DrawQuadRangeInBuffer(GraphicsDevice device, Effect effect, int startQuad, int quadDrawLength)
//    {
//        int startVertice = startQuad * 2 * 3;
//        int numberOfTriangles = quadDrawLength * 2;
//        if (vertices != null)
//        {
//            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
//            {
//                pass.Apply();
//                device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, startVertice, numberOfTriangles);
//            }
//        }
//    }
//}