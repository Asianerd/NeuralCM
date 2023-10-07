using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NeuralCellularAutomata
{
    public class Main : Game
    {
        public static Main Instance = null;

        private GraphicsDeviceManager _graphics;
        public SpriteBatch _spriteBatch;
        static SpriteFont font;
        static SpriteFont mono_font;
        static Texture2D blank;

        public static Random random = new Random();
        public static Point screen_size = new Point(1920, 1080);
        //public static Point screen_size = new Point(200, 200);

        static bool showDebug = false;
        static float fpsIncrement = 0;
        static float fps = 0;

        static bool drawIncrement = false;

        public static int algo_title = 120;

        public Main()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            _graphics = new GraphicsDeviceManager(this);

            _graphics.PreferredBackBufferWidth = screen_size.X;
            _graphics.PreferredBackBufferHeight = screen_size.Y;

            IsFixedTimeStep = false;

            if (screen_size == new Point(1920, 1080))
            {
                _graphics.ToggleFullScreen();
            }

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += (s, e) =>
            {
                OnScreenResize(_graphics);
            };

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            OnScreenResize(_graphics);
        }

        public static void OnScreenResize(GraphicsDeviceManager g)
        {
            screen_size = new Point(
                g.PreferredBackBufferWidth,
                g.PreferredBackBufferHeight
                );
            Tile.OnScreenResize();
        }

        protected override void Initialize()
        {
            Input.Initialize(new List<Keys>() {
                Keys.F3,
                Keys.F11,
                Keys.R,
                Keys.E,
                Keys.W,
                Keys.T,
                Keys.Q,
                Keys.A,
                Keys.G
            });
            Tile.Initialize();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Tile.LoadContent(Content.Load<Texture2D>("blank"));
            font = Content.Load<SpriteFont>("font");
            mono_font = Content.Load<SpriteFont>("mono_font");
            blank = Content.Load<Texture2D>("blank");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Input.UpdateAll(Keyboard.GetState());

            if (algo_title > 0)
            {
                algo_title--;
            }

            if (Input.collection[Keys.F3].active)
            {
                showDebug = !showDebug;
            }
            if (Input.collection[Keys.R].active || !Input.collection[Keys.E].isPressed)
            {
                Tile.Update();
            }
            if (Input.collection[Keys.A].active)
            {
                for (int x = 0; x < Tile.grid_size.X; x++)
                {
                    for (int y = 0; y < Tile.grid_size.Y; y++)
                    {
                        Tile.grid[x, y] = 0f;
                    }
                }
            }
            if (Input.collection[Keys.Q].active)
            {
                Tile.OnScreenResize();
            }
            if (Input.collection[Keys.F11].active)
            {
                if (!_graphics.IsFullScreen)
                {
                    _graphics.PreferredBackBufferWidth = 1920;
                    _graphics.PreferredBackBufferHeight = 1080;
                }
                _graphics.ToggleFullScreen();
            }

            if (Input.collection[Keys.T].active || Input.collection[Keys.G].active)
            {
                int i = Enum.GetValues(typeof(Tile.SimulationSpecies)).Cast<Tile.SimulationSpecies>().ToList().IndexOf(Tile.species);
                if (Input.collection[Keys.T].active)
                {
                    i++;
                    if (i >= Enum.GetValues(typeof(Tile.SimulationSpecies)).Length)
                    {
                        i = 0;
                    }
                }
                if (Input.collection[Keys.G].active)
                {
                    i--;
                    if (i < 0)
                    {
                        i = 0;
                    }
                }

                Tile.species = Enum.GetValues(typeof(Tile.SimulationSpecies)).Cast<Tile.SimulationSpecies>().ToList()[i];
                Tile.currentFilter = Tile.filter[Tile.species];

                Tile.UpdateProfiles();
            }

            MouseState state = Mouse.GetState();

            #region Mouse actions
            if ((state.LeftButton == ButtonState.Pressed) && Input.collection[Keys.W].isPressed)
            {
                for (int y = -5; y <= 5; y++)
                {
                    for (int x = -5; x <= 5; x++)
                    {
                        Tile.grid[
                            Tile.fixedX((int)(state.Position.X / Tile.tile_size) + x),
                            Tile.fixedY((int)(state.Position.Y / Tile.tile_size) + y)
                            ] = Tile.currentProfile.Item4 ? 1f : random.Next(0, 1000) * 0.001f;
                    }
                }
            }
            else
            {
                if (state.LeftButton == ButtonState.Pressed)
                {
                    for (int y = -5; y <= 5; y++)
                    {
                        for (int x = -5; x <= 5; x++)
                        {
                            Tile.grid[
                                Tile.fixedX((int)(state.Position.X / Tile.tile_size) + x),
                                Tile.fixedY((int)(state.Position.Y / Tile.tile_size) + y)
                                ] = 0f;
                        }
                    }
                }
            }

            /*if (state.LeftButton == ButtonState.Pressed)
            {
                for (int y = -5; y <= 5; y++)
                {
                    for (int x = -5; x <= 5; x++)
                    {
                        Tile.grid[
                            Tile.fixedX((int)(state.Position.X / Tile.tile_size) + x),
                            Tile.fixedY((int)(state.Position.Y / Tile.tile_size) + y)
                            ] = 0f;
                    }
                }
            }

            if (state.RightButton == ButtonState.Pressed)
            {
                for (int y = -5; y <= 5; y++)
                {
                    for (int x = -5; x <= 5; x++)
                    {
                        Tile.grid[
                            Tile.fixedX((int)(state.Position.X / Tile.tile_size) + x),
                            Tile.fixedY((int)(state.Position.Y / Tile.tile_size) + y)
                            ] = Tile.currentProfile.Item4 ? 1f : random.Next(0, 1000) * 0.001f;
                    }
                }
            }*/
            #endregion

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            drawIncrement = !drawIncrement;
            Tile.Render(drawIncrement);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            Tile.Draw();
            if (showDebug)
            {
                fpsIncrement++;
                if (fpsIncrement >= 60)
                {
                    fpsIncrement = 0;
                    fps = 1f / (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                string text = $"FPS : {fps}";
                Rectangle debugRect = new Rectangle(new Point(0, 0), font.MeasureString(text).ToPoint());
                _spriteBatch.Draw(blank, debugRect, Color.Black * 0.5f);
                _spriteBatch.DrawString(font, text, Vector2.Zero, Color.White);
            }

            _spriteBatch.DrawString(mono_font, Tile.currentProfile.Item1, new Vector2(50, 50), Color.White * Math.Clamp(((algo_title * 2f) / 120f), 0, 1));
            //_spriteBatch.Begin(samplerState:SamplerState.LinearClamp);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
