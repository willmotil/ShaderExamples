
//using System;
//using System.Collections.Generic;
//using System.Text;
//using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    public static class MgExt
    {
        public static Vector2 ToVector2(this Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }
        public static Vector3 ToVector3(this Vector2 v)
        {
            return new Vector3(v.X, v.Y, 0f);
        }
        public static Vector3 ToVector3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
        public static Vector4 ToVector4(this Vector3 v)
        {
            return new Vector4(v.X, v.Y, v.Z, 1f);
        }
        public static Vector4 ToVector4(this Vector3 v, float w)
        {
            return new Vector4(v.X, v.Y, v.Z, w);
        }

        public static float EnsureWrapInRange(this float n, float min, float max)
        {
            if (n > max)
                n = min;
            if (n < min)
                n = max;
            return n;
        }
        public static int EnsureWrapInRange(this int n, int min, int max)
        {
            if (n > max)
                n = min;
            if (n < min)
                n = max;
            return n;
        }
        public static float EnsureClampInRange(this float n, float min, float max)
        {
            if (n > max)
                n = max;
            if (n < min)
                n = min;
            return n;
        }
        public static int EnsureClampInRange(this int n, int min, int max)
        {
            if (n > max)
                n = max;
            if (n < min)
                n = min;
            return n;
        }

        public static Vector2 VirtualScreenCoords(this Vector2 v, GraphicsDevice gd)
        {
            return v / gd.Viewport.Bounds.Size.ToVector2();
        }


        public static bool IsKeyPressedWithDelay(this Keys key, GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(key) && IsUnDelayed(gameTime))
                return true;
            else
                return false;
        }
        public static bool IsKeyPressedWithDelay(this GameTime gameTime, Keys key)
        {
            if (Keyboard.GetState().IsKeyDown(key) && IsUnDelayed(gameTime))
                return true;
            else
                return false;
        }

        public static float DelayTime { get; set; } = 0.25f;
        static float delay = 0f;
        public static bool IsUnDelayed(this GameTime gametime, float newDelayAmount)
        {
            DelayTime = newDelayAmount;
            return gametime.IsUnDelayed();
        }
        public static bool IsUnDelayed(this GameTime gametime)
        {
            if (delay < 0)
            {
                delay = DelayTime;
                return true;
            }
            else
            {
                delay -= (float)gametime.ElapsedGameTime.TotalSeconds;
                return false;
            }
        }

        /// <summary>
        /// Allows a position to be inflected against a unit normal and any position on its surface plane. 
        /// This is useful in mirroring positions across a plane 
        /// When for example, you want to find in a water reflection cameras inflected position.
        /// </summary>
        public static Vector3 InflectPositionFromPlane(this Vector3 theCameraPostion, Vector3 thePlanesSurfaceNormal, Vector3 anyPositionOnThatSurfacePlane)
        {
            // the dot product also gives the length. 
            // when placed againsts a unit normal so any unit n * a distance is the distance to that normals plane no matter the normals direction. 
            // i didn't know that relation was so straight forward.
            float camToPlaneDist = Vector3.Dot(thePlanesSurfaceNormal, theCameraPostion - anyPositionOnThatSurfacePlane);
            return theCameraPostion - thePlanesSurfaceNormal * camToPlaneDist * 2;
        }

        /// <summary>
        /// Takes a screen position Point and reurns a ray in world space using viewport . unproject(...) , 
        /// The near and far are the z plane depth values used and found in your projection matrix.
        /// </summary>
        public static Ray GetScreenPointAsRayInto3dWorld(this Point screenPosition, Matrix projectionMatrix, Matrix viewMatrix, Matrix world, float near, float far, GraphicsDevice device)
        {
            return GetScreenVector2AsRayInto3dWorld(screenPosition.ToVector2(), projectionMatrix, viewMatrix, world, near, far, device);
        }

        /// <summary>
        /// Or not ?
        /// Takes a screen position Vector2 and reurns a ray in world space using viewport . unproject(...) , 
        /// The near and far are the z plane depth values used and found in your projection matrix.
        /// </summary>
        public static Ray GetScreenVector2AsRayInto3dWorld(this Vector2 screenPosition, Matrix projectionMatrix, Matrix viewMatrix, Matrix world, float near, float far, GraphicsDevice device)
        {
            Vector3 farScreenPoint = new Vector3(screenPosition.X, screenPosition.Y, far); // the projection matrice's far plane value.
            Vector3 nearScreenPoint = new Vector3(screenPosition.X, screenPosition.Y, near); // must be more then zero.
            Vector3 nearWorldPoint = device.Viewport.Unproject(nearScreenPoint, projectionMatrix, viewMatrix, world);
            Vector3 farWorldPoint = device.Viewport.Unproject(farScreenPoint, projectionMatrix, viewMatrix, world);
            Vector3 worldRaysNormal = Vector3.Normalize(farWorldPoint - nearWorldPoint);

            return new Ray(nearWorldPoint, worldRaysNormal);
        }

        /// <summary>
        /// Creates a world with a target.
        /// </summary>
        public static Matrix CreateWorldToTarget(this Matrix m, Vector3 position, Vector3 targetPosition, Vector3 up)
        {
            return Matrix.CreateWorld(position, targetPosition - position, up);
        }

        public static string VectorToString(this Vector4 v, string message)
        {
            string f = "+###0.0;-###0.0";
            return "\n " + message + "  " + v.X.ToString(f) + "  " + v.Y.ToString(f) + "  " + v.Z.ToString(f) + "  " + v.W.ToString(f);
        }
        public static string VectorToString(this Vector3 v, string message)
        {
            string f = "+###0.0;-###0.0";
            return "\n " + message + "  " + v.X.ToString(f) + "  " + v.Y.ToString(f) + "  " + v.Z.ToString(f);
        }
        public static string VectorToString(this Vector2 v, string message)
        {
            string f = "+###0.0;-###0.0";
            return "\n " + message + "  " + v.X.ToString(f) + "  " + v.Y.ToString(f);
        }
        public static string VectorToString(this Vector4 v)
        {
            string f = "+###0.0;-###0.0";
            return " " + "  " + v.X.ToString(f) + "  " + v.Y.ToString(f) + "  " + v.Z.ToString(f) + "  " + v.W.ToString(f);
        }
        public static string VectorToString(this Vector3 v)
        {
            string f = "+###0.0;-###0.0";
            return " " + "  " + v.X.ToString(f) + "  " + v.Y.ToString(f) + "  " + v.Z.ToString(f);
        }
        public static string VectorToString(this Vector2 v)
        {
            string f = "+###0.0;-###0.0";
            return " " + "  " + v.X.ToString(f) + "  " + v.Y.ToString(f);
        }

        /// <summary>
        /// Display matrix
        /// </summary>
        public static string DisplayMatrix(this Matrix m, string name)
        {
            string f = "##0.###"; //"+000.000;-000.000";
            return name +=
                "\n { " + m.M11.ToString(f) + ", " + m.M12.ToString(f) + ", " + m.M13.ToString(f) + ", " + m.M14.ToString(f) + " }" +
                "\n { " + m.M21.ToString(f) + ", " + m.M22.ToString(f) + ", " + m.M23.ToString(f) + ", " + m.M24.ToString(f) + " }" +
                "\n { " + m.M31.ToString(f) + ", " + m.M32.ToString(f) + ", " + m.M33.ToString(f) + ", " + m.M34.ToString(f) + " }" +
                "\n { " + m.M41.ToString(f) + ", " + m.M42.ToString(f) + ", " + m.M43.ToString(f) + ", " + m.M44.ToString(f) + " }";
        }
    }
}
