using System;
using System.Collections.Generic;
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
        public static void StartGame(int players, int monets)
        {
            // adding new players to game
            for(int i = 0; i < players; i++)
            {
                Player player = new Player(monets, i == 0 ? true : false ,2);
                listOfPlayers.Add(player);
            }

            // Shuffle deck
            Player.Shuffle();
        }

    }
}
