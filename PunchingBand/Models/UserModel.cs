using System;

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
        private DateTime birthDate;

        public UserModel()
        {
            // HACK: hard code user to me for now...
            gender = Gender.Male;
            weight = 74.8427;
            birthDate = new DateTime(1982, 1, 15, 0, 0, 0, DateTimeKind.Utc);
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

        public DateTime BirthDate
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
