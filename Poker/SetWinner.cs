using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
                // Continue if someone pass
                if (TexasHoldem.listOfPlayers[i].LastMove == Move.Pass)
                    continue;

                if (winner == null)
                {
                    winner = TexasHoldem.listOfPlayers[i];
                    continue;
                }

                // setting hand for player
                if ((int)winner.Hand == (int)TexasHoldem.listOfPlayers[i].Hand)
                {
                    switch(winner.Hand)
                    {
                        case HandRank.HighCard:
                            winner = HighCard(winner, TexasHoldem.listOfPlayers[i]);
                            break;
                        case HandRank.OnePair:
                            winner = OnePair(winner, TexasHoldem.listOfPlayers[i]);
                            break;
                        case HandRank.TwoPair:
                            winner = TwoPair(winner, TexasHoldem.listOfPlayers[i]);
                            break;
                        case HandRank.ThreeOfAKind:
                            winner = ThreeOfAKind(winner, TexasHoldem.listOfPlayers[i]);
                            break;
                        case HandRank.Straight:
                            winner = Straight(winner, TexasHoldem.listOfPlayers[i]);
                            break;
                        case HandRank.Flush:
                            winner = Flush(winner, TexasHoldem.listOfPlayers[i]);
                            break;
                        case HandRank.FullHouse:
                            winner = FullHouse(winner, TexasHoldem.listOfPlayers[i]);
                            break;
                        case HandRank.FourOfAKind:
                            winner = FourOfAKind(winner, TexasHoldem.listOfPlayers[i]);
                            break;
                        case HandRank.StraightFlush:
                            winner = StraightFlush(winner, TexasHoldem.listOfPlayers[i]);
                            break;
                        case HandRank.RoyalFlush: 
                            // Lack of probability
                            break;
                    }
                }
                else if ((int)winner.Hand < (int)TexasHoldem.listOfPlayers[i].Hand)
                    winner = TexasHoldem.listOfPlayers[i];
            }

            // Adding monets to the winner
            TexasHoldem.listOfPlayers.Where(x => x.Name == winner.Name).First().Monets += TexasHoldem.bank;
            return winner.Name;
        } // Choosing winner by hand
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
                winnerDeck = (int)winner.Deck.Min(x => x.Rank);
                playerDeck = (int)player.Deck.Min(x => x.Rank);

                if(winnerDeck < playerDeck)
                    return winner;
                else if(winnerDeck > playerDeck)
                    return winner;
                else
                {
                    return Suit(winner, player);
                }
            }
        } // High Card Chooser
        public static Player OnePair(Player winner, Player player)
        {

            int winnerRank = 0;
            int playerRank = 0;

            if (winner.Deck[0].Rank == winner.Deck[1].Rank)
                winnerRank = (int)winner.Deck[0].Rank;
            else
            {
                foreach (var card in winner.Deck)
                {
                    foreach (var tableRank in TexasHoldem.cardsOnTable)
                    {
                        if (card.Rank == tableRank.Rank)
                            winnerRank = (int)tableRank.Rank;
                    }
                }
            }

            if (player.Deck[0].Rank == player.Deck[1].Rank)
                playerRank = (int)player.Deck[0].Rank;
            else
            {
                foreach (var card in player.Deck)
                {
                    foreach (var tableRank in TexasHoldem.cardsOnTable)
                    {
                        if (card.Rank == tableRank.Rank)
                            playerRank = (int)tableRank.Rank;
                    }
                }
            }


            if (winnerRank < playerRank)
                return player;
            else if(winnerRank > playerRank)
                return winner;
            else
            {

                if (winner.Deck[0].Rank == winner.Deck[1].Rank)
                    winnerRank = (int)winner.Deck[0].Rank;
                else if ((int)winner.Deck[0].Rank == winnerRank)
                    winnerRank = (int)winner.Deck[1].Rank;
                else
                    winnerRank = (int)winner.Deck[0].Rank;

                if (player.Deck[0].Rank == player.Deck[1].Rank)
                    playerRank = (int)player.Deck[0].Rank;
                else if ((int)player.Deck[0].Rank == playerRank)
                    playerRank = (int)player.Deck[1].Rank;
                else
                    playerRank = (int)player.Deck[0].Rank;

                if (winnerRank < playerRank)
                    return player;
                else if (winnerRank > playerRank)
                    return winner;
                else
                {
                    return Suit(winner, player);
                }
            }
        } // One pair Chooser
        public static Player TwoPair(Player winner, Player player)
        {
            int winnerRank = (int)winner.Deck.Max(x => x.Rank);
            int playerRank = (int)player.Deck.Max(x => x.Rank);

            if (winnerRank < playerRank)
                return player;
            else if (winnerRank > playerRank)
                return winner;
            else
            {
                winnerRank = (int)winner.Deck.Min(x => x.Rank);
                playerRank = (int)player.Deck.Min(x => x.Rank);

                if (winnerRank < playerRank)
                    return player;
                else if (winnerRank > playerRank)
                    return winner;
                else
                {
                    return Suit(winner,player);
                }
            }
        } // Two pair Chooser
        public static Player ThreeOfAKind(Player winner, Player player)
        {
            int winnerRank = 0;
            int playerRank = 0;

            if (winner.Deck[0].Rank == winner.Deck[1].Rank && TexasHoldem.cardsOnTable.Any(x => x.Rank == winner.Deck[0].Rank))
                winnerRank = (int)winner.Deck[0].Rank;
            else
            {
                int threeCards = 1;
                foreach (var card in winner.Deck)
                {
                    foreach (var tableRank in TexasHoldem.cardsOnTable)
                    {
                        if (card.Rank == tableRank.Rank)
                            threeCards++;

                        if (threeCards == 3)
                            winnerRank = (int)tableRank.Rank;
                    }
                    threeCards = 1;
                }
            }

            if (player.Deck[0].Rank == player.Deck[1].Rank && TexasHoldem.cardsOnTable.Any(x => x.Rank == player.Deck[0].Rank))
                playerRank = (int)player.Deck[0].Rank;
            else
            {
                int threeCards = 1;
                foreach (var card in player.Deck)
                {
                    foreach (var tableRank in TexasHoldem.cardsOnTable)
                    {
                        if (card.Rank == tableRank.Rank)
                            threeCards++;

                        if (threeCards == 3)
                            playerRank = (int)tableRank.Rank;
                    }
                    threeCards = 1;
                }
            }


            if (winnerRank < playerRank)
                return player;
            else if (winnerRank > playerRank)
                return winner;
            else
            {
                if (winner.Deck[0].Rank == winner.Deck[1].Rank)
                    winnerRank = (int)winner.Deck[0].Rank;
                else if ((int)winner.Deck[0].Rank == winnerRank)
                    winnerRank = (int)winner.Deck[1].Rank;
                else
                    winnerRank = (int)winner.Deck[0].Rank;

                if (player.Deck[0].Rank == player.Deck[1].Rank)
                    playerRank = (int)player.Deck[0].Rank;
                else if ((int)player.Deck[0].Rank == playerRank)
                    playerRank = (int)player.Deck[1].Rank;
                else
                    playerRank = (int)player.Deck[0].Rank;

                if (winnerRank < playerRank)
                    return player;
                else if (winnerRank > playerRank)
                    return winner;
                else
                {
                    return Suit(winner, player);
                }
            }
        } // Three of a kind chooser
        public static Player Straight(Player winner, Player player)
        {
            List<(string, Card)> listWinner = new List<(string, Card)>();
            List<(string, Card)> listPlayer = new List<(string, Card)>();

            foreach (var card in winner.Deck)
            {
                listWinner.Add(("Person", card));
            }
            foreach (var card in player.Deck)
            {
                listPlayer.Add(("Person", card));
            }
            foreach (var tablecard in TexasHoldem.cardsOnTable)
            {
                listPlayer.Add(("Table", tablecard));
                listWinner.Add(("Table", tablecard));
            }


            listWinner.OrderBy(x => x.Item2.Rank).Distinct();
            listPlayer.OrderBy(x => x.Item2.Rank).Distinct();

            int winnerRank = 0;
            List<(string, Card)> copyListWinner = new List<(string, Card)>();
            foreach (var card in listWinner)
            {
                if ((int)card.Item2.Rank + 1 != winnerRank)
                    copyListWinner.Clear();

                copyListWinner.Add(card);
                winnerRank = (int)card.Item2.Rank;
            }

            int playerRank = 0;
            List<(string, Card)> copyListPlayer = new List<(string, Card)>();
            foreach (var card in listWinner)
            {
                if ((int)card.Item2.Rank + 1 != playerRank)
                    copyListWinner.Clear();

                copyListWinner.Add(card);
                playerRank = (int)card.Item2.Rank;
            }

            if (winnerRank < playerRank)
                return player;
            else if (winnerRank > playerRank)
                return winner;
            else
            {
                winnerRank = (int)winner.Deck.Max(x => x.Rank);
                playerRank = (int)player.Deck.Max(x => x.Rank);

                if (winnerRank < playerRank)
                    return player;
                else if (winnerRank > playerRank)
                    return winner;
                else
                {
                    winnerRank = (int)winner.Deck.Min(x => x.Rank);
                    playerRank = (int)player.Deck.Min(x => x.Rank);

                    if (winnerRank < playerRank)
                        return player;
                    else if (winnerRank > playerRank)
                        return winner;
                    else
                    {
                        return Suit(winner, player);
                    }
                }
            }
        } // Straight chooser
        public static Player Flush(Player winner, Player player)
        {
            Dictionary<Suit, int> winnerKeyValuePairs = new Dictionary<Suit, int>();
            Dictionary<Suit, int> playerKeyValuePairs = new Dictionary<Suit, int>();

            foreach (var card in winner.Deck)
            {
                if (!winnerKeyValuePairs.ContainsKey(card.Suit))
                    winnerKeyValuePairs.Add(card.Suit, 1);
                else
                    winnerKeyValuePairs[card.Suit] += 1;
            }

            foreach (var card in player.Deck)
            {
                if (!playerKeyValuePairs.ContainsKey(card.Suit))
                    playerKeyValuePairs.Add(card.Suit, 1);
                else
                    playerKeyValuePairs[card.Suit] += 1;
            }


            foreach (var tablecard in TexasHoldem.cardsOnTable)
            {
                if (winnerKeyValuePairs.ContainsKey(tablecard.Suit))
                    winnerKeyValuePairs[tablecard.Suit] += 1;

                if (playerKeyValuePairs.ContainsKey(tablecard.Suit))
                    playerKeyValuePairs[tablecard.Suit] += 1;
            }

            int winnerRank = winnerKeyValuePairs.Max(x => x.Value);
            int playerRank = playerKeyValuePairs.Max(x => x.Value);

            if (winnerRank < playerRank)
                return player;
            else if (winnerRank > playerRank)
                return winner;
            else
            {
                winnerRank = (int)winner.Deck.Max(x => x.Rank);
                playerRank = (int)player.Deck.Max(x => x.Rank);

                if (winnerRank < playerRank)
                    return player;
                else if (winnerRank > playerRank)
                    return winner;
                else
                {
                    winnerRank = (int)winner.Deck.Min(x => x.Rank);
                    playerRank = (int)player.Deck.Min(x => x.Rank);

                    if (winnerRank < playerRank)
                        return player;
                    else if (winnerRank > playerRank)
                        return winner;
                    else
                    {
                        return Suit(winner, player);
                    }
                }
            }
        } // Flush chooser
        public static Player FullHouse(Player winner, Player player)
        {
            var threeOfKindWinner = PokerHandEvaluator.IsThreeOfAKind(winner.Deck, TexasHoldem.cardsOnTable.ToArray());
            var threeOfKindPlayer = PokerHandEvaluator.IsThreeOfAKind(player.Deck, TexasHoldem.cardsOnTable.ToArray());

            if (threeOfKindPlayer.Item2 < threeOfKindWinner.Item2)
                return winner;
            else if (threeOfKindPlayer.Item2 > threeOfKindWinner.Item2)
                return player;
            else
            {
                var onePairWinner = PokerHandEvaluator.IsOnePair(winner.Deck, TexasHoldem.cardsOnTable.ToArray());
                var onePairPlayer = PokerHandEvaluator.IsOnePair(player.Deck, TexasHoldem.cardsOnTable.ToArray());

                if (onePairPlayer.Item2 < onePairWinner.Item2)
                    return winner;
                else if (onePairPlayer.Item2 > onePairWinner.Item2)
                    return player;
                else
                    return Suit(winner, player);
            }
        } // Full house chooser
        public static Player FourOfAKind(Player winner, Player player)
        {
            int winnerRank = 0;
            int playerRank = 0;

            if (winner.Deck[0].Rank == winner.Deck[1].Rank && TexasHoldem.cardsOnTable.Where(x => x.Rank == winner.Deck[0].Rank).Count() == 2)
                winnerRank = (int)winner.Deck[0].Rank;
            else
            {
                int fourCards = 1;

                foreach (var card in winner.Deck)
                {
                    foreach (var tableRank in TexasHoldem.cardsOnTable)
                    {
                        if (card.Rank == tableRank.Rank)
                            fourCards++;

                        if (fourCards == 4)
                            winnerRank = (int)tableRank.Rank;
                    }
                    fourCards = 1;
                }
            }

            if (player.Deck[0].Rank == player.Deck[1].Rank && TexasHoldem.cardsOnTable.Where(x => x.Rank == player.Deck[0].Rank).Count() == 2)
                playerRank = (int)player.Deck[0].Rank;
            else
            {
                int fourCards = 1;

                foreach (var card in winner.Deck)
                {
                    foreach (var tableRank in TexasHoldem.cardsOnTable)
                    {
                        if (card.Rank == tableRank.Rank)
                            fourCards++;

                        if (fourCards == 4)
                            playerRank = (int)tableRank.Rank;
                    }
                    fourCards = 1;
                }
            }

            if (winnerRank < playerRank)
                return player;
            else if (winnerRank > playerRank)
                return winner;
            else
            {
                if ((int)winner.Deck[0].Rank == winnerRank)
                    winnerRank = (int)winner.Deck[1].Rank;
                else
                    winnerRank = (int)winner.Deck[0].Rank;

                if ((int)player.Deck[0].Rank == playerRank)
                    playerRank = (int)player.Deck[1].Rank;
                else
                    playerRank = (int)player.Deck[0].Rank;

                if (winnerRank < playerRank)
                    return player;
                else if (winnerRank > playerRank)
                    return winner;
                else
                {
                    return Suit(winner, player);
                }
            }
        } // Four of a kind chooser
        public static Player StraightFlush(Player winner, Player player)
        {
            return Straight(winner, player);
        } // Straight flush chooser
        public static Player Suit(Player winner, Player player)
        {
            Card winnerRank = winner.Deck.MaxBy(x => x.Rank);
            Card playerRank = player.Deck.MaxBy(x => x.Rank);

            return (int)winnerRank.Suit > (int)playerRank.Suit ? winner : player;
        } // Suit chooser
    }
}
