namespace PunchingBand.Recognition
{
    public class PunchInfo
    {
        public FistSides FistSide { get; private set; }

        public PunchStatus Status { get; private set; }

        /// <summary>
        /// Punch strength in g's (1g = 9.81m/s^2).
        /// </summary>
        public double? Strength { get; private set; }

        public PunchRecognition PunchRecognition { get; private set; }

        public PunchInfo(FistSides fistSide, PunchStatus status, double? strength, PunchRecognition punchRecognition)
        {
            FistSide = fistSide;
            Status = status;

            // HACK: make sure strength in range from 0 to PunchDetector.MaximumAcceleration incase punch algorithm messes up
            if (strength.HasValue)
            {
                if (strength.Value > PunchDetector.MaximumAcceleration)
                {
                    strength = PunchDetector.MaximumAcceleration;
                }
                else if (strength.Value < 0.0)
                {
                    strength = 0.0;
                }
            }

            Strength = strength;
            PunchRecognition = punchRecognition;
        }
    }
}
