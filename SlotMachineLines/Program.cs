using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SlotMachineSim
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load all data from JSON
            List<Reel> reels = LoadReelsFromJson("data/Reels.json");
            Dictionary<string, int[]> payoutTable = LoadPayoutTable("data/Paytable.json");
            List<int[,]> paylines = LoadPaylinesFromJson("data/Paylines.json");

            // Create game instance
            BaseGame game = new BaseGame(reels, payoutTable, paylines);

            Console.WriteLine("Choose a bet multiplier:");
            if (!int.TryParse(Console.ReadLine(), out int betMultiplier))
            {
                Console.WriteLine("Invalid multiplier. Please choose a valid option.");
            }

            // Run the game loop
            while (true)
            {
                game.Start(betMultiplier);

                Console.WriteLine("Play again? (y/n):");
                if (Console.ReadLine()?.ToLower() != "y")
                {
                    break;
                }
            }
        }


        static List<Reel> LoadReelsFromJson(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Reels JSON file not found at: {filePath}");
            }

            string jsonContent = File.ReadAllText(filePath);
            var reelData = JsonSerializer.Deserialize<ReelData>(jsonContent);

            if (reelData?.Reels == null || !reelData.Reels.Any())
            {
                throw new Exception("Failed to load valid reels from JSON.");
            }

            return reelData.Reels.Select(symbols => new Reel(symbols)).ToList();
        }

        static Dictionary<string, int[]> LoadPayoutTable(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Payout JSON file not found at: {filePath}");
            }

            string jsonContent = File.ReadAllText(filePath);
            var payoutTable = JsonSerializer.Deserialize<Dictionary<string, int[]>>(jsonContent);

            if (payoutTable == null || !payoutTable.Any())
            {
                throw new Exception("Failed to load payout table from JSON.");
            }

            return payoutTable;
        }

        static List<int[,]> LoadPaylinesFromJson(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Paylines JSON file not found at: {filePath}");
            }

            string jsonContent = File.ReadAllText(filePath);
            var rawPaylines = JsonSerializer.Deserialize<List<List<int[]>>>(jsonContent);

            if (rawPaylines == null || rawPaylines.Count == 0)
            {
                throw new Exception("Failed to load valid paylines from JSON.");
            }

            List<int[,]> paylines = new List<int[,]>();

            foreach (var payline in rawPaylines)
            {
                int[,] convertedPayline = new int[payline.Count, 2];

                for (int i = 0; i < payline.Count; i++)
                {
                    convertedPayline[i, 0] = payline[i][0]; // Row
                    convertedPayline[i, 1] = payline[i][1]; // Column
                }

                paylines.Add(convertedPayline);
            }

            return paylines;
        }

    }
}






