namespace SlotMachineSim
{
    public class Player
    {
        public double Balance { get; private set; }
        public double TotalWagered { get; private set; }
        public double TotalWinnings { get; private set; }
        public double RTP { get; private set; }
        public long GamesPlayed { get; private set; }
        public long LevelsCleared { get; private set; }


        public Player(int initialBalance = 0)
        {
            Balance = initialBalance;
            TotalWagered = 0;
            TotalWinnings = 0;
            RTP = 0;
            GamesPlayed = 0;
            LevelsCleared = 0;
        }

        public void PlaceBet(double TotalCost)
        {
            
            Balance -= TotalCost;
            TotalWagered += TotalCost;
            GamesPlayed++;
        }

        public void AddWinnings(double amount)
        {
            Balance += amount;
            TotalWinnings += amount;
            if (amount > 0) {
                LevelsCleared++;
            }

            CalculateRTP();
        }

        public double CalculateRTP()
        {
            RTP = TotalWagered > 0 ? (double)TotalWinnings / TotalWagered * 100 : 0;
            return RTP;
        }

        public override string ToString()
        {
            return $"Player(Balance={Balance}, TotalWagered={TotalWagered}, " +
                   $"TotalWinnings={TotalWinnings}, RTP={RTP:F2}%, GamesPlayed={GamesPlayed}, GameCleared={LevelsCleared})";
        }
    }
}