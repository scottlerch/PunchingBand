using System;
using System.Collections.Generic;
using System.Globalization;

namespace PunchingBand.Models
{
    public class UserModel : ModelBase
    {
        private const double LengthOfYearInDays = 365.24218967;

        private Gender gender;
        private double weight;
        private DateTimeOffset birthDate;
        private FistSide defaultFistSide;
        private TemperatureUnit temperatureUnit;
        private WeightUnit weightUnit;
        private BandPosition bandPosition;

        public UserModel()
        {
            // HACK: hard code user to me for now...
            gender = Gender.Male;
            weight = 74.8427;
            birthDate = new DateTimeOffset(1982, 1, 15, 0, 0, 0, TimeSpan.Zero);
            bandPosition = BandPosition.ButtonFacingIn;

            if (RegionInfo.CurrentRegion.IsMetric)
            {
                temperatureUnit = TemperatureUnit.Celsius;
                weightUnit = WeightUnit.Kg;
            }
            else
            {
                temperatureUnit = TemperatureUnit.Fahrenheit;
                weightUnit = WeightUnit.Lbs; 
            }
        }

        public IEnumerable<Gender> Genders
        {
            get
            {
                yield return Gender.Male;
                yield return Gender.Female;
            }
        }

        public BandPosition BandPosition
        {
            get { return bandPosition; }
            set { Set("BandPosition", ref bandPosition, value); }
        }

        public FistSide DefaultFistSide
        {
            get { return defaultFistSide; }
            set { Set("DefaultFistSide", ref defaultFistSide, value); }
        }

        public TemperatureUnit TemperatureUnit
        {
            get { return temperatureUnit; }
            set { Set("TemperatureUnit", ref temperatureUnit, value); }
        }

        public WeightUnit WeightUnit
        {
            get { return weightUnit; }
            set { Set("WeightUnit", ref weightUnit, value); }
        }

        public Gender Gender
        {
            get { return gender; }
            set { Set("Gender", ref gender, value); }
        }

        /// <summary>
        /// Weight in kilograms.
        /// </summary>
        public double Weight
        {
            get { return weight; }
            set { Set("Weight", ref weight, value); }
        }

        public DateTimeOffset BirthDate
        {
            get { return birthDate; }
            set
            {
                Set("BirthDate", ref birthDate, value);
                RaisePropertyChanged("Age");
            }
        }

        public int Age
        {
            get { return (int) Math.Round((DateTime.UtcNow - birthDate).TotalDays/LengthOfYearInDays); }
        }
    }
}
