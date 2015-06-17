namespace PunchingBand.Models.Enums
{
    /// <summary>
    /// Position user wears the band.  Typically this is on the inside wrist with
    /// the physical buttons facing away from the hand towards the arm/body.
    /// It's possible some users may wear the band on the outside wrist
    /// with the physical buttons facing towards the hand and away from the body.
    /// </summary>
    public enum BandPosition
    {
        ButtonFacingIn,
        ButtonFacingOut,
    }
}
