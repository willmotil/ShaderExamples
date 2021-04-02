
//using System;
//using System.Collections.Generic;
//using System.Text;
//using Microsoft.Xna.Framework;

using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    public static class MgExtensions
    {
        public static Texture2D Dot(this Texture2D t)
        {
            return MgDrawExt.dotRed;
        }
        public static Texture2D DotRed(this Texture2D t)
        {
            return MgDrawExt.dotRed;
        }
        public static Texture2D DotGreen(this Texture2D t)
        {
            return MgDrawExt.dotGreen;
        }
        public static Texture2D DotBlue(this Texture2D t)
        {
            return MgDrawExt.dotBlue;
        }
        public static Vector2 ToVector2(this Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }
        public static Vector3 ToVector3(this Vector2 v)
        {
            return new Vector3(v.X, v.Y, 0f);
        }
        public static Vector3 ToVector3(this Vector2 v, float z)
        {
            return new Vector3(v.X, v.Y, z);
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


        public static float Wrap(this float n, float min, float max)
        {
            if (n > max)
                n = min;
            if (n < min)
                n = max;
            return n;
        }
        public static int Wrap(this int n, int min, int max)
        {
            if (n > max)
                n = min;
            if (n < min)
                n = max;
            return n;
        }
        public static float Clamp(this float n, float min, float max)
        {
            if (n > max)
                n = max;
            if (n < min)
                n = min;
            return n;
        }
        public static int Clamp(this int n, int min, int max)
        {
            if (n > max)
                n = max;
            if (n < min)
                n = min;
            return n;
        }

        public static float DotProduct2D(this Vector2 A, Vector2 B)
        {
            return A.X * B.X + A.Y * B.Y;
        }

        public static Vector2 Cross2D(this Vector2 B, bool getCcw)
        {
            return getCcw ? new Vector2(-B.Y, +B.X) : new Vector2(+B.Y, -B.X);
        }

        public static float DirectionToRadians(this Vector2 v)
        {
            return (float)System.Math.Atan2(v.X, v.Y) * -1f;
        }

        public static float Atan2Xna(float difx, float dify, bool useSpriteBatchAtan2)
        {
            if (useSpriteBatchAtan2)
                return (float)System.Math.Atan2(difx, dify) * -1f;
            else
                return (float)System.Math.Atan2(difx, dify);
        }

        public static bool IsEven(this int c)
        {
            return ((c % 2) == 0);
        }

        public static float Remainder(this float n)
        {
            return n - (float)(int)(n);
        }

        public static int Sign(this int n)
        {
            return n < 0 ? -1 : 1;
        }

        public static float Sign(this float n)
        {
            return n < 0 ? -1f : 1f;
        }

        public static float RatioTo(this float a, float n)
        {
            return a / (a + n);
        }

        public static float SlopeTo(this float a, float n)
        {
            return a / n;
        }

        public static float CoEfficient(this float x)
        {
            return 1f / (x);
        }

        public static Vector2 Coefficent(this Vector2 v)
        {
            return new Vector2(1f / (v.X), 1f / (v.Y));
        }

        public static Vector2 AbsoluteCoefficent(this Vector2 v)
        {
            return new Vector2(1f / (v.X).Absolute(), 1f / (v.Y).Absolute());
        }

        public static int Absolute(this int n)
        {
            if (n < 0) 
                return -n;
            return n;
        }
        public static float Absolute(this float n)
        {
            if (n < 0) 
                return -n; 
            return n;
        }
        public static double Absolute(this double n)
        {
            if (n < 0) 
                return -n; 
            return n;
        }
        public static Vector2 Absolute(this Vector2 v)
        {
            if (v.X < 0f) { v.X = -v.X; }
            if (v.Y < 0f) { v.Y = -v.Y; }
            return v;
        }
        public static Vector3 Absolute(this Vector3 v)
        {
            if (v.X < 0f) { v.X = -v.X; }
            if (v.Y < 0f) { v.Y = -v.Y; }
            if (v.Z < 0f) { v.Z = -v.Z; }
            return v;
        }
        public static float AcosineOfTwoNormalizedVectors(this Vector2 A, Vector2 B)
        {
            var n = ((A.X * B.X) + (A.Y * B.Y));
            return n * n;
        }

        public static float AcosineOfTwoNormalizedVectors(this Vector3 A, Vector3 B)
        {
            var n = ((A.X * B.X) + (A.Y * B.Y) + (A.Z * B.Z));
            return n * n;
        }

        public static int Fibonacci(this int n)
        {
            int a = 0;
            int b = 1;
            // In N steps compute Fibonacci sequence iteratively.
            for (int i = 0; i < n; i++)
            {
                int temp = a;
                a = b;
                b = temp + b;
            }
            return a;
        }

        public static float Square(this float n)
        {
            return n * n;
        }

        public static double Square(this double n)
        {
            return n * n;
        }

        public static float SquareRoot(this float n)
        {
            return (float)(Math.Sqrt(n));
        }

        public static double SquareRoot(this double n)
        {
            return Math.Sqrt(n);
        }

        public static Vector2 SquareRootElements(this Vector2 A)
        {
            return new Vector2((float)Math.Sqrt(A.X), (float)Math.Sqrt(A.Y));
        }
        public static Vector3 SquareRootElements(this Vector3 A)
        {
            return new Vector3((float)Math.Sqrt(A.X), (float)Math.Sqrt(A.Y), (float)Math.Sqrt(A.Z));
        }

        public static float Pow(this float x, float y)
        {
            return (float)Math.Pow(x, y);
        }

        /// <summary>
        /// pow(x, y) == pow(x * x,  y/x)
        //  pow(x, y) == 1 / pow(x, -y)
        //  
        //  pow(x, y) == pow(x * x,  y/x) == 1 / pow(x, -y)
        /// </summary>
        public static double PowManual(this double a, double b)
        {
            int accuracy = 1000000;

            bool negExponent = b < 0;
            b = Math.Abs(b);
            double accuracy2 = 1.0 + 1.0 / accuracy;
            bool ansMoreThanA = (a > 1 && b > 1) || (a < 1 && b < 1);   // Example 0.5^2=0.25 so answer is lower than A.

            double total = Math.Log(a) * accuracy * b;

            double t = a;
            while (true)
            {
                double t2 = Math.Log(t) * accuracy;
                if ((ansMoreThanA && t2 > total) || (!ansMoreThanA && t2 < total)) break;
                if (ansMoreThanA) t *= accuracy2; else t /= accuracy2;
            }
            if (negExponent) t = 1 / t;
            return t;
        }

        public static Vector2 SinCos(this Vector2 directionVector)
        {
            return Vector2.Normalize(directionVector);
        }

        /// <summary>
        /// Given a distance from a point and a direction we might obtain points on a unit length circle in relation.
        /// this functions returns the equivillent points on a square that contains that circle for a given vector.
        /// </summary>
        public static Vector2 OuterSquareSinCosVector(this Vector2 directionVector)
        {
            float d = directionVector.Length();
            var n = directionVector / d;
            var rate = new Vector2((n.X < 0) ? -(1 / (n.X)) : (1 / (n.X)), (n.Y < 0) ? -(1 / (n.Y)) : (1 / (n.Y)));
            var s = new Vector2((n.X < 0) ? -1 : 1, (n.Y < 0) ? -1 : 1);
            if (n.X * n.X > .5f)
                return new Vector2(s.X * d, n.Y * rate.X * d);
            else
                return new Vector2(n.X * rate.Y * d, s.Y * d);
        }

        /// <summary>
        /// the asin a cosine a given vector scaled by its distance.
        /// </summary>
        public static Vector2 InnerSquareAsinACosVector(this Vector2 directionVector)
        {
            float d = directionVector.Length();
            var n = directionVector / d;
            var s = new Vector2((n.X < 0) ? -1 : 1, (n.Y < 0) ? -1 : 1);
            return n * n * s * d;
        }

        /// <summary>
        /// Given a square that encompases a circle to find a point on it corresponding to a given vector.
        /// </summary>
        public static Vector2 OuterSquareAsinACosVector(this Vector2 directionVector)
        {
            float d = directionVector.Length();
            var n = directionVector / d;
            var a = n * n;
            var rate = new Vector2((a.X < 0) ? -(1 / (a.X)) : (1 / (a.X)), (a.Y < 0) ? -(1 / (a.Y)) : (1 / (a.Y)));
            var s = new Vector2((n.X < 0) ? -1 : 1, (n.Y < 0) ? -1 : 1);
            if (a.X > a.Y)
                return new Vector2(s.X * d, d * rate.X * a.Y * s.Y);
            else
                return new Vector2(a.X * s.X * rate.Y * d, d * s.Y);
        }


        public static Vector2 VirtualScreenCoords(this Vector2 v, GraphicsDevice gd)
        {
            return v / gd.Viewport.Bounds.Size.ToVector2();
        }

        public static bool IsKeyDown(this Keys key)
        {
            if (Keyboard.GetState().IsKeyDown(key))
                return true;
            else
                return false;
        }

        public static bool IsKeyPressedWithDelay(this Keys key, GameTime gameTime)
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

        public static Vector3 RotatePointAboutZaxis(Vector3 p, double q)
        {
            //x' = x*cos s - y*sin s;   y' = x*sin s + y*cos s;   z' = z;
            return new Vector3((float)(p.X * Math.Cos(q) - p.Y * Math.Sin(q)), (float)(p.X * Math.Sin(q) + p.Y * Math.Cos(q)), p.Z);
        }

        public static byte[] Compress(this byte[] rawByteData)
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

        static byte[] DeCompress(this byte[] gzipCompressedByteData)
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

        /// <summary>
        /// Bitmath bit check.
        /// </summary>
        public static bool IsIntBitOn(this int inValue, int bitIndexToTest)
        {
            return ((inValue & (1 << (bitIndexToTest))) > 0) ? true : false;
        }

        /// <summary>
        /// Bitmath Valid bit indexs are 0 to 31 and setBitOn is true or false for off.
        /// </summary>
        public static int SetBitPosition(this int value, int bitIndexToSet, bool setBitOn)
        {
            int bitvalue = (1 << (bitIndexToSet));
            bool isCurrentlyOn = (value & bitvalue) > 0; // is it on already.
            if (setBitOn == false && isCurrentlyOn == true)
                value = value ^ bitvalue; // turn it off.
            if (setBitOn == true && isCurrentlyOn == false)
                value = value | bitvalue; // turn it on.
            return value;
        }

        public static string ToWellFormatedBinaryString(this int inValue)
        {
            string result = "{ ";
            int spaceEvery = 8;
            for (int bitIndex = 0; bitIndex < 32; bitIndex++)
            {
                result += ((inValue & (1 << (bitIndex))) > 0) ? '1' : '0';

                if (bitIndex < 31)
                {
                    // format spaces and stuff.
                    if (((bitIndex + 1) % spaceEvery) == 0)
                        result += "  ";
                    if (((bitIndex + 5) % spaceEvery) == 0)
                        result += '-';
                }
            }
            result += " }";
            return result;
        }

        public static string ToSpacedBinaryString(this int inValue)
        {
            string result = "{ ";
            for (int bitIndex = 0; bitIndex < 32; bitIndex++)
            {
                result += ((inValue & (1 << (bitIndex))) > 0) ? '1' : '0';
                if (bitIndex < 31)
                {
                    if (((bitIndex + 1) % 8) == 0) // add a space
                        result += "  ";
                }
            }
            result += " }";
            return result;
        }

        public static string ToBinaryString(this int inValue)
        {
            string result = "";
            for (int bitIndex = 0; bitIndex < 32; bitIndex++)
                result += ((inValue & (1 << (bitIndex))) > 0) ? '1' : '0';
            return result;
        }

        public static string GetListingOfSupportedDisplayModesToString(this GraphicsDevice gd)
        {
            string msg = "";
            msg += ("\n" + " list supported Display modes");
            msg += ("  Current Mode " + gd.Adapter.CurrentDisplayMode + "\n");
            int counter = 0;
            counter = 0;
            foreach (DisplayMode dm in gd.Adapter.SupportedDisplayModes) // we can alternately loop GraphicsAdapter.DefaultAdapter.SupportedDisplayModes. this is typically the current adapter.
            {
                msg += (
                    "\n" +
                    "   DisplayMode[" + counter.ToString() + "] " +
                    "   Width:" + dm.Width.ToString() + " Height:" + dm.Height.ToString() +
                    "   AspectRatio " + dm.AspectRatio.ToString() +
                    "   SurfaceFormat " + dm.Format.ToString() +
                    "\n"
                    );
                //+" RefreshRate " + dm.RefreshRate.ToString()  //in monogame but not in xna 4.0 that's required for arm i think
                counter++;
            }
            return msg;
        }

        public static string VectorToString(this Vector4 v, string message)
        {
            string f = "+####0.000;-####0.000";
            return "\n " + message + "  " + v.X.ToString(f) + "  " + v.Y.ToString(f) + "  " + v.Z.ToString(f) + "  " + v.W.ToString(f);
        }
        public static string VectorToString(this Vector3 v, string message)
        {
            string f = "+####0.000;-####0.000";
            return "\n " + message + "  " + v.X.ToString(f) + "  " + v.Y.ToString(f) + "  " + v.Z.ToString(f);
        }
        public static string VectorToString(this Vector2 v, string message)
        {
            string f = "+####0.000;-####0.000";
            return "\n " + message + "  " + v.X.ToString(f) + "  " + v.Y.ToString(f);
        }
        public static string VectorToString(this Vector4 v)
        {
            string f = "+####0.000;-####0.000";
            return " " + "  " + v.X.ToString(f) + "  " + v.Y.ToString(f) + "  " + v.Z.ToString(f) + "  " + v.W.ToString(f);
        }
        public static string VectorToString(this Vector3 v)
        {
            string f = "+####0.000;-####0.000";
            return " " + "  " + v.X.ToString(f) + "  " + v.Y.ToString(f) + "  " + v.Z.ToString(f);
        }
        public static string VectorToString(this Vector2 v)
        {
            string f = "+####0.000;-####0.000";
            return " " + "  " + v.X.ToString(f) + "  " + v.Y.ToString(f);
        }

        ///// <summary>
        ///// Display matrix.
        ///// </summary>
        public static string DisplayMatrix(this Matrix m, string name)
        {
            string f0 = "{0,12:+####0.000;-####0.000;+####0.000}";
            string f1 = "{1,12:+####0.000;-####0.000;+####0.000}";
            string f2 = "{2,12:+####0.000;-####0.000;+####0.000}";
            string f3 = "{3,12:+####0.000;-####0.000;+####0.000}";
            return name +=
                "\n " + String.Format(f0 + f1 + f2 + f3, m.M11, m.M12, m.M13, m.M14) + " " +
                "\n " + String.Format(f0 + f1 + f2 + f3, m.M21, m.M22, m.M23, m.M24) + " " +
                "\n " + String.Format(f0 + f1 + f2 + f3, m.M31, m.M32, m.M33, m.M34) + " " +
                "\n " + String.Format(f0 + f1 + f2 + f3, m.M41, m.M42, m.M43, m.M44) + " " +
                ""
                ;
        }

    }
}
