using LittleNN;
using System;
using System.Collections.Generic;

namespace FiveInARow
{
    public class Hill : IController
    {
        public const float BelieveSelf = 0.2f;
        public const float BelieveRuleAllow = 0.1f;

        public ChessType ChessType { get; set; }
        public float LastLoss;
        private NeuralNetwork m_NeuralNetwork;
        private List<Vector2Int> m_PositionList;
        private Random m_Random;
        private GameLogic m_Notebook;

        public Hill()
        {
            m_PositionList = new List<Vector2Int>();
            m_Random = new Random();
            m_Notebook = new GameLogic();
        }

        public void ClearMemory()
        {
            List<Sequential> sequential = Sequential.CreateNew();
            sequential.Add(Sequential.Neural("input layer", Defined.Size));
            sequential.Add(Sequential.Activation("linear link", ActivationsFunctionType.LeakyReLU));
            sequential.Add(Sequential.Neural("hidden layer", 500));
            sequential.Add(Sequential.Activation("sigmoid link", ActivationsFunctionType.Sigmoid));
            sequential.Add(Sequential.Neural("hidden layer", 500));
            sequential.Add(Sequential.Activation("sigmoid link", ActivationsFunctionType.Sigmoid));
            sequential.Add(Sequential.Neural("hidden layer", 500));
            sequential.Add(Sequential.Activation("sigmoid link", ActivationsFunctionType.Sigmoid));
            sequential.Add(Sequential.Neural("output layer", Defined.Size));
            m_NeuralNetwork = new NeuralNetwork(Defined.Size, new int[] { 500, 500, 500 }, Defined.Size);
        }
        public void TryRecallOrClearMemory()
        {
            string path = Path.Combine(Defined.ModelDirectory, "hill.bin");
            if (File.Exists(path))
                m_NeuralNetwork = NeuralNetwork.LoadFrom(path);
            else
                ClearMemory();
        }
        public void SaveMemory()
        {
            m_NeuralNetwork.SaveTo(Path.Combine(Defined.ModelDirectory, "hill.bin"));
        }

        public void Play(GameLogic gameLogic, out Vector2Int position)
        {
            position = new Vector2Int(-1, -1); // just for compile...
            float[] chessboard = gameLogic.ConvertToNNFormat(ChessType);
            float[] hillEvaluation = m_NeuralNetwork.Forward(chessboard);
            float maxEvaluation = 0f;
            for (int i = 0; i < hillEvaluation.Length; i++)
            {
                if (hillEvaluation[i] < maxEvaluation)
                    continue;
                int row = i / Defined.Width;
                int column = i % Defined.Height;
                if (gameLogic.Chessboard[row, column] != ChessType.Empty)
                    continue;
                maxEvaluation = hillEvaluation[i];
                position.X = column;
                position.Y = row;
            }

            if (maxEvaluation >= BelieveSelf)
            {
                m_PositionList.Clear();
                for (int i = 0; i < chessboard.Length; i++)
                {
                    int row = i / Defined.Width;
                    int column = i % Defined.Height;
                    if (gameLogic.Chessboard[row, column] == ChessType.Empty)
                        m_PositionList.Add(new Vector2Int(column, row));
                }
                position = m_PositionList[m_Random.Next() % m_PositionList.Count];
            }
        }
        public void GameEnd(GameLogic gameLogic)
        {
            m_Notebook.Copy(gameLogic);
            Vector2Int opponentChessPosition;
            ChessType opponentChessType;
            float fixedValue;
            // If Hill win, she will learn to avoid opponent's mistakes
            if (gameLogic.Winner == ChessType)
            {
                m_Notebook.Repentance(out Vector2Int _, out ChessType _);
                m_Notebook.Repentance(out opponentChessPosition, out opponentChessType);
                fixedValue = Defined.AbortValue;
            }
            // If Hill lose, she will learn opponent's the last action
            else if (gameLogic.Winner == gameLogic.ToOpponent(this).ChessType)
            {
                m_Notebook.Repentance(out opponentChessPosition, out opponentChessType);
                fixedValue = Defined.PickValue;
            }
            else
                return;

            float[] opponentPerspective = m_Notebook.ConvertToNNFormat(opponentChessType);
            float[] hillEvaluation = m_NeuralNetwork.Forward(opponentPerspective);
            float[] hillEvaluationCopy = new float[hillEvaluation.Length];
            Array.Copy(hillEvaluation, hillEvaluationCopy, hillEvaluation.Length);
            for (int i = 0; i < hillEvaluation.Length; i++)
            {
                // check this position is allowed by game rule
                if (hillEvaluation[i] > BelieveRuleAllow)
                {
                    int row = i / Defined.Width;
                    int column = i % Defined.Height;
                    if (gameLogic.Chessboard[row, column] != ChessType.Empty)
                        hillEvaluation[i] = 0f;
                }
            }
            hillEvaluation[opponentChessPosition.Y * Defined.Width + opponentChessPosition.X] = fixedValue;
            LastLoss = LossFuntion.MSELoss(hillEvaluationCopy, hillEvaluation);
            m_NeuralNetwork.OptimizerBackward(hillEvaluation);
            m_NeuralNetwork.OptimizerStep();
        }
    }
}