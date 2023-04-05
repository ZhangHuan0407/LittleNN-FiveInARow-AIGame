namespace FiveInARow
{
    public struct Vector2Int
    {
        public int X, Y;

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override int GetHashCode() => (X << 16 | Y);
    }
}