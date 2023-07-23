using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slipperysnake.Objects
{
    public class SnakePart
    {
        public Vector2 position;
        public bool move = false;
        public float timeToMove = 0.125f;
        private float currentTime = 0;
        public int index;
        public float scale = 3;

        public float eatAnimTimer = 0;
        public bool eatAnim = false;
        public int IoD = 1;
        public bool endAnim = false;
        public float endAnimTimer = 0;

        private List<SnakePart> snake;
        private int myIndex;
        private Game1 game;

        public SnakePart(Vector2 position, int index, Game1 game) {
            this.position = position;
            this.index = index;
            this.game = game;
        }

        public void Update(GameTime gametime, List<Vector2> snake_history, Vector2 direction)
        {
            if (!move)
            {
                currentTime += (float)gametime.ElapsedGameTime.TotalSeconds;
                if(currentTime >= timeToMove)
                {
                    move = true;
                }
            }
            else
            {
                index++;
                this.position = snake_history[index];
            }

            Animate(gametime, direction);
        }

        public void PlayEatAnim(List <SnakePart> snake, int index)
        {
            this.snake = snake;
            eatAnim = true;
            eatAnimTimer = 0;
            scale = 3;
            IoD = 1;
            myIndex = index;
        }

        public void PlayEndAnim(List <SnakePart> snake, int index)
        {
            this.snake = snake;
            endAnim = true;
            endAnimTimer = 0;
            myIndex = index;
            eatAnim = false;
            scale = 3;
        }

        public void Animate(GameTime gametime, Vector2 direction)
        {
            if (eatAnim)
            {
                eatAnimTimer += (float)gametime.ElapsedGameTime.TotalSeconds;
                scale += (float)gametime.ElapsedGameTime.TotalSeconds * 10 * IoD;
                if (eatAnimTimer > 0.15)
                {
                    IoD = -1;
                }
                if (eatAnimTimer > 0.3)
                {
                    eatAnim = false;
                    if(!(myIndex+1 > snake.Count() - 1))
                    {
                        snake[myIndex + 1].PlayEatAnim(snake, myIndex+1);
                    }
                }
            }
            if (endAnim)
            {
                endAnimTimer += (float)gametime.ElapsedGameTime.TotalSeconds;
                position -= direction*2;
                if(endAnimTimer > 0.10)
                {
                    endAnim = false;
                    if(!(myIndex+1 > snake.Count() - 1))
                    {
                        snake[myIndex + 1].PlayEndAnim(snake, myIndex + 1);
                    }
                    else
                    {
                        game.Reset();
                    }
                }
            }
        }
    }
}
