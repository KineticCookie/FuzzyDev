using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDL.Utility;

namespace FDL.NeuralNetwork
{
    #region Activation functions

    public interface IActivationFunction
    {
        double Function(double value);
        double Derivative(double value);
    }
    public static class ActivationFunctions
    {
        public static readonly LinearFunction Linear = new LinearFunction();
        public static readonly SigmoidFunction Sigmoid = new SigmoidFunction();
    }

    /// <summary>
    /// Linear activation function
    /// </summary>
    public class LinearFunction : IActivationFunction
    {
        public double Function(double value)
        {
            return value;
        }
        public double Derivative(double value)
        {
            return 1;
        }
    }
    /// <summary>
    /// Sigmoid actiation function
    /// </summary>
    public class SigmoidFunction : IActivationFunction
    {
        public double Function(double value)
        {
            return (1 / (1 + Math.Exp(-value)));
        }
        public double Derivative(double value)
        {
            return Function(value) * (1 - Function(value));
        }
    }

    #endregion Activation functions

    #region Neuron

    public class Neuron
    {
        public IActivationFunction Activation;

        public Dictionary<Neuron, double> In;

        public double Charge { get; set; }
        public double Error { get; set; }

        public Neuron(IActivationFunction activationFunc) : this(activationFunc, 0) { }

        public Neuron(IActivationFunction activationFunc, double charge)
        {
            Charge = charge;
            Error = 0;
            Activation = activationFunc;
            In = new Dictionary<Neuron, double>();
        }

        public void ConnectToInput(IEnumerable<Neuron> inputs)
        {
            if (ReferenceEquals(inputs, null))
                throw new ArgumentNullException(nameof(inputs));

            In.Add(new Neuron(Activation, 1), Helper.Rand.NextDouble); //Bias
            foreach (var neuron in inputs)
                In.Add(neuron, Helper.Rand.NextDouble);
        }

        public void ConnectToInput(Neuron input)
        {
            if (ReferenceEquals(input, null))
                throw new ArgumentNullException(nameof(input));
             In.Add(input, Helper.Rand.NextDouble);
        }

        public double Calculate()
        {
            double sum = In.Sum((k) => { return k.Key.Charge * k.Value; });
            Charge = Activation.Function(sum);
            return Charge;
        }

        public void CalculateError(double error) // TODO Only in output layer. 
        {
            Error = error;
            BackPropagnation();
        }

        public void BackPropagnation()
        {
            Error *= Activation.Derivative(Charge);

            foreach (var neuron in In.Keys)
            {
                neuron.Error += Error;
                In[neuron] += Error * neuron.Charge;
            }
            Error = 0;
        }
    }

    #endregion Neuron

    #region NeuralLayer

    public class NeuralLayer
    {
        public List<Neuron> Layer { get; private set; }

        public NeuralLayer(int neuronsQuantity, IActivationFunction activationFunc)
        {
            Layer = new List<Neuron>(neuronsQuantity);
            for (int i = 0; i < neuronsQuantity; i++)
                Layer.Add(new Neuron(activationFunc));
        }
        /// <summary>
        /// Connects target layer neurons to this layer 
        /// </summary>
        /// <param name="inputs">Target layer, that will be connected</param>
        public void ConnectToMe(NeuralLayer inputs)
        {
            if (ReferenceEquals(inputs, null))
                throw new ArgumentNullException(nameof(inputs));
            foreach (var neuron in Layer)
                neuron.ConnectToInput(inputs.Layer);
        }
        public void ConnectToMe(Neuron input)
        {
            if (ReferenceEquals(input, null))
                throw new ArgumentNullException(nameof(input));
            foreach (var item in Layer)
            {
                item.ConnectToInput(input);
            }
        }
        /// <summary>
        /// Calculate all neurons in layer
        /// </summary>
        /// <returns>Array with neurons' charges</returns>
        public double[] Calculate()
        {
            return Layer.Select(n => n.Calculate()).ToArray();
        }
    }

    #endregion NeuralLayer

    #region NeuralNetwork

    public class NeuralNetwork
    {
        NeuralLayer InputNeurons;
        List<NeuralLayer> HiddenLayers;
        NeuralLayer OutputNeurons;

        public NeuralNetwork(IActivationFunction activationFunc, int layersQuantity, int neuronsPerLayer, Neuron bias = null)
        {
            if (ReferenceEquals(bias, null))
            {
                InputNeurons = new NeuralLayer(neuronsPerLayer, activationFunc);
                NeuralLayer prevLayer = InputNeurons;

                HiddenLayers = new List<NeuralLayer>(layersQuantity);
                for (int i = 0; i < layersQuantity; i++)
                {
                    HiddenLayers.Add(new NeuralLayer(neuronsPerLayer, activationFunc));
                    HiddenLayers[i].ConnectToMe(prevLayer);
                    prevLayer = HiddenLayers[i];
                }

                OutputNeurons = new NeuralLayer(neuronsPerLayer, activationFunc);
                OutputNeurons.ConnectToMe(prevLayer);
            }
            else
            {
                InputNeurons = new NeuralLayer(neuronsPerLayer, activationFunc);
                NeuralLayer prevLayer = InputNeurons;

                HiddenLayers = new List<NeuralLayer>(layersQuantity);
                for (int i = 0; i < layersQuantity; i++)
                {
                    HiddenLayers.Add(new NeuralLayer(neuronsPerLayer, activationFunc));
                    HiddenLayers[i].ConnectToMe(prevLayer);
                    HiddenLayers[i].ConnectToMe(bias);
                    prevLayer = HiddenLayers[i];
                }
                OutputNeurons = new NeuralLayer(neuronsPerLayer, activationFunc);
                OutputNeurons.ConnectToMe(prevLayer);
                OutputNeurons.ConnectToMe(bias);
            }

        }

        public double[] Calculate(double[] inputs)
        {
            if (inputs.Length != InputNeurons.Layer.Count)
                throw new ArgumentOutOfRangeException(nameof(inputs), "inputs size must be equal to input neurons quantity");

            int i = 0;
            foreach (var item in InputNeurons.Layer)
            {
                item.Charge = inputs[i++];
            }

            foreach (var item in HiddenLayers)
            {
                item.Calculate();
            }

            HiddenLayers.ForEach(l => l.Calculate());

            return OutputNeurons.Calculate();
        }

        public double Train(double[] inputs, double[] expected)
        {
            double error = 0;
            var answers = Calculate(inputs);

            for (int i = 0; i < expected.Length; i++)
                error += Math.Pow(expected[i] - answers[i], 2);

            for (int i = 0; i < OutputNeurons.Layer.Count; i++)
            {
                double errorNeuron = expected[i] - answers[i];
                OutputNeurons.Layer[i].CalculateError(errorNeuron);
            }

            foreach (var layer in HiddenLayers)
            {
                foreach (var neuron in layer.Layer)
                {
                    neuron.BackPropagnation();
                }
            }
            return error/2;
        }
    }

    #endregion NeuralNetwork

}