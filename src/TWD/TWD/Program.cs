using System;
using System.Windows.Forms;

namespace TWD
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length >0 && args[0] == "-debug")
            {
                Application.Run(new Debugger());
            }
            else using (Game1 game = new Game1())
            {
                game.Run();
            }
        }
    }
#endif
}

