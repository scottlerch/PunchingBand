using AForge.Neuro;

namespace PunchingBand.Portable.Neuro
{
    public class JsonNetwork : Network
    {
        public JsonNetwork() : base(0, 0)
        {
        }

        public JsonNetwork(int inputsCount, int layersCount)
            : base(inputsCount, layersCount)
        {
            layers = new JsonLayer[layersCount];
        }

        public new int InputsCount
        {
            get { return inputsCount; }
            set { inputsCount = value; }
        }

        public new JsonLayer[] Layers
        {
            get { return layers as JsonLayer[]; }
            set { layers = value; }
        }

        public new double[] Output
        {
            get { return output; }
            set { output = value; }
        }

        private void Populate(Network network)
        {
            for (var i = 0; i < Layers.Length; i++)
            {
                for (var j = 0; j < Layers[i].Neurons.Length; j++)
                {
                    for (var n = 0; n < Layers[i].Neurons[j].Weights.Length; n++)
                    {
                        network.Layers[i].Neurons[j].Weights[n] = Layers[i].Neurons[j].Weights[n];

                        var activationNeuron = network.Layers[i].Neurons[j] as ActivationNeuron;

                        if (activationNeuron != null)
                        {
                            activationNeuron.Threshold = Layers[i].Neurons[j].Threshold;
                        }
                    }
                }
            }
        }

        public ActivationNetwork CreateActivationNetwork()
        {
            var network = new ActivationNetwork(
                new BipolarSigmoidFunction(),
                InputsCount,
                Layers[0].Neurons.Length,
                Output.Length);

            network.Compute(new double[InputsCount]);

            Populate(network);

            return network;
        }
    }
}
