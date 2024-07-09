using System;
using System.Collections.Generic;
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
                Player.CratingDeck();
                Player.Shuffle();
                // Exit game
                if (!IsPlaying)
                    return;


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
                    choose = int.Parse(Console.ReadLine());
                    if (choose == 1)
                    {
                        // Checking our player that he has got 50 monets in pocket
                        if (listOfPlayers.Where(x => x.IsPlayer == true).First().Monets - 50 < 0)
                            throw new FormatException("You dont have enough monets to play!");

                        // Taking money from pocket to bank
                        foreach(Player player in listOfPlayers)
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


            }while(true);

        } // Starting round for player

    }
}
