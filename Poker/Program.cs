using Poker;
using System.ComponentModel;
using System.Xml.Linq;

internal class Program
{
    private static void Main(string[] args)
    {
        // Setting name for a main player
        string name = NickName();
        Console.Clear();

        // FALSE - Five-card
        // TRUE - Texas holdem
        bool mode = WhichMode();
        Console.Clear();

        // choosing how many players do you want
        int players = HowManyPlayers();
        Console.Clear();

        // Choosing lvl of players
        List<Player> listOfPlayers = LevelOfGame(players, name, mode);
        Console.Clear();

        
        if (mode)
            TexasHoldem.Game(listOfPlayers);
    }

    private static int HowManyPlayers() 
    {
        while (true)
        {
            try
            {
                Console.WriteLine("How many players do you want to play with? (1 - 5)\n");
                Console.Write("Number: ");
                int players = int.Parse(Console.ReadLine());

                if (players > 0 && players < 6)
                    return players;
                else
                    throw new FormatException();
            }
            catch (Exception)
            {
                ExceptionString();
            }
        }
    } // Choosing how many players do you want in game
    private static List<Player> LevelOfGame(int players, string name, bool mode)
    {
        List<Player> list = new List<Player>();
        while (true)
        {
            try
            {
                Console.Clear();
                Console.Write("Choose level of game\n\n1. Easy\n2. Normal\n3. Hard\n\nOption: ");
                Random random = new Random();
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(intercept: true);
                int mainMoney;

                switch (consoleKeyInfo.KeyChar)
                {
                    case '1':
                        mainMoney = 500;
                        break;
                    case '2':
                        mainMoney = 1000;
                        break;
                    case '3':
                        mainMoney = 650;
                        break;
                    default:
                        continue;
                }

                list.Add(new Player(mainMoney, true, (mode == true ? 2 : 5), name));

                Console.Clear();

                for(int i = 1; i <= players; i++)
                {
                    switch(consoleKeyInfo.KeyChar)
                    {
                        case '1':
                            mainMoney = (random.Next(200,401)/10) * 10;
                            break;
                        case '2':
                            mainMoney = 1000;
                            break;
                        case '3':
                            mainMoney = (random.Next(500, 1200)/10) * 10;
                            break;
                    }
                    list.Add(new Player(mainMoney, false, (mode == true ? 2 : 5), $"Player {i}"));
                }

                bool loop = true;
                while (true)
                {
                    for(int j = 0; j < list.Count; j++)
                    {
                        if(j != 0 && loop)
                            Thread.Sleep(1000);

                        Console.Write($"{list[j].Name} ");
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"Monets: {list[j].Monets}");
                        Console.ResetColor();
                    }

                    Console.WriteLine("\n\nDo you accepting amount of monets?\n\n1. Yes\n2. No");
                    consoleKeyInfo = Console.ReadKey(intercept: true);
                    if (consoleKeyInfo.KeyChar == '1')
                        return list;
                    else if (consoleKeyInfo.KeyChar == '2')
                    {
                        list.Clear();
                        break;
                    }

                    loop = false;
                    Console.Clear();
                }

            }
            catch (Exception)
            {
                ExceptionString();
                continue;
            }
        }
    } // Choosing how many monets do you want
    private static string NickName()
    {
        string name;
        while(true)
        {
            try
            {
                Console.Clear();
                Console.Write("Nickname: ");
                name = Console.ReadLine();
                if (string.IsNullOrEmpty(name))
                    throw new FormatException("Name is empty");
                else if (name.Length > 25)
                    throw new FormatException("Name is to long");

                return name;
            }
            catch (Exception ex)
            {
                ExceptionString(ex.Message);
            }
        }
    } // Choosing nickname for a main player
    private static bool WhichMode()
    {
        while (true)
        {
            try
            {
                Console.WriteLine("Choose which mode do you want to play\n1 - Texas Holdem\n2 - Five-card Draw");
                Console.Write("\nMode: ");
                int mode = int.Parse(Console.ReadLine());
                if (mode > 0 && mode < 3)
                    return mode == 1;
                else
                    throw new FormatException();
            }
            catch (Exception)
            {
                ExceptionString();
            }
        }
    } // Choosing which game do you want to play
    public static void ExceptionString(string exception = "")
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(exception == "" ? "Wrong number or format" : exception);
        Console.ResetColor();
        Console.ReadKey();
        Console.Clear();
    } // Excpetion information
}