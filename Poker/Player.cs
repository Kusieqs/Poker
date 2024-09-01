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
        public HandRank Hand { get; set; }

        public Player(int monets, bool player, int deck, string name) 
        {
            Monets = monets;
            IsPlayer = player;
            Deck = new Card[deck];
            Name = name;
        }
        public void ShowDeck()
        {
            Console.WriteLine("Deck:");
            for (int i = 0; i < Deck.Length;i++)
            {
                Deck[i].DrawCard();
            }
            Console.WriteLine();
        } // Showing your deck
        public void RaiseMoney(int amount)
        {
            if (amount >= Monets)
            {
                amount = Monets;
                LastMove = Move.AllIn;
            }    

            TexasHoldem.bank += amount;
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
            if(lvl == 0)
                LastMove = NumericData.ChooseMove(handRank, value, Monets ,valueOfCards);
            else
                LastMove = NumericData.ChooseMove(handRank, value, Monets, valueOfCards, lvl - 1);

            return LastMove.ToString();
        } // Adding value to property 'LastMove'
        public Move CallOrPass(int amount, int monets)
        {
            // Adding the highest hand rank
            HandRank handRank = PokerHandEvaluator.CheckHand(this);

            return NumericData.CallOrPass(amount, monets, handRank);

        } // Feature to choose pass or call for computer 
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
        public static void SetFold(bool startRound = false)
        {
            for(int i = 0; i < TexasHoldem.listOfPlayers.Count; i++)
            { 
                if (!startRound && (TexasHoldem.listOfPlayers[i].LastMove == Move.Pass || TexasHoldem.listOfPlayers[i].LastMove == Move.AllIn))
                    continue;

                TexasHoldem.listOfPlayers[i].LastMove = Move.Fold;
            }
        } // Set fold for all players
        public static string ChooseWinner()
        {
            Player winner = null;

            for (int i = 0; i < TexasHoldem.listOfPlayers.Count; i ++)
            {
                if (TexasHoldem.listOfPlayers[i].LastMove == Move.Pass)
                    continue;

                if (i == 0)
                {
                    winner = TexasHoldem.listOfPlayers[i];
                    continue;
                }

                if ((int)winner.Hand == (int)TexasHoldem.listOfPlayers[i].Hand)
                {
                    // dla najwyzeszj karty inaczej, dla par inaczej,
                    // wyznaczenie zwyciescy = przelanie mu pieniedzy
                    int winnerHand = (int)winner.Deck[0].Rank + (int)winner.Deck[1].Rank;
                    int otherHand = (int)TexasHoldem.listOfPlayers[i].Deck[0].Rank + (int)TexasHoldem.listOfPlayers[i].Deck[1].Rank;

                    if (winnerHand < otherHand)
                        winner = TexasHoldem.listOfPlayers[i];
                }
                else if ((int)winner.Hand < (int)TexasHoldem.listOfPlayers[i].Hand)
                    winner = TexasHoldem.listOfPlayers[i];
            }

            TexasHoldem.listOfPlayers.Where(x => x.Name != winner.Name).Select(x => x.LastMove = Move.Pass);

            return winner.Name;
        }

    }

    public class Card
    {
        public Suit Suit { get; set; }
        public Rank Rank { get; set; }
        public static string infoDeck = "";
        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }
        public void DrawCard(ConsoleColor color = ConsoleColor.Magenta)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{Rank}\t{Suit}");
            Console.ResetColor();
        } // draw card
        public static void DrawCardOnTable(List<Card> listOfCards, int sleep = 0, bool justShow = false)
        {
            if(!justShow)
            {
                var cords = Console.GetCursorPosition();
                switch (listOfCards.Count)
                {
                    case 3:
                        foreach (Card card in listOfCards)
                        {
                            Thread.Sleep(sleep);
                            card.DrawCard(ConsoleColor.DarkMagenta);
                            infoDeck += $"{card.Rank}\t{card.Suit}\n";
                        }
                        break;
                    case 4:
                    case 5:
                        Thread.Sleep(sleep);
                        infoDeck += $"{listOfCards[listOfCards.Count - 1].Rank}\t{listOfCards[listOfCards.Count - 1].Suit}\n";
                        listOfCards[listOfCards.Count - 1].DrawCard(ConsoleColor.DarkMagenta);
                        break;

                }
                Console.SetCursorPosition(cords.Left, cords.Top);
            }
            else
            {
                foreach (Card card in listOfCards)
                {
                    card.DrawCard(ConsoleColor.DarkMagenta);
                }
            }
        } // draw cards on the table
        public static void ShowTable()
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(infoDeck);
            Console.ResetColor();
        } // Showing table
    }


}
