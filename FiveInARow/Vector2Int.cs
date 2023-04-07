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
        public override string ToString() => $"X: {X},  Y: {Y}";

        public static bool operator ==(Vector2Int l, Vector2Int r)
        {
            return l.X == r.X && l.Y == r.Y;
        }
        public static bool operator !=(Vector2Int l, Vector2Int r)
        {
            return l.X != r.X || l.Y != r.Y;
        }
    }
}