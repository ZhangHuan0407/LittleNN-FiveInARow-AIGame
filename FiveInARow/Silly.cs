﻿using LittleNN;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FiveInARow
{
    public class Silly : IController
    {
        public ChessType ChessType { get; set; }
        private int m_TrainTimes;
        private float[] m_SillyEvaluationCopy;
        public float LastLoss;
        public NeuralNetwork NeuralNetwork;
        public readonly GameLogic Notebook;
        private List<Vector2Int> m_PositionList;

        public Silly()
        {
            Notebook = new GameLogic();
            m_SillyEvaluationCopy = new float[Defined.Size];
            m_PositionList = new List<Vector2Int>();
        }

        public void TryRecall()
        {
            string path = Path.Combine(Defined.ModelDirectory, "Silly.bin");
            if (File.Exists(path))
            {
                NeuralNetwork = NeuralNetwork.LoadFrom(path);
                Console.WriteLine("load nn model at: " + path);
            }
            else
            {
                List<Sequential> sequential = Sequential.CreateNew();
                sequential.Add(Sequential.Neural("input layer", Defined.NNInputSize));
                sequential.Add(Sequential.Activation("linear link", ActivationsFunctionType.LeakyReLU));
                sequential.Add(Sequential.Neural("hidden layer", Defined.Size * 2));
                sequential.Add(Sequential.Activation("sigmoid link", ActivationsFunctionType.Sigmoid));
                sequential.Add(Sequential.Neural("hidden layer", Defined.Size * 2));
                sequential.Add(Sequential.Activation("sigmoid link", ActivationsFunctionType.Sigmoid));
                sequential.Add(Sequential.Neural("output layer", Defined.Size));
                NeuralNetwork = new NeuralNetwork(sequential, 0.02f, 0.75f);
                Console.WriteLine("create new nn model");
            }
        }
        public void SaveMemory()
        {
            NeuralNetwork.SaveTo(Path.Combine(Defined.ModelDirectory, "Silly.bin"));
            m_TrainTimes = 0;
        }

        public void LogEvaluation(GameLogic gameLogic, StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"Evaluation {ChessType}");
            stringBuilder.Append("     ");
            for (int column = 0; column < Defined.Width; column++)
                stringBuilder.Append(column).Append("   ");
            stringBuilder.AppendLine();
            float[] chessboard = gameLogic.ConvertToNNFormat(ChessType);
            float[] evaluation = NeuralNetwork.Forward(chessboard);
            ArrayBuffer.Revert(chessboard);
            for (int i = 0; i < evaluation.Length; i++)
            {
                int row = i / Defined.Width;
                int column = i % Defined.Width;
                if (column == 0)
                    stringBuilder.Append(row).Append("  ");
                float pValue = evaluation[i];
                if (pValue < Defined.AIChooseValue)
                {
                    string content = ((int)MathF.Floor(pValue * 10f)).ToString();
                    stringBuilder.Append(("." + content).PadLeft(3));
                }
                else
                {
                    string content = ((int)MathF.Floor(pValue * 100f)).ToString();
                    stringBuilder.Append(content.PadLeft(3));
                }
                stringBuilder.Append(" ");
                if (column == Defined.Width - 1)
                    stringBuilder.AppendLine();
            }
        }

        public void Play(GameLogic gameLogic, out Vector2Int position)
        {
            if (Defined.Random.NextDouble() < Defined.SillyMakeMistake)
            {
                position = gameLogic.RandomPickEmptyPosition();
                return;
            }
            float[] chessboard = gameLogic.ConvertToNNFormat(ChessType);
            float[] sillyEvaluation = NeuralNetwork.Forward(chessboard);
            ArrayBuffer.Revert(chessboard);
            m_PositionList.Clear();
            for (int i = 0; i < sillyEvaluation.Length; i++)
            {
                if (sillyEvaluation[i] < Defined.AIChooseValue)
                    continue;
                int row = i / Defined.Width;
                int column = i % Defined.Width;
                if (gameLogic.Chessboard[row, column] != ChessType.Empty)
                    continue;
                m_PositionList.Add(new Vector2Int(column, row));
            }

            if (m_PositionList.Count > 0)
                position = m_PositionList[Defined.Random.Next() % m_PositionList.Count];
            else
                position = gameLogic.RandomPickEmptyPosition();
        }
        public void LearnLastStep()
        {
            Notebook.Repentance(out Vector2Int chessPosition, out ChessType chessType);
            float[] chessboard = Notebook.ConvertToNNFormat(chessType);
            float[] sillyEvaluation = NeuralNetwork.Forward(chessboard);
            ArrayBuffer.Revert(chessboard);
            Array.Copy(sillyEvaluation, m_SillyEvaluationCopy, sillyEvaluation.Length);
            for (int i = 0; i < sillyEvaluation.Length; i++)
            {
                // check this position is allowed by game rule
                if (sillyEvaluation[i] > Defined.AIBelieveRuleAllow)
                {
                    int row = i / Defined.Width;
                    int column = i % Defined.Width;
                    if (Notebook.Chessboard[row, column] != ChessType.Empty)
                        sillyEvaluation[i] = (Defined.AIAbortValue + sillyEvaluation[i]) / 2f;
                }
            }
            float sillyP = sillyEvaluation[chessPosition.Y * Defined.Width + chessPosition.X];
            if (sillyP < Defined.AIChooseValue)
                sillyEvaluation[chessPosition.Y * Defined.Width + chessPosition.X] = (sillyP + Defined.PickValue) / 2f;
            LastLoss = LossFuntion.MSELoss(m_SillyEvaluationCopy, sillyEvaluation);
            NeuralNetwork.OptimizerBackward(sillyEvaluation);
            NeuralNetwork.OptimizerStep();
            if (m_TrainTimes++ > 30000)
                SaveMemory();
        }
        public void GameEnd(GameLogic gameLogic)
        {
        }
    }
}