using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TWD
{
    public class Personagem : IDrawObject
    {
        public Personagem(Vector2 rect, Texture2D texture, string name)
        {
            Position = rect;
            Texture = texture;
            Name = name;
        }
        public Personagem(Vector2 rect, string name)
        {
            Position = rect;
            Name = name;
        }

        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }
        public string Name { get; set; }
        public bool Salvo { get; set; }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Rectangle(((int)Position.X * 15) -8, ((int)Position.Y * 15) -24, 32,32), Color.White);
        }
        public void Draw(SpriteBatch spriteBatch, float opacity)
        {
            if(!Salvo)
            spriteBatch.Draw(Texture, new Rectangle(((int)Position.X * 15) - 8, ((int)Position.Y * 15) - 24, 32, 32), Color.White * opacity);
        }
    }
}
