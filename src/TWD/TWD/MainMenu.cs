using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using EAppsEngine.Engine;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace TWD
{
    public class MainMenu : IGameObject
    {
        Texture2D BG;
        Texture2D Logo;
        Texture2D Btn;
        Song menuSong;
        Game game;
        SoundEffect bells;

        float bgOpacity = 0f;
        float logoOpacity = 0f;
        float BtnOpacity = 0f;
        bool canSkip = false;
        bool ended = false;

        Storyboard sb,sb2,sb3;

        public MainMenu(Game game)
        {
            this.game = game;
            Initialize();
        }
        private void Initialize()
        {
            sb = new Storyboard(game, 0, 1, new TimeSpan(0, 0, 4));
            sb2 = new Storyboard(game, 0, 1, new TimeSpan(0, 0, 1));
            sb3 = new Storyboard(game, 1, 0, new TimeSpan(0, 0, 3));
            sb.EasingFunction = Storyboard.AnimationType.CubicEaseInOut;
            sb2.EasingFunction = Storyboard.AnimationType.Linear;
            sb2.IsRepeating = true;
            sb2.AutoReverse = true;
            sb.CurrentValueChanged += bg_OpacityValueHandler;
            sb.StoryboardEnded += bg_OpacityValueEnded;

            sb2.CurrentValueChanged += (sender, value) =>
            {
                BtnOpacity = (float)value;
                canSkip = true;
            };
            sb3.CurrentValueChanged += (sender, value) =>
            {
                bgOpacity = BtnOpacity = logoOpacity = (float)value;
            };
            sb3.StoryboardEnded += (sender, value) => { Game1.CurrentState = 1; ended = true; };

            game.Components.Add(sb);
            game.Components.Add(sb2);
            game.Components.Add(sb3);
        }

        private void bg_OpacityValueEnded(object sender, double FinalValue)
        {
            sb.CurrentValueChanged -= bg_OpacityValueHandler;
            sb.StoryboardEnded -= bg_OpacityValueEnded;

            sb.CurrentValueChanged += logo_OpacityValueHandler;
            sb.StoryboardEnded += logo_OpacityValueEnded;

            sb.Start();
        }

        private void logo_OpacityValueEnded(object sender, double FinalValue)
        {
            sb.CurrentValueChanged -= logo_OpacityValueHandler;
            sb.StoryboardEnded -= logo_OpacityValueEnded;

            game.Components.Remove(sb);

            sb2.Start();
        }

        private void logo_OpacityValueHandler(object sender, double NewValue)
        {
            logoOpacity = (float)NewValue;
        }

        private void bg_OpacityValueHandler(object sender, double NewValue)
        {
            bgOpacity = (float)NewValue;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(BG, new Rectangle(0, 0, 630, 630), Color.White * bgOpacity);
            spriteBatch.Draw(Logo, new Vector2((315 - Logo.Width / 2), 50f), Color.White * logoOpacity);
            spriteBatch.Draw(Btn, new Vector2((315 - Btn.Width / 2), 550f), Color.White * BtnOpacity);
        }

        public void Load(ContentManager content)
        {
            BG = content.Load<Texture2D>("MMenu");
            Logo = content.Load<Texture2D>("Logo");
            Btn = content.Load<Texture2D>("Play");
            menuSong = content.Load<Song>("MMenuLoop1");
            bells = content.Load<SoundEffect>("bells");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(menuSong);
            sb.Start();
        }

        public void Update(GameTime gametime)
        {
            if (canSkip)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    sb2.Stop();
                    sb3.Start();
                    bells.Play();
                    canSkip = false;
                }
            }
            if (ended)
            {
                Initialize();
                ended = false;
                sb.Start();
            }
        }
    }
}
