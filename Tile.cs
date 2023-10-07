using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NeuralCellularAutomata
{
    class Tile
    {
        public static RenderTarget2D renderTarget;
        public static Texture2D drawn;

        public static Texture2D blank;
        public static float tile_size = 6;
        public static int blur_amount = 4;
        public static float blur_coefficient = 1f / (float)blur_amount;

        public static Point grid_size;

        public static List<float[,]> grids;
        public static float[,] grid;
        public static SimulationSpecies species = SimulationSpecies.Worm;

        public static Dictionary<SimulationSpecies, float[,]> filter = new Dictionary<SimulationSpecies, float[,]>();
        //                                            name, blur, color, int/float, persistance
        public static Dictionary<SimulationSpecies, (string, int, Color, bool, bool)> speciesProfile = new Dictionary<SimulationSpecies, (string, int, Color, bool, bool)>();
        public static Dictionary<SimulationSpecies, Func<float, float>> activationFunctions = new Dictionary<SimulationSpecies, Func<float, float>>()
        {
            [SimulationSpecies.Worm] = x => (-MathF.Pow(2.0f, (-0.6f * MathF.Pow(x, 2)))) + 1f,
            [SimulationSpecies.SlimeMold] = x => -1f / ((0.89f * MathF.Pow(x, 2)) + 1f) + 1f,
            [SimulationSpecies.GameOfLife] = x => (x == 3f || x == 11f || x == 12f) ? 1f : 0f,
            [SimulationSpecies.Rule30] = x => (x == 1f || x == 2f || x == 3f || x == 4f) ? 1f : 0f,
            [SimulationSpecies.Mitosis] = x => -1f / (0.9f * MathF.Pow(x, 2f) + 1f) + 1f,
            [SimulationSpecies.Pathways] = x => 1f / MathF.Pow(2f, (MathF.Pow(x - 3.5f, 2f)))
        /*[SimulationSpecies.Waves] = x => MathF.Abs(1.2f * x)*/
    };

        public static (string, int, Color, bool, bool) currentProfile;
        public static float[,] currentFilter;
        public static Func<float, float> currentFunction;

        public static int[] memoizedFilterCoordsX;
        public static int[] memoizedFilterCoordsY;
        public static int memoizationOffset = 2;

        public static void Initialize()
        {
            OnScreenResize(false);

            filter = new Dictionary<SimulationSpecies, float[,]>();
            speciesProfile = new Dictionary<SimulationSpecies, (string, int, Color, bool, bool)>();

            filter.Add(SimulationSpecies.Worm, new float[3, 3] {
                { 0.68f, -0.9f, 0.68f },
                { -0.9f, -0.66f, -0.9f },
                { 0.68f, -0.9f, 0.68f }
            });
            speciesProfile.Add(SimulationSpecies.Worm, ("Organelle simulation", 4, new Color(153, 229, 80), false, false));

            filter.Add(SimulationSpecies.SlimeMold, new float[3, 3] {
                { 0.8f, -0.85f, 0.8f },
                { -0.85f, -0.2f, -0.85f },
                { 0.8f, -0.85f, 0.8f }
            });
            speciesProfile.Add(SimulationSpecies.SlimeMold, ("Slime mold simulation", 2, new Color(255, 234, 0), false, false));

            filter.Add(SimulationSpecies.GameOfLife, new float[3, 3] {
                { 1f, 1f, 1f },
                { 1f, 9f, 1f },
                { 1f, 1f, 1f },
            });
            speciesProfile.Add(SimulationSpecies.GameOfLife, ("Conway's Game of Life", 1, new Color(255, 255, 255), true, false));

            filter.Add(SimulationSpecies.Rule30, new float[3, 3] {
                { 4f, 0f, 0f },
                { 2f, 0f, 0f },
                { 1f, 0f, 0f },
            });
            speciesProfile.Add(SimulationSpecies.Rule30, ("Wolfram's Rule 30", 1, new Color(255, 255, 255), true, true));

            filter.Add(SimulationSpecies.Mitosis, new float[3, 3] {
                { -0.939f, 0.88f, -0.939f },
                { 0.88f, 0.4f, 0.88f },
                { -0.939f, 0.88f, -0.939f },
            });
            speciesProfile.Add(SimulationSpecies.Mitosis, ("Cell mitosis simulation", 1, new Color(255, 234, 0), false, false));

            filter.Add(SimulationSpecies.Pathways, new float[3, 3] {
                { 0f, 1f, 0f },
                { 1f, 1f, 1f },
                { 0f, 1f, 0f }
            });
            speciesProfile.Add(SimulationSpecies.Pathways, ("Pheromonic simulation", 1, new Color(100, 256, 134), false, false));

            /*filter.Add(SimulationSpecies.Waves, new float[3, 3] {
                { 0.5645999908447266f, -0.7159000039100647f, 0.5645999908447266f },
                { -0.7159000039100647f, 0.6269000172615051f, -0.7159000039100647f },
                { 0.5645999908447266f, -0.7159000039100647f, 0.5645999908447266f },
            });
            speciesProfile.Add(SimulationSpecies.Waves, ("Progressive wave simulation", 1, new Color(0, 170, 255), false, false));*/

            UpdateProfiles();

            grid = new float[grid_size.X, grid_size.Y];
            memoizedFilterCoordsX = new int[grid_size.X + memoizationOffset * 3];
            memoizedFilterCoordsY = new int[grid_size.Y + memoizationOffset * 3];

            for (int x = 0; x < grid_size.X; x++)
            {
                for (int y = 0; y < grid_size.Y; y++)
                {
                    if (currentProfile.Item4)
                    {
                        grid[x, y] = Main.random.Next(-1, 2);
                        if (species == SimulationSpecies.Rule30)
                        {
                            grid[x, y] = 0;
                            if ((x == (grid_size.X / 2) && (y == 5))) {
                                grid[x, y] = 1;
                            }
                        }
                    }
                    else
                    {
                        grid[x, y] = Main.random.Next(0, 100) / 100f;
                    }
                    //grid[x, y] = Main.random.Next(0, 100) / 100f;
                }
            }

            for (int x = -1; x <= grid_size.X; x++)
            {
                memoizedFilterCoordsX[x + memoizationOffset] = fixedX(x);
            }
            for (int y = -1; y <= grid_size.Y; y++)
            {
                memoizedFilterCoordsY[y + memoizationOffset] = fixedY(y);
            }

            grids = new List<float[,]>();
            for (int x = 0; x < blur_amount; x++)
            {
                grids.Add(grid);
            }
        }

        public static void LoadContent(Texture2D b)
        {
            blank = b;
        }

        public static void Update()
        {
            float[,] old = grid.Clone() as float[,];
            //float[,] filterCoords = new float[3, 3];
            for (int x = 0; x < grid_size.X; x++)
            {
                for (int y = 0; y < grid_size.Y; y++)
                {
                    float total = 0;
                    for (int fx = -1; fx < 2; fx++)
                    {
                        for (int fy = -1; fy < 2; fy++)
                        {
                            /*total += filter[fx + 1, fy + 1] * old[
                                fixedX(x + fx),
                                fixedY(y + fy)
                                ];*/
                            total += currentFilter[fx + 1, fy + 1] * old[
                                memoizedFilterCoordsX[x + fx + memoizationOffset],
                                memoizedFilterCoordsY[y + fy + memoizationOffset]
                            ];
                        }
                    }
                    grid[x, y] = currentFunction(total);
                }
            }
            if (species == SimulationSpecies.Rule30)
            {
                grid[grid_size.X / 2, 5] = 1;
            }

            grids.Add(grid.Clone() as float[,]);
            grids.RemoveAt(0);
        }

        public static void UpdateProfiles()
        {
            currentFilter = filter[species];
            currentProfile = speciesProfile[species];
            currentFunction = activationFunctions[species];

            Main.algo_title = 240;

            blur_amount = currentProfile.Item2;
            blur_coefficient = 1f / (float)blur_amount;
        }

        /*public static float activationFunction(float x)
        {
            //return x;
            //return -1./ (0.89 * pow(x, 2.) + 1.) + 1.;
            //return -1f / ((0.89f * MathF.Pow(x, 2)) + 1f) + 1f;
            //return (-MathF.Pow(2.0f, (-0.6f * MathF.Pow(x, 2)))) + 1f;

            switch (species)
            {
                case SimulationSpecies.Worm:
                    return Worm(x);
                case SimulationSpecies.GameOfLife:
                    return GameOfLife(x);
                case SimulationSpecies.SlimeMold:
                    return SlimeMold(x);
                default:
                    return 0;
            }
        }*/

        public static int fixedX(int x)
        {
            /*if (x < 0)
            {
                return 0;
            }
            if (x >= grid_size.X)
            {
                return grid_size.X - 1;
            }
            return x;*/

            if ((x >= 0) && (x < grid_size.X))
            {
                return x;
            }
            if (x < 0)
            {
                // TODO : optimise this
                return (grid_size.X + (x % grid_size.X)) % grid_size.X; // modulos are expensive
                //return grid_size.X + x;
            }
            /*if (x >= grid_size.X)
            {*/
            return x % grid_size.X;
            //}
        }

        public static int fixedY(int y)
        {
            /*if (y < 0)
            {
                return 0;
            }
            if (y >= grid_size.Y)
            {
                return grid_size.Y - 1;
            }
            return y;*/

            if ((y >= 0) && (y < grid_size.Y))
            {
                return y;
            }
            if (y < 0)
            {
                // TODO : optimise this
                return (grid_size.Y + (y % grid_size.Y)) % grid_size.Y; // modulos are expensive
                //return grid_size.Y + y;
            }
            /*if (x >= grid_size.X)
            {*/
            return y % grid_size.Y;
            //}
        }

        public static void Render(bool draw = false)
        {
            if (!draw)
            {
                return;
            }

            Main.Instance.GraphicsDevice.SetRenderTarget(renderTarget);

            Main.Instance._spriteBatch.Begin();
            if (currentProfile.Item5)
            {
                Main.Instance._spriteBatch.Draw(drawn, Vector2.Zero, Color.White);
            }

            Vector2 recyleVector2 = new Vector2(0, 0);
            for (int x = 0; x < grid_size.X; x++)
            {
                /*recyleVector2.X = x * tile_size;*/
                recyleVector2.X = x;
                for (int y = 0; y < grid_size.Y; y++)
                {
                    /*recyleVector2.Y = y * tile_size;*/
                    recyleVector2.Y = y;
                    float total = 0;
                    if (currentProfile.Item2 == 1)
                    {
                        total = grid[x, y];
                    }
                    else
                    {
                        foreach (float[,] g in grids)
                        {
                            total += g[x, y];
                        }
                        total *= blur_coefficient;
                    }
                    if (total > 0)
                    {
                        Main.Instance._spriteBatch.Draw(blank, recyleVector2, currentProfile.Item3 * total);
                    }
                    /*else
                    {
                        Main.Instance._spriteBatch.Draw(blank, recyleVector2, Color.Red);
                    }*/
                    //Main.Instance._spriteBatch.Draw(blank, recyleVector2, currentProfile.Item3 * total);
                    //Main.Instance._spriteBatch.Draw(blank, recyleVector2, null, Color.White * total, 0f, Vector2.Zero, tile_size, SpriteEffects.None, 0f);
                }
            }
            Main.Instance._spriteBatch.End();

            Main.Instance.GraphicsDevice.SetRenderTarget(null);

            drawn = (Texture2D)renderTarget;

            /*Main.Instance._spriteBatch.Draw(drawn, Vector2.Zero, Color.White);*/
        }

        public static void Draw()
        {
            /*for (int x = 0; x <= Main.screen_size.X; x += grid_size.X)
            {
                for (int y = 0; y <= Main.screen_size.Y; y += grid_size.Y)
                {
                    Main.Instance._spriteBatch.Draw(drawn, new Vector2(x, y), Color.White);
                }
            }*/
            Main.Instance._spriteBatch.Draw(drawn, new Rectangle(0, 0, Main.screen_size.X, Main.screen_size.Y), Color.White);
        }

        public static void OnScreenResize(bool withInit = true)
        {
            grid_size = new Point(
                (int)(Main.screen_size.X / tile_size),
                (int)(Main.screen_size.Y / tile_size)
                );
            renderTarget = new RenderTarget2D(
                Main.Instance.GraphicsDevice,
                grid_size.X,
                grid_size.Y
                );
            if (!withInit)
            {
                return;
            }
            Initialize();
        }

        public enum SimulationSpecies
        {
            Worm,
            SlimeMold,
            GameOfLife,
            Rule30,
            Mitosis,
            Pathways
            /*Waves,*/
        }
    }
}
