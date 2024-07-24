﻿namespace Poker
{
    public enum Suit
    {
        Heart,
        Diamond,
        Club,
        Spade
    }
    public enum Rank
    {
        Two = 2,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }
    public enum HandRank
    {
        HighCard = 1,
        OnePair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush,
        RoyalFlush
    }
    public enum Move
    {
        Raise = 1,
        Fold,
        Pass,
        Call,
    }
}