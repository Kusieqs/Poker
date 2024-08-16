using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    internal interface ILogger
    {
        void LogMessage(string message);
        void LogMove();
        void LogDecks();
        void LogHand();
        void LogProbability();
    }
}
