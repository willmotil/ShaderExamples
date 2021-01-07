using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
    // Todo.       
    // I made this a while back now but still haven't taken any time to improve it.
    // I should later fix this up later to just take a set of waypoints and allow for the camera to generate a new uniformed set from them. 
    // To allow for the motion to be proportioned smoothly, that may not always be desired though.

    public class CinematicCamera
    {
        Vector3 _camPos = Vector3.Zero;
        Vector3 _camLastPos = Vector3.Zero;
        Vector3 _camLastLastPos = Vector3.Zero;
        Vector3 _targetLookAtPos = Vector3.One;
        Vector3 _forward = Vector3.Zero;
        Vector3 _lastForward = Vector3.Zero;
        Vector3 _camUp = Vector3.Zero;
        Matrix _cameraWorld = Matrix.Identity;
        float _near = 1f;
        float _far = 1000f;
        float _fieldOfView = 1.0f;
        bool _perspectiveStyle = false;
        bool _spriteBatchStyle = false;
        float inv = 1f;
        Matrix _projection = Matrix.Identity;
        float _elapsed = 0;
        float _durationElapsed = 0f;
        float _durationInSeconds = 1f;

        private Vector4[] wayPointReference;

        Curve_WeightedBezier wayPointCurve;

        public Matrix Projection { get { return _projection; } set { _projection = value; } }
        public Matrix View { get { return Matrix.Invert(_cameraWorld); } }
        public Matrix World { get { return _cameraWorld; } }
        public Vector3 Position { get { return _cameraWorld.Translation; } }
        public Vector3 Forward { get { return _cameraWorld.Forward; } }
        public Vector3 Up { get { return _cameraWorld.Up; } set { _cameraWorld.Up = value; _camUp = value; } }
        public Vector3 Right { get { return _cameraWorld.Right; } }
        public float Near { get { return _near; } }
        public float Far { get { return _far; } }

        public Vector3 TargetPosition { get { return _targetLookAtPos; } }
        public bool IsSpriteBatchStyled { get { return _spriteBatchStyle; } }
        public bool IsPerspectiveStyled { get { return _perspectiveStyle; } }
        public bool UseForwardPathLook { get; set; }
        public bool UseWayPointMotion { get; set; }
        public float WayPointCycleDurationInTotalSeconds { get { return _durationInSeconds; } set { _durationInSeconds = value; } }
        public float LookAtSpeedPerSecond { get; set; } = 1f;
        public float MovementSpeedPerSecond { get; set; } = 1f;
        public float VisualizationScale { get; set; } = .25f;
        public Vector3 VisualizationOffset{ get; set; } = new Vector3(0,0,0);

        public void SetWayPoints(Vector4[] waypoints, int numberOfSegments, bool closePathEnds, bool makePathUniform)
        {
            wayPointReference = waypoints;
            wayPointCurve = new Curve_WeightedBezier(waypoints, numberOfSegments, closePathEnds, makePathUniform);
        }

        /// <summary>
        /// This is a cinematic styled fixed camera it uses way points to traverse thru the world.
        /// </summary>
        public CinematicCamera(GraphicsDevice device, SpriteBatch spriteBatch, Texture2D dot, Vector3 pos, Vector3 target, Vector3 up, float nearClipPlane, float farClipPlane, float fieldOfView, bool perspective, bool spriteBatchStyled, bool inverseOthographicProjection)
        {
            MgDrawExtras.Initialize(device, spriteBatch);
            //wayPointCurvature = new MyImbalancedSpline();
            TransformCamera(pos, target, up);
            SetProjection(device, nearClipPlane, farClipPlane, fieldOfView, perspective, spriteBatchStyled, inverseOthographicProjection);
        }

        public void CalledOnClientSizeChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// This defaults to using the camera settings for UseForwardPathLook or UseWayPointMotion
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if(UseForwardPathLook)
                ProcessUpdate(false, _forward + _camPos, gameTime);
            else
                ProcessUpdate(false, _forward + _camPos, gameTime);
        }

        /// <summary>
        /// Calling this overides the forward look to point at a position temporarily.
        /// </summary>
        public void Update(Vector3 targetPosition, GameTime gameTime)
        {
            if(targetPosition != _camPos)
                ProcessUpdate(true, targetPosition, gameTime);
            else
                ProcessUpdate(false, _forward + _camPos, gameTime);
        }

        /// <summary>
        /// Process update determines how the camera will behave via user input or under a automated motion.
        /// </summary>
        void ProcessUpdate(bool usePassedTarget, Vector3 targetPosition, GameTime gameTime)
        {
            _elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _durationElapsed += _elapsed;
            if (_durationElapsed >= WayPointCycleDurationInTotalSeconds)
                _durationElapsed -= WayPointCycleDurationInTotalSeconds;

            Vector3 nowPosition = _camPos;

            float timeOnCurve = _durationElapsed / WayPointCycleDurationInTotalSeconds;

            if (UseWayPointMotion)
                nowPosition = ToVector3(wayPointCurve.GetUniformSplinePoint(timeOnCurve));
            else 
            {
                // manual movement with camera.
                UpdateCameraMotionUsingDefaultKeyboardCommands(gameTime);
                nowPosition = _cameraWorld.Translation;
            }

            if (usePassedTarget) 
                _targetLookAtPos = targetPosition; // here we just pass the given target position passed in by the user as this call is a overide command.
            else
            {
                if (UseForwardPathLook)
                    _targetLookAtPos = (nowPosition - _camPos) + nowPosition;  // view locked to forward direction on waypoint path.
                else
                {
                    // manual look at controls.
                    UpdateCameraLookAtUsingDefaultKeyboardCommands(gameTime);
                    _targetLookAtPos = _cameraWorld.Forward + nowPosition; 
                }
            }

            TransformCamera(nowPosition, _targetLookAtPos, _camUp);
        }

        void UpdateCameraLookAtUsingDefaultKeyboardCommands(GameTime gameTime)
        {
            // look
            if (Keyboard.GetState().IsKeyDown(Keys.A))
                LookLeftLocally(LookAtSpeedPerSecond * _elapsed);
            if (Keyboard.GetState().IsKeyDown(Keys.D))
                LookRightLocally(LookAtSpeedPerSecond * _elapsed);

            if (Keyboard.GetState().IsKeyDown(Keys.W))
                LookUpLocally(LookAtSpeedPerSecond * _elapsed);
            if (Keyboard.GetState().IsKeyDown(Keys.S))
                LookDownLocally(LookAtSpeedPerSecond * _elapsed);

            // roll
            if (Keyboard.GetState().IsKeyDown(Keys.C))
                RollClockwise(MovementSpeedPerSecond * _elapsed);
            if (Keyboard.GetState().IsKeyDown(Keys.Z))
                RollCounterClockwise(MovementSpeedPerSecond * _elapsed);
        }
        void UpdateCameraMotionUsingDefaultKeyboardCommands(GameTime gameTime)
        {
            // move
            if (Keyboard.GetState().IsKeyDown(Keys.E))
                MoveForwardLocally(MovementSpeedPerSecond * _elapsed);
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
                MoveBackLocally(MovementSpeedPerSecond * _elapsed);

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                MoveUpLocally(MovementSpeedPerSecond * _elapsed);
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                MoveDownLocally(MovementSpeedPerSecond * _elapsed);
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                MoveLeftLocally(MovementSpeedPerSecond * _elapsed);
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                MoveRightLocally(MovementSpeedPerSecond * _elapsed);
        }

        public void TransformCamera(Vector3 currentPosition, Vector3 targetPosition, Vector3 upNormalDirection)
        {
            
            _targetLookAtPos = targetPosition;
            _camLastLastPos = _camLastPos;
            _camLastPos = _camPos;
            _camPos = currentPosition;
            _camUp = upNormalDirection;
            _forward = _targetLookAtPos - _camPos;

            if (_forward.X == 0 && _forward.Y == 0 && _forward.Z == 0)
                _forward = _lastForward;
            else
                _lastForward = _forward;

            // TODO handle up down vector gimble lock astetic under fixed camera.
            // ...
            
            // you might ask why is ther a lastlastCamPos well that is all part of my evil plan to handle figuring out were a free cameras up vector should want to be placed or drift to in order to make everything really cool.
            // basically we want a up drift in some cases like for a space or cinimatic camera if we are turning in a direction the 3 positions last2 last1 now0 ,  MAY or may not, form a triangle A,B,C were if B lies at a tangent to a point along the line of A-B
            // its possible that A B C simply line up in that case its mute and no Up vector drift can be recommended.


            // ...

            _cameraWorld = Matrix.CreateWorld(_camPos, _forward, _camUp);
        }

        public void SetProjection(GraphicsDevice device, float nearClipPlane, float farClipPlane, float fieldOfView, bool perspective, bool spriteBatchStyled, bool inverseOrthoGraphicProjection)
        {
            _near = nearClipPlane;
            _far = farClipPlane;
            _fieldOfView = fieldOfView;
            _perspectiveStyle = perspective;
            _spriteBatchStyle = spriteBatchStyled;

            // Allows a change to a spritebatch style orthagraphic or inverse styled persepective, e.g. a viewer imagining a forward z positive depth going into the screen.
            inv = 1f;
            if (inverseOrthoGraphicProjection)
                inv *= -1f;

            UpdateProjectionViaPresets(device);
        }

        public void UpdateProjectionViaPresets(GraphicsDevice device)
        {
            if (_perspectiveStyle)
            {
                if (_spriteBatchStyle)
                {
                    CreatePerspectiveViewSpriteBatchAligned(device, _camPos, _fieldOfView, _near, _far, out _cameraWorld, out _projection);
                    TransformCamera(_cameraWorld.Translation, _cameraWorld.Forward + _cameraWorld.Translation, _cameraWorld.Up); // take care _camPos is not yet set.
                }
                else
                {
                    _projection = Matrix.CreatePerspectiveFieldOfView(_fieldOfView, device.Viewport.AspectRatio, _near, _far);
                }
            }
            else
            {
                if (_spriteBatchStyle)
                {
                    CreateOrthographicViewSpriteBatchAligned(device, _camPos, false, out _cameraWorld, out _projection);
                    TransformCamera(_cameraWorld.Translation, _cameraWorld.Forward + _cameraWorld.Translation, _cameraWorld.Up); // take care _camPos is not yet set.
                }
                else
                {
                    _projection = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, device.Viewport.Height, 0, _near, inv * _far);
                }
            }
        }

        public void CreatePerspectiveViewSpriteBatchAligned(GraphicsDevice device, Vector3 scollPositionOffset, float fieldOfView, float near, float far, out Matrix cameraWorld, out Matrix projection)
        {
            var dist = -((1f / (float)Math.Tan(fieldOfView / 2)) * (device.Viewport.Height / 2));
            var pos = new Vector3(device.Viewport.Width / 2, device.Viewport.Height / 2, dist) + scollPositionOffset;
            var target = new Vector3(0, 0, 1) + pos;
            cameraWorld = Matrix.CreateWorld(pos, target - pos, Vector3.Down);
            projection = CreateInfinitePerspectiveFieldOfViewRHLH(fieldOfView, device.Viewport.AspectRatio, near, far, true);
        }

        public void CreateOrthographicViewSpriteBatchAligned(GraphicsDevice device, Vector3 scollPositionOffset, bool inverseOrthoDirection, out Matrix cameraWorld, out Matrix projection)
        {
            float forwardDepthDirection = 1f;
            if (inverseOrthoDirection)
                forwardDepthDirection = -1f;
            cameraWorld = Matrix.CreateWorld(scollPositionOffset, new Vector3(0, 0, 1), Vector3.Down);
            projection = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, -device.Viewport.Height, 0, forwardDepthDirection * 0, forwardDepthDirection * 1f);
        }

        public float GetRequisitePerspectiveSpriteBatchAlignmentZdistance(GraphicsDevice device, float fieldOfView)
        {
            var dist = -((1f / (float)Math.Tan(fieldOfView / 2)) * (device.Viewport.Height / 2));
            return dist;
        }

        /// <summary>
        /// Pre loaded class callable best guess to initialize a z camera perspective to be close to spritebatch at start.
        /// You should know all these values at the beginning.
        /// </summary>
        public static float GetRequisitePerspectiveSpriteBatchAlignmentZdistance( int preferedWidth, int preferedHeight, float fieldOfView)
        {
            var dist = -((1f / (float)Math.Tan(fieldOfView / 2)) * (preferedHeight / 2));
            return dist;
        }

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
        /// Moves the camera thru paths in straight lines from point to point.
        /// </summary>
        public void InterpolateThruWayPoints(Vector3 targetPosition, Vector3[] waypoints, bool useSmoothStep, GameTime gameTime)
        {
            _durationElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_durationElapsed >= WayPointCycleDurationInTotalSeconds)
                _durationElapsed -= WayPointCycleDurationInTotalSeconds;

            var interpolation = _durationElapsed / WayPointCycleDurationInTotalSeconds;
            float coeff = 1f / (float)waypoints.Length;
            int index = (int)(interpolation / coeff);
            int index2 = index + 1;
            if (index2 >= waypoints.Length)
                index2 = 0;
            float adjustedInterpolator = (interpolation - (coeff * index)) / coeff;
            if (useSmoothStep)
                TransformCamera(Vector3.SmoothStep(waypoints[index], waypoints[index2], adjustedInterpolator), targetPosition, _camUp);
            else
                TransformCamera(Vector3.Lerp(waypoints[index], waypoints[index2], adjustedInterpolator), targetPosition, _camUp);
        }


        public void DrawCurveThruWayPointsWithSpriteBatch( int PlaneOption, GameTime gameTime)
        {
            DrawCurveThruWayPointsWithSpriteBatch(VisualizationScale, VisualizationOffset, PlaneOption,gameTime);

        }

        /// <summary>
        /// Tells the camera to execute a visualization of the waypoint camera path.
        /// </summary>
        /// <param name="visualizationScale">Scale of visualization</param>
        /// <param name="visualizationOffset">positional offset on screen</param>
        /// <param name="PlaneOption">change of the signifigant input offsets to , xyz  0 = xy0, 1 =x0y, 2 = 0yz</param>
        /// <param name="gameTime"></param>
        public void DrawCurveThruWayPointsWithSpriteBatch(float visualizationScale, Vector3 visualizationOffset, int PlaneOption, GameTime gameTime)
        {
            if (wayPointCurve != null)
            {
                Vector2 offset2d = Get2dVectorAxisElements(visualizationOffset, PlaneOption);
                // current 2d camera position and forward on the orthographic xy plane.
                var camTargetPos = Get2dVectorAxisElements(_targetLookAtPos, PlaneOption);
                var camPosition = Get3dTwistedVectorAxisElements(_cameraWorld.Translation, PlaneOption);
                var camForward = Get3dTwistedVectorAxisElements(_cameraWorld.Forward, PlaneOption);
                //
                var drawnCam2dHeightAdjustment = new Vector2(0, camPosition.Z *-.5f) * visualizationScale;
                var drawnCamTargetPos = camTargetPos * visualizationScale + offset2d;
                var drawnCamForwardRay = new Vector2(camForward.X, camForward.Y) * 15;
                var drawnCamIteratedPos = new Vector2(camPosition.X, camPosition.Y) * visualizationScale + offset2d;
                var drawnCamIteratedOffsetPos = drawnCamIteratedPos + drawnCam2dHeightAdjustment * visualizationScale;
                var drawCamForwardRayEndPoint = drawnCamIteratedPos + drawnCamForwardRay;
                Vector2 drawCrossHairLeft, drawCrossHairRight, drawCrossHairUp, drawCrossHairDown;
                GetIndividualCrossHairVectors(drawnCamIteratedOffsetPos, 7, out drawCrossHairLeft, out drawCrossHairRight, out drawCrossHairUp, out drawCrossHairDown);

                // Draw cross hair for camera position
                MgDrawExtras.DrawBasicLine(drawCrossHairLeft, drawCrossHairRight, 1, Color.White);
                MgDrawExtras.DrawBasicLine(drawCrossHairUp, drawCrossHairDown, 1, Color.White);

                // Draw a line from camera from current position on way point curve to offset position.
                if (drawnCam2dHeightAdjustment.Y < 0)
                    MgDrawExtras.DrawBasicLine(drawnCamIteratedPos, drawnCamIteratedOffsetPos, 1, Color.LightGreen);
                else
                    MgDrawExtras.DrawBasicLine(drawnCamIteratedPos, drawnCamIteratedOffsetPos, 1, Color.Red);

                // Draw forward camera direction
                MgDrawExtras.DrawBasicLine(drawnCamIteratedPos, drawCamForwardRayEndPoint , 1, Color.Beige);
                // Draw camera crosshairs forward to target.
                MgDrawExtras.DrawBasicLine(drawnCamIteratedOffsetPos, drawCamForwardRayEndPoint, 1, Color.Yellow);

                // draw curved segmented output.
                var loopAdjustment = 1;
                if (wayPointCurve._closedControlPoints)
                    loopAdjustment = 0;
                var curveLineSegments = wayPointCurve.curveLinePoints;
                for (int i = 0; i < curveLineSegments.Length - loopAdjustment; i++)
                {
                    int i2 = i + 1;
                    if (i2 >= curveLineSegments.Length)
                        i2 = i2 - curveLineSegments.Length;
                    var segment = ToVector3( curveLineSegments[i] );
                    var segment2 = ToVector3( curveLineSegments[i2] );
                    var start = Get2dVectorAxisElements(segment, PlaneOption) * visualizationScale + offset2d;
                    var end = Get2dVectorAxisElements(segment2, PlaneOption) * visualizationScale + offset2d;

                    if (i % 2 == 0)
                        MgDrawExtras.DrawBasicLine(start, end, 1, Color.Black);
                    else
                        MgDrawExtras.DrawBasicLine(start, end, 1, Color.Blue);
                }

                // Draw current 2d waypoint positions on the orthographic xy plane.
                foreach (var p in wayPointReference)
                {
                    var waypointPos = Get2dVectorAxisElements(ToVector3( p ), PlaneOption) * visualizationScale + offset2d;
                    GetIndividualCrossHairVectors(waypointPos, 4, out drawCrossHairLeft, out drawCrossHairRight, out drawCrossHairUp, out drawCrossHairDown);
                    MgDrawExtras.DrawBasicLine(drawCrossHairLeft, drawCrossHairRight, 1, Color.DarkGray);
                    MgDrawExtras.DrawBasicLine(drawCrossHairUp, drawCrossHairDown, 1, Color.DarkGray);
                }
            }
        }

        private void GetIndividualCrossHairVectors(Vector2 v, float radius, out Vector2 left, out Vector2 right, out Vector2 up, out Vector2 down)
        {
            left = new Vector2(-radius, 0) + v;
            right = new Vector2(0 + radius, 0) + v;
            up = new Vector2(0, 0 - radius) + v;
            down = new Vector2(0, radius) + v;
        }
        private Vector3 Get3dTwistedVectorAxisElements(Vector3 v, int OptionXY_XZ_YZ)
        {
            if (OptionXY_XZ_YZ == 1)
            {
                return new Vector3(v.X, v.Z, v.Y);
            }
            if (OptionXY_XZ_YZ == 2)
            {
                return new Vector3(v.Y, v.Z, v.X);
            }
            return new Vector3(v.X, v.Y, v.Z);
        }
        private Vector2 Get2dVectorAxisElements(Vector3 v, int OptionXY_XZ_YZ_012)
        {
            if (OptionXY_XZ_YZ_012 == 1)
            {
                return new Vector2(v.X, v.Z);
            }
            if (OptionXY_XZ_YZ_012 == 2)
            {
                return new Vector2(v.Y, v.Z);
            }
            return new Vector2(v.X, v.Y);
        }

        public Vector3 ToVector3(Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public void MoveForwardLocally(float amount)
        {
            _cameraWorld.Translation += _cameraWorld.Forward * amount;
        }
        public void MoveBackLocally(float amount)
        {
            _cameraWorld.Translation += _cameraWorld.Backward * amount;
        }
        public void MoveLeftLocally(float amount)
        {
            _cameraWorld.Translation += _cameraWorld.Left * amount;
        }
        public void MoveRightLocally(float amount)
        {
            _cameraWorld.Translation += _cameraWorld.Right * amount;
        }
        public void MoveUpLocally(float amount)
        {
            _cameraWorld.Translation += _cameraWorld.Up * amount;
        }
        public void MoveDownLocally(float amount)
        {
            _cameraWorld.Translation += _cameraWorld.Down * amount;
        }

        public void LookLeftLocally(float amountInRadians)
        {
            var m = Matrix.CreateFromAxisAngle(_cameraWorld.Up, amountInRadians);
            var t = _cameraWorld.Translation;
            _cameraWorld *= m;
            _cameraWorld.Translation = t;
        }
        public void LookRightLocally(float amountInRadians)
        {
            var m = Matrix.CreateFromAxisAngle(_cameraWorld.Up, -amountInRadians);
            var t = _cameraWorld.Translation;
            _cameraWorld *= m;
            _cameraWorld.Translation = t;
        }
        public void LookUpLocally(float amountInRadians)
        {
            var m = Matrix.CreateFromAxisAngle(_cameraWorld.Right, amountInRadians);
            var t = _cameraWorld.Translation;
            _cameraWorld *= m;
            _cameraWorld.Translation = t;
        }
        public void LookDownLocally(float amountInRadians)
        {
            var m = Matrix.CreateFromAxisAngle(_cameraWorld.Right, -amountInRadians);
            var t = _cameraWorld.Translation;
            _cameraWorld *= m;
            _cameraWorld.Translation = t;
        }

        public void RollClockwise(float amountInRadians)
        {
            var m = Matrix.CreateFromAxisAngle(_cameraWorld.Forward, amountInRadians);
            var t = _cameraWorld.Translation;
            _cameraWorld *= m;
            _cameraWorld.Translation = t;
        }
        public void RollCounterClockwise(float amountInRadians)
        {
            var m = Matrix.CreateFromAxisAngle(_cameraWorld.Forward, -amountInRadians);
            var t = _cameraWorld.Translation;
            _cameraWorld *= m;
            _cameraWorld.Translation = t;
        }

        public void LookLeftSystem(float amountInRadians)
        {
            var m = Matrix.CreateRotationY(amountInRadians);
            var t = _cameraWorld.Translation;
            _cameraWorld *= m;
            _cameraWorld.Translation = t;
        }
        public void LookRightSystem(float amountInRadians)
        {
            var m = Matrix.CreateRotationY(-amountInRadians);
            var t = _cameraWorld.Translation;
            _cameraWorld *= m;
            _cameraWorld.Translation = t;
        }
        public void LookUpSystem(float amountInRadians)
        {
            var m = Matrix.CreateRotationX(amountInRadians);
            var t = _cameraWorld.Translation;
            _cameraWorld *= m;
            _cameraWorld.Translation = t;
        }
        public void LookDownSystem(float amountInRadians)
        {
            var m = Matrix.CreateRotationX(-amountInRadians);
            var t = _cameraWorld.Translation;
            _cameraWorld *= m;
            _cameraWorld.Translation = t;
        }
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++


    public class Curve_WeightedBezier
    {
        public bool _showTangents = false;

        #region  non requisite optional astetic visual values.
        //List<Vector3> artificialCpLine = new List<Vector3>();
        //List<Vector3> artificialTangentLine = new List<Vector3>();
        #endregion

        #region  temporary tracking values used thruout many methods as the curves are processed.
        int currentCpIndex = 0;
        int index0 = 0;
        int index1 = 0;
        int index2 = 0;
        int index3 = 0;
        #endregion


        /// <summary>
        /// This holds the information relating to the control points given or that will be calculated.
        /// </summary>
        ControlPoint[] cps;
        /// <summary>
        /// these are the reslting generated NonUniform timed or Uniformly timed line points.
        /// </summary>
        public Vector4[] curveLinePoints;
        /// <summary>
        /// when set to true or closed this will loop the last point to curve round to the first and the curve will be a loop.
        /// when set to false the end points will be doubled in the algorithm to calculate the clamping for the first and last curve ending segments.
        /// </summary>
        public bool _closedControlPoints = true;
        /// <summary>
        /// when uniformed is true the resulting curve will be made with uniformly spaced positions and the timed traversal rate across the curve will be smooth.
        /// </summary>
        public bool _uniformedCurve = true;
        /// <summary>
        /// the number of timed points generated along the entire curve
        /// </summary>
        public int _numOfCurvatureSegmentPoints = 100;
        /// <summary>
        /// the higher this number the more refined the timing along the curve will be and the more time it will take to calculate it.
        /// </summary>
        public int _numberOfIntigrationStepsPerSegment = 10;
        /// <summary>
        /// Not yet implemented ... this value acts as a scalar on all weights.
        /// </summary>
        private float _globalWeight = 1.0f;

        float totaldist = 0;

        /// <summary>
        /// the calculated total integrated distance of the curve.
        /// </summary>
        public float TotalCurveDistance { get { return totaldist; } }



        #region constructors

        /// <summary>
        /// </summary>
        public Curve_WeightedBezier(Vector4[] controlPoints)
        {
            CreateSpline(controlPoints);
        }

        /// <summary>
        /// </summary>
        public Curve_WeightedBezier(Vector4[] controlPoints, int numOfVisualCurvatureSegmentPoints, bool closedControlPoints, bool uniformedCurve)
        {
            _closedControlPoints = closedControlPoints;
            _uniformedCurve = uniformedCurve;
            _numOfCurvatureSegmentPoints = numOfVisualCurvatureSegmentPoints;
            CreateSpline(controlPoints);
        }

        /// <summary>
        /// 
        /// </summary>
        public Curve_WeightedBezier(Vector4[] controlPoints, int numOfVisualCurvatureSegmentPoints, bool closedControlPoints, bool uniformedCurve, float globalWeight, bool showTangents)
        {
            _closedControlPoints = closedControlPoints;
            _uniformedCurve = uniformedCurve;
            _globalWeight = globalWeight;
            _numOfCurvatureSegmentPoints = numOfVisualCurvatureSegmentPoints;
            _showTangents = showTangents;
            CreateSpline(controlPoints);
        }

        #endregion

        #region curve calculation methods

        private void CreateSpline(Vector4[] controlPoints)
        {
            //artificialCpLine.Clear();
            //artificialTangentLine.Clear();
            cps = new ControlPoint[controlPoints.Length];
            for (int i = 0; i < controlPoints.Length; i++)
            {
                var cpInstance = new ControlPoint();
                cpInstance.position = new Vector3(controlPoints[i].X, controlPoints[i].Y, controlPoints[i].Z);
                cpInstance.weight = controlPoints[i].W;
                cpInstance.cpIndex = i;
                cps[i] = cpInstance;
            }

            FindCpLengthsAndIntegratedTotalCurveLength();

            curveLinePoints = new Vector4[_numOfCurvatureSegmentPoints];

            var loopCount = _numOfCurvatureSegmentPoints;
            var divisor = loopCount - 1;

            // Create the curve either uniformed or non uniformed.
            if (_uniformedCurve)
            {
                for (int i = 0; i < loopCount; i++)
                {
                    float t = (float)(i) / (float)(divisor);
                    curveLinePoints[i] = GetUniformSplinePoint(t);
                }
            }
            else
            {
                for (int i = 0; i < loopCount; i++)
                {
                    float t = (float)(i) / (float)(divisor);
                    curveLinePoints[i] = GetNonUniformSplinePoint(t);
                }
            }
        }

        public void FindCpLengthsAndIntegratedTotalCurveLength()
        {
            var loopCount = cps.Length;
            var divisor = loopCount - 1;

            var lastPos = cps[0].position;
            totaldist = 0;
            float prevTotalDist = 0;

            var integrateStepAmount = 1f / _numberOfIntigrationStepsPerSegment;
            for (int cptomeasure = 0; cptomeasure < loopCount; cptomeasure++)
            {
                float cpToNextCpDistance = 0;
                for (float time = 0f; time < integrateStepAmount + 000001f; time += integrateStepAmount)
                {
                    var nowPosition = DetermineSplinesAndGetPointOnCurve(cptomeasure, time); // what we get here is the raw tangental splined point by the polynominal.
                    if (time > 0f)
                    {
                        var dist = Vector3.Distance(nowPosition, lastPos);
                        if (nowPosition != lastPos)
                            cpToNextCpDistance += dist;
                    }
                    lastPos = nowPosition;
                }

                prevTotalDist = totaldist;
                if (_closedControlPoints == false && cptomeasure == loopCount - 1)
                    cpToNextCpDistance = 0;
                else
                    totaldist += cpToNextCpDistance;

                cps[cptomeasure].startDistance = prevTotalDist;
                cps[cptomeasure].distanceToNextCp = cpToNextCpDistance;
            }
        }

        public Vector4 GetNonUniformSplinePoint(float Time)
        {
            int resultIndex = 0;
            float fractionalTime = 0;
            if (_closedControlPoints)
            {
                var plotRange = cps.Length;
                var offset = plotRange * Time;
                resultIndex = (int)(offset);
                fractionalTime = offset - (float)resultIndex;
            }
            else
            {
                var plotRange = cps.Length - 1;
                var offset = plotRange * Time;
                resultIndex = (int)(offset);
                fractionalTime = offset - (float)resultIndex;
            }
            var p = DetermineSplinesAndGetPointOnCurve(resultIndex, fractionalTime);
            var w = (cps[index2].weight - cps[index1].weight) * fractionalTime + cps[index1].weight;
            Vector4 result = new Vector4(p.X, p.Y, p.Z, w);
            return result;
        }

        public Vector4 GetUniformSplinePoint(float time)
        {
            int resultIndex = 0;
            float fractionalTime = 0;
            float currentDistance = time * totaldist;
            if (_closedControlPoints)
            {
                int i = 0;
                while (i < cps.Length)
                {
                    var start = cps[i].startDistance;
                    var end = start + cps[i].distanceToNextCp;
                    if (currentDistance >= start && currentDistance <= end)
                    {
                        resultIndex = i;
                        var len = end - start;
                        fractionalTime = (currentDistance - start) / len;
                        if (fractionalTime > 1f)
                            fractionalTime = 1f;
                        i = cps.Length; // break
                    }
                    i++;
                }
            }
            else
            {
                int i = 0;
                while (i < cps.Length)
                {
                    var start = cps[i].startDistance;
                    var end = start + cps[i].distanceToNextCp;
                    if (currentDistance >= start && currentDistance <= end)
                    {
                        resultIndex = i;
                        var len = end - start;
                        fractionalTime = (currentDistance - start) / len;
                        if (fractionalTime > 1f)
                            fractionalTime = 1f;
                        i = cps.Length; // break
                    }
                    i++;
                }
            }
            var p = DetermineSplinesAndGetPointOnCurve(resultIndex, fractionalTime);
            var w = (cps[index2].weight - cps[index1].weight) * fractionalTime + cps[index1].weight;
            Vector4 result = new Vector4(p.X, p.Y, p.Z, w);
            return result;
        }

        private Vector3 DetermineSplinesAndGetPointOnCurve(int cpIndex, float fracTime)
        {
            if (_closedControlPoints || (cpIndex > 0 && cpIndex < cps.Length - 2))
            {
                // caluclate conditional cp indexs at the moments
                index0 = EnsureIndexInRange(cpIndex - 1);
                index1 = EnsureIndexInRange(cpIndex + 0);//<
                index2 = EnsureIndexInRange(cpIndex + 1);
                index3 = EnsureIndexInRange(cpIndex + 2);
            }
            else
            {
                if (cpIndex == 0)
                {
                    index0 = EnsureIndexInRange(cpIndex + 0);
                    index1 = EnsureIndexInRange(cpIndex + 0);//<<
                    index2 = EnsureIndexInRange(cpIndex + 1);
                    index3 = EnsureIndexInRange(cpIndex + 2);
                }
                if (cpIndex >= cps.Length - 2)
                {
                    index0 = EnsureIndexInRange(cpIndex - 1);
                    index1 = EnsureIndexInRange(cpIndex + 0); // <<
                    index2 = EnsureIndexInRange(cpIndex + 1);
                    index3 = EnsureIndexInRange(cpIndex + 1);
                }
            }
            currentCpIndex = index1;
            //return GetSegmentsTangentalWeightedPoint(cps[index0].position, cps[index1].position, cps[index2].position, cps[index3].position, fracTime);
            return GetSegmentV1V2TangentalWeightedPointThruVector(cps[index0].position, cps[index1].position, cps[index2].position, cps[index3].position, cps[index1].weight, cps[index2].weight, fracTime);
            
        }
        public int EnsureIndexInRange(int i)
        {
            while (i < 0)
                i = i + (cps.Length);
            while (i > (cps.Length - 1))
                i = i - (cps.Length);
            return i;
        }

        /// <summary>
        /// This is a generalized 3rd degree polynominal non uniform segment with curvature along that segment affected by the weights the curve intersects all the vectors more importantly vectors 1 and 2. 
        /// This function relys on the GetIdealTangentVector function.
        /// </summary>
        /// <returns></returns>
        private Vector3 GetSegmentV1V2TangentalWeightedPointThruVector(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float vector1Weight, float vector2Weight, float time)
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

        public Vector3 GetIdealTangentVector(Vector3 a, Vector3 b, Vector3 c)
        {
            float disa = Vector3.Distance(a, b);
            float ratioa = disa / (disa + Vector3.Distance(b, c));
            var result = (((c - b) * ratioa) + b) - (((b - a) * ratioa) + a);
            // prevent nan later on.
            if (result == Vector3.Zero)
                result = c - a;
            return result;
        }


        #endregion

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public void DrawWithSpriteBatch(SpriteBatch _spriteBatch, SpriteFont font, GameTime gameTime)
        {
            DrawWithSpriteBatch(_spriteBatch, gameTime);

            for (int i = 0; i < cps.Length; i++)
            {
                string msg =
                    $" CP[{i}]  w: {cps[i].weight.ToString("0.000")}" +
                    $"\n startDist: {cps[i].startDistance.ToString("###0.00")}" +
                    $"\n segDist: {cps[i].distanceToNextCp.ToString("###0.00")}"
                    ;
                _spriteBatch.DrawString(font, msg, new Vector2(cps[i].position.X, cps[i].position.Y), Color.Black);
            }
        }

        public void DrawWithSpriteBatch(SpriteBatch _spriteBatch, GameTime gameTime)
        {
            bool flip = false;
            int lineThickness = 2;

            for (int i = 0; i < cps.Length; i++)
                MgDrawExtras.DrawBasicPoint(new Vector2(cps[i].position.X, cps[i].position.Y), Color.Red , 4);

            for (int i = 0; i < curveLinePoints.Length - 1; i++)
            {
                if (flip)
                    MgDrawExtras.DrawBasicLine(ToVector2(curveLinePoints[i]), ToVector2(curveLinePoints[i + 1]), lineThickness, Color.Green);
                else
                    MgDrawExtras.DrawBasicLine(ToVector2(curveLinePoints[i]), ToVector2(curveLinePoints[i + 1]), lineThickness, Color.Black);

                if (i < 1)
                    MgDrawExtras.DrawBasicLine(ToVector2(curveLinePoints[i]), ToVector2(curveLinePoints[i + 1]), lineThickness, Color.Yellow);

                flip = !flip;
            }
        }

        public Vector3 MidPoint(Vector3 a, Vector3 b)
        {
            return (a + b) / 2;
        }
        public Vector3 MidPoint(Vector3 a, Vector3 b, Vector3 c)
        {
            return (a + b + c) / 3;
        }

        public Vector3 ToVector3(Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
        public Vector2 ToVector2(Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }
        public Vector2 ToVector2(Vector4 v)
        {
            return new Vector2(v.X, v.Y);
        }

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // supporting class.
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public class ControlPoint
        {
            public Vector3 position = Vector3.Zero;
            public float weight = 0;
            public float distanceToNextCp = 0;
            public float startDistance = 0;
            public float ratio = 0;
            public int cpIndex = 0;
        }

        public class LineSegment
        {
            public Vector3 Start { get; set; }
            public Vector3 End { get; set; }
            public LineSegment(Vector3 start, Vector3 end)
            {
                Start = start;
                End = end;
            }
        }

    }
}


