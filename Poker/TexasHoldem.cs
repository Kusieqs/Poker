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
        public static void Game(int players, int monets)
        {
            // adding new players to game
            for(int i = 0; i < players; i++)
            {
                Player player = new Player(monets, i == 0 ? true : false ,2);
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


                /*  Metoda ktora bedzie nam sprawdzac czy komputer chce grac dalej czy nie czy chce podbic czy czekac
                 *  Metoda do podbicia/ czekania/ spasowania -> rozegranie gry przez komputer, Rozdanie 3 kart poczatkowych
                 */
                string cardsOnDeck = "";
                Card card;
                Console.Clear();

                // Showing your deck
                Console.WriteLine("Your Deck\n");
                listOfPlayers.Where(x=> x.IsPlayer).First().ShowDeck();

                // Croupier deals 3 cards on the table
                Console.WriteLine("\n\n\nCards putted by croupier\n");
                for(int i = 0; i < 3;  i++)
                {
                    Thread.Sleep(400);
                    card = Player.gameDeck.Pop();
                    cardsOnDeck += "\n" + card.DrawCard();
                    cardsOnTable.Add(card);
                    Console.WriteLine(cardsOnDeck);
                }

                /*  Metoda ktora bedzie nam sprawdzac czy komputer chce grac dalej czy nie czy chce podbic czy czekac
                *  Metoda do podbicia/ czekania/ spasowania -> rozegranie gry przez komputer, Rozdanie 3 kart poczatkowych
                */

                // Croupier deals 1 card on the table
                for (int i = 0; i < 2; i ++)
                {
                    Console.Clear();
                    Console.WriteLine("Your Deck\n");
                    listOfPlayers.Where(x => x.IsPlayer).First().ShowDeck();

                    Console.WriteLine("\n\n\nCards putted by croupier\n");
                    Thread.Sleep(400);
                    card = Player.gameDeck.Pop();
                    cardsOnDeck += "\n" + card.DrawCard();
                    cardsOnTable.Add(card);
                    Console.Write(cardsOnDeck);

                    /*  Metoda ktora bedzie nam sprawdzac czy komputer chce grac dalej czy nie czy chce podbic czy czekac
                    *  Metoda do podbicia/ czekania/ spasowania -> rozegranie gry przez komputer, Rozdanie 3 kart poczatkowych
                    */
                }

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

                
                if(player.IsPlayer)
                {
                    Console.WriteLine("Your deck\n");
                    for(int j = 0; j < player.Deck.Length; j++)
                    {
                        Thread.Sleep(1300);
                        Console.WriteLine(player.Deck[j].DrawCard());
                    }
                    Console.WriteLine("\n\n");
                }
                else
                {
                    Console.Write($"Player {i}");
                    Thread.Sleep(1300);
                    Console.WriteLine(" is ready!");
                    ++i;
                }
            }
            Console.WriteLine("\n\nClick enter to continue");
            Console.ReadKey();
        } // Dealing 2 cards for each player

    }
}
