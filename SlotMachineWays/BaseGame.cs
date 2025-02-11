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


        public BaseGame(List<Reel> reels, Dictionary<string, int[]> payoutTable, List<int[,]> paylines)
        {
            Reels = reels ?? throw new ArgumentNullException(nameof(reels));
            PayoutTable = payoutTable ?? throw new ArgumentNullException(nameof(payoutTable));
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
            var totalWin = EvaluateWays(grid);
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


        private int EvaluateWays(string[,] grid)
        {
            int totalPayout = 0;
            HashSet<string> countedWays = new HashSet<string>();

            for (int row = 0; row < 3; row++)
            {
                string firstSymbol = grid[row, 0];

                if (countedWays.Contains(firstSymbol)) continue; // Prevent duplicate payouts for the same symbol

                if (IsWinningWay(grid, firstSymbol, out int payout))
                {
                    totalPayout += payout;
                    countedWays.Add(firstSymbol); // Mark this symbol as counted
                }
            }

            return totalPayout;
        }


        private bool IsWinningWay(string[,] grid, string symbol, out int payout)
        {
            payout = 0;
            int[] reelCounts = new int[5];

            for (int reel = 0; reel < 5; reel++)
            {
                for (int row = 0; row < 3; row++)
                {
                    if (grid[row, reel] == symbol || grid[row, reel] == "WW") 
                    {
                        reelCounts[reel]++; // 
                    }
                }

                if (reelCounts[reel] == 0) break; 
            }

            int wayCount = reelCounts[0] * reelCounts[1] * reelCounts[2] * reelCounts[3] * reelCounts[4]; 
            if (wayCount > 0)
            {
                payout = GetPayoutForSymbol(symbol, wayCount);
                return true;
            }

            return false;
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


