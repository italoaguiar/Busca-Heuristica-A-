using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading;

namespace TWD
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Stage stage;

        List<IGameObject> scenes = new List<IGameObject>();
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.graphics.PreferredBackBufferWidth = 630;
            this.graphics.PreferredBackBufferHeight = 630;
            //Window.AllowUserResizing = true;
            //Window.ClientSizeChanged += Window_ClientSizeChanged;
            //graphics.IsFullScreen = true;

            this.IsMouseVisible = true;

        }
        public static int CurrentState = 0;



        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
           // GraphicsDevice.Viewport = new Viewport(0, 0, 130, 130);
            

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            var Personagens = new ObservableCollection<Personagem>();
            Personagens.Add(new Personagem(new Vector2(12, 20), Content.Load<Texture2D>("R"), "Rick"));
            Personagens.Add(new Personagem(new Vector2(08, 32), Content.Load<Texture2D>("G"), "Glen"));
            Personagens.Add(new Personagem(new Vector2(32, 05), Content.Load<Texture2D>("C"), "Carl"));
            Personagens.Add(new Personagem(new Vector2(31, 13), Content.Load<Texture2D>("M"), "Maggie"));            
            Personagens.Add(new Personagem(new Vector2(35, 35), Content.Load<Texture2D>("D"), "Daryl"));                       
            Personagens.Add(new Personagem(new Vector2(21, 42), Content.Load<Texture2D>("S"), "Saida"));//cmd

            stage = new Stage(this, ref Personagens);

            scenes.Add(new MainMenu(this));
            scenes.Add(new SelectionMenu(ref Personagens, ref stage));
            scenes.Add(stage);


            foreach (IGameObject o in scenes)
                o.Load(Content);

            // TODO: use this.Content to load your game content here            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            if (stage.mPFThread != null && stage.mPFThread.ThreadState != ThreadState.Stopped)
                stage.mPFThread.Abort();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.F11))
                this.graphics.ToggleFullScreen();

            // TODO: Add your update logic here
            scenes[CurrentState].Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            //s.Draw(spriteBatch);
            scenes[CurrentState].Draw(spriteBatch);
            //spriteBatch.Draw(texture, new Rectangle(100,100, 220,71), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
