using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    public class SetWinner
    {
        public static string ChooseWinner()
        {
            Player winner = null;

            for (int i = 0; i < TexasHoldem.listOfPlayers.Count; i++)
            {
                if (TexasHoldem.listOfPlayers[i].LastMove == Move.Pass)
                    continue;

                if (i == 0)
                {
                    winner = TexasHoldem.listOfPlayers[i];
                    continue;
                }

                if ((int)winner.Hand == (int)TexasHoldem.listOfPlayers[i].Hand)
                {
                    switch(winner.Hand)
                    {
                        case HandRank.HighCard:
                            winner = HighCard(winner, TexasHoldem.listOfPlayers[i]);
                            break;
                        case HandRank.OnePair:
                            break;
                        case HandRank.TwoPair:
                            break;
                        case HandRank.ThreeOfAKind:
                            break;
                        case HandRank.Straight:
                            break;
                        case HandRank.Flush:
                            break;
                        case HandRank.FullHouse:
                            break;
                        case HandRank.FourOfAKind:
                            break;
                        case HandRank.StraightFlush:
                            break;
                        case HandRank.RoyalFlush: 
                            break;
                    }

                    int winnerHand = (int)winner.Deck[0].Rank + (int)winner.Deck[1].Rank;
                    int otherHand = (int)TexasHoldem.listOfPlayers[i].Deck[0].Rank + (int)TexasHoldem.listOfPlayers[i].Deck[1].Rank;

                    if (winnerHand < otherHand)
                        winner = TexasHoldem.listOfPlayers[i];
                }
                else if ((int)winner.Hand < (int)TexasHoldem.listOfPlayers[i].Hand)
                    winner = TexasHoldem.listOfPlayers[i];
            }

            TexasHoldem.listOfPlayers.Where(x => x.Name != winner.Name).Select(x => x.LastMove = Move.Pass);

            return winner.Name;
        }

        public static Player HighCard(Player winner, Player player)
        {
            int winnerDeck = (int)winner.Deck.Max(x => x.Rank);
            int playerDeck = (int)player.Deck.Max(x => x.Rank);

            if (winnerDeck < playerDeck)
                return player;
            else if (winnerDeck > playerDeck)
                return winner;
            else
            {
                return null;
                // metoda na suit
            }
        }

        public static Player OnePair(Player winner, Player player)
        {
            // ogarniecie ktora karta ma pare
            return null;
        }

        public static Player TwoPair(Player winner, Player player)
        {
            // najwyzsza karta? 
            return null;
        }

        public static Player ThreeOfAKind(Player winner, Player player)
        {
            // ogarniecie ktora karta ma 3 karty
            return null;
        }

        public static Player Straight(Player winner, Player player)
        {
            return null;
        }

        public static Player Flush(Player winner, Player player)
        {
            return null;
        }

        public static Player FullHouse(Player winner, Player player)
        {
            return null;
        }

        public static Player FourOfAKind(Player winner, Player player)
        {
            return null;
        }

        public static Player StraightFlush(Player winner, Player player)
        {
            return null;
        }

        public static Player RoyalFlush(Player winner, Player player)
        {
            return null;
        }
    }
}
