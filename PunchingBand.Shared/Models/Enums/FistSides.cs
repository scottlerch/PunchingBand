using System;

namespace PunchingBand.Models.Enums
{
    [Flags]
    public enum FistSides
    {
        Unknown = 0,
        Left = 1,
        Right = 2,
        LeftAndRight = Left | Right,
    }
}
