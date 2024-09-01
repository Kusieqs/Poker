using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
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

        public static (List<Card>, List<Card>) ListConverter(Player winner, Player player)
        {
            List<Card> winnerDeck = TexasHoldem.cardsOnTable;
            List<Card> playerDeck = TexasHoldem.cardsOnTable;
            winnerDeck.Add(winner.Deck[0]);
            winnerDeck.Add(winner.Deck[1]);
            playerDeck.Add(player.Deck[0]);
            playerDeck.Add(player.Deck[1]);

            return (winnerDeck, playerDeck);
        } // Creating to list of cards 

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
            var decks = ListConverter(winner, player);

            int winnerRank = decks.Item1
            .GroupBy(x => x.Rank)
            .Where(group => group.Count() == 2)
            .Select(group => (int)group.Key)
            .FirstOrDefault(); 

            int playerRank = decks.Item2
            .GroupBy(x => x.Rank)
            .Where(group => group.Count() == 2)
            .Select(group => (int)group.Key)
            .FirstOrDefault();

            if (winnerRank < playerRank)
                return player;
            else if(winnerRank > playerRank)
                return winner;
            else
            {
                return null;
                // suit
            }
        }

        public static Player TwoPair(Player winner, Player player)
        {
            var decks = ListConverter(winner, player);

            var winnerList = decks.Item1
            .GroupBy(card => card.Rank)
            .Where(group => group.Count() == 2)
            .Take(2)
            .Select(group => (int)group.Key)
            .ToList();

            int winnerRank = winnerList.Max();

            var playerList = decks.Item2
            .GroupBy(card => card.Rank)
            .Where(group => group.Count() == 2)
            .Take(2)
            .Select(group => (int)group.Key)
            .ToList();

            int playerRank = playerList.Max();

            if (winnerRank < playerRank)
                return player;
            else if (winnerRank > playerRank)
                return winner;
            else
            {
                return null;
                // suit
            }
        }

        public static Player ThreeOfAKind(Player winner, Player player)
        {
            var decks = ListConverter(winner, player);

            int winnerRank = decks.Item1
            .GroupBy(card => card.Rank)
            .Where(group => group.Count() == 3)
            .Select(group => (int)group.Key)
            .FirstOrDefault();

            int playerRank = decks.Item2
            .GroupBy(card => card.Rank)
            .Where(group => group.Count() == 3)
            .Select(group => (int)group.Key)
            .FirstOrDefault();

            if (winnerRank < playerRank)
                return player;
            else if (winnerRank > playerRank)
                return winner;
            else
            {
                return null;
                // suit
            }
        }

        public static Player Straight(Player winner, Player player)
        {
            var decks = ListConverter(winner, player);
            decks.Item1 = decks.Item1.OrderBy(x => x.Rank).ToList();
            decks.Item2 = decks.Item2.OrderBy(x => x.Rank).ToList();
            // Pomyslenie

            return null;
        }

        public static Player Flush(Player winner, Player player)
        {
            var decks = ListConverter(winner, player);

            var winnerList = decks.Item1
            .GroupBy(x => x.Suit)
            .Where(x => x.Count() >= 5)
            .SelectMany(x => x.OrderByDescending(x => x.Rank).Take(5))
            .Select(x => x.Rank)
            .ToList();

            int winnerRank = (int)winnerList.Max();

            var playerList = decks.Item2
            .GroupBy(x => x.Suit)
            .Where(x => x.Count() >= 5)
            .SelectMany(x => x.OrderByDescending(x => x.Rank).Take(5))
            .Select(x => x.Rank)
            .ToList();

            int playerRank = (int)playerList.Max();

            if (winnerRank < playerRank)
                return player;
            else if (winnerRank > playerRank)
                return winner;
            else
            {
                return null;
                // suit
            }
        }

        public static Player FullHouse(Player winner, Player player)
        {
            return null;
        }

        public static Player FourOfAKind(Player winner, Player player)
        {
            var decks = ListConverter(winner, player);
            int winnerRank = decks.Item1
            .GroupBy(card => card.Rank)
            .Where(group => group.Count() == 4)
            .Select(group => (int)group.Key)
            .FirstOrDefault();

            int playerRank = decks.Item2
            .GroupBy(card => card.Rank)
            .Where(group => group.Count() == 4)
            .Select(group => (int)group.Key)
            .FirstOrDefault();

            if (winnerRank < playerRank)
                return player;
            else if (winnerRank > playerRank)
                return winner;
            else
            {
                return null;
                // suit
            }
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
