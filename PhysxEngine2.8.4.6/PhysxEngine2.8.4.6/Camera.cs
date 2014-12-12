using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PhysxEngine
{
    public class Camera
    {
        #region Variables

        GraphicsDevice device;
        GameWindow window;
        private float _cameraPitch, _cameraYaw;

        private float nearClip, farClip;

        public float FarClip
        {
            get { return farClip; }
            set { farClip = value; }
        }

        public float NearClip
        {
            get { return nearClip; }
            set { nearClip = value; }
        }

        #endregion

        internal Camera(GraphicsDevice device, GameWindow window, Vector3 position, Vector3 target, float near, float far)
        {
            this.device = device;
            this.window = window;
            this.Position = position;

            nearClip = near;
            farClip = far;

            this.View = Matrix.CreateLookAt(position, target, new Vector3(0, 1, 0));
            this.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)device.Viewport.Width / (float)device.Viewport.Height, near, far);

            CenterCursor();
        }

        public void Update(GameTime elapsedTime)
        {

            Vector2 cursorPosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            Vector2 mouseCenter = new Vector2(window.ClientBounds.Width / 2, window.ClientBounds.Height / 2);
            Vector2 delta = cursorPosition - mouseCenter;
            Vector2 deltaDampened = delta * 0.0005f;
            CenterCursor();

            // Should perhaps extract the yaw and pitch from the current direction of the camera
            _cameraYaw -= deltaDampened.X;
            _cameraPitch -= deltaDampened.Y;

            //

            Vector3 forward = Matrix.Invert(this.View).Forward;
            Vector3 position = Matrix.Invert(this.View).Translation;

            Matrix cameraRotation = Matrix.CreateFromYawPitchRoll(_cameraYaw, _cameraPitch, 0.0f);
            Vector3 newForward = Vector3.TransformNormal(Vector3.Forward, cameraRotation);

            float elapsed = (float)(elapsedTime.ElapsedGameTime.TotalMilliseconds / 1000.0); // Elapsed time since last frame in seconds
            const float speed = 20.0f; // 20 distance units per second
            float distance = speed * elapsed; // d = vt

            // The amount of movement * the direction of movement, then rotate that along the direction we are looking
            Vector3 translateDirection = Vector3.Zero;

            KeyboardState states = Keyboard.GetState();

            if (states.IsKeyDown(Keys.W)) // Forwards
                translateDirection += Vector3.TransformNormal(Vector3.Forward, cameraRotation);

            if (states.IsKeyDown(Keys.S)) // Backwards
                translateDirection += Vector3.TransformNormal(Vector3.Backward, cameraRotation);

            if (states.IsKeyDown(Keys.A)) // Left
                translateDirection += Vector3.TransformNormal(Vector3.Left, cameraRotation);

            if (states.IsKeyDown(Keys.D)) // Right
                translateDirection += Vector3.TransformNormal(Vector3.Right, cameraRotation);

            Vector3 newPosition = position;
            if (translateDirection.LengthSquared() > 0)
                newPosition += Vector3.Normalize(translateDirection) * distance;

            this.Position = newPosition;
            this.View = Matrix.CreateLookAt(newPosition, newPosition + newForward, Vector3.Up);
        }

        private void CenterCursor()
        {
            Mouse.SetPosition(window.ClientBounds.Width / 2, window.ClientBounds.Height / 2);
        }

        #region Properties
        public Matrix View
        {
            get;
            set;
        }
        public Matrix Projection
        {
            get;
            private set;
        }
        public Vector3 Position
        {
            get;
            set;
        }
        #endregion
    }
}
