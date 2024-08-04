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
                // Setting fold for everyone
                Player.SetFold();

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

                // Take move from players
                OptionsInGame(true, 0);
                Console.Clear();

                // Showing your deck
                Console.WriteLine("Your Deck\n");
                listOfPlayers.Where(x => x.IsPlayer).First().ShowDeck();

                // Croupier deals 3 cards on the table
                Console.WriteLine("Cards putted by croupier\n");
                Card card;
                for (int i = 0; i < 3; i++)
                {
                    card = Player.gameDeck.Pop();
                    cardsOnTable.Add(card);
                }
                Card.DrawCardOnTable(cardsOnTable,2000);

                // ###############################################
                // ###############################################          
                // ###############################################


                // Croupier deals 1 card on the table
                for (int i = 0; i < 2; i++)
                {
                    OptionsInGame(true, i+1);
                    Console.Clear();
                    Console.WriteLine("Your Deck\n");
                    listOfPlayers.Where(x => x.IsPlayer).First().ShowDeck();

                    Console.WriteLine("\n\n\nCards putted by croupier\n");
                    card = Player.gameDeck.Pop();
                    cardsOnTable.Add(card);
                    Card.DrawCardOnTable(cardsOnTable,2000);
                }

                OptionsInGame(false, 3);
                Console.Clear();
                HandRank handRank = PokerHandEvaluator.CheckHand(listOfPlayers.Where(x => x.IsPlayer).FirstOrDefault());

                listOfPlayers.Where(x => x.IsPlayer).FirstOrDefault().ShowDeck();
                Card.DrawCardOnTable(cardsOnTable);
                Console.WriteLine(handRank.ToString());
                Console.ReadKey();
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
                Card.DrawCardOnTable(cardsOnTable);
            }

            ShowingPlayers(cursor); // Showing Players
            string whichOption = option == true ? "Wait" : "Check";
            string options = $"\nOptions:\n\n1 - Raise\n2 - {whichOption}\n3 - Pass \n\n";
            (int, int) cords = (0, 0);

            // Menu for player
            switch(listOfPlayers[0].LastMove)
            {
                case Move.AllIn:
                    Console.Write("Your move: ");
                    cords = Console.GetCursorPosition();
                    HandleUserAndComputerMoves(lvl, cursor).Wait();
                    break;

                case Move.Pass:
                    cords = Console.GetCursorPosition();
                    GetComputerMoveAsync(lvl, cursor);
                    break;

                default:
                    Console.WriteLine(options);
                    Console.Write("Your move: ");
                    cords = Console.GetCursorPosition();
                    HandleUserAndComputerMoves(lvl, cursor).Wait();
                    break;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            if (firstRaise && listOfPlayers[0].LastMove != Move.Raise)
            {
                Console.SetCursorPosition(0, cords.Item2 + 1);
                Console.WriteLine("Someone took raise \n\n");
            }
            else if(firstRaise)
            {
                Console.SetCursorPosition(0, cords.Item2 + 1);
                Console.WriteLine("You have to raise !\n\n");

            }
            else
                Console.SetCursorPosition(0, cords.Item2 + 2);


            // ###

            Console.ResetColor();
            EnterPress();

            // Method to raise amount of money (if somebody choosed raise)
            if (listOfPlayers.Any(x => x.LastMove == Move.Raise && x.LastMove != Move.Pass))
                RaiseActivePlayers();

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

                        // Adding Last move for a main player
                        if (Move.Raise == moveEnum)
                        {
                            firstRaise = true;
                            computerToken.Cancel(); // Deleting computer async method
                        }
                        // setting last move for user
                        listOfPlayers[0].LastMove = (Move)(int.Parse(consoleKeyInfo.KeyChar.ToString()));
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        // Breaking method
                        return;
                    }
                }
            }, token);
        } // User Async method to choose move
        private static Task GetComputerMoveAsync(int lvl, Dictionary<Player, (int, int)> cursor)
        {
            return Task.Run( async () =>
            {
                await RandomPlayerChoose(cursor, false, lvl);
            });
        } // Computer async method to choose move
        private static Task RandomPlayerChoose(Dictionary<Player, (int, int)> cursor, bool raise, int lvl = 0, int amount = 0)
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

                        if (!chooseOption)
                            Console.SetCursorPosition(cords.Item1, cords.Item2);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            },token);
        }
        private static async Task HandleUserAndComputerMoves(int lvl, Dictionary<Player, (int, int)> cursor)
        {
            userToken = new CancellationTokenSource(); // Special token to delete user method
            computerToken = new CancellationTokenSource();  // Special token to delete Computer method
            await Task.WhenAll(GetUserMoveAsync(), GetComputerMoveAsync(lvl, cursor));
        }
        private static void RaiseComputer(int amount, Dictionary<Player, (int,int)> cursor)
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
                string move = callOrPass.ToString();

                if (callOrPass == Move.Call)
                    listOfPlayers.Where(x => x.Name == name).First().RaiseMoney(amount);
                else
                    listOfPlayers.Where(x => x.Name == name).First().LastMove = Move.Pass;


                Console.SetCursorPosition(cursor[player].Item1, cursor[player].Item2);
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write($"{move}");
                Console.ResetColor();

                if (!chooseOption)
                    Console.SetCursorPosition(cords.Item1, cords.Item2);
            }
        }
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
                        amount = (amount / 10) * 10;
                    else
                    {
                        cursor.Clear();
                        continue;
                    }
                        
                    // Setting amount 
                    if (listOfPlayers.Where(x => !x.IsPlayer && x.LastMove != Move.Pass).All(x => x.Monets < amount))
                        amount = listOfPlayers.Where(x => !x.IsPlayer && x.LastMove != Move.Pass).Max(x => x.Monets);

                    // Raise monets
                    listOfPlayers[0].RaiseMoney(amount);
                    break;
                }
                RaiseComputer(amount, cursor); // Random computer move 
            }
            else // Raise money for computer
            {
                Console.Clear();
                BankShow(); // Bank Show
                Console.WriteLine("\n1. Pass\n2. Call\n\n");
                ShowingPlayers(cursor); // Showing Players
                
                Console.Write("Your move: ");
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

                // Sleep for 2 s
                Thread.Sleep(2000);

                Player computer = listOfPlayers.Where(x => x.LastMove == Move.Raise).FirstOrDefault();
                Console.SetCursorPosition(cursor[computer].Item1, cursor[computer].Item2); // Setting curosr position

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Raised {monets} monets!"); // Information about monets
                Console.SetCursorPosition(cords.Item1, cords.Item2);
                listOfPlayers.Where(x => x.LastMove == Move.Raise).FirstOrDefault().RaiseMoney(monets);
                Console.ResetColor();

                Task.WhenAll(PlayerCallOrPass(monets), ComputerCallOrPass(cords, cursor, monets)).Wait(); // Call or pass for user and computer
            }

            Console.SetCursorPosition(0, cords.Item2+2);
            EnterPress();
        }
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
        }
        private static Dictionary<Player, bool> ActivePlayersToDictionary()
        {
            Dictionary<Player, bool> activePlayers = new Dictionary<Player, bool>();
            for (int i = 0; i < listOfPlayers.Count; i++)
            {
                if (!listOfPlayers[i].IsPlayer && listOfPlayers[i].LastMove != Move.Pass)
                    activePlayers.Add(listOfPlayers[i], false);
            }
            return activePlayers;
        }
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
        }
        private static Task PlayerCallOrPass(int amount)
        {
            return Task.Run(() =>
            {
                do
                {
                    ConsoleKeyInfo key = Console.ReadKey(intercept : true);

                    if(int.TryParse(key.KeyChar.ToString(), out int result) && result <= 2 && result >= 1)
                    {
                        listOfPlayers[0].LastMove = (Move)(result + 2);

                        if (listOfPlayers[0].LastMove == Move.Call)
                            listOfPlayers[0].RaiseMoney(amount);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(listOfPlayers[0].LastMove.ToString());
                        Console.ResetColor();
                        break;
                    }

                } while (true);
            });
        }
        private static Task ComputerCallOrPass((int,int) cords, Dictionary<Player, (int, int)> cursor, int amount)
        {
            return Task.Run( async () =>
            {
                Dictionary<Player, bool> activePlayers = new Dictionary<Player, bool>();

                for(int i = 1; i < listOfPlayers.Count; i++)
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
            });
        }
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

        }
        private static void EnterPress()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Click Enter to continue");
            ConsoleKeyInfo consoleKeyInfo;
            do
            {
                consoleKeyInfo = Console.ReadKey(intercept: true);
                if (consoleKeyInfo.Key == ConsoleKey.Enter)
                    break;
            } while (true);
            Console.ResetColor();
        }
        private static void ShowingPlayers(Dictionary<Player, (int,int)> cursor)
        {
            foreach (Player player in listOfPlayers)
            {
                if (!player.IsPlayer && player.LastMove != Move.Pass)
                {
                    Console.Write($"{player.Name}\tMonets: {player.Monets}\tMove: ");
                    cursor.Add(player, (Console.GetCursorPosition()));
                    Console.WriteLine("\n");
                }
            }
        }
    }
}
