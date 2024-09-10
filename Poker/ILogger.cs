using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    // Logger interface
    internal interface ILogger
    {
        void LogMessage(string message);
        void LogMove(string message);
        void LogDecks(string message);
        void LogHand(string message);
    }
}
