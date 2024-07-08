internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("How many players do you want to play with? (1 - 5)\n");
        int players = HowManyPlayers();

        Console.WriteLine("Select a monetary amount (100 - 1000)\n");
        int monets = HowManyMonets();

        

    }

    private static int HowManyPlayers() 
    {
        while (true)
        {
            try
            {
                Console.Write("Number: ");
                int players = int.Parse(Console.ReadLine());

                if (players > 0 && players < 6)
                    return players;
                else
                    throw new FormatException();
            }
            catch (Exception)
            {
                Console.WriteLine("Wrong number or format");
                Console.ReadKey();
                Console.Clear();
            }
        }
    }
    private static int HowManyMonets()
    {
        while (true)
        {
            try
            {
                Console.Write("Monets: ");
                int monets = int.Parse(Console.ReadLine());
                if (monets > 99 && monets < 1001)
                    return monets;
                else
                    throw new FormatException();
            }
            catch (Exception)
            {
                Console.WriteLine("Wrong number or format");
                Console.ReadKey();
                Console.Clear();
            }
        }
    }
}