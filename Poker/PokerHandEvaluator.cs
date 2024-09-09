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
            if (IsThreeOfAKind(player.Deck, checkTable).Item1) return HandRank.ThreeOfAKind;
            if (IsTwoPair(player.Deck, checkTable)) return HandRank.TwoPair;
            if (IsOnePair(player.Deck, checkTable).Item1) return HandRank.OnePair;
            return HandRank.HighCard;
        } // Checking rate of cards
        private static bool IsRoyalFlush(Card[] hand, Card[] table)
        {
            return IsStraightFlush(hand, table) && hand.Min(card => card.Rank) == Rank.Ten;
        } 
        private static bool IsStraightFlush(Card[] hand, Card[] table)
        {
            return IsFlush(hand, table) && IsStraight(hand, table);
        } 
        private static bool IsFourOfAKind(Card[] hand, Card[] table)
        {
            if (hand[0].Rank == hand[1].Rank && table.Where(x => x.Rank == hand[0].Rank).Count() == 2)
                return true;
            else
            {
                int fourCards = 1;

                foreach (var card in hand)
                {
                    foreach (var tableRank in table)
                    {
                        if (card.Rank == tableRank.Rank)
                            fourCards++;

                        if (fourCards == 4)
                            return true;
                    }
                    fourCards = 1;
                }
            }

            return false;
        }
        private static bool IsFullHouse(Card[] hand, Card[] table)
        {

            var threeOfKinds = IsThreeOfAKind(hand, table);
            var onePairOfKind = IsOnePair(hand, table);


            if (threeOfKinds.Item1 && table.Where(x => (int)x.Rank != threeOfKinds.Item2).GroupBy(x => x.Rank).Any(x => x.Count() == 2))
                return true;

            if (onePairOfKind.Item1 && table.Where(x => (int)x.Rank != onePairOfKind.Item2).GroupBy(x => x.Rank).Any(x => x.Count() == 3))
                return true;

            if (threeOfKinds.Item1 && onePairOfKind.Item1 && onePairOfKind.Item2 != threeOfKinds.Item2)
                return true;

            return false;
        } 
        private static bool IsFlush(Card[] hand, Card[] table)
        {
            Dictionary<Suit,int> keyValuePairs = new Dictionary<Suit,int>();
            foreach(var card in hand)
            {
                if (!keyValuePairs.ContainsKey(card.Suit))
                    keyValuePairs.Add(card.Suit, 1);
                else
                    keyValuePairs[card.Suit] += 1;
            }

            foreach(var tablecard in table)
            {
                if (keyValuePairs.ContainsKey(tablecard.Suit))
                    keyValuePairs[tablecard.Suit] += 1;
            }

            if (keyValuePairs.Any(x => x.Value >= 5))
                return true;

            return false;
        }
        private static bool IsStraight(Card[] hand, Card[] table)
        {
            List<(string, Card)> list = new List<(string, Card)> ();

            foreach(var card in hand)
            {
                list.Add(("Person", card));
            }

            foreach(var tablecard in table)
            {
                list.Add(("Table", tablecard));
            }

            list.OrderBy(x => x.Item2.Rank);
            list.Distinct();

            int rankOfLastCard = 0;
            List<(string, Card)> copyList = new List<(string, Card)>();
            foreach(var card in list)
            {
                if ((int)card.Item2.Rank + 1 != rankOfLastCard)
                    copyList.Clear();

                copyList.Add(card);
                rankOfLastCard = (int)card.Item2.Rank;
            }

            if (copyList.Count >= 5 && copyList.Any(x => x.Item1 == "Person"))
                return true;

            return false;
        } 
        public static (bool, int) IsThreeOfAKind(Card[] hand, Card[] table)
        {

            if (hand[0].Rank == hand[1].Rank && table.Any(x => x.Rank == hand[0].Rank))
                return (true, (int)hand[0].Rank);
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
                            return (true, (int)card.Rank);
                    }
                    threeCards = 1;
                }
            }
            return (false, 0);
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
                    }

                    if (correctTwoPairs == 2)
                        return true;
                }
            }
            return false;
        }
        public static (bool, int) IsOnePair(Card[] hand, Card[] table)
        {
            if (hand[0].Rank == hand[1].Rank)
                return (true, (int)hand[0].Rank);

            foreach (var card in hand)
            {
                foreach(var tableRank in table)
                {
                    if(card.Rank == tableRank.Rank)
                        return (true, (int)card.Rank);
                }
            }
            return (false, 0);
        }
    }
}
