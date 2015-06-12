namespace PunchingBand.Utilities
{
    internal class PunchInfo
    {
        public FistSide FistSide { get; private set; }

        public PunchStatus Status { get; private set; }

        public double? Strength { get; private set; }

        public PunchInfo(FistSide fistSide, PunchStatus status, double? strength)
        {
            FistSide = fistSide;
            Status = status;

            // HACK: make sure strength in range from 0 to 1 incase punch algorithm messes up
            if (strength.HasValue)
            {
                if (strength.Value > 1.0)
                {
                    strength = 1.0;
                }
                else if (strength.Value < 0.0)
                {
                    strength = 0.0;
                }
            }

            Strength = strength;
        }
    }
}
