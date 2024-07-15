using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    public class Player
    {
        public static List<Card> mainDeck = new List<Card>();
        public static Stack<Card> gameDeck = new Stack<Card>();

        public string Name { get; set; }
        public int Monets { get; set; }
        public bool IsPlayer { get; private set; }
        public Move LastMove { get; set; }
        public Card[] Deck { get; set; }

        public Player(int monets, bool player,  int deck, string name) 
        {
            Monets = monets;
            IsPlayer = player;
            Deck = new Card[deck];
            Name = name;
        }
        public void ShowDeck()
        {
            Console.WriteLine("Your deck: ");
            for (int i = 0; i < Deck.Length;i++)
            {
                Console.WriteLine(Deck[i].DrawCard());
            }
        } // Showing your deck
        public void RaiseMoney(ref int bank, int amount)
        {
            bank += amount;
            Monets -= amount;
        } // Raise money to bank and also taking from pocket
        public void RaiseCard()
        {
            for(int i = 0; i < Deck.Length; i++)
            {
                Deck[i] = gameDeck.Pop();
            }
        } // Raising card to deck
        public string ChooseMoveForComputer(int lvl)
        {
            // Adding the highest hand rank
            HandRank handRank = PokerHandEvaluator.CheckHand(this);

            // Random value for computer
            Random random = new Random();
            int value = random.Next(1, 11);

            // Rate of deck cards
            int valueOfCards = (int)Deck[0].Rank + (int)Deck[1].Rank;

            // Choosing which move will computer take
            if(lvl == 1)
                LastMove = ChooseMove(handRank, value, valueOfCards);
            else
                LastMove = ChooseMove(handRank, value, valueOfCards, lvl - 1);

            return LastMove.ToString();
        } // Adding value to property 'LastMove'
        private Move ChooseMove(HandRank handRank, int value, int valueOfCards)
        {
            if (handRank == HandRank.OnePair)
            {
                if (value >= 1 && value <= 7)
                    return Move.Raise;
                else
                    return Move.Fold;
            }
            else
            {
                if (valueOfCards >= 4 && valueOfCards <= 12)
                {
                    if (value >= 1 && value <= 7)
                        return Move.Pass;
                    else if (value >= 8 && value <= 9)
                        return Move.Fold;
                    else
                        return Monets == 0 ? Move.Fold : Move.Raise;
                }
                else if (valueOfCards > 12 && valueOfCards <= 20)
                {
                    if (value == 1)
                        return Move.Pass;
                    else if (value >= 3 && value <= 7)
                        return Move.Fold;
                    else
                        return Monets == 0 ? Move.Fold : Move.Raise;
                }
                else
                {
                    if (value >= 1 && value <= 5)
                        return Move.Fold;
                    else
                        return Monets == 0 ? Move.Fold : Move.Raise;
                }
            }
        } // Logic for computer to choose move when deck will be raised
        private Move ChooseMove(HandRank handRank, int value, int valueOfCards, int lvlRate)
        {
            if ((int)handRank >= 1 && 2 + lvlRate >= (int)handRank)
            {
                if (valueOfCards >= 4 && valueOfCards <= 16)
                {
                    if (value >= 1 && value <= 7)
                        return Move.Pass;
                    else if (value >= 8 && value <= 9)
                        return Move.Fold;
                    else
                        return Monets == 0 ? Move.Fold : Move.Raise;
                }
                else if (valueOfCards > 16 && valueOfCards <= 21)
                {
                    if (value >= 1 && value <= 5)
                        return Move.Pass;
                    else if (value >= 6 && value <= 8)
                        return Move.Fold;
                    else
                        return Monets == 0 ? Move.Fold : Move.Raise;
                }
                else
                {
                    if (value >= 1 && value <= 3)
                        return Move.Pass;
                    else if (value >= 3 && value <= 8)
                        return Move.Fold;
                    else
                        return Monets == 0 ? Move.Fold : Move.Raise;
                }
            }
            else if ((int)handRank >= 3 + lvlRate && 6 + lvlRate >= (int)handRank)
            {
                if (valueOfCards >= 4 && valueOfCards <= 16)
                {
                    if (value >= 1 && value <= 2)
                        return Move.Pass;
                    else if (value >= 3 && value <= 8)
                        return Move.Fold;
                    else
                        return Monets == 0 ? Move.Fold : Move.Raise;
                }
                else if (valueOfCards > 16 && valueOfCards <= 21)
                {
                    if (value == 1)
                        return Move.Pass;
                    else if (value >= 2 && value <= 8)
                        return Move.Fold;
                    else
                        return Monets == 0 ? Move.Fold : Move.Raise;
                }
                else
                {
                    if (value >= 1 && value <= 8)
                        return Move.Fold;
                    else
                        return Monets == 0 ? Move.Fold : Move.Raise;
                }
            }
            else
            {
                if (valueOfCards >= 4 && valueOfCards <= 16)
                {
                    if (value == 1)
                        return Move.Pass;
                    else if (value >= 2 && value <= 6)
                        return Move.Fold;
                    else
                        return Monets == 0 ? Move.Fold : Move.Raise;
                }
                else if (valueOfCards > 16 && valueOfCards <= 21)
                {
                    if (value >= 1 && value <= 4)
                        return Move.Fold;
                    else
                        return Monets == 0 ? Move.Fold : Move.Raise;
                }
                else
                {
                    if (value >= 1 && value <= 2)
                        return Move.Fold;
                    else
                        return Monets == 0 ? Move.Fold : Move.Raise;
                }
            }
        } // Logic for computer to choose move ( additionals cards on table)
        public static void CreatingDeck()
        {
            var suits = Enum.GetValues(typeof(Suit));
            var ranks = Enum.GetValues(typeof(Rank));

            foreach (var rank in ranks)
            {
                foreach (var suit in suits)
                {
                    Card card = new Card((Suit)suit, (Rank)rank);
                    mainDeck.Add(card);
                }
            }
        } // Creating main deck
        public static void Shuffle()
        {
            Random random = new Random();
            List<Card> shuffle = mainDeck.OrderBy(s => random.Next()).ToList();
            foreach (Card card in shuffle)
            {
                gameDeck.Push(card);
            }
        } // Shuffle the deck


    }

    public class Card
    { 
        public Suit Suit { get; set; }
        public Rank Rank { get; set; }
        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public string DrawCard()
        {
            return $"{Rank} {Suit}";
        } // draw card
    }


}
