using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using slipperysnake.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace slipperysnake
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics; //manages graphics for game system
        private SpriteBatch _spriteBatch;

        private List<SnakePart> snake = new List<SnakePart>();
        private List<Vector2> snake_history = new List<Vector2>();
        private float snakeSpeed = 3.5f;
        private Vector2 food = Vector2.Zero;
        float score = 0;
        float highScore = 0;

        //sprites
        private Texture2D snake_head;
        private Texture2D snake_body;
        private Texture2D board;
        private Texture2D food_spr;
        private SpriteFont font;

        Vector2 ScreenCenter;
        Vector2 mousePos;
        float snakeHitbox = 5f;
        float foodHitbox = 20f;
        bool checkHits = false;
        float timer = 0f;
        bool playing = false;
        Vector2 dir;
        float rotation = (float)Math.PI/2;
        bool canStart = true;

        //dewbug line
        private Texture2D line;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {

            //get screen center
            ScreenCenter = new Vector2(GraphicsDevice.Viewport.Bounds.Width / 2, GraphicsDevice.Viewport.Bounds.Height / 2);

            //create head of snake
            snake.Add(new SnakePart(new Vector2(440, 360), 0, this));

            //add bodies
            snake.Add(new SnakePart(new Vector2(440, 360), 0, this));
            snake.Add(new SnakePart(new Vector2(440, 360), 0, this));
            snake[snake.Count() - 1].timeToMove *= 2; //set the amount to be doubled so it waits logner

            //randomzie food pos
            food = new Vector2(840, 360);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            board = Content.Load<Texture2D>("board");
            snake_head = Content.Load<Texture2D>("head");
            snake_body = Content.Load<Texture2D>("body");
            food_spr = Content.Load<Texture2D>("apple");
            font = Content.Load<SpriteFont>("File");

            //line
            line = new Texture2D(_graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            line.SetData(new[] { Color.White });
               
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState mouseState = Mouse.GetState();
            mousePos = new Vector2(mouseState.X, mouseState.Y);

            if (!playing && canStart)
            {
                if(mouseState.LeftButton == ButtonState.Pressed)
                {
                    playing = true;
                    canStart = false;
                }
            }

            if (playing)
            {
                MoveSnake(gameTime);
            }
            else
            {
                for(int i = 0; i < snake.Count(); i++)
                {
                    snake[i].Animate(gameTime, dir);
                }
            }
            base.Update(gameTime);
        }

        void MoveSnake(GameTime gameTime)
        {
            //move head
            snake_history.Add(snake[0].position);
            dir = Vector2.Normalize((mousePos - snake[0].position));
            snake[0].position += dir * snakeSpeed;

            snake[0].Animate(gameTime, dir);
            for (int i = 1; i < snake.Count(); i++)
            {
                snake[i].Update(gameTime, snake_history, dir);
            }

            //collisions
            //food
            if (Vector2.Distance(snake[0].position, food) <= snakeHitbox + foodHitbox)
            {
                RandomizeFood();
                snake.Add(new SnakePart(snake[snake.Count() - 1].position, snake[snake.Count() - 1].index, this));
                PlayEatAnimation();
                score++;
            }

            //check if body should checkhits
            if(checkHits == false)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timer >= 0.5)
                {
                    checkHits = true;
                }
            }


            //body
            if (checkHits)
            {
                for (int i = 1; i < snake.Count(); i++)
                {
                    if (Vector2.Distance(snake[0].position, snake[i].position) <= snakeHitbox + snakeHitbox)
                    {
                        PlayEndAnimation();
                    }
                }

            }

            //edgecollisions
            if (snake[0].position.X + snakeHitbox * 3 > 940)
            {
                PlayEndAnimation();
            }
            else if (snake[0].position.X - snakeHitbox*3 < 340)
            {
                PlayEndAnimation();
            }
            else if (snake[0].position.Y + snakeHitbox*3 > 660)
            {
                PlayEndAnimation();
            }
            else if (snake[0].position.Y - snakeHitbox*3 < 60)
            {
                PlayEndAnimation();
            }


            //make the array smaller so not to keep 1000000000 billion elements!
            clearArray();
            //Debug.WriteLine(snake_history.Count());
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(38,92,66));

            _spriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp);

            //draw board
            int boardScale = 10;
            _spriteBatch.Draw(board, new Vector2(ScreenCenter.X - board.Width/2*boardScale, ScreenCenter.Y - board.Height/2* boardScale), null, Color.White, 0, new Vector2(0,0), boardScale, SpriteEffects.None, 0);

            //draw food
            int foodScale = 3;
            _spriteBatch.Draw(food_spr, new Vector2(food.X - food_spr.Width / 2 * foodScale, food.Y - food_spr.Height / 2 * foodScale), null, Color.White, 0, new Vector2(0, 0), foodScale, SpriteEffects.None, 0);

            //draw head of snake
            if (playing)
            {
                rotation = (float)Math.Atan2(mousePos.Y - snake[0].position.Y, mousePos.X - snake[0].position.X) + (float)Math.PI/2;
            }
            _spriteBatch.Draw(snake_head, new Vector2(snake[0].position.X, snake[0].position.Y), null, Color.White, rotation, new Vector2(snake_head.Width/2, snake_head.Height/2), snake[0].scale, SpriteEffects.None, 0);

            //draw all body parts
            for(int i = 1; i < snake.Count(); i++)
            {
                _spriteBatch.Draw(snake_body, new Vector2(snake[i].position.X, snake[i].position.Y), null, Color.White, 0, new Vector2(snake_body.Width / 2, snake_body.Height / 2), snake[i].scale, SpriteEffects.None, 0);
            }

            //debug 
            _spriteBatch.DrawString(font, "X: " + mousePos.X.ToString() + " Y: " + mousePos.Y.ToString(), new Vector2(10, 10), Color.White);
            _spriteBatch.DrawString(font, "Score: " + score, new Vector2(10, 30), Color.White);
            _spriteBatch.DrawString(font, "High Score: " + highScore, new Vector2(10, 50), Color.White);

            if (!playing)
            {
                _spriteBatch.DrawString(font, "Click anywhere to play!", new Vector2(ScreenCenter.X, ScreenCenter.Y + 200), Color.White, 0, font.MeasureString("Click anywhere to play!")/2, Vector2.One, SpriteEffects.None, 0, false);
            }

/*            float angleOfLine = (float)Math.Atan2(food.Y - snake[0].position.Y, food.X - snake[0].position.X);
            DrawLine(_spriteBatch, snake[0].position, food, Color.White, 1);*/

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        void clearArray()
        {
            snake_history.RemoveRange(0, snake[snake.Count() - 1].index);
            for(int i = 1; i < snake.Count(); i++)
            {
                snake[i].index -= snake[snake.Count() - 1].index;
            }
        }

        void RandomizeFood()
        {
            Random random = new Random();

            bool broken = false;
            while (!broken)
            {
                Vector2 temp = new Vector2(random.Next(378, 898), random.Next(99, 620));

                bool hit = false;
                for(int i = 0; i < snake.Count(); i++)
                {
                    if(Vector2.Distance(temp, snake[i].position) < snakeHitbox + foodHitbox)
                    {
                        hit = true;
                    }
                }
                if (!hit)
                {
                    broken = true;
                    food = temp;
                }
            }

        }

        void DrawLine(SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color, int width = 1)
        {
            Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
            Vector2 v = Vector2.Normalize(begin - end);
            float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
            if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
            spriteBatch.Draw(line, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }

        void PlayEatAnimation()
        {
            snake[0].PlayEatAnim(snake,0);
        }

        void PlayEndAnimation()
        {
            playing = false;
            snake[0].PlayEndAnim(snake, 0);
        }

        public void Reset()
        {
            canStart = true;

            if(score > highScore)
            {
                highScore = score;
            }
            score = 0;

            snake.RemoveRange(1, snake.Count() - 1);
            snake[0].position = new Vector2(440, 360);
            snake_history.Clear();
            timer = 0;
            checkHits = false;
            snake[0].endAnim = false;
            snake.Add(new SnakePart(new Vector2(440, 360), 0, this));
            rotation = (float)Math.PI/2;
            snake.Add(new SnakePart(new Vector2(440, 360), 0, this));
            snake[snake.Count() - 1].timeToMove *= 2; //set the amount to be doubled so it waits logner

            //randomzie food pos
            food = new Vector2(840, 360);
        }
    }
}
