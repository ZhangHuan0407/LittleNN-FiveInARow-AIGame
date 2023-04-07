namespace FiveInARow
{
    public interface IController
    {
        ChessType PlayerChessType { get; set; }
        void Play(GameLogic gameLogic, out OneStep oneStep);
        void GameEnd(GameLogic gameLogic);
    }
}