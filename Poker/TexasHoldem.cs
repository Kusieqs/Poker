using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Reflection;

namespace Poker
{
    public static class TexasHoldem
    {
        public static List<Player> listOfPlayers = new List<Player>(); // list of players
        public static List<Card> cardsOnTable = new List<Card>(); // cards on the table
        public static int bank; // bank amount
        private static bool chooseOption;
        public static bool firstRaise = false;
        private static CancellationTokenSource userToken;
        private static CancellationTokenSource computerToken;
        private static BlockingCollection<ConsoleKeyInfo> keyBuffer;
        private static (int, int) cords;

        public static void Game(List<Player> players)
        {
            // Players added to main list
            listOfPlayers = players;

            // Engine of game
            EngineOfGame();

            // END
        } // Main Game
        public static void EngineOfGame()
        {
            Console.Clear();
            // Game engine
            do
            {
                try
                {
                    // Setting fold for everyone
                    Player.SetFold(true);

                    // Checking whether our player is enable to play
                    bool IsPlaying = StartRoundMenu();

                    // Exit game
                    if (!IsPlaying)
                        Environment.Exit(0);

                    // Creating deck and shuffle
                    Player.CreatingDeck();
                    Player.Shuffle();
                    Console.Clear();

                    // Dealing cards to decks
                    DealCards();

                    CheckPlayers(); // Optional method to show decks of players

                    // Take move from players
                    OptionsInGame(true, 0);
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
                    Console.Clear();

                    // Croupier deals 1 card on the table
                    for (int i = 0; i < 2; i++)
                    {
                        BankShow();
                        Console.WriteLine(Card.infoDeck);
                        card = Player.gameDeck.Pop();
                        cardsOnTable.Add(card);
                        Card.DrawCardOnTable(cardsOnTable, 2000);
                        OptionsInGame(false, i + 2);
                    }

                    Console.Clear();
                    HandRank handRank = PokerHandEvaluator.CheckHand(listOfPlayers.Where(x => x.IsPlayer).FirstOrDefault());

                    listOfPlayers.Where(x => x.IsPlayer).FirstOrDefault().ShowDeck();
                    Card.DrawCardOnTable(cardsOnTable);
                    Console.WriteLine(handRank.ToString());
                    Console.ReadKey();

                    // Koncowa metoda na porowbnanioe pokazanie kard i wywolanie zwyciescy
                }
                catch (OnePlayerException ex)
                {
                    Console.SetCursorPosition(0, cords.Item2 + 2);
                    Console.WriteLine(ex.Message);
                    EnterPress();
                }
                finally
                {
                    listOfPlayers.Where(x => x.LastMove != Move.Pass).First().Monets += bank;
                    bank = 0;
                }

            } while (true);
        } // Engine of the game
        public static bool StartRoundMenu()
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
            BankShow(false, "Croupier deals the cards . . .\n");
            Thread.Sleep(2000);
            int i = 1;

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
            HandleUserAndComputerMoves(lvl, cursor, options).Wait();
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
        private static Task GetUserMoveAsync()
        {
            CancellationToken token = userToken.Token;
            return Task.Run(() =>
            {
                while (true)
                {
                    if (listOfPlayers[0].LastMove == Move.AllIn)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(Move.AllIn);
                        Console.ResetColor();
                        break;
                    }

                    try
                    {
                        Move moveEnum;
                        ConsoleKeyInfo consoleKeyInfo;
                        do
                        {

                            StartReadingKeys(); // Reading key if token is open
                            keyBuffer = new BlockingCollection<ConsoleKeyInfo>();
                            consoleKeyInfo = keyBuffer.Take(token);

                            // Choosing one of three options
                            if (int.TryParse(consoleKeyInfo.KeyChar.ToString(), out int result))
                            {
                                if (result >= 1 && result <= 3)
                                {
                                    moveEnum = (Move)result;
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.Write(moveEnum.ToString());
                                    Console.ResetColor();
                                    chooseOption = true;
                                    break;
                                }
                            }

                        } while (true);

                        // setting last move for user
                        listOfPlayers[0].LastMove = (Move)(int.Parse(consoleKeyInfo.KeyChar.ToString()));

                        // Adding Last move for a main player
                        if (Move.Raise == moveEnum)
                        {
                            firstRaise = true;
                            computerToken.Cancel(); // Deleting computer async method
                        }
                        else if (Move.Pass == moveEnum && listOfPlayers.Where(x => x.LastMove != Move.Pass).Count() == 1)
                            throw new OnePlayerException();

                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        // Breaking method
                        return;
                    }
                    catch (OnePlayerException)
                    {
                        computerToken.Cancel();
                        return;
                    }
                }
            }, token);
        } // User Async method to choose move
        private static Task GetComputerMoveAsync(Dictionary<Player, (int, int)> cursor, int lvl = 0, int amount = 0)
        {
            CancellationToken token = computerToken.Token;
            return Task.Run( async () =>
            {
                try
                {
                    Dictionary<Player, bool> activePlayers = ActivePlayersToDictionary(); // Setting active players and cords for them
                    var cords = Console.GetCursorPosition();
                    Random random = new Random();

                    foreach(var dict in activePlayers)
                    {
                        if(dict.Key.LastMove == Move.AllIn)
                        {
                            Console.SetCursorPosition(cursor[dict.Key].Item1, cursor[dict.Key].Item2);
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.Write($"{Move.AllIn}");
                            Console.ResetColor();
                            activePlayers.Remove(dict.Key);
                            Console.SetCursorPosition(cords.Left, cords.Top);
                        }
                    }

                    for (int i = 0; i < activePlayers.Count; i++)
                    {
                        await Task.Delay(random.Next(1000, 5000), token);
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

                        string move = listOfPlayers.Where(x => x.Name == name).First().ChooseMoveForComputer(lvl);
                        Console.SetCursorPosition(cursor[player].Item1, cursor[player].Item2);
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write($"{move}");
                        Console.ResetColor();

                        if (Enum.Parse<Move>(move) == Move.Raise)
                        {
                            firstRaise = true;
                            userToken.Cancel();
                            break;
                        }
                        else if (Move.Pass == Enum.Parse<Move>(move) && listOfPlayers.Where(x => x.LastMove != Move.Pass).Count() == 1)
                            throw new OnePlayerException();

                        if (!chooseOption)
                            Console.SetCursorPosition(cords.Item1, cords.Item2);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (OnePlayerException)
                {
                    userToken.Cancel();
                    return;
                }
            },token);
        } // Computer Async method to choose move
        private static async Task HandleUserAndComputerMoves(int lvl, Dictionary<Player, (int, int)> cursor, string options)
        {
            userToken = new CancellationTokenSource(); // Special token to delete user method
            computerToken = new CancellationTokenSource();  // Special token to delete Computer method

            switch (listOfPlayers[0].LastMove)
            {
                case Move.AllIn:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"Your move: {listOfPlayers[0].LastMove}");
                    Console.ResetColor();
                    cords = Console.GetCursorPosition();
                    Console.WriteLine("Waiting for the rest of the players . . .");
                    await GetComputerMoveAsync(cursor, lvl);
                    break;

                case Move.Pass:
                    cords = Console.GetCursorPosition();
                    await GetComputerMoveAsync(cursor, lvl);
                    break;

                default:
                    Console.WriteLine(options);
                    Console.Write("Your move: ");
                    cords = Console.GetCursorPosition();
                    await Task.WhenAll(GetUserMoveAsync(), GetComputerMoveAsync(cursor, lvl));
                    break;
            }
        } // Setting new tokens and active async methods
        private static void RaiseComputer(int amount, Dictionary<Player, (int,int)> cursor)
        {
            try
            {
                Dictionary<Player, bool> activePlayers = ActivePlayersToDictionary();
                var cords = Console.GetCursorPosition();
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

                    Move callOrPass = listOfPlayers.Where(x => x.Name == name).First().CallOrPass(amount);

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
                        Console.WriteLine(move + "\n");
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
                Console.SetCursorPosition(0, cords.Item2);
            }
            else // Raise money for computer
            {
                Console.Clear();
                BankShow(); // Bank Show
                ShowingPlayers(cursor); // Showing Players
                cords = Console.GetCursorPosition(); // Setting cords
                HandRank handRank = PokerHandEvaluator.CheckHand(listOfPlayers.Where(x => x.LastMove == Move.Raise).First()); // Hand rank for computer
                int monets = listOfPlayers.Where(x => x.LastMove == Move.Raise).First().Monets; // Setting monets of computer

                // Setting call
                if ((int)handRank >= 0 && (int)handRank <= 3)
                    monets = StrongCall(0.3, monets);
                else if ((int)handRank >= 4 && (int)handRank <= 8)
                    monets = StrongCall(0.6, monets);
                else
                    monets = StrongCall(1, monets);

                monets = (monets / 10) * 10;

                // Sleep for 2 s
                Thread.Sleep(2000);

                Player computer = listOfPlayers.Where(x => x.LastMove == Move.Raise).FirstOrDefault();
                Console.SetCursorPosition(cursor[computer].Item1, cursor[computer].Item2); // Setting curosr position

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
                        ComputerCallOrPass(cords, cursor, monets).Wait();
                        break;
                    case Move.Pass:
                        ComputerCallOrPass(cords, cursor, monets).Wait();
                        break;
                    default:
                        string options = monets >= listOfPlayers[0].Monets ? "AllIn" : "Call";
                        Console.WriteLine($"\n1. {options}\n2. Pass\n\n");
                        Console.Write("Your move: ");
                        cords = Console.GetCursorPosition(); // Setting cords
                        listOfPlayers.Where(x => x.LastMove == Move.Raise).FirstOrDefault().RaiseMoney(monets);
                        Console.ResetColor();
                        Task.WhenAll(PlayerCallOrPass(monets), ComputerCallOrPass(cords, cursor, monets)).Wait(); // Call or pass for user and computer
                        break;
                }
                Console.SetCursorPosition(0, cords.Item2 + 2);
            }
        } // Choosing who was first to raise
        private static void BankShow(bool x = true, string additionalString = "")
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Bank: {bank}");
            Console.ResetColor();
            Console.WriteLine($"Your money: {listOfPlayers[0].Monets}\n");

            if(!string.IsNullOrEmpty(additionalString))
                Console.WriteLine(additionalString);

            if (x)
                listOfPlayers[0].ShowDeck();
        } // Showing bank account
        private static Dictionary<Player, bool> ActivePlayersToDictionary()
        {
            Dictionary<Player, bool> activePlayers = new Dictionary<Player, bool>();
            for (int i = 0; i < listOfPlayers.Count; i++)
            {
                if (!listOfPlayers[i].IsPlayer && listOfPlayers[i].LastMove != Move.Pass)
                    activePlayers.Add(listOfPlayers[i], false);
            }
            return activePlayers;
        } // Setting active players to dictionary
        private static void StartReadingKeys()
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
        private static Task PlayerCallOrPass(int amount)
        {
            CancellationToken token = userToken.Token;
            return Task.Run(() =>
            {
                do
                {
                    try
                    {
                        ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                        if (int.TryParse(key.KeyChar.ToString(), out int result) && result <= 2 && result >= 1)
                        {

                            if (result == 1)
                            {
                                listOfPlayers[0].LastMove = Move.Call;
                                listOfPlayers[0].RaiseMoney(amount);
                            }
                            else
                                listOfPlayers[0].LastMove = Move.Pass;

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(listOfPlayers[0].LastMove.ToString());
                            Console.ResetColor();
                            break;
                        }
                    }
                    catch (OnePlayerException)
                    {
                        computerToken.Cancel();
                        return;
                    }
                } while (true);
            },token);
        } // User method to choose Call (Allin) or pass
        private static Task ComputerCallOrPass((int,int) cords, Dictionary<Player, (int, int)> cursor, int amount)
        {
            CancellationToken token = computerToken.Token;
            return Task.Run( async () =>
            {
                try
                {
                    Dictionary<Player, bool> activePlayers = new Dictionary<Player, bool>();

                    for (int i = 1; i < listOfPlayers.Count; i++)
                    {
                        if (listOfPlayers[i].LastMove != Move.Raise && listOfPlayers[i].LastMove != Move.Pass)
                            activePlayers.Add(listOfPlayers[i], false);
                    }

                    for (int i = 0; i < listOfPlayers.Where(x => !x.IsPlayer && x.LastMove != Move.Pass && x.LastMove != Move.Raise).Count(); i++)
                    {
                        Random random = new Random();
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
                        await Task.Delay(random.Next(1000, 5000));

                        Move callOrPass = listOfPlayers.Where(x => x.Name == name).First().CallOrPass(amount);
                        string move = callOrPass.ToString();

                        if (callOrPass == Move.Call)
                            listOfPlayers.Where(x => x.Name == name).First().RaiseMoney(amount);
                        else
                            listOfPlayers.Where(x => x.Name == name).First().LastMove = Move.Pass;

                        Console.SetCursorPosition(cursor[player].Item1, cursor[player].Item2);
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write($"{move}");
                        Console.ResetColor();
                        Console.SetCursorPosition(cords.Item1, cords.Item2);

                    }
                }
                catch (OnePlayerException)
                {
                    userToken.Cancel();
                    return;
                }                
            },token);
        } // Computer method to choose Call (Allin) or pass
        private static int StrongCall(double procent, int monets)
        {
            Random random = new Random();
            int amount = (int)Math.Round(((monets * procent)/10)*10);
            int probability = random.Next(1, 11);
            do
            {
                int raise = 0;
                switch (procent)
                {
                    case 0.3:
                        if (probability >= 1 && probability <= 6)
                            raise = (random.Next(1, amount) / 10) * 10;
                        else
                            raise = amount;
                        break;
                    case 0.6:
                        if (probability >= 1 && probability <= 6)
                            raise = (random.Next(1, amount) / 10) * 10;
                        else if (probability == 10)
                            raise = monets;
                        else
                            raise = amount;
                        break;
                    case 1:
                        if (probability >= 1 && probability <= 6)
                            raise = (random.Next(1, amount) / 10) * 10;
                        else if (probability == 7 || probability == 8)
                            raise = monets;
                        else
                            raise = amount;
                        break;
                }

                if (raise >= 10)
                    return raise;

            } while (true);

        } // setting amount of monets for comupter
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
        private static void CheckPlayers()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"Rozdania");
            string body = string.Empty;
            foreach(Player player in listOfPlayers)
            {
                body += $"{player.Name}\n\n{player.Deck[0].Rank} {player.Deck[0].Suit}\n{player.Deck[1].Rank} {player.Deck[1].Suit}\n\n\n";
            }
            File.WriteAllText(Path.Combine(path,"Info.txt"), body);
        } // Additional method to downwrite file with decks into txt file
    }
}
