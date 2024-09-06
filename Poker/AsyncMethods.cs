using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    public static class AsyncMethods
    {
        public static async Task HandleUserAndComputerMoves(int lvl, Dictionary<Player, (int, int)> cursor, string options)
        {
            TexasHoldem.userToken = new CancellationTokenSource(); // Special token to delete user method
            TexasHoldem.computerToken = new CancellationTokenSource();  // Special token to delete Computer method

            switch (TexasHoldem.listOfPlayers[0].LastMove)
            {
                case Move.AllIn:
                    Console.Write($"Your move:");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(TexasHoldem.listOfPlayers[0].LastMove);
                    Console.ResetColor();
                    TexasHoldem.cords = Console.GetCursorPosition();
                    Console.WriteLine("Waiting for the rest of the players . . .");
                    await GetComputerMoveAsync(cursor, lvl);
                    break;

                case Move.Pass:
                    TexasHoldem.cords = Console.GetCursorPosition();
                    await GetComputerMoveAsync(cursor, lvl);
                    break;

                default:
                    Console.WriteLine(options);
                    Console.Write("Your move: ");
                    TexasHoldem.cords = Console.GetCursorPosition();
                    await Task.WhenAll(GetUserMoveAsync(), GetComputerMoveAsync(cursor, lvl));
                    break;
            }
        } // Setting new tokens and active async methods
        private static Task GetUserMoveAsync()
        {
            CancellationToken token = TexasHoldem.userToken.Token;
            return Task.Run(() =>
            {
                while (true)
                {
                    if (TexasHoldem.listOfPlayers[0].LastMove == Move.AllIn)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(Move.AllIn);
                        Console.ResetColor();
                        break;
                    }

                    try
                    {
                        Move moveEnum;
                        ConsoleKeyInfo consoleKeyInfo;
                        do
                        {

                            StartReadingKeys(); // Reading key if token is open
                            TexasHoldem.keyBuffer = new BlockingCollection<ConsoleKeyInfo>();
                            consoleKeyInfo = TexasHoldem.keyBuffer.Take(token);

                            // Choosing one of three options
                            if (int.TryParse(consoleKeyInfo.KeyChar.ToString(), out int result))
                            {
                                if (result >= 1 && result <= 3)
                                {
                                    moveEnum = (Move)result;
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.Write(moveEnum.ToString());
                                    Console.ResetColor();
                                    TexasHoldem.chooseOption = true;
                                    break;
                                }
                            }

                        } while (true);

                        // setting last move for user
                        TexasHoldem.listOfPlayers[0].LastMove = (Move)(int.Parse(consoleKeyInfo.KeyChar.ToString()));

                        // Adding Last move for a main player
                        if (Move.Raise == moveEnum)
                        {
                            TexasHoldem.firstRaise = true;
                            TexasHoldem.computerToken?.Cancel(); // Deleting computer async method
                        }
                        else if (Move.Pass == moveEnum && TexasHoldem.listOfPlayers.Where(x => x.LastMove != Move.Pass).Count() == 1)
                            throw new OnePlayerException();

                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        // Breaking method
                        return;
                    }
                    catch (OnePlayerException)
                    {
                        TexasHoldem.computerToken?.Cancel();
                        return;
                    }
                }
            }, token);
        } // User Async method to choose move
        private static Task GetComputerMoveAsync(Dictionary<Player, (int, int)> cursor, int lvl = 0, int amount = 0)
        {
            CancellationToken token = TexasHoldem.computerToken.Token;
            return Task.Run(async () =>
            {
                try
                {
                    Dictionary<Player, bool> activePlayers = TexasHoldem.ActivePlayersToDictionary(); // Setting active players and cords for them
                    var cords = Console.GetCursorPosition();
                    Random random = new Random();

                    foreach (var dict in activePlayers)
                    {
                        if (dict.Key.LastMove == Move.AllIn)
                        {
                            Console.SetCursorPosition(cursor[dict.Key].Item1, cursor[dict.Key].Item2);
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.Write($"{Move.AllIn}");
                            Console.ResetColor();
                            activePlayers.Remove(dict.Key);
                            Console.SetCursorPosition(cords.Left, cords.Top);
                        }
                    }

                    for (int i = 0; i < activePlayers.Count; i++)
                    {
                        await Task.Delay(random.Next(1000, 5000), token);
                        Player player = null;
                        string name;
                        while (true)
                        {
                            int indexOfPlayer = random.Next(0, activePlayers.Count);
                            player = activePlayers.Keys.ElementAt(indexOfPlayer);

                            if (activePlayers[player] == true)
                                continue;
                            else
                            {
                                activePlayers[player] = true;
                                name = player.Name;
                                break;
                            }
                        }

                        string move = TexasHoldem.listOfPlayers.Where(x => x.Name == name).First().ChooseMoveForComputer(lvl);
                        Console.SetCursorPosition(cursor[player].Item1, cursor[player].Item2);
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write($"{move}");
                        Console.ResetColor();

                        if (Enum.Parse<Move>(move) == Move.Raise)
                        {
                            TexasHoldem.firstRaise = true;
                            TexasHoldem.userToken?.Cancel();
                            break;
                        }
                        else if (Move.Pass == Enum.Parse<Move>(move) && TexasHoldem.listOfPlayers.Where(x => x.LastMove != Move.Pass).Count() == 1)
                            throw new OnePlayerException();

                        if (!TexasHoldem.chooseOption)
                            Console.SetCursorPosition(cords.Item1, cords.Item2);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (OnePlayerException)
                {
                    TexasHoldem.userToken?.Cancel();
                    return;
                }
            }, token);
        } // Computer Async method to choose move
        public static Task PlayerCallOrPass(int amount)
        {
            CancellationToken token = TexasHoldem.userToken.Token;
            return Task.Run(() =>
            {
                do
                {
                    try
                    {
                        ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                        if (int.TryParse(key.KeyChar.ToString(), out int result) && result <= 2 && result >= 1)
                        {

                            if (result == 1)
                            {
                                TexasHoldem.listOfPlayers[0].LastMove = Move.Call;
                                TexasHoldem.listOfPlayers[0].RaiseMoney(amount);
                            }
                            else
                                TexasHoldem.listOfPlayers[0].LastMove = Move.Pass;

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(TexasHoldem.listOfPlayers[0].LastMove.ToString());
                            Console.ResetColor();
                            break;
                        }
                    }
                    catch (OnePlayerException)
                    {
                        TexasHoldem.computerToken?.Cancel();
                        return;
                    }
                } while (true);
            }, token);
        } // User method to choose Call (Allin) or pass
        public static Task ComputerCallOrPass((int, int) cords, Dictionary<Player, (int, int)> cursor, int amount)
        {
            CancellationToken token = TexasHoldem.computerToken.Token;
            return Task.Run(async () =>
            {
                try
                {
                    Dictionary<Player, bool> activePlayers = new Dictionary<Player, bool>();

                    for (int i = 1; i < TexasHoldem.listOfPlayers.Count; i++)
                    {
                        if (TexasHoldem.listOfPlayers[i].LastMove != Move.Raise && TexasHoldem.listOfPlayers[i].LastMove != Move.Pass)
                            activePlayers.Add(TexasHoldem.listOfPlayers[i], false);
                    }

                    for (int i = 0; i < TexasHoldem.listOfPlayers.Where(x => !x.IsPlayer && x.LastMove != Move.Pass && x.LastMove != Move.Raise).Count(); i++)
                    {
                        Random random = new Random();
                        Player player = null;
                        string name;
                        while (true)
                        {
                            int indexOfPlayer = random.Next(0, activePlayers.Count);
                            player = activePlayers.Keys.ElementAt(indexOfPlayer);

                            if (activePlayers[player] == true)
                                continue;
                            else
                            {
                                activePlayers[player] = true;
                                name = player.Name;
                                break;
                            }
                        }
                        await Task.Delay(random.Next(1000, 5000));

                        Move callOrPass = TexasHoldem.listOfPlayers.Where(x => x.Name == name).First().CallOrPass(amount, player.Monets);
                        string move = callOrPass.ToString();

                        if (callOrPass == Move.Call)
                            TexasHoldem.listOfPlayers.Where(x => x.Name == name).First().RaiseMoney(amount);
                        else
                            TexasHoldem.listOfPlayers.Where(x => x.Name == name).First().LastMove = Move.Pass;

                        Console.SetCursorPosition(cursor[player].Item1, cursor[player].Item2);
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write($"{move}");
                        Console.ResetColor();
                        Console.SetCursorPosition(cords.Item1, cords.Item2);

                    }
                }
                catch (OnePlayerException)
                {
                    TexasHoldem.userToken?.Cancel();
                    return;
                }
            }, token);
        } // Computer method to choose Call (Allin) or pass
        private static void StartReadingKeys()
        {
            Task.Run(() =>
            {
                while (!TexasHoldem.userToken.Token.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true);
                        TexasHoldem.keyBuffer.Add(key);
                    }
                }
            });
        } // Method for user to enter char

    }
}
