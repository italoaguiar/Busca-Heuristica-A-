using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Algoritmos;
using System.IO;
using System.Threading;

namespace TWD
{
    public partial class Debugger : Form
    {
        public Debugger()
        {
            InitializeComponent();

            comboBox1.SelectedIndex = 0;

            List<Personagem> p = new List<Personagem>();
            //p.Add(new Personagem(new Vector2(12, 20), "Rick"));
            p.Add(new Personagem(new Vector2(32, 05), "Carl"));
            p.Add(new Personagem(new Vector2(35, 35), "Daryl"));
            p.Add(new Personagem(new Vector2(08, 32), "Glen"));
            p.Add(new Personagem(new Vector2(31, 13), "Maggie"));
            //p.Add(new Personagem(new Vector2(21, 42), "Saída"));


            var k = p.GeneratePermutations();
            foreach(var item in k)
            {
                item.Insert(0, new Personagem(new Vector2(12, 20), "Rick"));
                item.Insert(5, new Personagem(new Vector2(21, 42), "Saída"));
            }
            Possibilidade  = k;

            LoadMap();
            aEstrela = new PathFinderFast(getMap());
            aEstrela.DebugFoundPath = false;
            aEstrela.DebugProgress = false;
        }
        List<Solution> Solucao = new List<Solution>();
        List<List<Personagem>> Possibilidade;
        Cell[,] Map = new Cell[42, 42];
        PathFinderFast aEstrela;

        private void LoadMap()
        {
            Map = new Cell[42, 42];

            if (File.Exists("MAP.txt"))
            {
                using (StreamReader reader = new StreamReader(File.OpenRead("MAP.txt")))
                {
                    string line = string.Empty;
                    for (int i = 0; i < 42; i++)
                    {
                        line = reader.ReadLine();
                        for (int j = 0; j < 42; j++)
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
                    return new Cell(null, Cell.TipoCelula.Grama);
                case 'A':
                    return new Cell(null, Cell.TipoCelula.Edificio);
                case 'M':
                    return new Cell(null, Cell.TipoCelula.Terra);
                case 'B':
                    return new Cell(null, Cell.TipoCelula.Paralelepipedo);
                default:
                    return new Cell(null, Cell.TipoCelula.Asfalto);

            }
        }
        private void Run()
        {
            Solucao.Clear();
            for (int i = 0; i < Possibilidade.Count; i++)
            {
                double tempoTotal = 0;
                List<Node> n = new List<Node>();
                int custoTotal = 0;

                Invoke(new Action(() => {
                    richTextBox1.Text += "===================================================================\n";
                    richTextBox1.Text += "Caminho: ";
                    progressBar1.Maximum = Possibilidade.Count;
                    progressBar1.Value = i;

                }));
                foreach (var item in Possibilidade[i])
                {
                    Invoke(new Action(() => {
                        richTextBox1.Text += item.Name + " =>";
                    }));
                }

                for (int z = 1; z < Possibilidade[i].Count; z++)
                {   
                    var r = aEstrela.FindPath(
                        new System.Drawing.Point((int)Possibilidade[i][z - 1].Position.Y, (int)Possibilidade[i][z - 1].Position.X),
                        new System.Drawing.Point((int)Possibilidade[i][z].Position.Y, (int)Possibilidade[i][z].Position.X)
                        );
                    tempoTotal += aEstrela.CompletedTime;
                    if (r != null)
                    {
                        n.AddRange(r);
                        custoTotal += r[0].F;
                    }                   

                    Invoke(new Action(() => {
                        if (r == null) richTextBox1.Text += "\nA HEURÍSTICA UTILIZADA NÃO PERMITIU ENCONTAR UM CAMINHO";
                    }));
                }
                Invoke(new Action(() => {
                    richTextBox1.Text += "\nCusto Total: " + custoTotal;
                    richTextBox1.Text += "\nTempo gasto: " + tempoTotal.ToString("N4") + " segundos\n\n";
                    
                }));
                Solucao.Add(new Solution()
                {
                    Custo = custoTotal,
                    Rota = Possibilidade[i],
                    Tempo = tempoTotal
                });
            }
            Invoke(new Action(() => {
                richTextBox1.Text += "\n===========================MELHOR CAMINHO==========================\n";
                var t = Solucao.Where(p=> p.Custo != 0).OrderBy(p => p.Custo).ThenBy(p=> p.Tempo).First();
                foreach (var item in t.Rota)
                {
                    richTextBox1.Text += item.Name + " =>";
                }
                richTextBox1.Text += "\nCusto Total: " + t.Custo;
                richTextBox1.Text += "\nTempo gasto: " + t.Tempo.ToString("N4") + " segundos\n\n";
                progressBar1.Value = 0;
            }));
            
            MessageBox.Show("Algorítmo executado com sucesso!");
        }
        private void button1_Click(object sender, EventArgs e)
        {
            aEstrela.Formula = (HeuristicFormula)(comboBox1.SelectedIndex + 1);
            aEstrela.Diagonals = radioButton1.Checked;
            richTextBox1.Clear();

            var mPFThread = new Thread(new ThreadStart(Run));
            mPFThread.Name = "Path Finder Thread";
            mPFThread.Start();
        }
    }
    public static class ExtensionMethods
    {
        static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] { item })
                );
        }
        public static List<List<T>> GeneratePermutations<T>(this List<T> items)
        {
            // Make an array to hold the
            // permutation we are building.
            T[] current_permutation = new T[items.Count];

            // Make an array to tell whether
            // an item is in the current selection.
            bool[] in_selection = new bool[items.Count];

            // Make a result list.
            List<List<T>> results = new List<List<T>>();

            // Build the combinations recursively.
            PermuteItems<T>(items, in_selection,
                current_permutation, results, 0);

            // Return the results.
            return results;
        }
        private static void PermuteItems<T>(List<T> items, bool[] in_selection,
            T[] current_permutation, List<List<T>> results,
            int next_position)
        {
            // See if all of the positions are filled.
            if (next_position == items.Count)
            {
                // All of the positioned are filled.
                // Save this permutation.
                results.Add(current_permutation.ToList());
            }
            else
            {
                // Try options for the next position.
                for (int i = 0; i < items.Count; i++)
                {
                    if (!in_selection[i])
                    {
                        // Add this item to the current permutation.
                        in_selection[i] = true;
                        current_permutation[next_position] = items[i];

                        // Recursively fill the remaining positions.
                        PermuteItems<T>(items, in_selection,
                            current_permutation, results,
                            next_position + 1);

                        // Remove the item from the current permutation.
                        in_selection[i] = false;
                    }
                }
            }
        }
    }
    public class Solution
    {
        public List<Personagem> Rota { get; set; }
        public int Custo { get; set; }
        public double Tempo { get; set; }
    }
}
