using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Vehicle;
using BEPUutilities;
using Vector3BEPU = BEPUutilities.Vector3;
using MatrixBEPU = BEPUutilities.Matrix;
using Vertex = Fusion.Graphics.Mesh.Vertex;
using Fusion;
using Fusion.Audio;
using Fusion.Content;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Utils;
using Color = SharpDX.Color;
using Quaternion = BEPUutilities.Quaternion;
using Vector3 = SharpDX.Vector3;

namespace DriftRacer
{
    public class DriftRacer : Game
    {

        /// <summary>
        /// Speed that the Vehicle tries towreach when moving backward.
        /// </summary>
        public float BackwardSpeed = -13;

        /// <summary>
        /// Speed that the Vehicle tries to reach when moving forward.
        /// </summary>
        public float ForwardSpeed = 130;

        /// <summary>
        /// Maximum turn angle of the wheels.
        /// </summary>
        public float MaximumTurnAngle = (float)Math.PI / 6;

        /// <summary>
        /// Turning speed of the wheels in radians per second.
        /// </summary>
        public float TurnSpeed = MathHelper.Pi;


        private Space space;
        private Car[] cars = new Car[4];

        private float vWidth = 2.5f * 10f;
        private float vHeight = 4.5f * 10f;
        private Box ground;

        private Texture2D tex;
        public static Bitmap Surfece;
        private Texture2D carTex;

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
            //device.FullScreen = true;

            base.Initialize();
            InitPhysics();

            addCars();


            tex = Content.Load<Texture2D>("images/route_1.png");

            carTex = Content.Load<Texture2D>("images/car1.png");
            var x1 = carTex.SizeRcpSize.X;

            //	add keyboard handler :
            InputDevice.KeyDown += InputDevice_KeyDown;


            Surfece = new Bitmap("C:/Users/Yaroslav/Documents/DriftRacing/DriftRacer/Content/images/route_1_mh.png");
        }

        private void addCars()
        {
            for (int i = 0; i < 2; i++)
            {
                cars[i] = new Car(new Vector3BEPU(100, 10, 100), space);
            }
        }


        private void InitPhysics()
        {
            int w = GraphicsDevice.Viewport.Width;
            int h = GraphicsDevice.Viewport.Height;

            space = new Space();

            // add up, right, down, left borders
            space.Add(new Box(new Vector3BEPU(w/2f, 0, -5), w, 20, 10));
            space.Add(new Box(new Vector3BEPU(w + 5, 0, h/2f), 10, 20, h));
            space.Add(new Box(new Vector3BEPU(w/2f, 0, h+5), w, 20, 10));
            space.Add(new Box(new Vector3BEPU(-5, 0, h/2f), 10, 20, h));


            ground = new Box(new Vector3BEPU(w / 2f, -10, h / 2f), w, 20, h);
            
            space.Add(ground);

            space.ForceUpdater.Gravity = new Vector3BEPU(0, -9.81f, 0f);
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
            

            space.Update(gameTime.ElapsedSec * 10f);

            for (int playerIndex = 0; playerIndex <= 3; playerIndex++)
            {
                var gp = InputDevice.GetGamepad(playerIndex);

                if (gp.IsConnected)
                {
                    var impulse = new Vector3BEPU(gp.RightStick.X, 0, -gp.RightStick.Y);
                    //ds.Add("Car: y={0}", carBox.Position.Y);

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


                    if (gp.IsKeyPressed(GamepadButtons.A))
                    {
                        //Drive
                        gp.SetVibration(0, 100);
                        updateCarsParameters();


                        cars[playerIndex].Vehicle.Wheels[0].DrivingMotor.TargetSpeed = ForwardSpeed;
                        cars[playerIndex].Vehicle.Wheels[2].DrivingMotor.TargetSpeed = ForwardSpeed;
                    }
                    else if (gp.IsKeyPressed(GamepadButtons.B))
                    {
                        //Reverse
                        cars[playerIndex].Vehicle.Wheels[0].DrivingMotor.TargetSpeed = BackwardSpeed;
                        cars[playerIndex].Vehicle.Wheels[2].DrivingMotor.TargetSpeed = BackwardSpeed;
                    }
                    else
                    {
                        //Idle
                        cars[playerIndex].Vehicle.Wheels[0].DrivingMotor.TargetSpeed = 0;
                        cars[playerIndex].Vehicle.Wheels[2].DrivingMotor.TargetSpeed = 0;
                    }
                    if (gp.IsKeyPressed(GamepadButtons.RightShoulder))
                    {
                        //Brake
                        foreach (Wheel wheel in cars[playerIndex].Vehicle.Wheels)
                        {
                            wheel.Brake.IsBraking = true;
                        }
                    }
                    else
                    {
                        //Release brake
                        foreach (Wheel wheel in cars[playerIndex].Vehicle.Wheels)
                        {
                            wheel.Brake.IsBraking = false;
                        }
                    }
                    //Use smooth steering; while held down, move towards maximum.
                    //When not pressing any buttons, smoothly return to facing forward.
                    float angle;
                    bool steered = false;
                    if (gp.IsKeyPressed(GamepadButtons.DPadLeft))
                    {
                        steered = true;
                        angle = Math.Max(cars[playerIndex].Vehicle.Wheels[1].Shape.SteeringAngle - TurnSpeed * gameTime.ElapsedSec, -MaximumTurnAngle);
                        cars[playerIndex].Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                        cars[playerIndex].Vehicle.Wheels[3].Shape.SteeringAngle = angle;
                    }
                    if (gp.IsKeyPressed(GamepadButtons.DPadRight))
                    {
                        steered = true;
                        angle = Math.Min(cars[playerIndex].Vehicle.Wheels[1].Shape.SteeringAngle + TurnSpeed * gameTime.ElapsedSec, MaximumTurnAngle);
                        cars[playerIndex].Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                        cars[playerIndex].Vehicle.Wheels[3].Shape.SteeringAngle = angle;
                    }
                    if (!steered)
                    {
                        //Neither key was pressed, so de-steer.
                        if (cars[playerIndex].Vehicle.Wheels[1].Shape.SteeringAngle > 0)
                        {
                            angle = Math.Max(cars[playerIndex].Vehicle.Wheels[1].Shape.SteeringAngle - TurnSpeed * gameTime.ElapsedSec, 0);
                            cars[playerIndex].Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                            cars[playerIndex].Vehicle.Wheels[3].Shape.SteeringAngle = angle;
                        }
                        else
                        {
                            angle = Math.Min(cars[playerIndex].Vehicle.Wheels[1].Shape.SteeringAngle + TurnSpeed * gameTime.ElapsedSec, 0);
                            cars[playerIndex].Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                            cars[playerIndex].Vehicle.Wheels[3].Shape.SteeringAngle = angle;
                        }
                    }

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
            foreach (var car in cars)
            {
                if (car != null)
                {
                    var cos1 = (car.Vehicle.Body.Orientation.Y < 0)
                        ? -car.Vehicle.Body.Orientation.W
                        : car.Vehicle.Body.Orientation.W;
                    sb.DrawSprite(carTex, car.Vehicle.Body.Position.X, car.Vehicle.Body.Position.Z, vHeight, vWidth,
                        (float) (Math.Acos(cos1)*2 + Math.PI/2), Color.White);
                }
            }
            sb.End();

            base.Draw(gameTime, stereoEye);
        }

        private void updateCarsParameters()
        {
            foreach (var car in cars)
            {
                if (car != null)
                {

                    car.Update();
                }
            }
        }
    }
}
