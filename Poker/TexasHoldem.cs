using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    public static class TexasHoldem
    {
        public static List<Player> listOfPlayers = new List<Player>(); // list of players
        public static List<Card> cardsOnTable = new List<Card>(); // cards on the table
        public static int bank; // bank amount
        private static (int, int) cords;
        public static void Game(int players, int monets, string name)
        {
            // adding new players to game
            for (int i = 0; i < players + 1; i++)
            {
                Player player = new Player(monets, i == 0 ? true : false, 2, i == 0 ? name : $"Player {i}");
                listOfPlayers.Add(player);
            }

            // Engine of game
            EngineOfGame();

            // END
        }
        public static void EngineOfGame()
        {
            // Game engine
            do
            {
                // Setting that everybody will play
                SettingWhichPlayerWillPlay(true);
                Console.Clear();

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
            int choose = 0;
            do
            {
                Console.Clear();
                try
                {
                    // choosing option
                    Console.WriteLine("1. Play (Raise 30 moents to enabled to play)\n2. Exit");
                    Console.Write("\nNumber: ");
                    choose = int.Parse(Console.ReadLine());
                    if (choose == 1)
                    {
                        // Checking our player that he has got 50 monets in pocket
                        if (listOfPlayers.Where(x => x.IsPlayer == true).First().Monets - 30 < 0)
                            throw new FormatException("You dont have enough monets to play!");

                        // Taking money from pocket to bank
                        foreach (Player player in listOfPlayers)
                        {
                            if (player.Monets - 30 < 0)
                            {
                                listOfPlayers.Remove(player);
                                continue;
                            }
                            player.RaiseMoney(30);
                        }

                        return true;

                    }
                    else if (choose == 2)
                    {
                        // Exit from table
                        bank = 0;
                        return false;
                    }
                    else
                        throw new FormatException("");
                }
                catch (Exception ex)
                {
                    Program.ExceptionString(ex.Message);
                }


            } while (true);

        } // Starting round for player
        public static void DealCards()
        {
            BankShow();
            Console.WriteLine("Croupier deals the cards . . .\n");
            Thread.Sleep(1000);
            int i = 1;

            foreach (Player player in listOfPlayers)
            {
                player.RaiseCard();

                if (player.IsPlayer)
                    player.ShowDeck();
                else
                {
                    Console.Write(player.Name);
                    Thread.Sleep(1300);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" is ready!");
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

            // Menu for player

            string whichOption = option == true ? "Wait" : "Check"; 
            string options = $"\nOptions:\n\n1 - Raise\n2 - {whichOption}\n3 - Pass \n\n";

            BankShow();
            Console.WriteLine();
            // Feature to show deck
            listOfPlayers.Where(x => x.IsPlayer).FirstOrDefault().ShowDeck();
            
            if(cardsOnTable.Count != 0)
            {
                Console.WriteLine("Cards on table:");
                Card.DrawCardOnTable(cardsOnTable);
            }

            Console.WriteLine(options);

            Dictionary<Player, (int, int)> cursor = new Dictionary<Player, (int, int)>();
            foreach(Player player in listOfPlayers)
            {
                if(!player.IsPlayer)
                {
                    Console.Write($"{player.Name}\tMonets: {player.Monets}\tMove: ");
                    cursor.Add(player, (Console.GetCursorPosition()));
                    Console.WriteLine("\n");
                }
            }

            Console.Write("Your move: ");
            var cords = Console.GetCursorPosition();

            // Async methods 
            var userMove = GetUserMoveAsync();
            var computerMove = GetComputerMoveAsync(lvl,cursor);
            Task.WhenAll(userMove, computerMove).Wait();

            // Setting which player choosed pass
            SettingWhichPlayerWillPlay();

            // Method to raise amount of money (if somebody choosed raise)
            if (listOfPlayers.Any(x => x.LastMove == Move.Raise && x.IsPlaying))
                RaiseActivePlayers();

        } // Options for players when cards was putted by croupier
        private static Task GetUserMoveAsync()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    int userDecision;

                    try
                    {

                        // wpisywanie stringa za pomoca charow???? !!!!!!!!!!!!!!!!!!!!!!!!!
                        int.TryParse(Console.ReadLine(), out userDecision);

                        // Excpetion when number is to low or high
                        if (userDecision < 1 || userDecision > 3)
                            throw new FormatException();

                        // Adding Last move for a main player
                        listOfPlayers.Where(x => x.IsPlayer).FirstOrDefault().LastMove = (Move)(userDecision - 1);
                        break;
                    }
                    catch (Exception)
                    {
                        Program.ExceptionString();
                    }
                }
            });
        } // Async method for user
        private static Task GetComputerMoveAsync(int lvl, Dictionary<Player, (int, int)> cursor)
        {
            return Task.Run(async () =>
            {
                Dictionary<Player,bool> activePlayers = new Dictionary<Player,bool>();

                for(int i = 0; i < listOfPlayers.Count; i++)
                {
                    if (!listOfPlayers[i].IsPlayer && listOfPlayers[i].IsPlaying)
                        activePlayers.Add(listOfPlayers[i], false);
                }

                Random random = new Random();
                Player player = null;
                for (int i = 0; i < activePlayers.Count; i++)
                {
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

                    string move = listOfPlayers.Where(x => x.Name == name).FirstOrDefault().ChooseMoveForComputer(lvl);
                    Console.SetCursorPosition(cursor[player].Item1, cursor[player].Item2);
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write($"{move}");
                    Console.ResetColor();
                }
            });
        } // Async method for computer
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
        private static void RaiseActivePlayers()
        {
            for(int i = 0; i < listOfPlayers.Where(x => x.IsPlaying).Count(); i++)
            {
                // CZY KAZDY PO KOLEI CZY NIE? / SPRAWDZANIE CZY MA MONETY / CALL / ALL IN / RAISE
            }
        } 
        private static void BankShow()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Bank: {bank}");
            Console.ResetColor();
        }

    }
}
