using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.ApplicationModel;

namespace PunchingBand.Models
{
    public class UserModel : PersistentModelBase
    {
        private const double LengthOfYearInDays = 365.24218967;

        private Gender gender;
        private double weight;
        private DateTimeOffset birthDate;
        private TemperatureUnit temperatureUnit;
        private WeightUnit weightUnit;
        private BandPosition bandPosition;
        private string name;

        public UserModel()
        {
            // HACK: hard code user to me for now...
            gender = Gender.Male;
            weight = 74.8427;
            birthDate = new DateTimeOffset(1982, 1, 15, 0, 0, 0, TimeSpan.Zero);
            bandPosition = BandPosition.ButtonFacingIn;
            name = "Clippy";

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

            if (!DesignMode.DesignModeEnabled)
            {
                PropertyChanged += async (s, e) => await Save();
            }
        }

        [JsonIgnore]
        public IEnumerable<Gender> Genders
        {
            get
            {
                yield return Gender.Male;
                yield return Gender.Female;
            }
        }

        public string Name
        {
            get { return name; }
            set { Set("Name", ref name, value); }
        }

        public BandPosition BandPosition
        {
            get { return bandPosition; }
            set { Set("BandPosition", ref bandPosition, value); }
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

        [JsonIgnore]
        public int Age
        {
            get { return (int) Math.Round((DateTime.UtcNow - birthDate).TotalDays/LengthOfYearInDays); }
        }
    }
}
