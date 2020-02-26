using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework.Media;
using Algoritmos;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace TWD
{
    public class Stage : IGameObject
    {
        private ContentManager contentManager { get; set; }
        private Cell [,] Map { get; set; }


        ObservableCollection<Personagem> Personagens;
        SpriteFont Font;
        Song GameSong1, GameSong2;
        EAppsEngine.Engine.Storyboard Storyboard1, Storyboard2;
        Game game;
        bool ended;
        int custoTotal = 0; int custoParcial = 0;

        Algoritmos.PathFinderFast a_estrela;

        public Algoritmos.HeuristicFormula Formula = HeuristicFormula.Manhattan;
        public bool Diagonais { get; set; }

        public Thread mPFThread;

        float opacity = 0.0f;

        public Stage(Game game, ref ObservableCollection<Personagem> Personagens)
        {
            this.game = game;
            Storyboard1 = new EAppsEngine.Engine.Storyboard(game, 0, 1, new TimeSpan(0,0,3));
            Storyboard2 = new EAppsEngine.Engine.Storyboard(game, 1, 0, new TimeSpan(0, 0, 3));
            game.Components.Add(Storyboard1);
            game.Components.Add(Storyboard2);


            Storyboard1.CurrentValueChanged += Storyboard_CurrentValueChanged;
            Storyboard2.CurrentValueChanged += Storyboard2_CurrentValueChanged;
            Storyboard1.StoryboardEnded += Storyboard1_StoryboardEnded;
            Storyboard2.StoryboardEnded += Storyboard2_StoryboardEnded;


            this.Personagens = Personagens;
        }

        private void Storyboard2_StoryboardEnded(object sender, double FinalValue)
        {
            Game1.CurrentState = 0;
            MediaPlayer.Play(GameSong1);
            MediaPlayer.Volume = 1;
            ended = true;
        }

        private void Storyboard2_CurrentValueChanged(object sender, double NewValue)
        {
            opacity = ((float)NewValue);
            MediaPlayer.Volume = ((float)NewValue);
        }

        private void Storyboard1_StoryboardEnded(object sender, double FinalValue)
        {
            Start();
        }

        private void Storyboard_CurrentValueChanged(object sender, double NewValue)
        {
            opacity =  ((float) NewValue);
        }

        private void LoadMap()
        {
            Map = new Cell[42, 42];

            if (File.Exists("MAP.txt"))
            {
                using (StreamReader reader = new StreamReader(File.OpenRead("MAP.txt")))
                {
                    string line = string.Empty;
                    for(int i = 0; i< 42; i++)
                    {
                        line = reader.ReadLine();
                        for(int j = 0; j< 42; j++)
                        {
                            Map[i, j] = GetCellByChar(line[j]);
                            
                        }
                    }
                }
            }
        }
        byte[,] getMap()
        {
            byte[,] m = new byte[64, 64];

            //inicializa a matriz com 1
            for (int x = 0; x < 64; x++)
                for (int y = 0; y < 64; y++)
                    m[x, y] = 1;

                    for (int i = 0; i < 42; i++)
                for (int j = 0; j < 42; j++)
                    m[i, j] = Map[i, j].GetValue();
            return m;
        }
        Cell GetCellByChar(char c)
        {
            switch (c)
            {
                case 'V':
                    return new Cell(contentManager, Cell.TipoCelula.Grama);
                case 'A':
                    return new Cell(contentManager, Cell.TipoCelula.Edificio);
                case 'M':
                    return new Cell(contentManager, Cell.TipoCelula.Terra);
                case 'B':
                    return new Cell(contentManager, Cell.TipoCelula.Paralelepipedo);
                default:
                    return new Cell(contentManager, Cell.TipoCelula.Asfalto);

            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < 42; x++)
            {
                for (int y = 0; y < 42; y++)
                {
                    Map[x, y].Draw(spriteBatch, new Rectangle(y * 15, x * 15, 15, 15),opacity);
                }
            }
            foreach (Personagem i in Personagens)
                i.Draw(spriteBatch,opacity);

            spriteBatch.DrawString(
                Font, 
                string.Format("Custo Total Real:{0}      Custo Parcial:{1}", custoTotal, custoParcial), 
                new Vector2(40, 8), 
                Color.White * opacity);
        }

        public void Load(ContentManager content)
        {
            contentManager = content;

            GameSong1 = content.Load<Song>("MMenuLoop1");
            GameSong2 = content.Load<Song>("MMenuLoop2");
            Font = content.Load<SpriteFont>("DefaultFont2");

            LoadMap();  

            a_estrela = new Algoritmos.PathFinderFast(getMap());
            a_estrela.PathFinderDebug += new Algoritmos.PathFinderDebugHandler(PathFinderDebug);
            a_estrela.DebugProgress = true;
            a_estrela.DebugFoundPath = true;            
        }

        private void PathFinderDebug(int fromX, int fromY, int x, int y, PathFinderNodeType type, int totalCost, int cost)
        {
            custoParcial = cost;

            if (x < 42 && y < 42)
            {
                Map[x, y].Estado = type;
                Map[x, y].Custo = cost;
            }
            Thread.Sleep(20);
        }
        private void Start()
        {
            mPFThread = new Thread(new ThreadStart(RunPathFinder));
            mPFThread.Name = "Path Finder Thread";
            mPFThread.Start();
        }
        private void RunPathFinder()
        {
            a_estrela.Diagonals = Diagonais;
            a_estrela.Formula = Formula;

            foreach (Personagem p in Personagens)
                p.Salvo = false;

            for (int z = 1; z < Personagens.Count; z++)
            {
                var r = a_estrela.FindPath(
                    new System.Drawing.Point((int)Personagens[z - 1].Position.Y, (int)Personagens[z - 1].Position.X),
                    new System.Drawing.Point((int)Personagens[z].Position.Y, (int)Personagens[z].Position.X)
                    );

                if (r == null)
                {
                    MessageBox.Show("A Heurística Utilizada não permitiu encontar uma solução.");                   
                    break;
                }
                custoTotal += r[0].F;
                custoParcial = r[0].F;
                for (int i = r.Count - 1; i >= 0; i--)
                {
                    Personagens[0].Position = new Vector2(r[i].Y, r[i].X);
                    Thread.Sleep(100);
                }
                Personagens[z].Salvo = true;
                ClearMatrix();
            }

            Storyboard2.Start();

        }

        bool audioChanged = false;
        private void HandleAudio()
        {
            if (!audioChanged)
            {
                MediaPlayer.IsRepeating = false;

                if (MediaPlayer.State == MediaState.Stopped)
                {
                    MediaPlayer.Play(GameSong2);
                    MediaPlayer.IsRepeating = true;                    
                    audioChanged = true;
                    Storyboard1.Start();
                }
                    
            }
        }
        private void ClearMatrix()
        {
            for (int x = 0; x < 42; x++)
            {
                for (int y = 0; y < 42; y++)
                {
                    Map[x, y].Custo = 0;
                    Map[x, y].Estado = PathFinderNodeType.Close;
                }
            }
        }
        public void Update(GameTime gametime)
        {
            HandleAudio();

            if (ended)
            {
                audioChanged = false;
                ended = false;
                ClearMatrix();
                Personagens[0].Position = new Vector2(12, 20);
                custoTotal = custoParcial = 0;
            }
        }

    }
}
