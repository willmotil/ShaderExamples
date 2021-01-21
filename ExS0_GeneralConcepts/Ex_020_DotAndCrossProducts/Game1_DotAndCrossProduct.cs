

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderExamples
{
    public class Game1_DotAndCrossProduct : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont font2;
        Texture2D texture;
        Texture2D texture2;
        Texture2D texture3;
        Effect effect;

        List<string> timedMsg = new List<string>();
        float elapsed = 0;
        float elapsedTotalMsgTime = 0;
        float msgDuration = 260;
        int index = 0;
        bool respectMsgTimer = true;
        float percent = 1.0f;

        float elapsedCycleTime = 0;
        Vector2 directionToCompare = new Vector2(0, -100);
        Vector2 originPoint = new Vector2(300, 300);

        public Game1_DotAndCrossProduct()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 800;
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
            //effect = Content.Load<Effect>("GammaTestEffect");
            //effect.CurrentTechnique = effect.Techniques["Gamma"];

            Content.RootDirectory = @"Content/Images";
            texture = Content.Load<Texture2D>("MG_Logo_Med_exCanvs");
            //texture2 = Content.Load<Texture2D>("Tanzania");
            //texture3 = Content.Load<Texture2D>("gamma_correction_brightness");

            Content.RootDirectory = @"Content/Fonts";
            font = Content.Load<SpriteFont>("MgFont");
            font2 = Content.Load<SpriteFont>("MgFont3");

            CreateMsgs();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            elapsedTotalMsgTime += elapsed;
            if (elapsedTotalMsgTime > msgDuration)
                elapsedTotalMsgTime = 0;

            float secondsPerCycle = 20f;
            elapsedCycleTime += elapsed * (1f / secondsPerCycle);
            if (elapsedCycleTime > 1f)
                elapsedCycleTime = 0;


            if (Keys.Up.IsKeyDown())
            {

            }
            if (Keys.Down.IsKeyDown())
            {

            }
            if (Keys.Left.IsKeyDown())
            {

            }
            if (Keys.Right.IsKeyDown())
            {

            }

            if (Keys.Space.IsKeyPressedWithDelay(gameTime))
            {
                if (index > timedMsg.Count - 2)
                    index = 0;
                index++;
                respectMsgTimer = false;
            }
            if (Keys.Back.IsKeyPressedWithDelay(gameTime))
            {
                if (index < 1)
                    index = timedMsg.Count - 1;
                index--;
                respectMsgTimer = false;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Moccasin);

            DrawDotAndCrossProductExamples(gameTime);

            DrawTheMsgs(gameTime);

            base.Draw(gameTime);
        }

        public void DrawDotAndCrossProductExamples(GameTime gameTime)
        {
            float sin = (float)Math.Sin(elapsedCycleTime * 6.28f);
            float cos = (float)Math.Cos(elapsedCycleTime * 6.28f);
            var normal1 = new Vector2(sin, cos);
            var distanceFromOrigin = 200f;
            var direction1 = normal1 * distanceFromOrigin;
            var position1 = direction1 + originPoint;

            // First we will show how to use a Cross product to get a new direction2 from direction1.
            // This is the same as the below in 2d the cross product can go left or right. 
            // in 3d it can go clockwise or counter clockwise round about the direction.
            // in order to get the normal since we don't have a sin cos we use the normalize function.
            // we will find a position for later use from the origin point on our screen for display.

            var direction2 = direction1.Cross2D(true);
            direction2 = new Vector2(-direction1.Y, +direction1.X); // same as above
            var normal2 = Vector2.Normalize(direction2);
            var position2 = direction2 + originPoint;

            // The direction we are checking against needs to be normalized as we did before we find a position.
            // With all these in hand well use the normals to compare the directional angles.

            var normalToCompare = Vector2.Normalize(directionToCompare);
            var positionToCompare = normalToCompare * distanceFromOrigin + originPoint;



            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);

            spriteBatch.DrawLineWithStringAtEnd(font2, "Origin Point", originPoint - new Vector2(10, 0), originPoint + new Vector2(10, 0), 1, Color.White);

            spriteBatch.DrawLineWithStringAtEnd(font2, $" normal direction { normalToCompare } " , originPoint, positionToCompare, 1, Color.Black);

            var dotResult1 = normal1.DotProduct2D(normalToCompare);
            spriteBatch.DrawLineWithStringAtEnd(font2, $"#1 Dot theta { dotResult1.ToString("##0.00") } ", originPoint, position1, 1, Color.DarkGreen);

            var dotResult2 = normal2.DotProduct2D(normalToCompare);
            spriteBatch.DrawLineWithStringAtEnd(font2, $"#2 Dot theta { dotResult2.ToString("##0.00") } ", originPoint, position2, 1, Color.Blue);


            spriteBatch.End();
        }

        public void DrawTheMsgs(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, null);
            if (respectMsgTimer)
                spriteBatch.DrawString(font2, timedMsg[(int)((timedMsg.Count - 1) * (elapsedTotalMsgTime / msgDuration))], new Vector2(10, 550), Color.Black, 0, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 0);
            else
                spriteBatch.DrawString(font2, timedMsg[index], new Vector2(10, 550), Color.Black, 0, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 0);
            spriteBatch.End();
        }

        public void CreateMsgs()
        {
            timedMsg.Add(" Dots n Crosses");
            timedMsg.Add(" About Dot Products and Cross Products.");
            timedMsg.Add(" Often we think of a position as some place in our world but it is composed of 2 values that are magnitudes.");
            timedMsg.Add(" Similarily we have like structures we call directions and normals but why bother? ");
            timedMsg.Add(" well... ");
            timedMsg.Add(" Because we wanna dot and cross stuff !!! ");
            timedMsg.Add(" So we need a new name for the same thing with a different twist on what it represents.");
            timedMsg.Add(" ");
            timedMsg.Add(" A Direction is simply a 2 element vector that always is related to specific origin point.\n Rotationally it is always 0,0 that is something to just gulp down for the moment.");
            timedMsg.Add(" ");
            timedMsg.Add(" A Normal is basically the same but it is unit length.\n Which is the distance of the elements of a direction divided back into those original elements.");
            timedMsg.Add(" That gives a sine cosine from a direction.\n This is via the pythagorean therom ");
            timedMsg.Add(" a^2+b^2 = c^2 \n c = sqroot(c^2) \n sin = x /c; cos = y /c;");
            timedMsg.Add(" The Dot product of two normals gives a angle between them that is called the theta.");
            timedMsg.Add(" The theta is a cosine it ranges between -1 and 1.");
            timedMsg.Add(" Multiplying the theta by the theta gives what is known as an Acosine a Linear version of a cosine... \n but you only want to know why its useful.");
            timedMsg.Add(" Well it's useful for well Everything! from collision detection to lighting to getting the degrees to a target.");
            timedMsg.Add(" The theta itself tells you if the two angles are parrallel when the theta is positive +1. \n perpendicular when 0 \n opposite when negative. -1");
            timedMsg.Add(" Acos * PI = a angle in radians in the direction of the first to the second. \n Directly one that can be passed to a Rotation matrix.");
            timedMsg.Add(" Acos *.5f + .5f is another useful function.");
            timedMsg.Add(" If you have a point on say a floor and point that represnts a players foot \n one can simply create a direction \n foot - floor = directionToFoot ");
            timedMsg.Add(" From there one can make a normal = Vector.Normalize(directionToFoot); \n provided we know the up direction of the floor.");
            timedMsg.Add(" We can dot that normal sticking out of the floor to the normal we made to the foot.\n  if that returns ... \n 0 the foot has contacted the floor. \n negative moved into the floor. \n positive above the floor.");
            timedMsg.Add(" Now you may ask... ");
            timedMsg.Add(" How do i get a normal sticking out of the floor ... ");
            timedMsg.Add(" In either 2d or 3d you use a cross product to generate them for two points or 3 to get a perpendicular point. \n which you normalize.");
            timedMsg.Add(" Cross products are heavily used in matrices.\n  were each vector in it is perpendicular to the others.");
            timedMsg.Add(" Forward Right and Up each is a cross of the other two. \n This applys in 3d but similariily in 2d.");
            timedMsg.Add(" That is called orthanormalization and it is seen in our little demo above.");
            timedMsg.Add(" The relation is shown in how the two directions remain perpendicular as they move.");
            timedMsg.Add(" Both the cross and dot are used in Lighting calculations \n but the dot is critical the dot is cheap as well.");
            timedMsg.Add(" Hope this helps.");
            timedMsg.Add(" ");
            timedMsg.Add(" ");
            timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");
            //timedMsg.Add(" ");


        }
    }
}
