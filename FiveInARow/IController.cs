namespace FiveInARow
{
    public interface IController
    {
        ChessType ChessType { get; set; }
        void Play(GameLogic gameLogic, out Vector2Int position);
        void GameEnd(GameLogic gameLogic);
    }
}