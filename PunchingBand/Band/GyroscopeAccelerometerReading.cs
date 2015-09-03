using Microsoft.Band.Portable.Sensors;
using System;

namespace PunchingBand.Band
{
    public class GyroscopeAccelerometerReading : IBandSensorReading
    {
        public static readonly GyroscopeAccelerometerReading Empty = new GyroscopeAccelerometerReading();

        public double AngularVelocityX { get; set; }

        public double AngularVelocityY { get; set; }

        public double AngularVelocityZ { get; set; }

        public double AccelerationX { get; set; }

        public double AccelerationY { get; set; }

        public double AccelerationZ { get; set; }

        public System.DateTimeOffset Timestamp { get; set; }

        public GyroscopeAccelerometerReading()
        {
        }

        public GyroscopeAccelerometerReading(BandGyroscopeReading gyroscopeReading, BandAccelerometerReading accelerometerReading)
        {
            if (gyroscopeReading != null)
            {
                AngularVelocityX = gyroscopeReading.AngularVelocityX;
                AngularVelocityY = gyroscopeReading.AngularVelocityY;
                AngularVelocityZ = gyroscopeReading.AngularVelocityZ;
            }

            if (accelerometerReading != null)
            {
                AccelerationX = accelerometerReading.AccelerationX;
                AccelerationY = accelerometerReading.AccelerationY;
                AccelerationZ = accelerometerReading.AccelerationZ;
            }

            Timestamp = DateTimeOffset.UtcNow;
        }
    }
}
