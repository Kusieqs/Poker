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
            int leng = player.Deck.Length + TexasHoldem.cardsOnTable.Count;
            Card[] hand = new Card[leng];

            // Adding to  new Tab
            for(int i = 0; i < leng; i++)
            {
                if (i == 0 || i == 1)
                    hand[i] = player.Deck[i];
                else
                    hand[i] = TexasHoldem.cardsOnTable[i-2];
            }

            Card[] checkTable = TexasHoldem.cardsOnTable.ToArray();

            if (IsRoyalFlush(hand, checkTable)) return HandRank.RoyalFlush;
            if (IsStraightFlush(hand, checkTable)) return HandRank.StraightFlush;
            if (IsFourOfAKind(hand, checkTable)) return HandRank.FourOfAKind;
            if (IsFullHouse(hand, checkTable)) return HandRank.FullHouse;
            if (IsFlush(hand, checkTable)) return HandRank.Flush;
            if (IsStraight(hand, checkTable)) return HandRank.Straight;
            if (IsThreeOfAKind(hand, checkTable)) return HandRank.ThreeOfAKind;
            if (IsTwoPair(hand, checkTable)) return HandRank.TwoPair;
            if (IsOnePair(hand, checkTable)) return HandRank.OnePair;
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
        private static bool IsThreeOfAKind(Card[] hand, Card[] checkTable)
        {
            return hand.GroupBy(card => card.Rank).Any(group => group.Count() == 3);
        }
        private static bool IsTwoPair(Card[] hand, Card[] checkTable)
        {
            var pairs = hand.GroupBy(card => card.Rank).Where(group => group.Count() == 2).ToList();
            return pairs.Count == 2;
        }
        private static bool IsOnePair(Card[] hand, Card[] checkTable)
        {
            return hand.GroupBy(card => card.Rank).Any(group => group.Count() == 2);
        }
    }
}
