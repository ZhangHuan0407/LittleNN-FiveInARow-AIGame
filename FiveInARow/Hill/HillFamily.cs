using System;
using LittleNN;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace FiveInARow
{
    public static class HillFamily
    {
        private static List<HillSection> m_HillSectionList;

        static HillFamily()
        {
            m_HillSectionList = new List<HillSection>();
            foreach (string filePath in Directory.GetFiles(Defined.ModelDirectory, "*.bin"))
            {
                if (Regex.Match(filePath, "Hill_(?<Age>[0-9]+).bin") is Match match &&
                    match.Success)
                {
                    int age = int.Parse(match.Groups["Age"].Value);
                    m_HillSectionList.Add(new HillSection()
                    {
                        FilePath = filePath,
                        Age = age,
                    });
                }
            }
            m_HillSectionList.Sort();
        }

        public static Hill CloneYoungestHill()
        {
            if (m_HillSectionList.Count > 0)
            {
                HillSection hillSection = m_HillSectionList[m_HillSectionList.Count - 1];
                NeuralNetwork neuralNetwork = NeuralNetwork.LoadFrom(hillSection.FilePath);
                Console.WriteLine("load Hill nn model at: " + hillSection.FilePath);
                Hill hill = new Hill(neuralNetwork, hillSection.Age, true);
                return hill;
            }
            else
            {
                List<Sequential> sequential = Sequential.CreateNew();
                sequential.Add(Sequential.Neural("input layer", Defined.NNInputSize));
                sequential.Add(Sequential.Activation("sigmoid link", ActivationsFunctionType.Sigmoid));
                sequential.Add(Sequential.Neural("hidden layer", Defined.Size * 2));
                sequential.Add(Sequential.Activation("sigmoid link", ActivationsFunctionType.Sigmoid));
                sequential.Add(Sequential.Neural("hidden layer", Defined.Size * 2));
                sequential.Add(Sequential.Activation("sigmoid link", ActivationsFunctionType.Sigmoid));
                sequential.Add(Sequential.Neural("output layer", Defined.Size));
                NeuralNetwork neuralNetwork = new NeuralNetwork(sequential, 0.02f, 0.75f);
                Console.WriteLine("create new Hill nn model");
                Hill hill = new Hill(neuralNetwork, 0, true);
                return hill;
            }
        }
        public static IController CloneHillOpponent()
        {
            if (m_HillSectionList.Count > 0)
            {
                HillSection hillSection = m_HillSectionList[m_HillSectionList.Count - 1];
                NeuralNetwork neuralNetwork = NeuralNetwork.LoadFrom(hillSection.FilePath);
                Console.WriteLine("load Hill opponent nn model at: " + hillSection.FilePath);
                Hill hill = new Hill(neuralNetwork, hillSection.Age, false);
                return hill;
            }
            else
            {
                Silly silly = new Silly();
                if (!silly.TryRecall())
                    throw new Exception("miss Silly...");
                Console.WriteLine("load Hill opponent Silly");
                return silly;
            }
        }
        public static void HillGrowUp(Hill hill)
        {
            int age = hill.Age + 1;
            hill.NeuralNetwork.SaveTo($"{Defined.ModelDirectory}/Hill_{age}.bin");
        }
    }
}