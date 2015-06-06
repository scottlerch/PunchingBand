using System;
using Windows.Graphics.Display;

namespace PunchingBand.Utilities
{
    public static class DeviceInfo
    {
        public static double GetActualResolution(double dimension)
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                return dimension;
            }

            double toReturn;
            var scale = DisplayInformation.GetForCurrentView().ResolutionScale;

            switch (scale)
            {
                case ResolutionScale.Scale100Percent:
                    toReturn = dimension;
                    break;
                case ResolutionScale.Scale120Percent:
                    toReturn = dimension * 1.2;
                    break;
                case ResolutionScale.Scale140Percent:
                    toReturn = dimension * 1.4;
                    break;
                case ResolutionScale.Scale150Percent:
                    toReturn = dimension * 1.5;
                    break;
                case ResolutionScale.Scale160Percent:
                    toReturn = dimension * 1.6;
                    break;
                case ResolutionScale.Scale180Percent:
                    toReturn = dimension * 1.8;
                    break;
                case ResolutionScale.Scale225Percent:
                    toReturn = dimension * 2.25;
                    break;
                default:
                    toReturn = dimension;
                    break;
            }

            return Math.Round(toReturn);
        }
    }
}
