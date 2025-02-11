using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SlotMachineSim
{
    public class BaseGame
    {
        private const int DEFAULT_NUM_ROUNDS = 10_000_000;
        public List<Reel> Reels { get; private set; }
        private Dictionary<string, int[]> PayoutTable { get; set; } // Store the payout table here
        private Player Player;
        private List<int[,]> Lines;

        public BaseGame(List<Reel> reels, Dictionary<string, int[]> payoutTable, List<int[,]> paylines)
        {
            Reels = reels ?? throw new ArgumentNullException(nameof(reels));
            PayoutTable = payoutTable ?? throw new ArgumentNullException(nameof(payoutTable));
            Lines = paylines ?? throw new ArgumentNullException(nameof(paylines)); // Set paylines from JSON
            Player = new Player();
        }


        public void Start(int betMultiplier)
        {


            if (Reels == null || !Reels.Any())
            {
                Console.WriteLine("Reels are not loaded!");
                return;
            }

            for (int i = 0; i < DEFAULT_NUM_ROUNDS; i++)
            {
                PlayRound(betMultiplier);

                // Print progress every 5% of the total rounds
                if (i % Math.Max(DEFAULT_NUM_ROUNDS / 20, 1) == 0 && i > 0)
                {
                    PrintProgress(i, DEFAULT_NUM_ROUNDS);
                }
            }

            Console.WriteLine($"{DEFAULT_NUM_ROUNDS} rounds completed!");
            Console.WriteLine(Player);
        }

        public void PlayRound(int betMultiplier)
        {
            int totalCostToCover = 50; // Fixed cost to cover all paylines
            int totalBet = totalCostToCover * betMultiplier;

            // Deduct the bet from the player's balance
            Player.PlaceBet(totalBet);

            // Generate the grid representing the spin result
            string[,] grid = GenerateGrid();

            // Evaluate paylines and determine total winnings
            var totalWin = EvaluatePaylines(grid, Lines);
            totalWin *= betMultiplier; // Apply the bet multiplier to the total win

            // Add the winnings to the player's balance
            Player.AddWinnings(totalWin);
        }

        private string[,] GenerateGrid()
        {
            string[,] grid = new string[3, 5];

            for (int reelIndex = 0; reelIndex < Reels.Count; reelIndex++)
            {
                int stoppingIndex = Reels[reelIndex].Spin();
                int reelSize = Reels[reelIndex].Symbols.Count;

                // int topIndex = (stoppingIndex - 1 + reelSize) % reelSize;
                // int middleIndex = stoppingIndex;
                // int bottomIndex = (stoppingIndex + 1) % reelSize;

                int topIndex = stoppingIndex;
                int middleIndex = (stoppingIndex + 1) % reelSize;
                int bottomIndex = (stoppingIndex + 2) % reelSize;

                grid[0, reelIndex] = Reels[reelIndex].GetSymbolAt(topIndex).Name;
                grid[1, reelIndex] = Reels[reelIndex].GetSymbolAt(middleIndex).Name;
                grid[2, reelIndex] = Reels[reelIndex].GetSymbolAt(bottomIndex).Name;
            }

            return grid;
        }

        private void PrintProgress(int currentRound, int totalRounds)
        {
            double percentage = (double)currentRound / totalRounds * 100;
            Console.Write($"\rProgress: {percentage:F2}% completed.");
        }


        private int EvaluatePaylines(string[,] grid, List<int[,]> paylines)
        {
            int totalPayout = 0;
      

            foreach (var payline in paylines)
            {
                List<string> symbolsInLine = new List<string>();

                for (int i = 0; i < payline.GetLength(0); i++)
                {
                    int row = payline[i, 0];
                    int col = payline[i, 1];
                    symbolsInLine.Add(grid[row, col]);
                }

                if (IsWinningLine(symbolsInLine, out int payout))
                {
                    totalPayout += payout;
                    
                }
            }

            return totalPayout; // Return total payout and example winning line
        }

        private bool IsWinningLine(List<string> symbolsInLine, out int payout)
        {
            payout = 0;
            if (symbolsInLine == null || symbolsInLine.Count < 3) return false;

            string firstSymbol = symbolsInLine[0];  // First symbol in the line
            int wildCount = 0;
            int count = 0;
            int maxPayout = 0;
            string? bestSymbol = null;

            // ✅ **Step 1: Check for pure Wild wins (consecutive Wilds at the start)**
            if (firstSymbol == "WW")
            {
                for (int i = 0; i < symbolsInLine.Count; i++)
                {
                    if (symbolsInLine[i] == "WW")
                        wildCount++;
                    else
                        break;  // Stop at first non-Wild symbol
                }

                if (wildCount >= 3)
                {
                    maxPayout = GetPayoutForSymbol("WW", wildCount);  // Store Wild payout
                }
            }

            // ✅ **Step 2: Check for regular symbol wins using Wilds**
            for (int i = 0; i < symbolsInLine.Count; i++)
            {
                string symbol = symbolsInLine[i];

                if (symbol == "WW" || bestSymbol == null)
                {
                    bestSymbol = symbol == "WW" ? bestSymbol : symbol;  // Track first real symbol
                    count++;
                }
                else if (symbol == bestSymbol)
                {
                    count++;  // Extend matching sequence
                }
                else
                {
                    break;  // Stop at first mismatch
                }
            }

            if (count >= 3 && bestSymbol != null)
            {
                int symbolPayout = GetPayoutForSymbol(bestSymbol, count);
                maxPayout = Math.Max(maxPayout, symbolPayout); 
            }

            payout = maxPayout;
            return payout > 0;
        }




        private int GetPayoutForSymbol(string symbol, int count)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                Console.WriteLine("⚠️ Warning: Attempted to retrieve payout for a null or empty symbol.");
                return 0;
            }

            if (!PayoutTable.TryGetValue(symbol, out var payouts))
            {
                Console.WriteLine($"⚠️ Warning: Symbol '{symbol}' not found in payout table.");
                return 0;
            }

            if (count < 1 || count > payouts.Length)
            {
                Console.WriteLine($"⚠️ Warning: Invalid count {count} for symbol '{symbol}'.");
                return 0;
            }

            return payouts[count - 1];
        }

    }
}


