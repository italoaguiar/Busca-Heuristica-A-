using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Algoritmos;

namespace TWD
{
    public class Cell
    {
        public Cell(ContentManager content, TipoCelula tipo)
        {
            this.Tipo = tipo;
            if (Terra == null && content != null) 
            {
                Terra = content.Load<Texture2D>("Terra");
                Grama = content.Load<Texture2D>("Grama");
                Cimento = content.Load<Texture2D>("Cimento");
                Edificio = content.Load<Texture2D>("Edificio");
                Paralelepipedo = content.Load<Texture2D>("Paralelepipedo");
                Fonte = content.Load<SpriteFont>("DefaultFont");
            }

            this.Textura = getTexture(tipo);
            this.content = content;
        }
        ContentManager content;
        static Texture2D Grama;
        static Texture2D Terra;
        static Texture2D Cimento;
        static Texture2D Edificio;
        static Texture2D Paralelepipedo;
        static SpriteFont Fonte;

        public Texture2D Textura { get; set; }
        public TipoCelula Tipo { get; set; }


        public enum TipoCelula
        {
            Asfalto = 1,
            Grama = 5,
            Terra = 3,
            Paralelepipedo = 10,
            Edificio = 0
        }
        private Texture2D getTexture(TipoCelula tipo)
        {
            switch (tipo)
            {
                case TipoCelula.Asfalto:return Cimento;
                case TipoCelula.Edificio: return Edificio;
                case TipoCelula.Grama: return Grama;
                case TipoCelula.Paralelepipedo: return Paralelepipedo;
                default: return Terra;
            }
        }
        public byte GetValue()
        {
            return byte.Parse(((int)Tipo).ToString());
        }
        PathFinderNodeType estado = PathFinderNodeType.Close;
        public PathFinderNodeType Estado
        {
            get { return estado; }
            set { estado = value; }
        }
        public int Custo { get; set; }
        public void Draw(SpriteBatch spriteBatch, Rectangle position, float opacity)
        {
            switch (estado)
            {
                case PathFinderNodeType.Close:
                    spriteBatch.Draw(Textura, position, Color.White * opacity);
                    if(Custo> 0) spriteBatch.DrawString(Fonte, Custo.ToString(), new Vector2(position.X, position.Y), Color.White * opacity);
                    break;
                case PathFinderNodeType.Open:
                    spriteBatch.Draw(Textura, position, Color.White * 0.5f * opacity);
                    spriteBatch.DrawString(Fonte, Custo.ToString(), new Vector2(position.X, position.Y), Color.Blue * opacity);
                    break;
                case PathFinderNodeType.Path:
                    spriteBatch.Draw(Textura, position, Color.White * 0.3f * opacity);
                    spriteBatch.DrawString(Fonte, Custo.ToString(), new Vector2(position.X, position.Y), Color.Red * opacity);
                    break;
            }
            
        }
    }
}
