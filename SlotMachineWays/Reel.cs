using System;
using System.Collections.Generic;
using System.Linq;

namespace SlotMachineSim
{
    public class Reel
    {
        public List<Symbol> Symbols;
        private Random RandomGenerator;

        public Reel(List<Symbol> symbols)
        {
            Symbols = symbols;
            RandomGenerator = new Random();
        }

        // Spin and return the stopping index
        public int Spin()
        {
            int totalWeight = Symbols.Sum(s => s.Weight);
            int randomValue = RandomGenerator.Next(1, totalWeight + 1);

            int cumulativeWeight = 0;
            for (int i = 0; i < Symbols.Count; i++)
            {
                cumulativeWeight += Symbols[i].Weight;
                if (randomValue <= cumulativeWeight)
                {
                    return i; // Return the index of the stopping symbol
                }
            }

            throw new Exception("Spin logic failed.");
        }

        // Get the symbol at a specific index
        public Symbol GetSymbolAt(int index)
        {
            return Symbols[index % Symbols.Count]; // Wraps around for circular behavior
        }
    }
}


