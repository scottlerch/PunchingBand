namespace PunchingBand
{
    public static class PortableDesignMode
    {
        static PortableDesignMode()
        {
            DesignModeEnabled = true;
        }

        public static bool DesignModeEnabled { get; set; }
    }
}
