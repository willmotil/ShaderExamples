using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Assorted functions for support.
    /// </summary>
    public static class MgHelpers
    {
        public static Matrix ViewMatrixForPerspectiveSpriteBatch(float width, float height, float _fov, Vector3 forward, Vector3 up)
        {
            var pos = new Vector3(width / 2, height / 2, -((1f / (float)Math.Tan(_fov / 2)) * (height / 2)));
            return Matrix.Invert(Matrix.CreateWorld(pos, forward + pos, up));
        }
        public static Matrix CameraMatrixForPerspectiveSpriteBatch(float width, float height, float _fov, Vector3 forward, Vector3 up)
        {
            var pos = new Vector3(width / 2, height / 2, -((1f / (float)Math.Tan(_fov / 2)) * (height / 2)));
            return Matrix.CreateWorld(pos, forward + pos, up);
        }
        public static Vector3 CameraPositionVectorForPerspectiveSpriteBatch(float width, float height, float _fov)
        {
            var pos = new Vector3(width / 2, height / 2, -((1f / (float)Math.Tan(_fov / 2)) * (height / 2)));
            return pos;
        }

        public static void CreatePerspectiveViewSpriteBatchAligned(GraphicsDevice device, Vector3 scollPositionOffset, float fieldOfView, float near, float far, out Matrix cameraWorld, out Matrix projection)
        {
            var dist = -((1f / (float)Math.Tan(fieldOfView / 2)) * (device.Viewport.Height / 2));
            var pos = new Vector3(device.Viewport.Width / 2, device.Viewport.Height / 2, dist) + scollPositionOffset;
            var target = new Vector3(0, 0, 1) + pos;
            cameraWorld = Matrix.CreateWorld(pos, target - pos, Vector3.Down);
            projection = CreateInfinitePerspectiveFieldOfViewRHLH(fieldOfView, device.Viewport.AspectRatio, near, far, true);
        }

        public static void CreateOrthographicViewSpriteBatchAligned(GraphicsDevice device, Vector3 scollPositionOffset, bool inverseOrthoDirection, out Matrix cameraWorld, out Matrix projection)
        {
            float forwardDepthDirection = 1f;
            if (inverseOrthoDirection)
                forwardDepthDirection = -1f;
            cameraWorld = Matrix.CreateWorld(scollPositionOffset, new Vector3(0, 0, 1), Vector3.Down);
            projection = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, -device.Viewport.Height, 0, forwardDepthDirection * 0, forwardDepthDirection * 1f);
        }

        public static float GetRequisitePerspectiveSpriteBatchAlignmentZdistance(GraphicsDevice device, float fieldOfView)
        {
            var dist = -((1f / (float)Math.Tan(fieldOfView / 2)) * (device.Viewport.Height / 2));
            //var pos = new Vector3(device.Viewport.Width / 2, device.Viewport.Height / 2, dist);
            return dist;
        }

        /// <summary>
        /// More complete capability wise perspectfov.
        /// </summary>
        public static Matrix CreateInfinitePerspectiveFieldOfViewRHLH(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, bool isRightHanded)
        {
            /* RH
             m11= xscale           m12= 0                 m13= 0                  m14=  0
             m21= 0                  m22= yscale          m23= 0                  m24= 0
             m31= 0                  0                          m33= f/(f-n) ~        m34= -1 ~
             m41= 0                  m42= 0                m43= n*f/(n-f) ~     m44= 0  
             where:
             yScale = cot(fovY/2)
             xScale = yScale / aspect ratio
           */
            if ((fieldOfView <= 0f) || (fieldOfView >= 3.141593f)) { throw new ArgumentException("fieldOfView <= 0 or >= PI"); }

            Matrix result = new Matrix();
            float yscale = 1f / ((float)Math.Tan((double)(fieldOfView * 0.5f)));
            float xscale = yscale / aspectRatio;
            var negFarRange = float.IsPositiveInfinity(farPlaneDistance) ? -1.0f : farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M11 = xscale;
            result.M12 = result.M13 = result.M14 = 0;
            result.M22 = yscale;
            result.M21 = result.M23 = result.M24 = 0;
            result.M31 = result.M32 = 0f;
            if (isRightHanded)
            {
                result.M33 = negFarRange;
                result.M34 = -1;
                result.M43 = nearPlaneDistance * negFarRange;
            }
            else
            {
                result.M33 = negFarRange;
                result.M34 = 1;
                result.M43 = -nearPlaneDistance * negFarRange;
            }
            result.M41 = result.M42 = result.M44 = 0;
            return result;
        }

        /// <summary>
        /// This returns a perspective projection matrix suitable for a rendertarget cube
        /// </summary>
        public static Matrix GetRenderTargetCubeProjectionMatrix(float near, float far)
        {
            return Matrix.CreatePerspectiveFieldOfView((float)MathHelper.Pi * .5f, 1, near, far);
        }

        /// <summary>
        /// This returns a matrix suitable for a render target cube.
        /// </summary>
        public static Matrix CreateCubeFaceLookAtViewMatrix(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            var vector = Vector3.Normalize(cameraPosition - cameraTarget);
            var vector2 = -Vector3.Normalize(Vector3.Cross(cameraUpVector, vector));
            var vector3 = Vector3.Cross(-vector, vector2);
            Matrix result = Matrix.Identity;
            result.M11 = vector2.X;
            result.M12 = vector3.X;
            result.M13 = vector.X;
            result.M14 = 0f;
            result.M21 = vector2.Y;
            result.M22 = vector3.Y;
            result.M23 = vector.Y;
            result.M24 = 0f;
            result.M31 = vector2.Z;
            result.M32 = vector3.Z;
            result.M33 = vector.Z;
            result.M34 = 0f;
            result.M41 = -Vector3.Dot(vector2, cameraPosition);
            result.M42 = -Vector3.Dot(vector3, cameraPosition);
            result.M43 = -Vector3.Dot(vector, cameraPosition);
            result.M44 = 1f;
            return result;
        }

        public static Vector2 GetPointAtTimeOn2ndDegreePolynominalCurve(Vector2 A, Vector2 B, Vector2 C, float t)
        {
            float i = 1.0f - t;
            float plotX = 0;
            float plotY = 0;
            plotX = (float)(A.X * 1 * (i * i) + B.X * 2 * (i * t) + C.X * 1 * (t * t));
            plotY = (float)(A.Y * 1 * (i * i) + B.Y * 2 * (i * t) + C.Y * 1 * (t * t));
            return new Vector2(plotX, plotY);
        }

        public static Vector3 BiCubicSubdivision(Vector3 a0, Vector3 a1, Vector3 a2, Vector3 a3, float time)
        {
            return (((((a3 - a2) * time + a2) - ((a2 - a1) * time + a1)) * time + ((a2 - a1) * time + a1)) - ((((a2 - a1) * time + a1) - ((a1 - a0) * time + a0)) * time + ((a1 - a0) * time + a0))) * time + ((((a2 - a1) * time + a1) - ((a1 - a0) * time + a0)) * time + ((a1 - a0) * time + a0));
        }

        public static Vector3 Bezier(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float time)
        {
            float t = time;
            float t2 = t * t;
            float t3 = t2 * t;
            float i = 1f - t;
            float i2 = i * i;
            float i3 = i2 * i;

            return
                (i3) * 1f * v0 +
                (i2 * t) * 3f * v1 +
                (i * t2) * 3f * v2 +
                (t3) * 1f * v3
                ;
        }
        

        /// <summary>
        /// This is a generalized 3rd degree polynominal.
        /// With the following attribues. It is a non uniform segment with curvature along that segment affected by the weights.
        /// The curve intersects all the input vectors more importantly vectors 1 and 2 as the time traces thru only these. 
        /// This function relys on the GetIdealTangentVector function.
        /// This functions usage is gear towards piecewise curvature thru n points by segment.
        /// </summary>
        /// <returns></returns>
        private static Vector3 GetSegmentV1V2BezierTangentalWeightedPointThruVector(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float vector1Weight, float vector2Weight, float time)
        {
            Vector3 p0 = v0; Vector3 p1 = v1; Vector3 p2 = v2; Vector3 p3 = v3;

            var segmentDistance = Vector3.Distance(v2, v1) * 0.35355339f;

            var n1 = Vector3.Normalize(GetIdealTangentVector(v0, v1, v2));
            p1 = v1 + n1 * segmentDistance * vector1Weight;
            p0 = v1;

            var n2 = Vector3.Normalize(GetIdealTangentVector(v3, v2, v1));
            p2 = v2 + n2 * segmentDistance * vector2Weight;
            p3 = v2;

            float t = time;
            float t2 = t * t;
            float t3 = t2 * t;
            float i = 1f - t;
            float i2 = i * i;
            float i3 = i2 * i;

            Vector3 result =
                (i3) * 1f * p0 +
                (i2 * t) * 3f * p1 +
                (i * t2) * 3f * p2 +
                (t3) * 1f * p3;

            return result;
        }

        public static Vector3 GetIdealTangentVector(Vector3 a, Vector3 b, Vector3 c)
        {
            float disa = Vector3.Distance(a, b);
            float ratioa = disa / (disa + Vector3.Distance(b, c));
            var result = (((c - b) * ratioa) + b) - (((b - a) * ratioa) + a);
            // prevent nan later on.
            if (result == Vector3.Zero)
                result = c - a;
            return result;
        }

        public static float Atan2Xna(float difx, float dify, bool useSpriteBatchAtan2)
        {
            if (useSpriteBatchAtan2)
                return (float)System.Math.Atan2(difx, dify) * -1f;
            else
                return (float)System.Math.Atan2(difx, dify);
        }

        public static float Power(int baseVal, int exponentVal)
        {
            float result = 0;
            for (float exponent = exponentVal; exponent > 0; exponent--)
            {
                result = result * baseVal;
            }
            return result;
        }

        public static Vector3 MidPoint(Vector3 a, Vector3 b)
        {
            return (a + b) / 2;
        }
        public static Vector3 MidPoint(Vector3 a, Vector3 b, Vector3 c)
        {
            return (a + b + c) / 3;
        }

        public static float EnsureWrapInRange(float n, float min, float max)
        {
            if (n > max)
                n = min;
            if (n < min)
                n = max;
            return n;
        }
        public static int EnsureWrapInRange(int n, int min, int max)
        {
            if (n > max)
                n = min;
            if (n < min)
                n = max;
            return n;
        }
        public static float EnsureClampInRange(float n, float min, float max)
        {
            if (n > max)
                n = max;
            if (n < min)
                n = min;
            return n;
        }
        public static int EnsureClampInRange(int n, int min, int max)
        {
            if (n > max)
                n = max;
            if (n < min)
                n = min;
            return n;
        }
    }
}


/*
 
         /// <summary>
        /// simplifyed writeExceptionMsg
        /// </summary>
        /// <param name="errmsg"></param>
        public static void WriteExceptionStackFramesToFile(string errmsg, int numberOfFramesToTrace)
        {
            string log_tempstring = "\n";
            log_tempstring += ("\n Exception Thrown");
            log_tempstring += ("\n _______________________________________________________");
            log_tempstring += ("\n " + errmsg);
            log_tempstring += ("\n _______________________________________________________");
            log_tempstring += ("\n StackTrace As Follows \n");
            try
            {
                // Create a StackTrace that captures filename,
                // linepieces number and column information.
                StackTrace st = new StackTrace(1, true);
                int count = st.FrameCount;
                if (numberOfFramesToTrace > count) { numberOfFramesToTrace = count; }
                //
                for (int i = 0; i < numberOfFramesToTrace; i++)
                {
                    StackFrame sf = st.GetFrame(i);

                    log_tempstring += " \n  stack # " + i.ToString();
                    log_tempstring += "  File: " + Path.GetFileNameWithoutExtension(sf.GetFileName());
                    log_tempstring += "  Method: " + sf.GetMethod().ToString();
                    log_tempstring += "  Line: " + sf.GetFileLineNumber().ToString();
                    log_tempstring += "  Column: " + sf.GetFileColumnNumber().ToString();
                    // output the full stack frame
                }
                throw new Exception();
            }
            catch (Exception e)
            {
                string fullpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ErrorLog.txt");
                File.WriteAllText(fullpath, log_tempstring);
                Process.Start(fullpath);
                throw e;
            }
        }

 */
