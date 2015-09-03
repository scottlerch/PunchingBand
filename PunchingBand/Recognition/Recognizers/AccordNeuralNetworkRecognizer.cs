using Accord.Math;
using AForge.Neuro;
using Microsoft.Band.Portable.Sensors;
using Newtonsoft.Json;
using PunchingBand.Band;
using PunchingBand.Recognition.Neuro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PunchingBand.Recognition.Recognizers
{
    public class AccordNeuralNetworkRecognizer : IPunchRecognizer
    {
        private Network network;
        private readonly Func<string, Task<Stream>> readFile;

        public AccordNeuralNetworkRecognizer(Func<string, Task<Stream>> readFile)
        {
            this.readFile = readFile;
        }

        public async Task Initialize(FistSides fistSide)
        {
            using (var stream = await readFile("Assets/NeuralNetworks/Punches" + fistSide + "Fist.json"))
            using (var streamReader = new StreamReader(stream))
            {
                var jsonNetwork = new JsonNetwork();
                var serializer = new JsonSerializer();
                serializer.Populate(streamReader, jsonNetwork);

                network = jsonNetwork.CreateActivationNetwork();
            }
        }

        public async Task<PunchRecognition> Recognize(IEnumerable<GyroscopeAccelerometerReading> readings)
        {
            if (network == null)
            {
                throw new InvalidOperationException("Neural network has not been initialized.");
            }

            var inputs = readings.SelectMany(r => new[] { r.AccelerationX, r.AccelerationY, r.AccelerationZ, r.AngularVelocityX, r.AngularVelocityY, r.AngularVelocityZ, }).ToArray();

            if (inputs.Length != network.InputsCount)
            {
                return await Task.FromResult(new PunchRecognition(PunchType.Unknown, 0, TimeSpan.Zero));
            }

            var sw = Stopwatch.StartNew();
            var answer = network.Compute(inputs);
            int actual; answer.Max(out actual);
            sw.Stop();

            return await Task.FromResult(new PunchRecognition((PunchType)actual, answer[actual], sw.Elapsed));
        }
    }
}
