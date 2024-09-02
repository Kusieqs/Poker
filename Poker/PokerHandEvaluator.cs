using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    public class PokerHandEvaluator
    {
        public static HandRank CheckHand(Player player)
        {

            Card[] checkTable = TexasHoldem.cardsOnTable.ToArray();

            if (IsRoyalFlush(player.Deck, checkTable)) return HandRank.RoyalFlush;
            if (IsStraightFlush(player.Deck, checkTable)) return HandRank.StraightFlush;
            if (IsFourOfAKind(player.Deck, checkTable)) return HandRank.FourOfAKind;
            if (IsFullHouse(player.Deck, checkTable)) return HandRank.FullHouse;
            if (IsFlush(player.Deck, checkTable)) return HandRank.Flush;
            if (IsStraight(player.Deck, checkTable)) return HandRank.Straight;
            if (IsThreeOfAKind(player.Deck, checkTable)) return HandRank.ThreeOfAKind;
            if (IsTwoPair(player.Deck, checkTable)) return HandRank.TwoPair;
            if (IsOnePair(player.Deck, checkTable)) return HandRank.OnePair;
            return HandRank.HighCard;
        } // Checking rate of cards
        private static bool IsRoyalFlush(Card[] hand, Card[] checkTable)
        {
            return IsStraightFlush(hand, checkTable) && hand.Min(card => card.Rank) == Rank.Ten;
        }
        private static bool IsStraightFlush(Card[] hand, Card[] checkTable)
        {
            return IsFlush(hand, checkTable) && IsStraight(hand, checkTable);
        }
        private static bool IsFourOfAKind(Card[] hand, Card[] checkTable)
        {
            return hand.GroupBy(card => card.Rank).Any(group => group.Count() == 4);
        }
        private static bool IsFullHouse(Card[] hand, Card[] checkTable)
        {
            var rankGroups = hand.GroupBy(card => card.Rank).ToList();
            return rankGroups.Count == 2 && rankGroups.Any(group => group.Count() == 3);
        }
        private static bool IsFlush(Card[] hand, Card[] checkTable)
        {
            return hand.GroupBy(card => card.Suit).Count() == 1;
        }
        private static bool IsStraight(Card[] hand, Card[] checkTable)
        {
            var orderedRanks = hand.Select(card => (int)card.Rank).OrderBy(rank => rank).ToList();
            for (int i = 1; i < orderedRanks.Count; i++)
            {
                if (orderedRanks[i] != orderedRanks[i - 1] + 1)
                    return false;
            }
            return true;
        }
        private static bool IsThreeOfAKind(Card[] hand, Card[] table)
        {

            if (hand[0].Rank == hand[1].Rank && table.Any(x => x.Rank == hand[0].Rank))
                return true;
            else
            {
                int threeCards = 1;

                foreach (var card in hand)
                { 
                    foreach (var tableRank in table)
                    {
                        if (card.Rank == tableRank.Rank)
                            threeCards++;

                        if (threeCards == 3)
                            return true;
                    }
                    threeCards = 1;
                }
            }

            return false;
        }
        private static bool IsTwoPair(Card[] hand, Card[] table)
        {
            int correctTwoPairs = 0;

            foreach (var card in hand)
            {
                foreach (var tableRank in table)
                {
                    if (card.Rank == tableRank.Rank)
                    {
                        correctTwoPairs++;
                        break;
                    }

                    if (correctTwoPairs == 2)
                        return true;
                }
            }
            return false;
        }
        private static bool IsOnePair(Card[] hand, Card[] table)
        {
            foreach(var card in hand)
            {
                foreach(var tableRank in table)
                {
                    if(card.Rank == tableRank.Rank)
                        return true;
                }
            }

            if (hand[0].Rank == hand[1].Rank)
                return true;

            return false;
        }
    }
}
