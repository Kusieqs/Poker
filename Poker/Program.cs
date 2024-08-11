using Poker;

internal class Program
{
    private static void Main(string[] args)
    {
        // Setting name for a main player
        string name = NickName();

        // choosing how many players do you want
        int players = HowManyPlayers();

        // Choosing lvl of players
        List<Player> listOfPlayers = LevelOfGame(players, name);
        Console.Clear();

        TexasHoldem.Game(listOfPlayers);
    }
    private static string NickName()
    {
        string name;
        while(true)
        {
            Console.Clear();
            Console.Write("Nickname: ");
            name = Console.ReadLine();

            if (!string.IsNullOrEmpty(name) && name.Length <= 20)
                return name;
        }
    } // Choosing nickname for a main player
    private static bool WhichMode()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Choose which mode do you want to play\n1 - Texas Holdem\n2 - Five-card Draw");
            Console.Write("\nMode: ");
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

            if (int.TryParse(keyInfo.KeyChar.ToString(), out int result) && result == 1 || result == 2)
                return result == 1;
        }
    } // Choosing which game do you want to play
    private static int HowManyPlayers() 
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("How many players do you want to play with? (1 - 5)\n");
            Console.Write("Number: ");
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

            if (int.TryParse(keyInfo.KeyChar.ToString(), out int number) && number >= 1 && number <= 5)
                return number;
        }
    } // Choosing how many players do you want in game
    private static List<Player> LevelOfGame(int players, string name)
    {
        List<Player> list = new List<Player>();
        while (true)
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

            list.Add(new Player(mainMoney, true, 2, name));
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
                list.Add(new Player(mainMoney, false, 2, $"Player {i}"));
            }

            bool loop = true;
            while (true)
            {
                Console.WriteLine("Players:\n\n");
                for(int j = 0; j < list.Count; j++)
                {
                    if(j != 0 && loop)
                        Thread.Sleep(1000);

                    Console.Write($"{list[j].Name} ");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"Monets: {list[j].Monets}\n");
                    Console.ResetColor();
                }

                Console.Write("\n\nDo you accepting amount of monets?\n\n1. Yes\n2. No\n\nOption: ");
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
    } // Choosing how many monets do you want
}