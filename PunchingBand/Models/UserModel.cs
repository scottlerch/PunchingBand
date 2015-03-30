using System;
using System.Collections.Generic;

namespace PunchingBand.Models
{
    public enum Gender
    {
        Male,
        Female,
    }

    public class UserModel : ModelBase
    {
        private const double LengthOfYearInDays = 365.24218967;

        private Gender gender;
        private double weight;
        private DateTimeOffset birthDate;

        public UserModel()
        {
            // HACK: hard code user to me for now...
            gender = Gender.Male;
            weight = 74.8427;
            birthDate = new DateTimeOffset(1982, 1, 15, 0, 0, 0, TimeSpan.Zero);
        }

        public IEnumerable<Gender> Genders
        {
            get
            {
                yield return Gender.Male;
                yield return Gender.Female;
            }
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
