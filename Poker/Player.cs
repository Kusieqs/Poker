using System;
using System.Collections.Generic;
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
            var names = new[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
            var suits = Enum.GetValues(typeof(Suit));
            var colors = Enum.GetValues(typeof(Color));

            foreach (var name in names)
            {
                foreach (var suit in suits)
                {
                    foreach (var color in colors)
                    {
                        Card card = new Card(name, (Suit)suit, (Color)color);
                        mainDeck.Add(card);
                    }
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
        public string name { get; set; }
        public Suit suit { get; set; }
        public Color color { get; set; }
        public Card(string name, Suit suit, Color color)
        {
            this.name = name;
            this.suit = suit;
            this.color = color;
        }

        public string DrawCard()
        {
            return $"{name} {suit} {color}";
        } // draw card
    }


}
