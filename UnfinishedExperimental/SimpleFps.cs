
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    public class SimpleFps
    {
        private double frames = 0;
        private double updates = 0;
        private double elapsed = 0;
        private double last = 0;
        private double now = 0;
        private double memNow = 0;
        private double memLast = 0;
        private double memLost = 0;
        private double memTotalLost = 0;
        private double memTotalLostPerSecond = 0;
        private double memTotalLostLastSecond = 0;
        private int numberOfCollects = 0;
        private int numberOfCollectsPerSecond = 0;
        private int numberOfCollectsLastSecond = 0;
        public double msgFrequency = .05f;
        public double secondsElapsed = 0f;
        public double secondsLast = 0f;
        public MgStringBuilder msg = "";


        /// <summary>
        /// The msgFrequency here is the reporting time to update the message.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            memNow = (double)GC.GetTotalMemory(false) * 0.0000009537;//0.000001d;
            if (memNow > memLast)
                memLast = memNow;
            if (memNow < memLast)
            {
                memLost = memLast - memNow;
                memTotalLostPerSecond += memLost;
                memTotalLost += memLost;
                memLast = memNow;
                numberOfCollects++;
            }

            now = gameTime.TotalGameTime.TotalSeconds;
            secondsElapsed = now - secondsLast;
            if (secondsElapsed > 1.0d)
            {
                memTotalLostLastSecond = memTotalLostPerSecond;
                memTotalLostPerSecond = 0;
                numberOfCollectsPerSecond = numberOfCollects - numberOfCollectsLastSecond;
                numberOfCollectsLastSecond = numberOfCollects;
                secondsLast = now;
            }

            elapsed = (double)(now - last);
            if (elapsed > msgFrequency)
            {
                msg.Clear();
                msg
                    .Append(" Time Running in Seconds: ").AppendTrim(gameTime.TotalGameTime.TotalSeconds)
                    .Append("\n Fps: ").AppendTrim(frames / elapsed)
                    .Append("\n")
                    .Append("\n Memory in (MB)...  ")
                    .Append("\n Now: ").AppendTrim(memNow)
                    .Append("\n Lost: ").AppendTrim(memLost)
                    .Append("\n TotalLost: ").AppendTrim(memTotalLost)
                    .Append("\n TotalLostPerSecond: ").AppendTrim(memTotalLostPerSecond)
                    .Append("\n Collections: ").Append(numberOfCollects)
                    .Append("\n CollectionsPerSecond: ").Append(numberOfCollectsPerSecond)
                    ;
                elapsed = 0;
                frames = 0;
                updates = 0;
                last = now;
            }
            updates++;
        }

        ///// <summary>
        ///// The msgFrequency here is the reporting time to update the message.
        ///// </summary>
        //public void Update(GameTime gameTime)
        //{
        //    memNow = (double)GC.GetTotalMemory(false) * 0.000001d;
        //    if (memNow > memLast)
        //        memLast = memNow;
        //    if (memNow < memLast)
        //    {
        //        memLost = memLast - memNow;
        //        memTotalLost += memLost;
        //        memLast = memNow;
        //        numberOfCollects++;
        //    }

        //    now = gameTime.TotalGameTime.TotalSeconds;
        //    secondsElapsed = now - secondsLast;
        //    if (secondsElapsed > 1.0d)
        //    {
        //        memTotalLostPerSecond = memTotalLost - memTotalLostLastSecond;
        //        memTotalLostLastSecond = memTotalLost;
        //        numberOfCollectsPerSecond = numberOfCollects - numberOfCollectsLastSecond;
        //        numberOfCollectsLastSecond = numberOfCollects;
        //        secondsLast = now;
        //    }

        //    elapsed = (double)(now - last);
        //    if (elapsed > msgFrequency)
        //    {
        //        msg.Clear();
        //        msg
        //            .Append(" Time Running in Seconds: ").AppendTrim(gameTime.TotalGameTime.TotalSeconds)
        //            .Append("\n Fps: ").AppendTrim(frames / elapsed)
        //            .Append("\n")
        //            .Append("\n Memory in (MB)...  ")
        //            .Append("\n Now: ").AppendTrim(memNow)
        //            .Append("\n Lost: ").AppendTrim(memLost)
        //            .Append("\n TotalLost: ").AppendTrim(memTotalLost)
        //            .Append("\n TotalLostPerSecond: ").AppendTrim(memTotalLostPerSecond)
        //            .Append("\n Collections: ").Append(numberOfCollects)
        //            .Append("\n CollectionsPerSecond: ").Append(numberOfCollectsPerSecond)
        //            ;
        //        elapsed = 0;
        //        frames = 0;
        //        updates = 0;
        //        last = now;
        //    }
        //    updates++;
        //}

        public void DrawFps(SpriteBatch spriteBatch, SpriteFont font, Vector2 fpsDisplayPosition, Color fpsTextColor)
        {
            spriteBatch.DrawString(font, msg, fpsDisplayPosition, fpsTextColor);
            frames++;
        }
    }
}
