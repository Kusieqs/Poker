using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    internal class OnePlayerException : Exception
    {
        public override string Message
        {
            get 
            {
                string messaage = $" won {TexasHoldem.bank} monets!";
                Card.infoDeck = "";
                return messaage;
            }
        }
    }
}
