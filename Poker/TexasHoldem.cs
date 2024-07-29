using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Poker
{
    public static class TexasHoldem
    {
        public static List<Player> listOfPlayers = new List<Player>(); // list of players
        public static List<Card> cardsOnTable = new List<Card>(); // cards on the table
        public static int bank; // bank amount
        private static bool chooseOption;
        public static bool firstRaise = false;
        private static CancellationTokenSource cts;
        private static BlockingCollection<ConsoleKeyInfo> keyBuffer;

        public static void Game(List<Player> players)
        {
            // Players added to main list
            listOfPlayers = players;

            // Engine of game
            EngineOfGame();

            // END
        }
        public static void EngineOfGame()
        {
            Console.Clear();
            // Game engine
            do
            {
                // Setting that everybody will play
                SettingWhichPlayerWillPlay(true);

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

                // ###############################################
                // ###############################################


                // Croupier deals 3 cards on the table
                Console.WriteLine("Cards putted by croupier\n");
                Card card;
                for (int i = 0; i < 3; i++)
                {
                    card = Player.gameDeck.Pop();
                    cardsOnTable.Add(card);
                }
                Card.DrawCardOnTable(cardsOnTable,2000);

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
        private static void SettingWhichPlayerWillPlay(bool isPlaying = false)
        {
            foreach (Player player in listOfPlayers)
            {
                if (!isPlaying && player.LastMove == Move.Pass)
                    player.IsPlaying = false;

                if (isPlaying)
                    player.LastMove = Move.Fold;
            }
        } // Setting which player is enable to play
        public static bool StartRoundMenu()
        {
            do
            {
                // choosing option
                Console.Clear();
                BankShow(false);
                Console.WriteLine();

                foreach (Player player in listOfPlayers)
                {
                    if (player.IsPlayer)
                        continue;

                    Console.WriteLine($"{player.Name} Monets: {player.Monets}\n");
                }

                Console.WriteLine("\n\n1. Play (Raise 30 monets to enabled to play)\n2. Exit");
                Console.Write("\nNumber: ");
                ConsoleKeyInfo choose = Console.ReadKey();
                if (choose.KeyChar.ToString() == "1")
                {
                    // Checking our player that he has got 30 monets in pocket
                    if (listOfPlayers.Where(x => x.IsPlayer == true).First().Monets - 30 < 0)
                        continue;

                    // Taking money from pocket to bank
                    foreach (Player player in listOfPlayers)
                    {
                        if (player.Monets - 30 < 0)
                        {
                            player.IsPlaying = false;
                            continue;
                        }
                        player.RaiseMoney(30);
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
            Thread.Sleep(1000);
            int i = 1;

            foreach (Player player in listOfPlayers)
            {
                player.RaiseCard();

                if (player.IsPlayer)
                {
                    player.ShowDeck();
                    Console.WriteLine();
                }
                else
                {
                    Console.Write(player.Name);
                    Thread.Sleep(1300);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" is ready!\n");
                    Console.ResetColor();
                    ++i;
                }
            }
            Console.WriteLine("\n\nClick enter to continue");
            Console.ReadKey();
        } // Dealing 2 cards for each player
        private static void OptionsInGame(bool option, int lvl)
        {
            Console.Clear();
            BankShow();
            Dictionary<Player, (int, int)> cursor = new Dictionary<Player, (int, int)>();
           
            if(cardsOnTable.Count != 0)
            {
                Console.WriteLine("Cards on table:");
                Card.DrawCardOnTable(cardsOnTable);
            }

            foreach(Player player in listOfPlayers)
            {
                if(!player.IsPlayer && player.IsPlaying)
                {
                    Console.Write($"{player.Name}\tMonets: {player.Monets}\tMove: ");
                    cursor.Add(player, (Console.GetCursorPosition()));
                    Console.WriteLine("\n");
                }
            }

            // Menu for player
            string whichOption = option == true ? "Wait" : "Check";
            string options = $"\nOptions:\n\n1 - Raise\n2 - {whichOption}\n3 - Pass \n\n";

            Console.WriteLine(options);
            Console.Write("Your move: ");
            var cords = Console.GetCursorPosition();

            // Async methods 

            cts = new CancellationTokenSource();
            Task.WhenAll(GetUserMoveAsync(cts.Token), GetComputerMoveAsync(lvl, cursor)).Wait();

            Console.SetCursorPosition(0, cords.Top+2);
            Console.WriteLine("Click enter to continue");
            Console.ReadKey();

            // Setting which player choosed pass
            SettingWhichPlayerWillPlay();

            // Method to raise amount of money (if somebody choosed raise)
            if (listOfPlayers.Any(x => x.LastMove == Move.Raise && x.IsPlaying))
                RaiseActivePlayers();

            firstRaise = false;
            Player.SetFold();

        } // Options for players when cards was putted by croupier
        private static Task GetUserMoveAsync(CancellationToken token)
        {
            return Task.Run(() =>
            {
                Move moveEnum;
                while (true)
                {
                    try
                    {
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
                            firstRaise = true;

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
        } // Async method for user
        private static Task GetComputerMoveAsync(int lvl, Dictionary<Player, (int, int)> cursor)
        {
            return Task.Run(async () =>
            {
                await RandomPlayerChoose(cursor, false,lvl);
            });
        } // Async method for computer
        private static void RaiseActivePlayers()
        {
            Dictionary<Player, (int, int)> cursor = new Dictionary<Player, (int, int)>();
            int amount;
            if (listOfPlayers[0].LastMove == Move.Raise)
            {
                while(true)
                {
                    Console.Clear();
                    BankShow();
                    Console.WriteLine();

                    foreach (Player player in listOfPlayers)
                    {
                        if (!player.IsPlayer && player.IsPlaying)
                        {
                            Console.Write($"{player.Name}\tMonets: {player.Monets}\tMove: ");
                            cursor.Add(player, Console.GetCursorPosition());
                            Console.WriteLine("\n");
                        }
                    }

                    Console.Write("\nWrite amount of monets (min 10 monets): ");
                    amount = (int.Parse(Console.ReadLine()) / 10) * 10;

                    if (amount < 10)
                    {
                        cursor.Clear();
                        continue;
                    }
                        

                    if (listOfPlayers.Where(x => !x.IsPlayer).All(x => x.Monets < amount))
                        amount = listOfPlayers.Where(x => !x.IsPlayer).Max(x => x.Monets);

                    listOfPlayers[0].RaiseMoney(amount);
                    break;
                }
                RandomPlayerChoose(cursor, true, amount: amount);
                Console.ReadKey();
            }
            else
            {
                Console.Clear();
                BankShow();

                Console.WriteLine("\n1. Pass\n2. Call\n\n");

                foreach (Player player in listOfPlayers)
                {
                    if (!player.IsPlayer && player.IsPlaying)
                    {
                        Console.Write($"{player.Name}\tMonets: {player.Monets}\tMove: ");
                        cursor.Add(player, Console.GetCursorPosition());
                        Console.WriteLine("\n");
                    }
                }
                Console.Write("Your move: ");
                var cords = Console.GetCursorPosition();


                HandRank handRank = PokerHandEvaluator.CheckHand(listOfPlayers.Where(x => x.LastMove == Move.Raise).First());
                int monets = listOfPlayers.Where(x => x.LastMove == Move.Raise).First().Monets;

                if ((int)handRank >= 0 && (int)handRank <= 3)
                    monets = StrongCall(0.3, monets);
                else if ((int)handRank >= 4 && (int)handRank <= 8)
                    monets = StrongCall(0.6, monets);
                else
                    monets = StrongCall(1, monets);

                Thread.Sleep(2000);

                Player computer = listOfPlayers.Where(x => x.LastMove == Move.Raise).FirstOrDefault();
                Console.SetCursorPosition(cursor[computer].Item1, cursor[computer].Item2);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Raised {monets} monets!");
                Console.SetCursorPosition(cords.Left, cords.Top);
                listOfPlayers.Where(x => x.LastMove == Move.Raise).FirstOrDefault().RaiseMoney(monets);
                Console.ResetColor();

                Task.WhenAll(PlayerCallOrPass(monets), ComputerCallOrPass(cords, cursor, monets)).Wait();
                Console.WriteLine("\n\nClick enter to continue");
                Console.ReadKey();
            }
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
                if (!listOfPlayers[i].IsPlayer && listOfPlayers[i].IsPlaying)
                    activePlayers.Add(listOfPlayers[i], false);
            }

            return activePlayers;
        }
        private static async Task RandomPlayerChoose(Dictionary<Player, (int, int)> cursor, bool raise, int lvl = 0, int amount = 0)
        {
            Dictionary<Player, bool> activePlayers = ActivePlayersToDictionary();
            var cords = Console.GetCursorPosition();


            for (int i = 0; i < activePlayers.Count; i++)
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

                if (firstRaise)
                    break;

                string move;
                if(raise)
                {
                    Move callOrPass = listOfPlayers.Where(x => x.Name == name).First().CallOrPass(amount);
                    move = callOrPass.ToString();

                    if (callOrPass == Move.Call)
                        listOfPlayers.Where(x => x.Name == name).First().RaiseMoney(amount);
                    else
                        listOfPlayers.Where(x => x.Name == name).First().IsPlaying = false;
                }
                else
                    move = listOfPlayers.Where(x => x.Name == name).First().ChooseMoveForComputer(lvl);


                Console.SetCursorPosition(cursor[player].Item1, cursor[player].Item2);
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write($"{move}");
                Console.ResetColor();

                if (Enum.Parse<Move>(move) == Move.Raise)
                {
                    firstRaise = true;
                    cts.Cancel();
                    break;
                }

                if (!chooseOption)
                    Console.SetCursorPosition(cords.Item1, cords.Item2);

            }
        }
        private static void StartReadingKeys()
        {
            Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
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
                    if (listOfPlayers[i].LastMove != Move.Raise && listOfPlayers[i].IsPlaying)
                        activePlayers.Add(listOfPlayers[i], false);
                }

                for (int i = 0; i < listOfPlayers.Where(x => !x.IsPlayer && x.IsPlaying && x.LastMove != Move.Raise).Count(); i++)
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
                        listOfPlayers.Where(x => x.Name == name).First().IsPlaying = false;

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
    }
}
