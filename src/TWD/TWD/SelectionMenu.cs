using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Input;

namespace TWD
{
    public class SelectionMenu : IGameObject
    {
        SpriteFont fonte1;
        int currentSelection = 0, selectedPersonage = 1, selectedFormula = 0;
        ObservableCollection<Personagem> personagens;
        List<Algoritmos.HeuristicFormula> formula;
        bool dialgonais = false;
        Vector2 WindowSize = new Vector2(630, 630);
        Stage stage;


        public SelectionMenu(ref ObservableCollection<Personagem> personagens, ref Stage stage)
        {
            this.personagens = personagens;
            this.stage = stage;

            formula = new List<Algoritmos.HeuristicFormula>();
            formula.Add(Algoritmos.HeuristicFormula.Manhattan);
            formula.Add(Algoritmos.HeuristicFormula.MaxDXDY);
            formula.Add(Algoritmos.HeuristicFormula.DiagonalShortCut);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(currentSelection == 0)
            {
                spriteBatch.DrawString(fonte1, "Percorrer Diagonais:  " + (dialgonais == true ? "Sim" : "Nao"), new Vector2(40, 120), Color.Yellow);
                spriteBatch.DrawString(fonte1, "Heuristica:", new Vector2(40, 220), Color.White);
                spriteBatch.DrawString(fonte1, "Ordem de busca:", new Vector2(40, 320), Color.White);
            }
            else if(currentSelection == 1)
            {
                spriteBatch.DrawString(fonte1, "Percorrer Diagonais:  " + (dialgonais == true ? "Sim" : "Nao"), new Vector2(40, 120), Color.White);
                spriteBatch.DrawString(fonte1, "Heuristica:", new Vector2(40, 220), Color.Yellow);
                spriteBatch.DrawString(fonte1, "Ordem de busca:", new Vector2(40, 320), Color.White);
            }
            else
            {
                spriteBatch.DrawString(fonte1, "Percorrer Diagonais:  " + (dialgonais == true ? "Sim" : "Nao"), new Vector2(40, 120), Color.White);
                spriteBatch.DrawString(fonte1, "Heuristica:", new Vector2(40, 220), Color.White);
                spriteBatch.DrawString(fonte1, "Ordem de busca:", new Vector2(40, 320), Color.Yellow);
            }

            for(int i = 1; i< personagens.Count -1; i++)
            {
                if(selectedPersonage == i)
                    spriteBatch.DrawString(fonte1, personagens[i].Name, new Vector2(250, 290 + (30 * i)), Color.Yellow);
                else
                    spriteBatch.DrawString(fonte1, personagens[i].Name, new Vector2(250, 290 + (30 * i)), Color.White);
            }
            for(int i = 0; i< formula.Count; i++)
            {
                if (selectedFormula == i)
                    spriteBatch.DrawString(fonte1, formula[i].ToString(), new Vector2(180 + (120 * i), 220), Color.Yellow);
                else
                    spriteBatch.DrawString(fonte1, formula[i].ToString(), new Vector2(180 + (120 * i), 220), Color.White);
            }
            string m = "Pressione Enter para continuar";
            Vector2 v = fonte1.MeasureString(m);

            spriteBatch.DrawString(fonte1, m, new Vector2(315 - v.X/2, 550), Color.White);
        }

        public void Load(ContentManager content)
        {
            fonte1 = content.Load<SpriteFont>("DefaultFont2");
        }
        KeyboardState oldState;
        public void Update(GameTime gametime)
        {
            var state = Keyboard.GetState();

            if(state.IsKeyDown(Keys.Down) && oldState.IsKeyUp(Keys.Down))
            {
                if (currentSelection < 5)
                    currentSelection++;
            }
            if (state.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Up))
            {
                if (currentSelection > 0)
                    currentSelection--;
            }
            if (currentSelection > 1)
                selectedPersonage = currentSelection - 1;


            if(state.IsKeyDown(Keys.Left) && oldState.IsKeyUp(Keys.Left))
            {
                if (currentSelection == 0) dialgonais = !dialgonais;
                else if (currentSelection == 1 && selectedFormula > 0)
                {
                    selectedFormula--;
                }

                else if (currentSelection > 2 && currentSelection <= 5)
                {
                    personagens.Move(selectedPersonage, selectedPersonage - 1);
                    currentSelection--;
                }
            }
            if (state.IsKeyDown(Keys.Right) && oldState.IsKeyUp(Keys.Right))
            {
                if (currentSelection == 0) dialgonais = !dialgonais;
                else if (currentSelection == 1 && selectedFormula < 2)
                {
                    selectedFormula++;
                }
                else if (currentSelection > 1 && currentSelection < 5)
                {
                    personagens.Move(selectedPersonage, selectedPersonage + 1);
                    currentSelection++;
                }
            }

            if(state.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter))
            {
                stage.Diagonais = dialgonais;
                stage.Formula = formula[selectedFormula];
                Game1.CurrentState++;
            }


            oldState = state;
        }
    }
}
