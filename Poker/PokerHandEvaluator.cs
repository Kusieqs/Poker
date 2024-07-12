using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    public class PokerHandEvaluator
    {
        public static HandRank CheckHand(Card[] deck, List<Card> cardsOnTable)
        {
            int leng = deck.Length + cardsOnTable.Count;
            Card[] hand = new Card[leng];

            // Adding to  new Tab
            for(int i = 0; i < leng; i++)
            {
                if (i == 0 || i == 1)
                    hand[i] = deck[i];
                else
                    hand[i] = cardsOnTable[i-2];
            }

            if (IsRoyalFlush(hand)) return HandRank.RoyalFlush;
            if (IsStraightFlush(hand)) return HandRank.StraightFlush;
            if (IsFourOfAKind(hand)) return HandRank.FourOfAKind;
            if (IsFullHouse(hand)) return HandRank.FullHouse;
            if (IsFlush(hand)) return HandRank.Flush;
            if (IsStraight(hand)) return HandRank.Straight;
            if (IsThreeOfAKind(hand)) return HandRank.ThreeOfAKind;
            if (IsTwoPair(hand)) return HandRank.TwoPair;
            if (IsOnePair(hand)) return HandRank.OnePair;
            return HandRank.HighCard;
        } // Checking rate of cards
        private static bool IsRoyalFlush(Card[] hand)
        {
            return IsStraightFlush(hand) && hand.Min(card => card.Rank) == Rank.Ten;
        }

        private static bool IsStraightFlush(Card[] hand)
        {
            return IsFlush(hand) && IsStraight(hand);
        }

        private static bool IsFourOfAKind(Card[] hand)
        {
            return hand.GroupBy(card => card.Rank).Any(group => group.Count() == 4);
        }

        private static bool IsFullHouse(Card[] hand)
        {
            var rankGroups = hand.GroupBy(card => card.Rank).ToList();
            return rankGroups.Count == 2 && rankGroups.Any(group => group.Count() == 3);
        }

        private static bool IsFlush(Card[] hand)
        {
            return hand.GroupBy(card => card.Suit).Count() == 1;
        }

        private static bool IsStraight(Card[] hand)
        {
            var orderedRanks = hand.Select(card => (int)card.Rank).OrderBy(rank => rank).ToList();
            for (int i = 1; i < orderedRanks.Count; i++)
            {
                if (orderedRanks[i] != orderedRanks[i - 1] + 1)
                    return false;
            }
            return true;
        }

        private static bool IsThreeOfAKind(Card[] hand)
        {
            return hand.GroupBy(card => card.Rank).Any(group => group.Count() == 3);
        }

        private static bool IsTwoPair(Card[] hand)
        {
            var pairs = hand.GroupBy(card => card.Rank).Where(group => group.Count() == 2).ToList();
            return pairs.Count == 2;
        }

        private static bool IsOnePair(Card[] hand)
        {
            return hand.GroupBy(card => card.Rank).Any(group => group.Count() == 2);
        }
    }
}
