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
        public static List<Card> cardsOnTable = new List<Card>();
        public static void Game(int players, int monets, string name)
        {
            // adding new players to game
            for (int i = 0; i < players; i++)
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
                // bank amount
                int bank = 0;
                Console.Clear();

                // Checking whether our player is enable to play
                bool IsPlaying = StartRoundMenu(ref bank);

                // Exit game
                if (!IsPlaying)
                    return;

                // Creating deck and shuffle
                Player.CreatingDeck();
                Player.Shuffle();
                Console.Clear();

                //Dealing cards to decks
                DealCards();


                OptionsInGame(true);

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
                    OptionsInGame(true);
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

                OptionsInGame(false);
                Console.Clear();
                HandRank handRank = PokerHandEvaluator.CheckHand(listOfPlayers.Where(x => x.IsPlayer).First().Deck, cardsOnTable);

                listOfPlayers.Where(x => x.IsPlayer).FirstOrDefault().ShowDeck();
                Console.WriteLine(cardsOnDeck);
                Console.WriteLine(handRank.ToString());
                Console.ReadKey();

            } while (true);
        } // Engine of the game
        public static bool StartRoundMenu(ref int bank)
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
                            player.RaiseMoney(ref bank, 50);
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

        private static void OptionsInGame(bool x)
        {
            Console.Clear();

            // Menu for player
            string whichOption = x == true ? "Wait" : "Check"; 
            string options = $"\nOptions: 1 - Raise, 2 - {whichOption}, 3 - Pass \n\n";

            // Feature to show  deck
            listOfPlayers.Where(x => x.IsPlayer).FirstOrDefault().ShowDeck();
            Console.WriteLine(options);

            // Async methods 
            var userMove = GetUserMoveAsync();
            var computerMove = GetComputerMoveAsync();
            Task.WhenAll(userMove, computerMove).Wait();

            // Take action of move
        }

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

                        if (userDecision < 1 || userDecision > 3)
                            throw new FormatException();

                        Move userMove = (Move)(userDecision - 1);
                        listOfPlayers.Where(x => x.IsPlayer).FirstOrDefault().LastMove = userMove;
                    }
                    catch (Exception)
                    {
                        Program.ExceptionString();
                    }
                }
            });
        }
        private static Task GetComputerMoveAsync()
        {
            return Task.Run(async () =>
            {
                Random random = new Random();
                bool[] playerCorrect = new bool[listOfPlayers.Count - 1];

                for(int i = 0; i < playerCorrect.Length; i++) 
                {
                    playerCorrect[i] = false;
                }

                int whichPlayer = 0;
                for (int i = 0; i < listOfPlayers.Count - 1; i++)
                {
                    while(true)
                    {
                        whichPlayer = random.Next(1, listOfPlayers.Count);
                        if (playerCorrect[whichPlayer - 1] == true)
                            continue;
                        else
                        {
                            playerCorrect[whichPlayer-1] = true;
                            break;
                        }
                    }
                    await Task.Delay(random.Next(1000, 5000));

                    string move = listOfPlayers[whichPlayer].ChooseMoveForComputer();
                    Console.WriteLine($"{listOfPlayers[whichPlayer].Name} {move}");
                }
            });
        }



    }
}
