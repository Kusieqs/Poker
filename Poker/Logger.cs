using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    internal class ConsoleLogger : ILogger
    {
        public void LogDecks(string message)
        {
            Console.Clear();
            Console.WriteLine(message);
            foreach(var players in TexasHoldem.listOfPlayers)
            {
                Console.WriteLine(players.Name);
                players.ShowDeck();
                Console.WriteLine("\n\n");
            }
            Console.ReadKey();
        }

        public void LogHand(string message)
        {
            Console.Clear();
            Console.WriteLine(message);
            foreach (var players in TexasHoldem.listOfPlayers)
            {
                Console.WriteLine(players.Name);
                Console.WriteLine(PokerHandEvaluator.CheckHand(players));
                Console.WriteLine("\n\n");
            }
            Console.ReadKey();
        }

        public void LogMessage(string message)
        {
            Console.Clear();
            Console.WriteLine(message);
            Console.ReadKey();
        }

        public void LogMove(string message)
        {
            Console.Clear();
            Console.WriteLine(message);
            foreach (var players in TexasHoldem.listOfPlayers)
            {
                Console.WriteLine(players.Name);
                Console.WriteLine(players.Hand);
                Console.WriteLine("\n\n");
            }
            Console.ReadKey();
        }


        // probability
    }
    internal class FileLogger : ILogger
    {
        private static string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public void LogDecks(string message)
        {
        }

        public void LogHand(string message)
        {
        }

        public void LogMessage(string message)
        {
        }

        public void LogMove(string message)
        {
        }

        public static void CreatingTxtFile()
        {
            if (!Directory.Exists(Path.Combine(path, "PokerLog")))
                Directory.CreateDirectory(Path.Combine(path, "PokerLog"));

            File.WriteAllText(Path.Combine(path, $"{DateTime.Now}"), "");
        }

        // probability
    }
}
