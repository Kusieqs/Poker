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
                    return;

                // Creating deck and shuffle
                Player.CreatingDeck();
                Player.Shuffle();
                Console.Clear();

                // Dealing cards to decks
                DealCards();

                // Take move from players
                OptionsInGame(true, 0);


                //// //// //// //// //// //// //// //// //// //// #########################

                string cardsOnDeck = "";
                Card card;
                Console.Clear();

                // Showing your deck
                Console.WriteLine("Your Deck\n");
                listOfPlayers.Where(x => x.IsPlayer).First().ShowDeck();

                // Croupier deals 3 cards on the table
                Console.WriteLine("\n\n\nCards putted by croupier\n");
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(400);
                    card = Player.gameDeck.Pop();
                    cardsOnDeck += "\n" + card.DrawCard();
                    cardsOnTable.Add(card);
                    Console.WriteLine(cardsOnDeck);
                }


                // Croupier deals 1 card on the table
                for (int i = 0; i < 2; i++)
                {
                    OptionsInGame(true, i+1);
                    Console.Clear();
                    Console.WriteLine("Your Deck\n");
                    listOfPlayers.Where(x => x.IsPlayer).First().ShowDeck();

                    Console.WriteLine("\n\n\nCards putted by croupier\n");
                    Thread.Sleep(400);
                    card = Player.gameDeck.Pop();
                    cardsOnDeck += "\n" + card.DrawCard();
                    cardsOnTable.Add(card);
                    Console.Write(cardsOnDeck);

                }

                OptionsInGame(false, 3);
                Console.Clear();
                HandRank handRank = PokerHandEvaluator.CheckHand(listOfPlayers.Where(x => x.IsPlayer).FirstOrDefault());

                listOfPlayers.Where(x => x.IsPlayer).FirstOrDefault().ShowDeck();
                Console.WriteLine(cardsOnDeck);
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
                    Console.WriteLine("1. Raise 50 monets\n2. Pass");
                    Console.Write("\nNumber: ");
                    choose = int.Parse(Console.ReadLine());
                    if (choose == 1)
                    {
                        // Checking our player that he has got 50 monets in pocket
                        if (listOfPlayers.Where(x => x.IsPlayer == true).First().Monets - 50 < 0)
                            throw new FormatException("You dont have enough monets to play!");

                        // Taking money from pocket to bank
                        foreach (Player player in listOfPlayers)
                        {
                            if (player.Monets - 50 < 0)
                            {
                                listOfPlayers.Remove(player);
                                continue;
                            }
                            player.RaiseMoney(50);
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
            Console.WriteLine("Croupier deals the cards . . .\n\n");
            Thread.Sleep(1000);
            int i = 1;

            foreach (Player player in listOfPlayers)
            {
                player.RaiseCard();


                if (player.IsPlayer)
                {
                    Console.WriteLine("Your deck\n");
                    for (int j = 0; j < player.Deck.Length; j++)
                    {
                        Thread.Sleep(1300);
                        Console.WriteLine(player.Deck[j].DrawCard());
                    }
                    Console.WriteLine("\n\n");
                }
                else
                {
                    Console.Write(player.Name);
                    Thread.Sleep(1300);
                    Console.WriteLine(" is ready!");
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
            string options = $"\nOptions: 1 - Raise, 2 - {whichOption}, 3 - Pass \n\n";

            // Feature to show deck
            listOfPlayers.Where(x => x.IsPlayer).FirstOrDefault().ShowDeck();
            Console.WriteLine(options);

            // Async methods 
            var userMove = GetUserMoveAsync();
            var computerMove = GetComputerMoveAsync(lvl);
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
        private static Task GetComputerMoveAsync(int lvl)
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
                    Console.WriteLine($"{listOfPlayers.Where(x => x.Name == name).FirstOrDefault().Name} {move}");
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

    }
}
