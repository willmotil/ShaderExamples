
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples //.ExampleSupportClasses
{

    public class CameraAndKeyboardControls
    {
        public Matrix view;
        public Matrix projection;

        public float speed = .25f;
        public float speed2 = .008f;
        public float fov = 0.85f;

        public Matrix cameraWorld = Matrix.Identity;
        public Vector3 cameraWorldPosition = new Vector3(0, 0, 500f);
        public Vector3 cameraForwardVector = Vector3.Forward;
        public Vector3 cameraUpVector = Vector3.Up;

        public void InitialView(GraphicsDevice device, Vector3 pos, Vector3 forward, Vector3 up)
        {
            cameraWorld = Matrix.CreateWorld(pos, forward, up);
            view = Matrix.Invert(cameraWorld);
            if (DiffuseLightEffectClass.effect != null)
                DiffuseLightEffectClass.View = view;
        }
        public void InitialView(GraphicsDevice device, Matrix camWorld)
        {
            cameraWorld = camWorld;
            view = Matrix.Invert(cameraWorld);
            if (DiffuseLightEffectClass.effect != null)
                DiffuseLightEffectClass.View = view;
        }
        public void InitialView(GraphicsDevice device)
        {
            cameraWorldPosition.Z = MgMathExtras.GetRequisitePerspectiveSpriteBatchAlignmentZdistance(device, fov);
            cameraWorld = Matrix.CreateWorld(cameraWorldPosition, Vector3.Zero - cameraWorldPosition, cameraUpVector);
            view = Matrix.Invert(cameraWorld);
            if (DiffuseLightEffectClass.effect != null)
                DiffuseLightEffectClass.View = view;
        }

        public void UpdateProjection(GraphicsDevice device)
        {
            projection = Matrix.CreatePerspectiveFieldOfView(fov, device.Viewport.AspectRatio, 1f, 10000f);
            if (DiffuseLightEffectClass.effect != null)
                DiffuseLightEffectClass.Projection = projection;
        }

        public void Update(GameTime gameTime)
        {
            // Use the arrow keys to alter the camera position.
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                cameraWorld.Translation += cameraWorld.Right * -speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                cameraWorld.Translation += cameraWorld.Right * +speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                cameraWorld.Translation += cameraWorld.Up * +speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                cameraWorld.Translation += cameraWorld.Up * -speed;
            if (Keyboard.GetState().IsKeyDown(Keys.E))
                cameraWorld.Translation += cameraWorld.Forward * speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
                cameraWorld.Translation += cameraWorld.Forward * -speed;

            // Use wasd to alter the lookat direction.
            var t = cameraWorld.Translation;
            cameraWorld.Translation = Vector3.Zero;
            // 
            if (Keyboard.GetState().IsKeyDown(Keys.D))
                cameraWorld *= Matrix.CreateFromAxisAngle(cameraWorld.Up, -speed2);
            if (Keyboard.GetState().IsKeyDown(Keys.A))
                cameraWorld *= Matrix.CreateFromAxisAngle(cameraWorld.Up, speed2);
            if (Keyboard.GetState().IsKeyDown(Keys.S))
                cameraWorld *= Matrix.CreateFromAxisAngle(cameraWorld.Right, -speed2);
            if (Keyboard.GetState().IsKeyDown(Keys.W))
                cameraWorld *= Matrix.CreateFromAxisAngle(cameraWorld.Right, speed2);
            cameraWorld.Translation = t;

            //// Use the Z and C keys to rotate the camera.
            //if (Keyboard.GetState().IsKeyDown(Keys.Z))
            //    ____ += speed * .01f;
            //if (Keyboard.GetState().IsKeyDown(Keys.C))
            //    ____ -= speed * .01f;

            // Set the view matrix.
            cameraWorld = Matrix.CreateWorld(cameraWorld.Translation, cameraWorld.Forward, cameraWorld.Up);
            view = Matrix.Invert(cameraWorld);
        }
    }
}
