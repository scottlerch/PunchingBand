using PunchingBand.Infrastructure;
using System;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace PunchingBand.Pages.UserControls
{
    public sealed partial class CountDown : INotifyPropertyChanged
    {
        private readonly DispatcherTimer timer;
        private readonly SoundEffect beepSound;
        private int count = 3;

        public CountDown()
        {
            InitializeComponent();

            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                DataContext = this;
                beepSound = new SoundEffect("Assets/countdownbeep.wav");

                timer = new DispatcherTimer();
                timer.Tick += TimerOnTick;
                timer.Interval = TimeSpan.FromSeconds(1);
            }
        }

        public void Start()
        {
            Count = 3;

            timer.Start();

            beepSound.Play(0.08);
            CountDownStoryboard.Begin();
        }

        public event EventHandler CountDownFinished = delegate { }; 

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public int Count
        {
            get { return count; }
            set
            {
                if (count != value)
                {
                    count = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Count"));
                }
            }
        }

        private void TimerOnTick(object sender, object o)
        {
            if (Count == 1)
            {
                timer.Stop();
                CountDownFinished(this, EventArgs.Empty);
            }
            else
            {
                beepSound.Play(0.08);
                Count--;
                CountDownStoryboard.Begin();
            }
        }
    }
}
