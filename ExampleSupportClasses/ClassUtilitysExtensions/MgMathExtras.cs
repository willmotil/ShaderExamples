using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Assorted functions for support.
    /// Additional Resources found 
    /// https://repo.progsbase.com/repoviewer/no.inductive.idea10.programs/math/latest/
    /// https://repo.progsbase.com/repoviewer/
    /// https://developer.mozilla.org/en-US/docs/Games/Techniques/2D_collision_detection
    /// https://mathworld.wolfram.com/Euclidean.html
    /// https://mathopenref.com/
    /// </summary>
    public static class MgMathExtras
    {
        public const float PI2 = 6.28318530717f;
        public const float PI = 3.141592653589f;
        public const float PIover2 = 1.570796326794f;
        public const float PIover4 = 0.785398163f;
        public const float Ee = 2.718281828459f;
        public const float SQROOTOf2 = 1.41421356237f;
        public const float SQROOTofPoint5 = 0.7071067811f;
        public const float ONEhalf = 0.5f;
        public const float TORADIANS = 0.0174532925199f;
        public const float TODEGREES = 57.29577951316f;
        public const double Epsilon = 1e-10;


        public static float GetRequisitePerspectiveSpriteBatchAlignmentZdistance(GraphicsDevice device, float fieldOfView)
        {
            var dist = -((1f / (float)Math.Tan(fieldOfView / 2)) * (device.Viewport.Height / 2));
            return dist;
        }

        public static Vector3 CameraVectorForPerspectiveSpriteBatch(float width, float height, float _fov)
        {
            var pos = new Vector3(width / 2, height / 2, -((1f / (float)Math.Tan(_fov / 2)) * (height / 2)));
            return pos;
        }

        public static Matrix CreateCameraMatrixForPerspectiveSpriteBatch(float width, float height, float _fov, Vector3 forward, Vector3 up)
        {
            var pos = new Vector3(width / 2,  height / 2, -((1f / (float)Math.Tan(_fov / 2)) * (height / 2)));
            return Matrix.CreateWorld(pos, forward, up);
        }

        public static Matrix CreateViewMatrixForPerspectiveSpriteBatch(float width, float height, float _fov, Vector3 forward, Vector3 up)
        {
            var pos = new Vector3(width / 2, height / 2, -((1f / (float)Math.Tan(_fov / 2)) * (height / 2)));
            return Matrix.Invert(Matrix.CreateWorld(pos, forward, up));
        }

        public static void CreatePerspectiveViewSpriteBatchAligned(GraphicsDevice device, Vector3 scollPositionOffset, float fieldOfView, float near, float far, out Matrix cameraWorld, out Matrix projection)
        {
            var dist = -((1f / (float)Math.Tan(fieldOfView / 2)) * (device.Viewport.Height / 2));
            var pos = new Vector3(device.Viewport.Width / 2, device.Viewport.Height / 2, dist) + scollPositionOffset;
            cameraWorld = Matrix.CreateWorld(pos, Vector3.Backward, Vector3.Down);
            projection = CreateInfinitePerspectiveFieldOfViewRHLH(fieldOfView, device.Viewport.AspectRatio, near, far, true);
        }

        public static void CreateOrthographicViewSpriteBatchAligned(GraphicsDevice device, Vector3 scollPositionOffset, bool inverseOrthoDirection, out Matrix cameraWorld, out Matrix projection)
        {
            float forwardDepthDirection = 1f;
            if (inverseOrthoDirection)
                forwardDepthDirection = -1f;
            cameraWorld = Matrix.CreateWorld(scollPositionOffset, Vector3.Backward, Vector3.Down);
            projection = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, -device.Viewport.Height, 0, forwardDepthDirection * 0, forwardDepthDirection * 1f);
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

        public static Vector3 MidPoint(Vector3 a, Vector3 b)
        {
            return (a + b) / 2;
        }
        public static Vector3 MidPoint(Vector3 a, Vector3 b, Vector3 c)
        {
            return (a + b + c) / 3;
        }

        public static bool IsEven(int c)
        {
            return ((c % 2) == 0);
        }

        public static int Absolute(int n)
        {
            if (n < 0) { return -n; }
            return n;
        }
        public static float Absolute(float n)
        {
            if (n < 0) { return -n; }
            return n;
        }
        public static double Absolute(double n)
        {
            if (n < 0) { return -n; }
            return n;
        }

        public static Vector2 Absolute(Vector2 v)
        {
            if (v.X < 0f) { v.X = -v.X; }
            if (v.Y < 0f) { v.Y = -v.Y; }
            return v;
        }
        public static Vector3 Absolute(Vector3 v)
        {
            if (v.X < 0f) { v.X = -v.X; }
            if (v.Y < 0f) { v.Y = -v.Y; }
            if (v.Z < 0f) { v.Z = -v.Z; }
            return v;
        }
        public static float AcosineOfTwoVectors(Vector2 A, Vector2 B)
        {
            var n = ((A.X * B.X) + (A.Y * B.Y));
            return n * n;
        }

        public static float AcosineOfTwoVectors(Vector3 A, Vector3 B)
        {
            var n = ((A.X * B.X) + (A.Y * B.Y) + (A.Z * B.Z));
            return n * n;
        }

        public static float SquareRoot(float n)
        {
            return (float)(Math.Sqrt(n));
        }

        public static double SquareRoot(double n)
        {
            return Math.Sqrt(n);
        }

        public static Vector2 SquareRootElements(Vector2 A)
        {
            return new Vector2((float)Math.Sqrt(A.X), (float)Math.Sqrt(A.Y));
        }
        public static Vector3 SquareRootElements(Vector3 A)
        {
            return new Vector3((float)Math.Sqrt(A.X), (float)Math.Sqrt(A.Y), (float)Math.Sqrt(A.Z));
        }

        public static float DotProduct2d(Vector2 A, Vector2 B)
        {
            return A.X * B.X + A.Y * B.Y;
        }
        public static float DotProduct3d(Vector3 A, Vector3 B)
        {
            return (A.X * B.X) + (A.Y * B.Y) + (A.Z * B.Z);
        }

        public static Vector3 Cross3D(Vector3 A, Vector3 B)
        {
            return new Vector3(A.Y * B.Z - B.Y * A.Z, A.Z * B.X - B.Z * A.X, A.X * B.Y - B.X * A.Y);
        }

        public static Vector2 Cross2D(Vector2 B, bool getCcw)
        {
            return getCcw ? new Vector2(-B.Y, +B.X) : new Vector2(+B.Y, -B.X);
        }
        public static Vector2 Cross2D(Vector2 A, Vector2 B) // left
        {
            // return new Vector2(+B.Y + A.X, -B.X + A.Y); // Cw version.
            return new Vector2(-B.Y + A.X, +B.X + A.Y);
        }

        public static Vector2 Reflection2d(Vector2 n, Vector2 i)
        {
            return new Vector2
               (
                 2 * n.X * (n.X * i.X + n.Y * i.Y) - i.X,
                 2 * n.Y * (n.X * i.X + n.Y * i.Y) - i.Y
                );
        }

        public static Vector3 Reflection3d(Vector3 n, Vector3 i)
        {
            return new Vector3
               (
                 2 * n.X * (n.X * i.X + n.Y * i.Y + n.Z * i.Z) - i.X,
                 2 * n.Y * (n.X * i.X + n.Y * i.Y + n.Z * i.Z) - i.Y,
                 2 * n.Z * (n.X * i.X + n.Y * i.Y + n.Z * i.Z) - i.Z
                );
        }

        public static Vector2 SinCos(Vector2 directionVector)
        {
            return Vector2.Normalize(directionVector);
        }

        /// <summary>
        /// Given a distance from a point and a direction we obtain points on a circle there is a square that contains.
        /// this functions returns the equivillent points on that square for a given vector.
        /// </summary>
        public static Vector2 OuterSquareSinCosVector(Vector2 directionVector)
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
        public static Vector2 InnerSquareAsinACosVector(Vector2 directionVector)
        {
            float d = directionVector.Length();
            var n = directionVector / d;
            var s = new Vector2((n.X < 0) ? -1 : 1, (n.Y < 0) ? -1 : 1);
            return n * n * s * d;
        }

        /// <summary>
        /// Given a square that encompases a circle to find a point on it corresponding to a given vector.
        /// </summary>
        public static Vector2 OuterSquareAsinACosVector(Vector2 directionVector)
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
        public static float RatioOfN(float n, float b)
        {
            return n / (n + b);
        }

        public static float SlopeOfN(float n, float b)
        {
            return n / b;
        }

        // euler rotations.
        public static Vector3 RotatePointAboutZaxis(Vector3 p, double q)
        {
            //x' = x*cos s - y*sin s //y' = x*sin s + y*cos s   //z' = z
            return new Vector3
                (
                (float)(p.X * Math.Cos(q) - p.Y * Math.Sin(q)),
                (float)(p.X * Math.Sin(q) + p.Y * Math.Cos(q)),
                p.Z
                );
        }
        public static Vector3 RotatePointAboutXaxis(Vector3 p, double q)
        {
            //y' = y*cos s - z*sin s //z' = y*sin s + z*cos s //x' = x
            return new Vector3
                (
                (float)(p.Y * Math.Cos(q) - p.Z * Math.Sin(q)),
                (float)(p.Y * Math.Sin(q) + p.Z * Math.Cos(q)),
                p.X
                );
        }

        public static Vector3 RotatePointAboutYaxis(Vector3 p, double q)
        {
            //z' = z*cos s - x*sin s //x' = z*sin s + x*cos s //y' = y
            return new Vector3
                (
                (float)(p.Z * Math.Cos(q) - p.X * Math.Sin(q)),
                (float)(p.Z * Math.Sin(q) + p.X * Math.Cos(q)),
                p.Y
                );
        }

        public static Vector2 CalculateNormalAndOutDistance(Vector2 v, out float dist)
        {
            dist = (float)(Math.Sqrt(v.X * v.X + v.Y * v.Y));
            return new Vector2(v.X / dist, v.Y / dist);
        }
        public static Vector3 CalculateNormalAndOutDistance(Vector3 v, out float dist)
        {
            dist = (float)(Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z));
            return new Vector3(v.X / dist, v.Y / dist, v.Z / dist);
        }


        //_________________________some old functions not 100% on there reliabiltiy i maybe broke the line line one_______________


        /// <summary>
        /// Normal probability density function.
        /// </summary>
        //public static double NormalDensity(double x, double mu, double sd)
        //{
        //    return 1d / (Sqrt(2d * PI) * sd) * Exp(-(Pow(x - mu, 2d) / (2d * Pow(sd, 2d))));
        //}

        public static float Sagitta(float chordlength, float radius)
        {
            float halfchord = chordlength * .5f;
            return (float)(radius - (float)Math.Sqrt(radius * radius - halfchord * halfchord));
        }

        public static float AngularVelocity(float radius, float radians, float time)
        {
            return radius * (radians / time);
        }

        public static Vector2 QuadricIntercept(Vector2 obj_position, float obj_speed, Vector2 target_position, float target_speed, Vector2 target_normal)
        {
            float tvx = target_normal.X * target_speed;
            float tvy = target_normal.Y * target_speed;
            float pdx = target_position.X - obj_position.X;
            float pdy = target_position.Y - obj_position.Y;

            float pdlength = Vector2.Normalize(target_position - obj_position).Length();
            float d = pdx * pdx + pdy * pdy;
            float s = (tvx * tvx + tvy * tvy) - obj_speed * obj_speed;
            float q = (tvx * pdx + tvy * pdy);
            //float sd = ((tvx * tvx + tvy * tvy) - obj_speed * obj_speed) * (tvx * pdx + tvy * pdy);
            float disc = (q * q) - s * d; // get rid of the fluff
            float disclen = (float)Math.Sqrt(disc);

            float t1 = (-q + disclen) / s;
            float t2 = (-q - disclen) / s;

            float t = t1;
            if (t1 < 0.0f) { t = t2; }

            Vector2 aimpoint = Vector2.Zero;
            if (t > 0.0f)
            {
                aimpoint.X = t * tvx + target_position.X;
                aimpoint.Y = t * tvy + target_position.Y;
            }
            return aimpoint; // returns Vector2.Zero if no positive time to fire exists
        }

        public static Vector2 QuadricIntercept(Vector2 obj_position, float obj_speed, Vector2 target_position, Vector2 target_velocity)
        {
            float tvx = target_velocity.X;
            float tvy = target_velocity.Y;
            float pdx = target_position.X - obj_position.X;
            float pdy = target_position.Y - obj_position.Y;

            float pdlength = Vector2.Normalize(target_position - obj_position).Length();
            float d = pdx * pdx + pdy * pdy;
            float s = (tvx * tvx + tvy * tvy) - obj_speed * obj_speed;
            float q = (tvx * pdx + tvy * pdy);
            //float sd = ((tvx * tvx + tvy * tvy) - obj_speed * obj_speed) * (tvx * pdx + tvy * pdy);
            float disc = (q * q) - s * d; // get rid of the fluff
            float disclen = (float)Math.Sqrt(disc);

            float t1 = (-q + disclen) / s;
            float t2 = (-q - disclen) / s;

            float t = t1;
            if (t1 < 0.0f) { t = t2; }

            Vector2 aimpoint = Vector2.Zero;
            if (t > 0.0f)
            {
                aimpoint.X = t * tvx + target_position.X;
                aimpoint.Y = t * tvy + target_position.Y;
            }
            return aimpoint; // returns Vector2.Zero if no positive time to fire exists
        }

        /// <summary>
        /// Very nice and works (provided i didn't break it with the wrong crosses and lessequal).
        /// got it from this dudes site
        /// http://www.codeproject.com/Tips/862988/Find-the-Intersection-Point-of-Two-Line-Segments
        /// </summary>
        public static bool LineSegementsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, out Vector2 intersection)
        {
            intersection = new Vector2();

            var r_pdif = p2 - p1;
            var s_qdiff = q2 - q1;
            var dqp = q1 - p1;
            var rxs = Cross2D(r_pdif, s_qdiff);
            var qpxr = Cross2D(dqp, r_pdif); // hopefully i didn't use the wrong handed cross product here and break it need to retest it.

            // 2. If r x s = 0 and (q - p) x r = 0, then the two lines are collinear.
            // 3. If r x s = 0 and (q - p) x r != 0, then the two lines are parallel and non-intersecting.
            //if ( (rxs == Vector2.Zero && qpxr == Vector2.Zero) || (rxs == Vector2.Zero && !(qpxr == Vector2.Zero)))
            //    return false;

            // fast fail.
            if (rxs == Vector2.Zero)
                return false;

            var t = Cross2D(dqp, s_qdiff) / rxs;   // t = (q - p) x s / (r x s)
            var u = Cross2D(dqp, r_pdif) / rxs;    // u = (q - p) x r / (r x s)

            // 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1 // the two line segments meet at the point p + t r = q + u s.
            if (!(rxs == Vector2.Zero) && (LessThanEqual(Vector2.Zero, t) && LessThanEqual(t, Vector2.One)) && (LessThanEqual(Vector2.Zero , u) && LessThanEqual(u , Vector2.One) )  )
            {
                intersection = p1 + t * r_pdif;  // We can calculate the intersection point using either t or u.
                return true;
            }
            // 5. Otherwise, the two line segments are not parallel but do not intersect.
            return false;
        }

        public static bool LessThanEqual(Vector2 a, Vector2 b)
        {
            if (a.X <= b.X && a.Y <= b.Y)
                return true;
            else
                return false;
        }

        /// <summary>
        /// maybe cheaper then the above not yet tested
        /// </summary>
        public static bool IsIntersecting(Point startA, Point endA, Point startB, Point endB)
        {
            float denominator = ((endA.X - startA.X) * (endB.Y - startB.Y)) - ((endA.Y - startA.Y) * (endB.X - startB.X));
            float numerator1 = ((startA.Y - startB.Y) * (endB.X - startB.X)) - ((startA.X - startB.X) * (endB.Y - startB.Y));
            float numerator2 = ((startA.Y - startB.Y) * (endA.X - startA.X)) - ((startA.X - startB.X) * (endA.Y - startA.Y));

            // Detect coincident lines (has a edge case, which is the case in which two lines are coincident but don't overlap)
            if (denominator == 0) return numerator1 == 0 && numerator2 == 0;

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;

            return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
        }

        public static int ShortestTurnToTargetLeftOrRight(Vector2 pDirection, Vector2 position, Vector2 targetPosition)
        {
            Vector2 target_dir = Vector2.Normalize( targetPosition - position);
            float r = -pDirection.Y * target_dir.X + pDirection.X * target_dir.Y; // cross and dot simultaneously
            float f = pDirection.X * target_dir.X + pDirection.Y * target_dir.Y; // dot
            if (f > .999f) return 0;
            if (r < 0f) return -1;
            if (r > 0) return 1;
            return 0;
        }

        public static float AngularDiffernceInRadians(Vector3 A, Vector3 B)
        {
            var n = ((A.X * B.X) + (A.Y * B.Y) + (A.Z * B.Z));
            float result = n * n;
            if (result < 0)
                return (-result * 3.141592653589f + 3.141592653589f);
            else
                return (1f - result) * 6.28318530f;
        }

        // this is sort of a guesstimation on the future positions.
        public static float DotRange(Vector2 position, Vector2 targetPosition, float targetSpeed, Vector2 targetNormal)
        {
            Vector2 t_pos2 = targetPosition + targetNormal * targetSpeed;
            return Vector2.Dot(Vector2.Normalize(targetPosition - position), Vector2.Normalize(t_pos2 - position));
        }

        public static float DistanceFromConstantAccelerationOverTime(float speed, float time, float acceleration)
        {
            // humm this is what i came up with im not sure this is the same as the book, seems to come out right, though that -1.
            return acceleration * (time - 1) + acceleration * time + speed;
        }

        public static int Fibonacci(int n)
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

        public static float SatisfysWhenZero_SquareSubtractionEquivilence(float a, float b)
        {
            float f1 = (a + b) * (a - b);
            float f2 = (a * a) - (b * b);
            return f1 - f2; // should return zero as they are equivilent
        }

        public static float CubicBeringToTargetAsMagnitude(Vector2 position, Vector2 direction, Vector2 targetPosition)
        {
            targetPosition = targetPosition - position;
            float forward_theta = direction.X * targetPosition.X + direction.Y * targetPosition.Y; // dot = 1 or -1 when north and postion align
            float right_theta = -direction.Y * targetPosition.X + direction.X * targetPosition.Y; // simultaneous cross right and dot = 0 when north and position align
            Vector2 cubic = CubicNormalize(new Vector2(forward_theta, right_theta));
            //
            if (cubic.Y >= 0f) // quads 1 and 2
            {
                if (cubic.X >= 0f) { return (cubic.Y) * .5f; } // Q1
                else { return -cubic.X * .5f + .5f; } //Q2
            }
            else // quads 3 and 4
            {
                if (cubic.X < 0f) { return cubic.X * .5f - .5f; } // Q3
                else { return cubic.Y * .5f; } //Q4
            }
        }

        public static Vector2 CubicNormalize(Vector2 v)
        {
            float n = v.X;
            if (n < 0)
                n = -n;
            if (v.Y < 0)
                n += -v.Y;
            else
                n += v.Y;
            n = 1.0f / n;
            v.X = v.X * n;
            v.Y = v.Y * n;
            return v;
        }

        public static void Swap<T>( ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }


        //_______________________________
        // L E S S  U S E D   C O M M O N
        //_______________________________


        public static double Factorial(double x)
        {
            double i, f;
            f = 1d;
            for (i = 2d; i <= x; i = i + 1d)
                f = f * i;
            return f;
        }

        public static double SurfaceAreaOfSphere(double r)
        {
            return 4d * PI * Math.Pow(r, 2d);
        }

        public static double AreaOfCircle(double r)
        {
            return PI * Math.Pow(r, 2d);
        }

        public static double SurfaceAreaOfCylinder(double r, double h)
        {
            return 2d * AreaOfCircle(r) + AreaOfRectangle(2d * PI * r, h);
        }

        public static double AreaOfRectangle(double b, double h)
        {
            return b * h;
        }

        public static double AreaOfTriangle(double b, double h)
        {
            return 1d / 2d * b * h;
        }

        public static double AreaOfTriangleSides(double a, double b, double c)
        {
            double s = 1d / 2d * (a + b + c);
            return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }

        public static double LeastCommonMultiple(double a, double b)
        {
            double lcm;

            if (a > 0d && b > 0d)
                lcm = Absolute(a * b) / GreatestCommonDivisor(a, b);
            else
                lcm = 0d;
            return lcm;
        }

        public static double GreatestCommonDivisor(double a, double b)
        {
            double t;
            for (; b != 0d;)
            {
                t = b;
                b = a % b;
                a = t;
            }
            return a;
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
