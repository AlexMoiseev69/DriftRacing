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

        private Space space;
        private List<User> users = new List<User>(); 

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
            LoadTextures();
            InitializeUzers();

            for (int i = 0; i < 100; i++)
            {
                space.Update(100f);
            }

            //	add keyboard handler :
            InputDevice.KeyDown += InputDevice_KeyDown;
        }

        private void LoadTextures()
        {
            tex = Content.Load<Texture2D>("images/route_1.png");

            carTex = Content.Load<Texture2D>("images/car1.png");

            Surfece = new Bitmap("C:/dududko/DriftRacing/DriftRacer/Content/images/route_1_mh.png");

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

            foreach (var user in users)
            {
                user.Update(gameTime);/*
                if (!user.Update(gameTime.ElapsedSec))
                {
                    users.Remove(user);
                }*/
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
            sb.End();
            foreach (User user in users)
            {
                user.Draw(sb);
            }

            base.Draw(gameTime, stereoEye);
        }

        private void InitializeUzers()
        {
            for (int i = 0; i < 4; i++)
            {
                var gp = InputDevice.GetGamepad(i);
                if (gp.IsConnected)
                {
                    var car = new Car(new Vector3BEPU(100 * (i+1), 30, 50 * (i+1)), space, carTex);
                    var name = "user " + i;
                    User user = new User(name, car, gp);
                    users.Add(user);
                }
            }
        }
    }
}
