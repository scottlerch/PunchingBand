using Accord.Math;
using Accord.Neuro;
using Accord.Neuro.Learning;
using AForge.Neuro;
using Newtonsoft.Json;
using PunchingBand.Portable.Neuro;
using System;
using System.IO;
using System.Linq;

namespace PunchingBand.MachineLearning
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var lines = File.ReadAllLines(@"C:\Users\Scott\Documents\GitHub\PunchingBand\punchdata\PunchVectorsRight.csv").Skip(1);
            var splitLines = lines.Select(line => line.Split(',')).ToList();
            var categories = new[] { "Jab", "UpperCut", "Hook" };
            var categoryIndices = categories.Select((category, index) => new { category, index }).ToDictionary(c => c.category, c => c.index);

            double[][] inputs = splitLines.Select(line => line.Skip(1).Select(double.Parse).ToArray()).ToArray();
            int[] classes = splitLines.Select(line => categoryIndices[line[0]]).ToArray();

            // First we have to convert this problem into a way that  the neural
            // network can handle. The first step is to expand the classes into 
            // indicator vectors, where a 1 into a position signifies that this
            // position indicates the class the sample belongs to.
            double[][] outputs = Accord.Statistics.Tools.Expand(classes, -1, +1);

            // Create an activation function for the net
            var function = new BipolarSigmoidFunction();

            // Create an activation network with the function and
            //  4 inputs, 5 hidden neurons and 3 possible outputs:
            var network = new ActivationNetwork(function, inputs[0].Length, 100, categories.Length);

            // Randomly initialize the network
            new NguyenWidrow(network).Randomize();

            // Teach the network using parallel Rprop:
            var teacher = new ParallelResilientBackpropagationLearning(network);

            double error = 1.0;
            while (error > 1e-5)
                error = teacher.RunEpoch(inputs, outputs);

            Console.WriteLine("Model trained!");

            // Checks if the network has learned
            for (int i = 0; i < inputs.Length; i++)
            {
                double[] answer = network.Compute(inputs[i]);

                int expected = classes[i];
                int actual; answer.Max(out actual);

                // actual should be equal to expected
                Console.WriteLine("Expected: {0}  Actual: {1}", expected, actual);
            }

            network.Save(@"C:\Users\Scott\Documents\GitHub\PunchingBand\punchdata\PunchRecognitionNetwork.dat");

            var networkJson = JsonConvert.SerializeObject(network);

            File.WriteAllText(@"C:\Users\Scott\Documents\GitHub\PunchingBand\punchdata\PunchRecognitionNetwork.json", networkJson);

            Console.WriteLine("Model written to disk!");

            Console.WriteLine("Verifying model on disk...");

            var jsonNetwork = new JsonNetwork();

            using (var file = File.OpenRead(@"C:\Users\Scott\Documents\GitHub\PunchingBand\punchdata\PunchRecognitionNetwork.json"))
            using (var streamReader = new StreamReader(file))
            {
                var serializer = new JsonSerializer();
                serializer.Populate(streamReader, jsonNetwork);
            }

            var newNetwork = jsonNetwork.CreateActivationNetwork();

            // Checks if the network has learned
            for (int i = 0; i < inputs.Length; i++)
            {
                double[] answer = newNetwork.Compute(inputs[i]);

                int expected = classes[i];
                int actual; answer.Max(out actual);

                // actual should be equal to expected
                Console.WriteLine("Expected: {0}  Actual: {1}", expected, actual);
            }

            Console.WriteLine("Done!");

            Console.ReadKey();
        }
    }
}
