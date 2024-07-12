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
        public int Monets { get; set; }
        public bool IsPlayer { get; private set; }
        public Card[] Deck { get; set; }

        public Player(int monets, bool player,  int deck) 
        {
            Monets = monets;
            IsPlayer = player;
            Deck = new Card[deck];
        }
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
        public void ShowDeck()
        {
            for (int i = 0; i < Deck.Length;i++)
            {
                Console.WriteLine(Deck[i].DrawCard());
            }
        } // Showing our deck
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
