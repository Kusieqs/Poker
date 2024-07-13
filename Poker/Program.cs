using Poker;

internal class Program
{
    private static void Main(string[] args)
    {
        // choosing how many players do you want
        int players = HowManyPlayers();
        Console.Clear();

        // Choosing how many monets do you want
        int monets = HowManyMonets();
        Console.Clear();

        // FALSE - Five-card
        // TRUE - Texas holdem
        bool mode = WhichMode();
        Console.Clear();


        if (mode)
            TexasHoldem.Game(players, monets,"MAIN PLAYER");
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
    private static int HowManyMonets()
    {
        while (true)
        {
            try
            {
                Console.WriteLine("Select a monetary amount (100 - 1000)\n");
                Console.Write("Monets: ");
                int monets = int.Parse(Console.ReadLine());
                if (monets > 99 && monets < 1001)
                    return monets;
                else
                    throw new FormatException();
            }
            catch (Exception)
            {
                ExceptionString();
            }
        }
    } // Choosing how many monets do you want
    private static bool WhichMode()
    {
        while (true)
        {
            try
            {
                Console.WriteLine("Choose which mode do you want to play\n1 - Texas Holdem\n2 - Five-card Draw");
                Console.Write("Monets: ");
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