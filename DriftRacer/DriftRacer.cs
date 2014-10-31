using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using BEPUphysics;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Vehicle;
using BEPUutilities;
using Fusion.UserInterface;
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
    public enum GameState
    {
        Play = 1,
        Pause = 2
    }

    public class DriftRacer : Game
    {
        public GameState GameState;
        private Space space;
        private Box tribunes;
        private Box[] borders;
        private List<User> users = new List<User>(); 

        private Texture2D tex;
        private Texture2D tribuneTex;
        private Texture2D borderTex;
        public static Bitmap Surfece;

        private int mapId = -1;

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
            Parameters.Width = 1024;
            Parameters.Height = 720;
            Parameters.Title = "Drift Racing";
            Parameters.Developer = true;

            //	enable debug graphics device in Debug :
#if DEBUG
				Parameters.UseDebugDevice	=	true;
#endif

            //	add services :
            AddService(new SpriteBatch(this), false, false, 0, 0);
            AddService(new DebugStrings(this), true, true, 9999, 9999);
            AddService(new DebugRender(this), true, true, 9998, 9998);
            AddService(new UserInterface(this, "segoe40"), true, true, 5000, 5000);
            AddService(new ConfigService(this), true, true, 9998, 9998);

            //	load configuration for each service :
            //LoadConfiguration();
            
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
            LoadTextures();
            InitPhysics();
            InitializeUzers();

            // there is a small bug with wheels on the game start
            // it is fixed with several iterations in physics at the begining 
            for (int i = 0; i < 100; i++) {
                space.Update(100f);
            }

            //	add keyboard handler :
            InputDevice.KeyDown += InputDevice_KeyDown;

            var ui = GetService<UserInterface>();
            ui.RootFrame = new Frame(this, 0, 0, 1280, 720, "", Color.Zero);


            SpriteFont font = Content.Load<SpriteFont>("stencil");
            var btn1 = new Frame(this, Parameters.Width / 2, 100, 0, 0, "Click \"BACK\" to change map", Color.Red) {
                Border = 1,
                BorderColor = Color.White,
                TextAlignment = Alignment.MiddleCenter,
                Font = font,
            };

            btn1.TextAlignment = Alignment.MiddleCenter;

<<<<<<< HEAD
            Surfece = new Bitmap("C:/Users/Yaroslav/Documents/DriftRacing/DriftRacer/Content/images/route_1_mh.png");
        }
=======
>>>>>>> origin/master

            ui.RootFrame.Add(btn1);

            var btn2 = new Frame(this, Parameters.Width / 2, 200, 0, 0, "or click \"START\" to continur", Color.Red)
            {
                Border = 1,
                BorderColor = Color.White,
                TextAlignment = Alignment.MiddleCenter,
                Font = font,
            };

            btn2.TextAlignment = Alignment.MiddleCenter;


            ui.RootFrame.Add(btn2);

            ui.Visible = false;
            GameState = GameState.Play;

            SetNextMap();
        }

        /// <summary>
        /// Loads surface texture. It is necessery to detect the type of surface wheel stands on
        /// </summary>
        private void LoadTextures()
        {
            var path = Environment.CurrentDirectory;
            path = path.Replace("bin\\x64\\Debug", GetService<ConfigService>().TrackConfig.Tracks[1].TrackSurfaceTexture);
            Surfece = new Bitmap(path);

            tribuneTex = Content.Load<Texture2D>("images/tribunes.png");
            borderTex = Content.Load<Texture2D>("images/border.png");
        }

        private void InitPhysics()
        {
            int w = GraphicsDevice.Viewport.Width;
            int h = GraphicsDevice.Viewport.Height;
            // init space
            space = new Space();

            // add up, right, down, left borders
            space.Add(new Box(new Vector3BEPU(w / 2f    , 0     , -5    ), w    , 20, 10    ));
            space.Add(new Box(new Vector3BEPU(w + 5     , 0     , h / 2f), 10   , 20, h     ));
            space.Add(new Box(new Vector3BEPU(w / 2f    , 0     , h + 5 ), w    , 20, 10    ));
            space.Add(new Box(new Vector3BEPU(-5        , 0     , h / 2f), 10   , 20, h     ));
            // add ground
            space.Add(new Box(new Vector3BEPU(w / 2f    , -10   , h / 2f), w    , 20, h     ));

            tribunes = new Box(new Vector3BEPU(w / 2f   , 0     , h / 4f), tribuneTex.Width, 20, tribuneTex.Height * 4f / 5f);
            space.Add(tribunes);

            borders = new Box[1];
            borders[0] = new Box(new Vector3BEPU(w / 2f - 10f, 0, h / 4f), w * 2f / 3f, 20, 20);
            space.Add(borders[0]);
            // set gravity vector
            space.ForceUpdater.Gravity = new Vector3BEPU(0, -9.81f, 0f);
        }

        private void InitializeUzers()
        {
            for (int i = 0; i < 4; i++)
            {
                var gp = InputDevice.GetGamepad(i);
                if (gp.IsConnected) {
                    var carPosition = GetService<ConfigService>().TrackConfig.Tracks[1].CarsStartPositions[i];
                    var car = new Car(new Vector3BEPU(carPosition.X, 30, carPosition.Y), space, Content.Load<Texture2D>("images/car" + (i + 1) + ".png"));
                    var name = "user " + i;
                    User user = new User(name, car, gp);
                    users.Add(user);
                }
            }
        }


        /// <summary>
        /// Handle keys for each demo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InputDevice_KeyDown(object sender, Fusion.Input.InputDevice.KeyEventArgs e)
        {
            if (e.Key == Keys.F1) {
                ShowEditor();
            }

            if (e.Key == Keys.F5) {
                BuildContent();
                Content.Reload<Texture2D>();
            }

            if (e.Key == Keys.F7) {
                BuildContent();
                Content.ReloadDescriptors();
            }

            if (e.Key == Keys.F12) {
                GraphicsDevice.Screenshot();
            }

            if (e.Key == Keys.Escape) {
                //tex.Dispose();
                //Surfece.Dispose();
                //GetService<ConfigService>();
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


        private float changeFlagTime = 1f;
        private float changeFlagTimeLeft = 1f;
        private bool changeFlag = true;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            var ds = GetService<DebugStrings>();

            if (!changeFlag && (changeFlagTimeLeft -= gameTime.ElapsedSec) <= 0)
            {
                changeFlag = true;
                changeFlagTimeLeft = changeFlagTime;
            }

            ds.Add(Color.Orange, "FPS {0}", gameTime.Fps);
            ds.Add("F1   - show developer console");
            ds.Add("F5   - build content and reload textures");
            ds.Add("F12  - make screenshot");
            ds.Add("ESC  - exit");

            if (GameState != GameState.Pause) {
                space.Update(gameTime.ElapsedSec*10f);
            }

            foreach (var user in users)
            {
                if (user.gp.IsKeyPressed(GamepadButtons.None)) {
                    Log.Error("error");
                }
                switch (GameState) 
                {
                    case GameState.Pause:
                        if (user.gp.IsKeyPressed(GamepadButtons.Start)) {
                            GameState = GameState.Play;
                            GetService<UserInterface>().Visible = false;
                        }
                        if (user.gp.IsKeyPressed(GamepadButtons.Back) && changeFlag) {
                            SetNextMap();
                            changeFlag = false;
                        }
                        user.gp.SetVibration(0, 0);
                        break;
                    case GameState.Play:
                        if (user.gp.IsKeyPressed(GamepadButtons.Back)) {
                            GameState = GameState.Pause;
                            GetService<UserInterface>().Visible = true;
                            changeFlag = false;
                        }
                        user.Update(gameTime);
                        break;
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
            sb.Draw(tex, 0, 0, Parameters.Width, Parameters.Height, Color.White);
            if (mapId == 1) {
                foreach (var border in borders) {
                    sb.Draw(borderTex, border.Position.X - border.Width / 2, border.Position.Z - border.Length / 2,
                       border.Width, border.Length, Color.White);
                }

                sb.Draw(tribuneTex, tribunes.Position.X - tribunes.Width/2, tribunes.Position.Z - tribunes.Length/2,
                    tribunes.Width, tribunes.Length, Color.White);
            }
            sb.End();
            foreach (User user in users) {
                user.Draw(sb);
            }

            base.Draw(gameTime, stereoEye);
        }

        private void SetNextMap()
        {
            mapId = (++mapId) % 2;
            Track track = GetService<ConfigService>().TrackConfig.Tracks[mapId];
            tex = Content.Load<Texture2D>(track.TrackTexture);

            var path = Environment.CurrentDirectory;
            path = path.Replace("bin\\x64\\Debug", GetService<ConfigService>().TrackConfig.Tracks[mapId].TrackSurfaceTexture);
            Surfece = new Bitmap(path);

            int i = 0;
            foreach (var user in users) {
                user.MoveToPosition(track.CarsStartPositions[i++]);
            }

            if (mapId == 0) {
                foreach (var border in borders) {
                    space.Remove(border);
                }
                space.Remove(tribunes);
            }
            else {
                foreach (var border in borders) {
                    space.Add(border);
                }
                space.Add(tribunes);
            }
            for (int j = 0; j < 100; j++) {
                space.Update(100f);
            }
        }
    }
}
