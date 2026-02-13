using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmExtensions
{
    public static class ProbabilitiesExpWeight
    {
        private static Random random = new Random();

        /// <summary>
        /// Select an object with probability based on exponential weights.
        /// </summary>
        /// <typeparam name="T">Type of objects to select from</typeparam>
        /// <param name="objects">Array of N objects to select from</param>
        /// <param name="weights">Array of N weights corresponding to each object</param>
        /// <param name="temperature">Temperature parameter for softmax (default=1.0)</param>
        /// <returns>The selected object based on exponential probability</returns>
        public static T SelectByExponentialWeight<T>(this T[] objects, double[] weights, double temperature = 1.0)
        {
            if (objects == null || weights == null)
                throw new ArgumentNullException("objects and weights cannot be null");

            if (objects.Length != weights.Length)
                throw new ArgumentException("objects and weights must have the same length");

            if (objects.Length == 0)
                throw new ArgumentException("objects array cannot be empty");

            // Calculate exponential weights
            double[] expWeights = new double[weights.Length];
            double sum = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                expWeights[i] = Math.Exp(weights[i] / temperature);
                sum += expWeights[i];
            }

            // Calculate probabilities (normalize)
            double[] probabilities = new double[weights.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                probabilities[i] = expWeights[i] / sum;
            }

            // Select based on probabilities
            double randomValue = random.NextDouble();
            double cumulative = 0;

            for (int i = 0; i < probabilities.Length; i++)
            {
                cumulative += probabilities[i];
                if (randomValue <= cumulative)
                    return objects[i];
            }

            // Fallback (should rarely happen due to floating point precision)
            return objects[objects.Length - 1];
        }

        /// <summary>
        /// Select an object with probability based on exponential weights.
        /// </summary>
        /// <typeparam name="T">Type of objects to select from</typeparam>
        /// <param name="weightedObjects">Dictionary mapping objects to their weights</param>
        /// <param name="temperature">Temperature parameter for softmax (default=1.0)</param>
        /// <returns>The selected object based on exponential probability</returns>
        public static T SelectByExponentialWeight<T>(this IDictionary<T, double> weightedObjects, double temperature = 1.0)
        {
            if (weightedObjects == null)
                throw new ArgumentNullException(nameof(weightedObjects));
            if (weightedObjects.Count == 0)
                throw new ArgumentException("Dictionary cannot be empty");

            // Calculate exponential weights and sum
            var expWeights = new Dictionary<T, double>();
            double sum = 0;

            foreach (var kvp in weightedObjects)
            {
                double expWeight = Math.Exp(kvp.Value / temperature);
                expWeights[kvp.Key] = expWeight;
                sum += expWeight;
            }

            // Select based on normalized probabilities
            double randomValue = random.NextDouble();
            double cumulative = 0;

            foreach (var kvp in expWeights)
            {
                cumulative += kvp.Value / sum;
                if (randomValue <= cumulative)
                    return kvp.Key;
            }

            // Fallback (should rarely happen due to floating point precision)
            return expWeights.Keys.Last();
        }

        /// <summary>
        /// Select an object with probability based on weights.
        /// </summary>
        /// <typeparam name="T">Type of objects to select from</typeparam>
        /// <param name="weightedObjects">Dictionary mapping objects to their weights</param>
        /// <returns>The selected object based on probability proportional to weights</returns>
        public static T SelectByWeight<T>(this IDictionary<T, double> weightedObjects)
        {
            if (weightedObjects == null)
                throw new ArgumentNullException(nameof(weightedObjects));
            if (weightedObjects.Count == 0)
                throw new ArgumentException("Dictionary cannot be empty");

            // Calculate sum of weights
            double sum = weightedObjects.Values.Sum();

            if (sum <= 0)
                throw new ArgumentException("Sum of weights must be positive");

            // Select based on normalized probabilities
            double randomValue = random.NextDouble() * sum;
            double cumulative = 0;

            foreach (var kvp in weightedObjects)
            {
                cumulative += kvp.Value;
                if (randomValue <= cumulative)
                    return kvp.Key;
            }

            // Fallback (should rarely happen due to floating point precision)
            return weightedObjects.Keys.Last();
        }

        /// <summary>
        /// Select multiple objects to demonstrate the probability distribution.
        /// </summary>
        public static List<T> SelectMultiple<T>(T[] objects, double[] weights, int nSamples = 10, double temperature = 1.0)
        {
            List<T> selections = new List<T>();
            for (int i = 0; i < nSamples; i++)
            {
                selections.Add(SelectByExponentialWeight(objects, weights, temperature));
            }
            return selections;
        }

        /// <summary>
        /// Calculate explicit probabilities for each object.
        /// </summary>
        public static double[] CalculateProbabilities(double[] weights, double temperature = 1.0)
        {
            double[] expWeights = new double[weights.Length];
            double sum = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                expWeights[i] = Math.Exp(weights[i] / temperature);
                sum += expWeights[i];
            }

            double[] probabilities = new double[weights.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                probabilities[i] = expWeights[i] / sum;
            }

            return probabilities;
        }

    }
}
