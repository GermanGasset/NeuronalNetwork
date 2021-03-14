using System;
using System.Collections.Generic;
using System.Text;

namespace Neuronal_Network
{
    public partial class NeuronalNetwork
    {
        /// <summary>
        /// Change every weight by a random positive or negative amount
        /// Useful to train the Network using Genetic Algorithm
        /// </summary>
        /// <param name="maxVariation">Maximum change in weights. Doesn't matter if its possitive</param>
        public void VariateWeights(double maxVariation)
        {
            if (maxVariation != 0)
                foreach (Layer layer in layers)
                    foreach (var neuron in layer.neurons)
                    {
                        bool isPositive = Convert.ToBoolean(random.Next(2));
                        double variation = maxVariation * random.NextDouble();
                        if (!isPositive)
                            variation *= -1;
                        neuron.weight += variation;
                    }
        }

        public double CostFunction()
        {//TODO: implement cost function.
            throw new NotImplementedException();

            double output;


            int sumatory(int startingNumber, int lastNumber)
            {
                for (int i = lastNumber - startingNumber; i <= lastNumber; i++)
                    startingNumber += startingNumber + i;
                return startingNumber;
            }
            return 0;
        }
    }
}
