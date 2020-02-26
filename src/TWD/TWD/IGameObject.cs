using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TWD
{
    interface IGameObject
    {
        void Load(ContentManager content);
        void Update(GameTime gametime);
        void Draw(SpriteBatch spriteBatch);
    }
    interface IDrawObject
    {
        void Draw(SpriteBatch spriteBatch);
    }
}
