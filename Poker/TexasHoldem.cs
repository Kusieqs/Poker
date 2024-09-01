using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace Poker
{
    public static class TexasHoldem
    {
        public static List<Player> listOfPlayers = new List<Player>(); // list of players
        public static List<Card> cardsOnTable = new List<Card>(); // cards on the table
        public static int bank; // bank amount
        public static bool chooseOption;
        public static bool firstRaise = false;
        public static CancellationTokenSource? userToken;
        public static CancellationTokenSource? computerToken;
        public static BlockingCollection<ConsoleKeyInfo>? keyBuffer;
        public static (int, int) cords;
        private static ILogger? logger;

        public static void Game(List<Player> players)
        {
            logger = GetLogger(); // setting logger

            // Players added to main list
            listOfPlayers = players;

            // Engine of game
            EngineOfGame();

            // END
        } // Main Game
        private static void EngineOfGame()
        {
            Console.Clear();

            // Game engine
            do
            {
                if (logger is FileLogger)
                    FileLogger.CreatingTxtFile();

                logger?.LogMessage($"Game at: {DateTime.Now}");

                try
                {
                    // Setting fold for everyone
                    Player.SetFold(true);
                    logger?.LogMove("Move of players:");

                    // Checking whether our player is enable to play
                    bool IsPlaying = StartRoundMenu();
                    logger?.LogMove("Move of active players:");

                    // Exit game
                    if (!IsPlaying)
                        Environment.Exit(0);

                    // Creating deck and shuffle
                    Player.mainDeck.Clear();
                    Player.gameDeck.Clear();
                    cardsOnTable.Clear();
                    Player.CreatingDeck();
                    Player.Shuffle();
                    Console.Clear();

                    // Dealing cards to decks
                    DealCards();
                    logger?.LogDecks("Decks of players:");

                    // Take move from players
                    OptionsInGame(true, 0);
                    logger?.LogMove("Move of players (After saw decks):");
                    Console.Clear();
                    BankShow();

                    // Croupier deals 3 cards on the table
                    Console.WriteLine("Cards on table:");
                    Card card;
                    for (int i = 0; i < 3; i++)
                    {
                        card = Player.gameDeck.Pop();
                        cardsOnTable.Add(card);
                    }
                    Card.DrawCardOnTable(cardsOnTable, 2000);
                    OptionsInGame(true, 1);
                    logger?.LogMove("Move of players (After 3 cards on table):");
                    Console.Clear();

                    // Croupier deals 1 card on the table
                    for (int i = 0; i < 2; i++)
                    {
                        BankShow();
                        Console.WriteLine("Cards on table:");
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Console.WriteLine(Card.infoDeck);
                        Console.ResetColor();
                        card = Player.gameDeck.Pop();
                        cardsOnTable.Add(card);
                        Card.DrawCardOnTable(cardsOnTable, 2000);
                        OptionsInGame(false, i + 2);
                        logger?.LogMove($"Move of players (After {i + 4} cards on table):");
                        Console.Clear();
                    }

                    BankShow(false);
                    Card.DrawCardOnTable(cardsOnTable);
                    FinalResult($" won {bank} monets!");

                }
                catch (OnePlayerException ex)
                {
                    Console.Clear();
                    FinalResult(ex.Message);
                }
                finally
                {
                    listOfPlayers.Where(x => x.LastMove != Move.Pass).First().Monets += bank;
                    bank = 0;
                }

            } while (true);
        } // Engine of the game
        private static bool StartRoundMenu()
        {
            do
            {
                // choosing option
                Console.Clear();
                BankShow(false);
                Console.WriteLine();

                // Showing players
                foreach (Player player in listOfPlayers)
                {
                    if (player.IsPlayer)
                        continue;
                    Console.WriteLine($"{player.Name} Monets: {player.Monets}\n");
                }

                Console.WriteLine("\n\n1. Play (Raise 20 monets to enabled to play)\n2. Exit");
                Console.Write("\nNumber: ");
                ConsoleKeyInfo choose = Console.ReadKey();
                if (choose.KeyChar.ToString() == "1")
                {
                    // Checking our player that he has got 30 monets in pocket
                    if (listOfPlayers.Where(x => x.IsPlayer == true).First().Monets - 20 < 0)
                        continue;

                    // Taking money from pocket to bank
                    foreach (Player player in listOfPlayers)
                    {
                        if (player.Monets - 20 < 0)
                        {
                            player.LastMove = Move.Pass;
                            continue;
                        }
                        player.RaiseMoney(20);
                    }
                    return true;
                }
                else if (choose.KeyChar.ToString() == "2")
                {
                    // Exit from table
                    return false;
                }

            } while (true);

        } // Starting round for player
        public static void DealCards()
        {
            BankShow(false);
            Thread.Sleep(2000);

            foreach (Player player in listOfPlayers)
            {
                // Raising cards for active players
                if(player.LastMove != Move.Pass)
                    player.RaiseCard(); 

                // Showing deck for player
                if (player.IsPlayer)
                {
                    player.ShowDeck();
                    Console.WriteLine();
                }
            }
            EnterPress();
        } // Dealing 2 cards for each player
        private static void OptionsInGame(bool option, int lvl)
        {
            Console.Clear();
            BankShow();
            Dictionary<Player, (int, int)> cursor = new Dictionary<Player, (int, int)>(); // Setting cords for cursor
           
            if(cardsOnTable.Count != 0)
            {
                Console.WriteLine("Cards on table:");
                Card.ShowTable();
            }

            ShowingPlayers(cursor); // Showing Players
            string whichOption = option == true ? "Wait" : "Check";
            string options = $"\nOptions:\n\n1 - Raise\n2 - {whichOption}\n3 - Pass \n\n";

            // Menu for player
            AsyncMethods.HandleUserAndComputerMoves(lvl, cursor, options).Wait();
            if (listOfPlayers.Where(x => x.LastMove != Move.Pass).Count() == 1)
                throw new OnePlayerException();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(0, cords.Item2 + 1);

            if (firstRaise && listOfPlayers[0].LastMove != Move.Raise)
                Console.WriteLine("Someone took raise \n\n");
            else if(firstRaise)
                Console.WriteLine("You have to raise !\n\n");
            else
                Console.SetCursorPosition(0, Console.GetCursorPosition().Top + 1);


            Console.ResetColor();
            EnterPress();
            chooseOption = false;

            // Method to raise amount of money (if somebody choosed raise)
            if (listOfPlayers.Any(x => x.LastMove == Move.Raise && x.LastMove != Move.Pass))
            {
                RaiseActivePlayers();
                if (listOfPlayers.Where(x => x.LastMove != Move.Pass).Count() == 1)
                    throw new OnePlayerException();
            }

            firstRaise = false;
            Player.SetFold();

        } // Options for players when cards was putted by croupier
        private static void RaiseComputer(int amount, Dictionary<Player, (int,int)> cursor)
        {
            try
            {
                Dictionary<Player, bool> activePlayers = ActivePlayersToDictionary();
                var cords = Console.GetCursorPosition();
                for (int i = 0; i < activePlayers.Count; i++)
                {
                    Player player = activePlayers.Keys.ElementAt(i);
                    if (player.LastMove == Move.AllIn)
                    {
                        Console.SetCursorPosition(cursor[player].Item1, cursor[player].Item2);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(Move.AllIn);
                        Console.ResetColor();
                    }
                }

                activePlayers = activePlayers.Where(x => x.Key.LastMove != Move.AllIn).ToDictionary();
                Random random = new Random();

                for (int i = 0; i < activePlayers.Count; i++)
                {
                    Thread.Sleep(random.Next(2000, 5000));
                    Player player = null;
                    string name;
                    while (true)
                    {
                        int indexOfPlayer = random.Next(0, activePlayers.Count);
                        player = activePlayers.Keys.ElementAt(indexOfPlayer);

                        if (activePlayers[player] == true)
                            continue;
                        else
                        {
                            activePlayers[player] = true;
                            name = player.Name;
                            break;
                        }
                    }

                    Move callOrPass = listOfPlayers.Where(x => x.Name == name).First().CallOrPass(amount, player.Monets);

                    if (callOrPass == Move.Call || callOrPass == Move.AllIn)
                        listOfPlayers.Where(x => x.Name == name).First().RaiseMoney(amount);
                    else
                        listOfPlayers.Where(x => x.Name == name).First().LastMove = Move.Pass;

                    Console.SetCursorPosition(cursor[player].Item1, cursor[player].Item2);
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write($"{callOrPass}");
                    Console.ResetColor();

                    if (listOfPlayers.Where(x => x.LastMove != Move.Pass).Count() == 1)
                        throw new OnePlayerException();

                    if (!chooseOption)
                        Console.SetCursorPosition(cords.Item1, cords.Item2);
                }
            }
            catch (OnePlayerException)
            {
                return;
            }
        } // Amount raise by computer
        private static void RaiseActivePlayers()
        {
            Dictionary<Player, (int, int)> cursor = new Dictionary<Player, (int, int)>(); // Cords for place to write information
            int amount; // Amount to raise
            (int, int) cords; // Cords of user

            if (listOfPlayers[0].LastMove == Move.Raise) // Raise for User
            {
                while(true)
                {
                    Console.Clear();
                    BankShow(); // Showing Bank
                    Card.DrawCardOnTable(cardsOnTable, justShow:true);
                    Console.WriteLine();
                    ShowingPlayers(cursor); // Showing players
                    Console.Write("\nWrite amount of monets (min 10 monets): ");

                    cords = Console.GetCursorPosition();
                    // Writing amount of monets
                    if (int.TryParse(Console.ReadLine(), out amount) && amount >= 10)
                    {
                        amount = (amount / 10) * 10;
                        Console.SetCursorPosition(cords.Item1, cords.Item2);
                        Console.ForegroundColor = ConsoleColor.Green;
                        amount = amount > listOfPlayers[0].Monets ? listOfPlayers[0].Monets : amount;

                        // Setting amount 
                        if (listOfPlayers.Where(x => !x.IsPlayer && x.LastMove != Move.Pass).All(x => x.Monets < amount))
                            amount = listOfPlayers.Where(x => !x.IsPlayer && x.LastMove != Move.Pass).Max(x => x.Monets);

                        string move = amount == listOfPlayers[0].Monets ? Move.AllIn.ToString() : amount.ToString();
                        Console.WriteLine(move + "    \n");
                        Console.ResetColor();
                        cords = Console.GetCursorPosition();
                        Console.WriteLine("Waiting for the rest of the players . . .");
                    }
                    else
                    {
                        cursor.Clear();
                        continue;
                    }
                       
                    // Raise monets
                    listOfPlayers[0].RaiseMoney(amount);
                    break;
                }
                RaiseComputer(amount, cursor); 
                Console.SetCursorPosition(0, cords.Item2 + 2);
            }
            else // Raise money for computer
            {
                Console.Clear();
                BankShow(); // Bank Show
                Card.DrawCardOnTable(cardsOnTable, justShow: true);
                Console.WriteLine();
                ShowingPlayers(cursor); // Showing Players
                cords = Console.GetCursorPosition(); // Setting cords
                HandRank handRank = PokerHandEvaluator.CheckHand(listOfPlayers.Where(x => x.LastMove == Move.Raise).First()); // Hand rank for computer
                int monets = listOfPlayers.Where(x => x.LastMove == Move.Raise).First().Monets; // Setting monets of computer

                // Setting call
                if ((int)handRank >= 0 && (int)handRank <= 3)
                    monets = NumericData.StrongCall(0.3, monets);
                else if ((int)handRank >= 4 && (int)handRank <= 8)
                    monets = NumericData.StrongCall(0.6, monets);
                else
                    monets = NumericData.StrongCall(1, monets);

                monets = (monets / 10) * 10;

                // Sleep for 2 s
                Thread.Sleep(2000);

                Player computer = listOfPlayers.Where(x => x.LastMove == Move.Raise).FirstOrDefault();
                Console.SetCursorPosition(cursor[computer].Item1, cursor[computer].Item2); // Setting cursor position

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Raised {monets} monets!"); // Information about monets
                Console.SetCursorPosition(cords.Item1, cords.Item2);
                Console.ResetColor();

                userToken = new CancellationTokenSource(); // Special token to delete user method
                computerToken = new CancellationTokenSource();  // Special token to delete Computer method
                switch (listOfPlayers[0].LastMove)
                {
                    case Move.AllIn:
                        Console.Write($"Your move: {listOfPlayers[0].LastMove}");
                        cords = Console.GetCursorPosition(); // Setting cords
                        listOfPlayers.Where(x => x.LastMove == Move.Raise).FirstOrDefault().RaiseMoney(monets);
                        Console.ResetColor();
                        AsyncMethods.ComputerCallOrPass(cords, cursor, monets).Wait();
                        break;
                    case Move.Pass:
                        AsyncMethods.ComputerCallOrPass(cords, cursor, monets).Wait();
                        break;
                    default:
                        string options = monets >= listOfPlayers[0].Monets ? "AllIn" : "Call";
                        Console.WriteLine($"\n1. {options}\n2. Pass\n\n");
                        Console.Write("Your move: ");
                        cords = Console.GetCursorPosition(); // Setting cords
                        listOfPlayers.Where(x => x.LastMove == Move.Raise).FirstOrDefault().RaiseMoney(monets);
                        Console.ResetColor();
                        Task.WhenAll(AsyncMethods.PlayerCallOrPass(monets), AsyncMethods.ComputerCallOrPass(cords, cursor, monets)).Wait(); // Call or pass for user and computer
                        break;
                }
                Console.SetCursorPosition(0, cords.Item2 + 2);
            }
            EnterPress();
        } // Choosing who was first to raise
        public static Dictionary<Player, bool> ActivePlayersToDictionary()
        {
            Dictionary<Player, bool> activePlayers = new Dictionary<Player, bool>();
            for (int i = 0; i < listOfPlayers.Count; i++)
            {
                if (!listOfPlayers[i].IsPlayer && listOfPlayers[i].LastMove != Move.Pass)
                    activePlayers.Add(listOfPlayers[i], false);
            }
            return activePlayers;
        } // Setting active players to dictionary
        public static void StartReadingKeys()
        {
            Task.Run(() =>
            {
                while (!userToken.Token.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true);
                        keyBuffer.Add(key);
                    }
                }
            });
        } // Method for user to enter char
        private static void EnterPress(string message = "Click Enter to continue")
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(message);
            ConsoleKeyInfo consoleKeyInfo;
            do
            {
                consoleKeyInfo = Console.ReadKey(intercept: true);
                if (consoleKeyInfo.Key == ConsoleKey.Enter)
                    break;
            } while (true);
            Console.ResetColor();
        } // Enter press
        private static void BankShow(bool x = true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Bank: {bank}");
            Console.ResetColor();
            Console.WriteLine($"Your money: {listOfPlayers[0].Monets}\n");

            if (x)
                listOfPlayers[0].ShowDeck();
        } // Showing bank account
        private static void ShowingPlayers(Dictionary<Player, (int,int)> cursor)
        {
            cursor.Clear();
            foreach (Player player in listOfPlayers)
            {
                if (!player.IsPlayer && player.LastMove != Move.Pass)
                {
                    Console.Write($"{player.Name}\tMonets: {player.Monets}\tMove: ");
                    cursor.Add(player, (Console.GetCursorPosition()));
                    Console.WriteLine("\n");
                }
            }
        } // Showing all players
        private static void FinalResult(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(Card.infoDeck);
            Console.ResetColor();

            foreach(var player in listOfPlayers)
            {
                Console.Write(player.Name + " ");
                if(player.LastMove == Move.Pass)
                    Console.Write($"({player.LastMove}) ");
                player.ShowDeck();
                player.Hand = PokerHandEvaluator.CheckHand(player);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.SetCursorPosition(Console.GetCursorPosition().Left, Console.GetCursorPosition().Top - 1);
                Console.WriteLine("Hand: " + player.Hand + "\n");
                Console.ResetColor();
            }

            Console.WriteLine("\n");
            Console.WriteLine(Player.ChooseWinner() + $" {message}");

            // ? wygranie monet

            EnterPress();
        } // Final result for players
        private static ILogger GetLogger()
        {
            return new FileLogger();
        } // Get class of logger
    }
}
