using AForge.Neuro;
using System;

namespace PunchingBand.Portable.Neuro
{
    public class JsonNeuron : Neuron
    {
        public JsonNeuron()
            : base(0)
        {
        }

        public JsonNeuron(int inputs)
            : base(inputs)
        {
        }

        public override double Compute(double[] input)
        {
            throw new NotImplementedException();
        }

        public new int InputsCount
        {
            get { return inputsCount; }
            set { inputsCount = value; }
        }

        public new double Output
        {
            get { return output; }
            set { output = value; }
        }

        public new double[] Weights
        {
            get { return weights; }
            set { weights = value; }
        }

        public double Threshold { get; set; }
    }
}
