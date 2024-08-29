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
        private static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PokerLog");
        private static string time;
        public void LogDecks(string message)
        {
            string decks = "";
            foreach (var players in TexasHoldem.listOfPlayers)
            {
                decks += players.Name + "\n";
                
                for(int i = 0; i < players.Deck.Length; i++)
                {
                    decks += players.Deck[i].Rank.ToString() + players.Deck[i].Suit.ToString() + "\n";
                }
                decks += "\n\n";
            }
            File.AppendAllText(Path.Combine(path, time.ToString().Split(' ')[0] + "_" + time.ToString().Split(' ')[1] + ".txt"),(message + "\n\n" + decks));
        }

        public void LogHand(string message)
        {
            string hand = "";
            foreach (var players in TexasHoldem.listOfPlayers)
            {
                hand += players.Name + "\n";
                hand += PokerHandEvaluator.CheckHand(players) + "\n\n";
            }
            File.AppendAllText(Path.Combine(path, time.ToString().Split(' ')[0] + "_" + time.ToString().Split(' ')[1] + ".txt"), (message + "\n" + hand));
        }

        public void LogMessage(string message)
        {
            File.AppendAllText(Path.Combine(path, time.ToString().Split(' ')[0] + "_" + time.ToString().Split(' ')[1] + ".txt"), message);
        }

        public void LogMove(string message)
        {
            string move = "";
            foreach (var players in TexasHoldem.listOfPlayers)
            {
                move += players.Name + "\n";
                move += players.LastMove + "\n\n";
            }
            File.AppendAllText(Path.Combine(path, time.ToString().Split(' ')[0] + "_" + time.ToString().Split(' ')[1] + ".txt"), (message + "\n" + move));
        }

        public static void CreatingTxtFile()
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            time = DateTime.Now.ToString();
            File.WriteAllText(Path.Combine(path, time.ToString().Split(' ')[0] + "_" + time.ToString().Split(' ')[1] + ".txt"), "");
        }

        // probability
    }
}
