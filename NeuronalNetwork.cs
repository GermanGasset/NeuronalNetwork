using System;
using System.IO;

namespace Neuronal_Network
{
    public partial class NeuronalNetwork
    {
        private readonly Random random = new Random();
        public Layer[] layers;
        public int NeuronCount;

        /// <summary>
        /// Instanciate a new network.
        /// </summary>
        /// <param name="neuronsPerLayer">An array that represents the number of neurons in hidden layers and the last value represents the neurons in the output layer</param>
        /// <param name="neuronWeights">Set an specific set of weights or they will be set randomly between -1 and 1</param>
        public NeuronalNetwork(int[] neuronsPerLayer, double[] neuronWeights = null)
        {
            layers = GenerateLayersFromWeigths(neuronsPerLayer, neuronWeights);
        }

        /// <param name="weights">Null for random</param>
        public Layer[] GenerateLayersFromWeigths(int[] neuronsPerLayer, double[] weights = null)
        {
            if (neuronsPerLayer.Length < 1)
                throw new Exception("A network needs at least an output layer");
            for (int i = 0; i < neuronsPerLayer.Length; i++)
                if (neuronsPerLayer[i] < 1)
                    throw new Exception("Impossible to assing 0 or negative number of neurons");

            int assignedNeurons = 0;
            Layer[] output = new Layer[neuronsPerLayer.Length];
            NeuronCount = 0;
            for (int i = 0; i < neuronsPerLayer.Length; i++)
                NeuronCount += neuronsPerLayer[i];

            for (int i = 0; i < output.Length; i++)
            {
                double[] layerWeights = null;
                if (weights != null)
                {
                    layerWeights = new double[neuronsPerLayer[i]];
                    for (int ii = 0; ii < layerWeights.Length; ii++)
                    {
                        try
                        {
                            layerWeights[ii] = weights[ii + assignedNeurons];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            layerWeights[ii] = double.PositiveInfinity;
                        }
                    }
                }
                output[i] = new Layer(i, neuronsPerLayer[i], this, layerWeights);
                assignedNeurons += neuronsPerLayer[i];
            }
            return output;
        }

        /// <param name="optionalInputweigths">Give focus to certain neurons</param>
        public double[] ExecuteNetwork(double[] input, double[] optionalInputweigths = null)
        {
            double[] output;

            if (optionalInputweigths == null)
                output = input;
            else
            {
                for (int i = 0; i < Math.Min(optionalInputweigths.Length, input.Length); i++)
                    input[i] *= optionalInputweigths[i];
                output = input;
            }

            for (int i = 0; i < layers.Length; i++)
                output = layers[i].ExecuteLayer(output);

            return output;
        }

        public double[] ExecuteNetworkWithPercentagedOutput(double[] input, double[] optionalInputWeights = null)
            => GetValuesInPercentages(ExecuteNetwork(input, optionalInputWeights));

        /// <summary>
        /// Executes the network and gets the index of the neuron with highestValue
        /// </summary>
        public int GetPredictionIndex(double[] input, double[] optionalInputWeights = null)
            => GetBiggestIndex(ExecuteNetwork(input, optionalInputWeights));

        private int GetBiggestIndex(double[] input)
        {
            int output = -1;
            double maxValue = double.MinValue;
            for (int i = 0; i < input.Length; i++)
                if (input[i] > maxValue)
                {
                    output = i;
                    maxValue = input[i];
                }
            return output;
        }


        /// <summary>
        /// Get each neuron value percetage. Note: Its really useful to easily read outputs
        /// </summary>
        private double[] GetValuesInPercentages(double[] neuronsValue)
        {
            double[] output = new double[neuronsValue.Length];
            double oneHundredPercentEquivalent = 0;

            foreach (double value in neuronsValue)
                oneHundredPercentEquivalent += value;
            for (int i = 0; i < output.Length; i++)
                output[i] = neuronsValue[i] * 100 / oneHundredPercentEquivalent;
            return output;
        }

        static public double[] StringToDoubleArray(string s)
        {
            string[] rawValues = s.Split("\n".ToCharArray());
            double[] parsedValues = new double[rawValues.Length];
            for (int i = 0; i < rawValues.Length; i++)
                parsedValues[i] = Convert.ToDouble(rawValues[i]);
            return parsedValues;
        }

        static public string DoubleArrayToString(double[] array)
        {
            string output = string.Empty;
            for (int i = 0; i < array.Length; i++)
                output += array[i].ToString() + "\n";
            return output;
        }

        public void WriteWeightsToFile(string path)
        {
            /*if (!path.EndsWith(".txt"))
                path += ".txt";
            if (!path.EndsWith("\\"))
                path += "weights.txt";*/
            if (Directory.Exists(path))
                path += @"\weights.txt";
            FileStream fileStream = File.Create(path);
            StreamWriter writer = new StreamWriter(fileStream);
            writer.Write(GetNetworkweightsString());
            writer.Close();
            fileStream.Close();
        }

        static public double[] ReadWeightsFromFile(string path)
        {
            StreamReader reader;
            try
            {
                reader = File.OpenText(path);
                double[] output = StringToDoubleArray(reader.ReadToEnd());
                reader.Close();
                return output;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string GetNetworkweightsString()
        {
            string output = string.Empty;
            foreach (Layer layer in layers)
                output += layer.GetLayerNeuronWeightsString();

            return output.Remove(output.Length - 1);
        }

        public double[] GetNetworkWeights() => StringToDoubleArray(GetNetworkweightsString());






        public class Layer
        {
            internal NeuronalNetwork parentNetwork;
            internal Layer previousLayer;
            internal Neuron[] neurons;
            public double bias = 1;
            public readonly int layerIndex, length;
            public readonly bool isInputLayer, isOutputLayer;

            internal Layer(int layerIndex, int neurons, NeuronalNetwork parentNetwork, double[] neuronWeights = null)
            {
                this.parentNetwork = parentNetwork;
                this.neurons = new Neuron[length = neurons];
                this.layerIndex = layerIndex;
                isInputLayer = layerIndex == 0;
                isOutputLayer = layerIndex == parentNetwork.layers.Length - 1;
                if (!isInputLayer)
                    previousLayer = parentNetwork.layers[layerIndex - 1];
                if (neuronWeights == null)
                    for (int i = 0; i < length; i++)
                        this.neurons[i] = new Neuron(i, this);
                else
                    for (int i = 0; i < length; i++)
                        this.neurons[i] = new Neuron(i, this, neuronWeights[i]);
            }

            internal double[] ExecuteLayer(double[] input)
            {
                double[] output = new double[neurons.Length];
                for (int i = 0; i < neurons.Length; i++)
                    for (int ii = 0; ii < input.Length; ii++)
                        output[i] += Neuron.TanhActivation(neurons[i].LinearFunction(input[ii]));
                return output;
            }

            public string GetLayerNeuronWeightsString()
            {
                string weights = string.Empty;
                foreach (Neuron neuron in neurons)
                    weights += neuron.weight.ToString() + "\n";
                return weights;
            }

            public double[] GetLayerNeuronWeights()
            {
                double[] weights = new double[neurons.Length];
                for (int i = 0; i < neurons.Length; i++)
                    weights[i] = neurons[i].weight;
                return weights;
            }

            public void SetLayerNeuronWeights(double[] weights)
            {
                for (int i = 0; length >= weights.Length ? length > i : weights.Length > i; i++)
                    neurons[i].weight = weights[i];
            }

            public void SetLayerNeuronWeights(string weights)
            {
                string[] rawValues = weights.Split("\n".ToCharArray());
                double[] parsedValues = new double[rawValues.Length];
                for (int i = 0; i < rawValues.Length; i++)
                    parsedValues[i] = Convert.ToDouble(rawValues[i]);
                SetLayerNeuronWeights(parsedValues);
            }






            internal class Neuron
            {
                internal Layer parentLayer;
                internal double weight;
                internal int neuronIndex;

                internal Neuron(int neuronIndex, Layer parentLayer, double weight = double.PositiveInfinity)
                {
                    this.neuronIndex = neuronIndex;
                    if (!double.IsPositiveInfinity(weight))
                        this.weight = weight;
                    else
                    {
                        double rDouble = parentLayer.parentNetwork.random.NextDouble();
                        int r = parentLayer.parentNetwork.random.Next(-1, 1);
                        this.weight = r + (-rDouble * Convert.ToByte(r >= 0)) + (rDouble * Convert.ToByte(r < 0));
                    }
                    this.parentLayer = parentLayer;
                }

                internal double LinearFunction(double input)
                {
                    //int numberOfInputs = parentLayer.parentNetwork.layers[parentLayer.layerIndex - 1].neurons.Length;
                    return input * weight + parentLayer.bias;
                }

                public static double ReLUActivation(double input) => Math.Max(0, input);

                public static double SigmoidActivation(double input) => 1.0 / (1 + Math.Exp(-input));

                public static double TanhActivation(double input) => (Math.Exp(input) - Math.Exp(-input)) / (Math.Exp(input) + Math.Exp(-input));

            }
        }
    }
}
