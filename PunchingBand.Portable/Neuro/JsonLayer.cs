using AForge.Neuro;

namespace PunchingBand.Portable.Neuro
{
    public class JsonLayer : Layer
    {
        public JsonLayer()
            : base(0, 0)
        {
        }

        public JsonLayer(int neuronsCount, int inputsCount)
            : base(neuronsCount, inputsCount)
        {
            neurons = new JsonNeuron[neuronsCount];
        }

        public new int InputsCount
        {
            get { return inputsCount; }
            set { inputsCount = value; }
        }

        public new JsonNeuron[] Neurons
        {
            get { return neurons as JsonNeuron[]; }
            set { neurons = value; }
        }

        public new double[] Output
        {
            get { return output; }
            set { output = value; }
        }
    }
}
