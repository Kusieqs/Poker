namespace Poker
{
    // Suit of card
    public enum Suit
    {
        Heart,
        Diamond,
        Club,
        Spade
    }

    // Rank of card
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

    // HandRank
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

    // Move
    public enum Move
    {
        Raise = 1,
        Fold,
        Pass,
        Call,
        AllIn
    }
}