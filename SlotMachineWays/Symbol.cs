using System.Collections.Generic;

namespace SlotMachineSim
{
    public class Symbol
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Weight { get; set; }

        public Symbol() { } 

        public Symbol(string name, int index, int weight)
        {
            Name = name;
            Index = index;
            Weight = weight;
        }

        public override string ToString() => Name;
    }


    public class ReelData
    {
        public List<List<Symbol>> Reels { get; set; } = new List<List<Symbol>>();
    }


}
