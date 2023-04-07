namespace FiveInARow
{
    public class SillyOneStep : OneStep
    {
        public bool Mistake;
        public bool Random;

        public SillyOneStep(Vector2Int position, bool mistake, bool random) : base(position)
        {
            Mistake = mistake;
            Random = random;
        }

        public override string ToString() => $"Silly Mistake: {Mistake}, Random: {Random}, Position: {Position}";
    }
}