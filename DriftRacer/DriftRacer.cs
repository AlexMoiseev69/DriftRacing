using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using Vector3BEPU = BEPUutilities.Vector3;
using Vertex = Fusion.Graphics.Mesh.Vertex;
using SharpDX;
using Fusion;
using Fusion.Audio;
using Fusion.Content;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Utils;
using Vector3 = SharpDX.Vector3;

namespace DriftRacer
{
    public class DriftRacer : Game
    {
        private Space space;
        private Box ground;
        private Box carBox;
        private Box barruer;

        Texture2D tex;
        Texture2D carTex;

        /// <summary>
        /// DriftRacer constructor
        /// </summary>
        public DriftRacer()
            : base()
        {
            //	root directory for standard x64 C# application
            Parameters.ContentDirectory = @"..\..\..\Content";

            //	enable object tracking :
            Parameters.TrackObjects = true;

            //	enable developer console :
            Parameters.Developer = true;

            //	enable debug graphics device in Debug :
#if DEBUG
				Parameters.UseDebugDevice	=	true;
#endif

            //	add services :
            AddService(new SpriteBatch(this), false, false, 0, 0);
            AddService(new DebugStrings(this), true, true, 9999, 9999);
            AddService(new DebugRender(this), true, true, 9998, 9998);

            //	add here additional services :

            //	load configuration for each service :
            LoadConfiguration();

            //	make configuration saved on exit :
            Exiting += FusionGame_Exiting;
        }


        /// <summary>
        /// Add services :
        /// </summary>
        protected override void Initialize()
        {
            //	initialize services :
            var device = GraphicsDevice;
            device.FullScreen = true;

            base.Initialize();
            InitPhysics();

            tex = Content.Load<Texture2D>("images/route.png");
            carTex = Content.Load<Texture2D>("images/car1.png");

            //	add keyboard handler :
            InputDevice.KeyDown += InputDevice_KeyDown;
        }


        private void InitPhysics()
        {
            int w = GraphicsDevice.Viewport.Width;
            int h = GraphicsDevice.Viewport.Height;

            int carWidth = w/5;
            int carHeight = h/10;

            space = new Space();

            // add up, right, down, left borders
            space.Add(new Box(new Vector3BEPU(w/2f, 0, 0), w, 20, 10));
            space.Add(new Box(new Vector3BEPU(w, 0, h/2f), 10, 20, h));
            space.Add(new Box(new Vector3BEPU(w/2f, 0, h), w, 20, 10));
            space.Add(new Box(new Vector3BEPU(0, 0, h/2f), 10, 20, h));


            ground = new Box(new Vector3BEPU(w / 2f, -2, h / 2f), w, 4, h);
            
            space.Add(ground);
            carBox = new Box(new Vector3BEPU(100, 0, 100), carWidth, 10, carHeight, 1);
            //carBox.AngularVelocity = new Vector3BEPU(0, 1, 0);

            barruer = new Box(new Vector3BEPU(w/2, 0, h/2), 200, 20, 50);
            space.Add(barruer);
            

            Vector3BEPU center = new Vector3BEPU(carBox.Width, 0, carBox.Height);
            Vector3BEPU vel = new Vector3BEPU(10, 0, -10);
            Vector3BEPU vel1 = new Vector3BEPU(10, 0, -10);
            //carBox.ApplyImpulse(ref center, ref vel);
            //carBox.ApplyAngularImpulse(ref vel1);

            space.Add(carBox);
            //space.Add(new Box(new Vector3BEPU(0, 8, 0), 10, 10, 10, 1));
            //space.Add(new Box(new Vector3BEPU(0, 12, 0), 10, 10, 10, 1));

            space.ForceUpdater.Gravity = new Vector3BEPU(0, -9.81f, 0);





        }



        /// <summary>
        /// Handle keys for each demo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InputDevice_KeyDown(object sender, Fusion.Input.InputDevice.KeyEventArgs e)
        {
            if (e.Key == Keys.F1)
            {
                ShowEditor();
            }

            if (e.Key == Keys.F5)
            {
                BuildContent();
                Content.Reload<Texture2D>();
            }

            if (e.Key == Keys.F7)
            {
                BuildContent();
                Content.ReloadDescriptors();
            }

            if (e.Key == Keys.F12)
            {
                GraphicsDevice.Screenshot();
            }

            if (e.Key == Keys.Escape)
            {
                Exit();
            }
        }



        /// <summary>
        /// Save configuration on exit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FusionGame_Exiting(object sender, EventArgs e)
        {
            SaveConfiguration();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            var ds = GetService<DebugStrings>();

            ds.Add(Color.Orange, "FPS {0}", gameTime.Fps);
            ds.Add("F1   - show developer console");
            ds.Add("F5   - build content and reload textures");
            ds.Add("F12  - make screenshot");
            ds.Add("ESC  - exit");

            space.Update(gameTime.ElapsedSec);

            for (int playerIndex = 0; playerIndex <= 3; playerIndex++)
            {
                var gp = InputDevice.GetGamepad(playerIndex);

                if (gp.IsConnected)
                {
                    var impulse = new Vector3BEPU(gp.RightStick.X, 0, -gp.RightStick.Y);
                    carBox.ApplyLinearImpulse(ref impulse);
                    //ds.Add("Car: y={0}", carBox.Position.Y);
                    ds.Add(Color.Red, "Car: x={0}, y={1}, z={2}, w={2}", ((int)(carBox.Orientation.X * 100)), (carBox.Orientation.W < 0 ? -1 : 1) * ((int)(carBox.Orientation.Y * 100)) / 100f, ((int)carBox.Orientation.Z * 100) / 100f, ((int)(carBox.Orientation.W * 100)) / 100f);
                    ds.Add(Color.Red, "Car: W={0}", carBox.Orientation.W);
                    ds.Add(Color.Red, "Car: Y={0}", carBox.Orientation.Y);
                    ds.Add("angle = {0}", (float)(Math.Acos(carBox.Orientation.W) * 2));

                    var impulse1 = new Vector3BEPU(gp.LeftStick.X, gp.LeftStick.X, gp.LeftStick.X);
                    //carBox.ApplyAngularImpulse(ref impulse1);

                    ds.Add(Color.LightGreen, "Gamepad #{0} is connected", playerIndex);

                    ds.Add(" - Left stick    : {0} {1}", gp.LeftStick.X, gp.LeftStick.Y);
                    ds.Add(" - Right stick   : {0} {1}", gp.RightStick.X, gp.RightStick.Y);
                    ds.Add(" - Left trigger  : {0} (left motor)", gp.LeftTrigger);
                    ds.Add(" - Right trigger : {0} (right motor)", gp.RightTrigger);

                    gp.SetVibration(gp.LeftTrigger, gp.RightTrigger);

                    if (gp.IsKeyPressed(GamepadButtons.X)) ds.Add(Color.Blue, "[X]");
                    if (gp.IsKeyPressed(GamepadButtons.Y)) ds.Add(Color.Yellow, "[Y]");
                    if (gp.IsKeyPressed(GamepadButtons.A)) ds.Add(Color.Green, "[A]");
                    if (gp.IsKeyPressed(GamepadButtons.B)) ds.Add(Color.Red, "[B]");

                    if (gp.IsKeyPressed(GamepadButtons.LeftShoulder)) ds.Add("[LS]");
                    if (gp.IsKeyPressed(GamepadButtons.RightShoulder)) ds.Add("[RS]");
                    if (gp.IsKeyPressed(GamepadButtons.LeftThumb)) ds.Add("[LT]");
                    if (gp.IsKeyPressed(GamepadButtons.RightThumb)) ds.Add("[RT]");

                    if (gp.IsKeyPressed(GamepadButtons.Back)) ds.Add("[Back]");
                    if (gp.IsKeyPressed(GamepadButtons.Start)) ds.Add("[Start]");

                    if (gp.IsKeyPressed(GamepadButtons.DPadLeft)) ds.Add("[Left]");
                    if (gp.IsKeyPressed(GamepadButtons.DPadRight)) ds.Add("[Right]");
                    if (gp.IsKeyPressed(GamepadButtons.DPadDown)) ds.Add("[Down]");
                    if (gp.IsKeyPressed(GamepadButtons.DPadUp)) ds.Add("[Up]");

                }
                else
                {

                    ds.Add(Color.Red, "Gamepad #{0} is diconnected", playerIndex);

                }
            }

            base.Update(gameTime);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="stereoEye"></param>
        protected override void Draw(GameTime gameTime, StereoEye stereoEye)
        {
            var sb = GetService<SpriteBatch>();

            sb.Begin();

            sb.Draw(tex, 0, 0, ground.Width, ground.Length, Color.White);
            sb.DrawSprite(tex, barruer.Position.X, barruer.Position.Z, barruer.Width, barruer.Length, 0, Color.White);
            var cos = (carBox.Orientation.Y < 0)
                ? -carBox.Orientation.W
                : carBox.Orientation.W;
            sb.DrawSprite(carTex, carBox.Position.X, carBox.Position.Z, carBox.Width, carBox.Length, (float)(Math.Acos(cos) * 2), Color.White);
                
            sb.End();

            base.Draw(gameTime, stereoEye);
        }
    }
}
