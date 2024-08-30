using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    public static class NumericData
    {
        public static int StrongCall(double procent, int monets)
        {
            Random random = new Random();
            int amount = (int)Math.Round(((monets * procent) / 10) * 10);
            int probability = random.Next(1, 11);
            do
            {
                int raise = 0;
                switch (procent)
                {
                    case 0.3:
                        if (probability >= 1 && probability <= 6)
                            raise = (random.Next(1, amount) / 10) * 10;
                        else
                            raise = amount;
                        break;
                    case 0.6:
                        if (probability >= 1 && probability <= 6)
                            raise = (random.Next(1, amount) / 10) * 10;
                        else if (probability == 10)
                            raise = monets;
                        else
                            raise = amount;
                        break;
                    case 1:
                        if (probability >= 1 && probability <= 6)
                            raise = (random.Next(1, amount) / 10) * 10;
                        else if (probability == 7 || probability == 8)
                            raise = monets;
                        else
                            raise = amount;
                        break;
                }

                if (raise >= 10)
                    return raise;

            } while (true);

        } // Setting call that will be strong or not
        public static Move ChooseMove(HandRank handRank, int value, int valueOfCards, int monets)
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
                        return monets == 0 ? Move.Fold : Move.Raise;
                }
                else if (valueOfCards > 12 && valueOfCards <= 20)
                {
                    if (value == 1)
                        return Move.Pass;
                    else if (value >= 3 && value <= 7)
                        return Move.Fold;
                    else
                        return monets == 0 ? Move.Fold : Move.Raise;
                }
                else
                {
                    if (value >= 1 && value <= 5)
                        return Move.Fold;
                    else
                        return monets == 0 ? Move.Fold : Move.Raise;
                }
            }
        } // Logic for computer to choose move when deck will be raised 
        public static Move ChooseMove(HandRank handRank, int value, int valueOfCards, int monets, int lvlRate)
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
                        return monets == 0 ? Move.Fold : Move.Raise;
                }
                else if (valueOfCards > 16 && valueOfCards <= 21)
                {
                    if (value >= 1 && value <= 5)
                        return Move.Pass;
                    else if (value >= 6 && value <= 8)
                        return Move.Fold;
                    else
                        return monets == 0 ? Move.Fold : Move.Raise;
                }
                else
                {
                    if (value >= 1 && value <= 3)
                        return Move.Pass;
                    else if (value >= 3 && value <= 8)
                        return Move.Fold;
                    else
                        return monets == 0 ? Move.Fold : Move.Raise;
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
                        return monets == 0 ? Move.Fold : Move.Raise;
                }
                else if (valueOfCards > 16 && valueOfCards <= 21)
                {
                    if (value == 1)
                        return Move.Pass;
                    else if (value >= 2 && value <= 8)
                        return Move.Fold;
                    else
                        return monets == 0 ? Move.Fold : Move.Raise;
                }
                else
                {
                    if (value >= 1 && value <= 8)
                        return Move.Fold;
                    else
                        return monets == 0 ? Move.Fold : Move.Raise;
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
                        return monets == 0 ? Move.Fold : Move.Raise;
                }
                else if (valueOfCards > 16 && valueOfCards <= 21)
                {
                    if (value >= 1 && value <= 4)
                        return Move.Fold;
                    else
                        return monets == 0 ? Move.Fold : Move.Raise;
                }
                else
                {
                    if (value >= 1 && value <= 2)
                        return Move.Fold;
                    else
                        return monets == 0 ? Move.Fold : Move.Raise;
                }
            }
        } // Logic for computer to choose move ( additionals cards on table) 
        public static Move CallOrPass(int amount, int monets, HandRank handRank)
        {

            // Random value for computer
            Random random = new Random();
            int value = random.Next(1, 11);
            Move move;

            double procentOfMonets;
            if (amount < monets)
                procentOfMonets = Math.Round((double)(amount / monets), 2);
            else
                procentOfMonets = 1;


            if (procentOfMonets <= 1 && procentOfMonets >= 0.66)
            {
                if ((int)handRank >= 5 && (int)handRank <= 10)
                    move = Move.Call;
                else
                    move = Bluff(value);
            }
            else if (procentOfMonets <= 0.65 && procentOfMonets >= 0.33)
            {
                if ((int)handRank >= 3 && (int)handRank <= 10)
                    move = Move.Call;
                else
                    move = Bluff(value);
            }
            else
            {
                if ((int)handRank >= 1 && (int)handRank <= 10)
                    move = Move.Call;
                else
                    move = Bluff(value);
            }

            if (procentOfMonets == 1 && move == Move.Call)
                return Move.AllIn;

            return move;

        } // Feature to choose pass or call for computer 
        public static Move Bluff(int value)
        {
            if (value >= 7)
                return Move.Call;
            else
                return Move.Pass;
        } // Method to choose bluff 

    }
}
