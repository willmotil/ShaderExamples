//using System;
//using System.Collections.Generic;
//using System.Text;
using Microsoft.Xna.Framework.Input;


namespace Microsoft.Xna.Framework
{
    // mouse keyboard and text

    /// <summary>
    /// last updated 2021 cleaned it up finally after years and years.
    /// </summary>
    public static class MouseHelper
    {
        #region private methods or variables

        private static Vector2 ScreenSize { get; set; }
        private static Vector2 ScreenSizeMultiplier { get; set; }

        private static int mouseWheelValue = 0;

        private static int mouseOldWheelValue = 0;

        private static MouseState mouseState;

        #endregion


        #region Left ______________________________

        /// <summary>
        /// is left being pressed now
        /// </summary>
        public static bool IsLeftDown = false;
        /// <summary>
        /// has left been held down now for more then one frame.
        /// </summary>
        public static bool IsLeftHeld = false;
        /// <summary>
        /// is clicked before is held is triggered aka the first frame.
        /// </summary>
        public static bool IsLeftClicked = false;
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

        #endregion

        #region Right ______________________________

        /// <summary>
        /// is right being pressed now
        /// </summary>
        public static bool IsRightDown = false;
        /// <summary>
        /// has right been held down for more then one frame
        /// </summary>
        public static bool IsRightHeld = false;
        /// <summary>
        /// is clicked before is held is triggered aka the first frame.
        /// </summary>
        public static bool IsRightClicked = false;
        /// <summary>
        /// right is true only in one single frame is the mouse just released
        /// </summary>
        public static bool IsRightJustReleased = false;
        /// <summary>
        /// has the right mouse been dragged
        /// </summary>
        public static bool IsRightDragged = false;
        /// <summary>
        /// dragged rectangle.
        /// </summary>
        public static Rectangle RightDragRectangle = Rectangle.Empty;
        /// <summary>
        /// right last position pressed while useing left mouse button
        /// </summary>
        public static Vector2 LastRightPressedAt;
        /// <summary>
        /// right last position draged from before release while useing left mouse button
        /// </summary>
        public static Vector2 LastRightDragReleased;
        /// <summary>
        /// Gets the direction and magnitude of left drag press to drag released
        /// </summary>
        public static Vector2 GetRightDragVector()
        {
            return LastRightDragReleased - LastRightPressedAt;
        }

        #endregion


        #region mouse position and wheel public methods or variables


        public static Point Pos = new Point(-1, -1);

        public static int X { get { return Pos.X; } }

        public static int Y { get { return Pos.Y; } }

        /// <summary>
        /// This is calculated as the mouse position in relation to the size of the screens width height. 
        /// </summary>
        public static Vector2 VirtualPos { get { return Pos.ToVector2() * ScreenSizeMultiplier; } }

        /// <summary>
        /// <para>this can be used instead of the bool methods</para> 
        /// <para>+1 if the wheel is getting rolled upwards</para>
        /// <para>-1 if the wheel is getting rolled downwards</para>
        /// <para>and 0 when wheel is not being rolled</para>
        /// </summary>
        public static int MouseWheelUpOrDown { get; set; } = 0;// if its +1 mouse scoll wheel got moved up if its -1 it got scrolled down

        /// <summary>
        /// returns true if the wheel is being wheeled up
        /// </summary>
        public static bool IsWheelingUp
        {
            get{   return (MouseWheelUpOrDown > 0) ? true : false;   }
        }
        /// <summary>
        /// returns true if the wheel is wheeling down
        /// </summary>
        public static bool IsWheelingDown
        {
            get{    return (MouseWheelUpOrDown < 0) ? true : false;    }
        }
        /// <summary>
        /// if the mouse wheel is not being wheeled up or down we return true
        /// </summary>
        public static bool IsWheelAtRest
        {
            get{    return (MouseWheelUpOrDown == 0) ? true : false;   }
        }

        #endregion


        public static void ChangeWindowSize(Point size)
        {
            ScreenSize = new Vector2(size.X, size.Y);
            ScreenSizeMultiplier = Vector2.One / ScreenSize;
        }

        public static void Update(Point windowSize)
        {
            ChangeWindowSize(windowSize);
            Update();
        }

        /// <summary>
        /// The logic function here is multi pass iteration dependant. ( we dont time things here we rely on frame pass logic over multiple frames )
        /// this fuction ONLY needs to and should be called 1 time per frame more then that will mess up the tracking of the mousejustreleased value
        /// </summary>
        public static void Update()
        {
            mouseState = Mouse.GetState();
            Pos = mouseState.Position;

            // mouse wheel

            MouseWheelUpOrDown = 0;// 0 is the state of no action on the wheel
            mouseWheelValue = mouseState.ScrollWheelValue;
            //
            if (mouseWheelValue < mouseOldWheelValue)
            {
                MouseWheelUpOrDown = -1;
                mouseOldWheelValue = mouseWheelValue;
            }
            else
            {
                if (mouseWheelValue > mouseOldWheelValue)
                {
                    MouseWheelUpOrDown = +1;
                    mouseOldWheelValue = mouseWheelValue;
                }
            }

            var state = mouseState.LeftButton;
            ProcessClick(ref state, ref IsLeftDown, ref IsLeftHeld, ref IsLeftClicked, ref IsLeftJustReleased, ref IsLeftDragged, ref LastLeftPressedAt, ref LastLeftDragReleased, ref LeftDragRectangle);

            state = mouseState.RightButton;
            ProcessClick(ref state, ref IsRightDown, ref IsRightHeld, ref IsRightClicked,  ref IsRightJustReleased, ref IsRightDragged, ref LastRightPressedAt, ref LastRightDragReleased, ref RightDragRectangle);
        }

        private static void ProcessClick(ref ButtonState state, ref bool IsDown, ref bool IsHeld, ref bool IsClicked, ref bool IsJustReleased, ref bool IsDragged,  ref Vector2 LastPressedAt, ref Vector2 LastDragReleased, ref Rectangle DragRectangle)
        {
            // Process when carrying out the left click of the mouse
            //_______________________________________________
            if (state == ButtonState.Pressed)
            {
                // NOTE THESE ARE PLACED IN A SPECIFIC ORDER TO ENSURE THAT DRAG CLICK LOGIC IS CORRECT
                // note to self thanks Never muck it up  , (this is a composite multi frame logic function)
                // on the first pass the below will set the lastposmousepressed to the current position
                if (IsHeld == false)
                {
                    // this executes on the first pass and the second pass but not the third pass
                    IsClicked = true;
                    if (IsDown == true) // this condition will not execute untill the second pass thru the function
                    {
                        IsHeld = true; // now we mark it as being held on the second pass 
                        IsClicked = false;
                    }
                    else
                    {
                        // this executes on the first pass only
                        LastPressedAt.X = X; // we save the click of the first point of holding 
                        LastPressedAt.Y = Y; // so when released we know were we draged from
                    }
                }
                // set at the end of but still in the first pass
                IsDown = true;// we SET this after the call to is leftbutton pressed now to ensure next pass is active
            }
            else
            {
                // mouse itself is no longer registering the button pressed so.. toggle held and button pressed off
                IsDown = false;
                IsJustReleased = false; // added this so i can get a ... just now released value
                IsDragged = false;
                if (IsHeld == true)
                {
                    LastDragReleased.X = X;
                    LastDragReleased.Y = Y;
                    IsJustReleased = true; // this gets reset to zero on next pass its good for one frame
                    var dr = GetDragRectangle(LastPressedAt, LastDragReleased);
                    if (dr.Width != 0 || dr.Height != 0)
                    {
                        IsDragged = true;
                        DragRectangle = dr;
                    }
                }
                IsHeld = false;
            }
        }
        public static Rectangle GetDragRectangle(Vector2 LastPressedAt, Vector2 LastDragReleased)
        {
            var v = LastDragReleased - LastPressedAt;
            var r = new Rectangle((int)LastPressedAt.X, (int)LastPressedAt.Y, (int)(v.X), (int)(v.Y));
            if(r.Width < 0)
            {
                r.X = (int)LastDragReleased.X;
                r.Width = -r.Width;
            }
            if (r.Height < 0)
            {
                r.Y = (int)LastDragReleased.Y;
                r.Height = -r.Height;
            }
            return r;
        }
    }
}
