namespace PunchingBand
{
    public abstract class ModelBase : ObservableObject
    {
        public bool Set<T>(string name, ref T property, T value, bool raiseSelf)
        {
            if (!Set(name, ref property, value))
            {
                return false;
            }

            if (raiseSelf)
            {
                RaisePropertyChanged("Self");
            }

            return true;
        }
    }
}
