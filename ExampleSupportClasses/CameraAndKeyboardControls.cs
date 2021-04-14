
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples
{

    public class CameraAndKeyboardControls
    {
        public Matrix view;
        public Matrix projection;

        public float moveSpeed = 1.6f; //.25f;
        public float lookatSpeed = .024f; //.008f;
        public float fov = 0.85f;

        public Matrix cameraWorld = Matrix.Identity;
        private Vector3 cameraWorldPosition = new Vector3(0, 0, 500f);
        private Vector3 cameraForwardVector = Vector3.Forward;
        private Vector3 cameraUpVector = Vector3.Up;

        public bool IsUpFixed = false;
        public Vector3 FixedUpVector = Vector3.Up;

        /// <summary>
        /// the up vector will be overriden if IsUpFixed is on.
        /// </summary>
        public void InitialView(GraphicsDevice device, Vector3 pos, Vector3 forward, Vector3 up)
        {
            cameraWorld = Matrix.CreateWorld(pos, forward, up);
            view = Matrix.Invert(cameraWorld);
        }

        /// <summary>
        /// the up vector will be overriden if IsUpFixed is on.
        /// </summary>
        public void InitialView(GraphicsDevice device, Matrix camWorld)
        {
            cameraWorld = camWorld;
            view = Matrix.Invert(cameraWorld);
        }

        /// <summary>
        /// the up vector will be overriden if IsUpFixed is on.
        /// </summary>
        public void InitialView(GraphicsDevice device)
        {
            cameraWorldPosition.Z = MgMathExtras.GetRequisitePerspectiveSpriteBatchAlignmentZdistance(device, fov);
            cameraWorld = Matrix.CreateWorld(cameraWorldPosition, Vector3.Zero - cameraWorldPosition, cameraUpVector);
            view = Matrix.Invert(cameraWorld);
        }

        public void UpdateProjection(GraphicsDevice device, float fieldOfView)
        {
            fov = fieldOfView;
            projection = Matrix.CreatePerspectiveFieldOfView(fov, device.Viewport.AspectRatio, 1f, 10000f);
        }
        public void UpdateProjection(GraphicsDevice device)
        {
            projection = Matrix.CreatePerspectiveFieldOfView(fov, device.Viewport.AspectRatio, 1f, 10000f);
        }

        public void Update(GameTime gameTime)
        {
            // Use the arrow keys to alter the camera position.
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                cameraWorld.Translation += cameraWorld.Right * -moveSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                cameraWorld.Translation += cameraWorld.Right * +moveSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                cameraWorld.Translation += cameraWorld.Up * +moveSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                cameraWorld.Translation += cameraWorld.Up * -moveSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.E))
                cameraWorld.Translation += cameraWorld.Forward * moveSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
                cameraWorld.Translation += cameraWorld.Forward * -moveSpeed;

            // Use wasd to alter the lookat direction.
            var t = cameraWorld.Translation;
            cameraWorld.Translation = Vector3.Zero;
            var temp = cameraWorld;
            // 
            if (Keyboard.GetState().IsKeyDown(Keys.D))
                temp *= Matrix.CreateFromAxisAngle(cameraWorld.Up, -lookatSpeed);
            if (Keyboard.GetState().IsKeyDown(Keys.A))
                temp *= Matrix.CreateFromAxisAngle(cameraWorld.Up, lookatSpeed);
            if (Keyboard.GetState().IsKeyDown(Keys.S))
                temp *= Matrix.CreateFromAxisAngle(cameraWorld.Right, -lookatSpeed);
            if (Keyboard.GetState().IsKeyDown(Keys.W))
                temp *= Matrix.CreateFromAxisAngle(cameraWorld.Right, lookatSpeed);

            //// Use the Z and C keys to rotate the camera.
            //if (Keyboard.GetState().IsKeyDown(Keys.Z))
            //    ____ += speed * .01f;
            //if (Keyboard.GetState().IsKeyDown(Keys.C))
            //    ____ -= speed * .01f;

            if (IsUpFixed)
            {
                temp.Up = FixedUpVector;
                if (IsApproachingGimble(temp.Forward, Vector3.Up) == false)
                    cameraWorld = temp;
            }
            else
                cameraWorld = temp;
            cameraWorld.Translation = t;

            // Set the view matrix.
            cameraWorld = Matrix.CreateWorld(cameraWorld.Translation, cameraWorld.Forward, cameraWorld.Up);
            view = Matrix.Invert(cameraWorld);
        }

        public bool IsApproachingGimble(Vector3 forward, Vector3 up)
        {
            return Vector3.Dot(up, forward) > .98f || Vector3.Dot(up, forward) < -.98f;
        }

        public static Matrix CreateWorldFixedUp(Vector3 position, Vector3 forward)
        {
            Matrix ret;
            CreateWorldFixedUp(ref position, ref forward, out ret);
            return ret;
        }

        public static void CreateWorldFixedUp(ref Vector3 position, ref Vector3 forward, out Matrix result)
        {
            var up = Vector3.Up;

            Vector3 x, y, z;
            Vector3.Normalize(ref forward, out z);
            Vector3.Cross(ref forward, ref up, out x);
            Vector3.Cross(ref x, ref forward, out y);
            x.Normalize();
            y.Normalize();

            result = new Matrix();
            result.Right = x;
            result.Up = y;
            result.Forward = z;
            result.Translation = position;
            result.M44 = 1f;
        }

        string msg = "";
    }
}
