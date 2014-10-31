using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Audio;
using Fusion.Content;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Utils;
using Fusion.UserInterface;
using Color = SharpDX.Color;


namespace DriftRacer
{
    internal class DrifterMenu : Game
    {
        public DrifterMenu()
            : base()
        {
            //	root directory for standard x64 C# application
            Parameters.ContentDirectory = @"..\..\..\Content";

            //	enable object tracking :
            Parameters.TrackObjects = true;

            //	enable developer console :
            Parameters.Developer = true;
            Parameters.Width = 1280;
            Parameters.Height = 720;
            Parameters.Title = "DriftRacing";

            //	enable debug graphics device in Debug :
#if DEBUG
            Parameters.UseDebugDevice = true;
#endif

            //	add services :
            AddService(new SpriteBatch(this), false, false, 0, 0);
            AddService(new DebugStrings(this), true, true, 9999, 9999);
            AddService(new DebugRender(this), true, true, 9998, 9998);
            AddService(new GameService(this), true, true, 1, 1);
            AddService(new UserInterface(this, "images/route.png"), true, true, 5000, 5000);

            //	add here additional services :

            //	load configuration for each service :
            LoadConfiguration();

            //	make configuration saved on exit :
            Exiting += DrifterMenu_Exiting;
        }
        //SpriteFont font;

        /// <summary>
        /// Add services :
        /// </summary>
        protected override void Initialize()
        {
            //font = Content.Load<SpriteFont>("stencil");


            var ui = GetService<UserInterface>();
            ui.RootFrame = new Frame(this, 0, 0, 1280, 720, "", Color.Zero);

            var btn1 = new Frame(this, 10, 100, 200, 40, "Button", Color.Red)
            {
                Border = 1,
                BorderColor = Color.White,
                TextAlignment = Alignment.MiddleCenter,
                //Font = font,
            };

            ui.RootFrame.Add(btn1);

            btn1.StatusChanged += (s, e) =>
            {
                if (e.Status == FrameStatus.None) btn1.BackColor = new Color(64, 64, 64, 255);
                if (e.Status == FrameStatus.Hovered) btn1.BackColor = new Color(96, 96, 96, 255);
                if (e.Status == FrameStatus.Pushed) btn1.BackColor = new Color(128, 128, 128, 255);
            };

            btn1.Click += RootFrame_Click;
            //ui.RootFrame.

            var btn2 = new Frame(this, 10, 150, 200, 40, "Button", Color.Red)
            {
                Border = 1,
                BorderColor = Color.White,
                TextAlignment = Alignment.MiddleCenter,
            };

            ui.RootFrame.Add(btn2);

            btn2.StatusChanged += (s, e) =>
            {
                if (e.Status == FrameStatus.None) btn2.BackColor = new Color(64, 64, 64, 255);
                if (e.Status == FrameStatus.Hovered) btn2.BackColor = new Color(96, 96, 96, 255);
                if (e.Status == FrameStatus.Pushed) btn2.BackColor = new Color(128, 128, 128, 255);
            };

            btn2.Click += (s, e) => { };// a = rand.NextFloat(0, 1) * MathUtil.Pi * 2;
            //ui.RootFrame.

            //	add keyboard handler :
            InputDevice.KeyDown += InputDevice_KeyDown;

            //	initialize services :
            base.Initialize();
        }

        Random rand = new Random();

        void RootFrame_Click(object sender, Frame.MouseEventArgs e)
        {
            //color = rand.NextColor();
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
        void DrifterMenu_Exiting(object sender, EventArgs e)
        {
            SaveConfiguration();
        }
       /* protected override void Update(GameTime gameTime)
        {
            var ds = GetService<DebugStrings>();

            ds.Add(Color.Orange, "FPS {0}", gameTime.Fps);
            ds.Add("F1   - show developer console");
            ds.Add("F5   - build content and reload textures");
            ds.Add("F12  - make screenshot");
            ds.Add("ESC  - exit");

            base.Update(gameTime);

            if (InputDevice.IsKeyDown(Keys.Left))
            {
                GetService<DrifterMenu>().a += 0.1f;
            }
            if (InputDevice.IsKeyDown(Keys.Right))
            {
                GetService<DrifterMenu>().a -= 0.1f;
            }
        }*/
    }
}

   
   


       