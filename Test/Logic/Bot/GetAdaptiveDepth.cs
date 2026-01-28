namespace Game.Logic.Bot
{
    public class GetAdaptiveDepth
    {
        public static int getAdaptiveDepth(Board board, int moveCount)
        {
            int material = CalculateMaterial.calculateMaterial(board);
            int depth = Kenith.MAX_DEPTH;


            if (material <= 1500)
                depth += 2;
            else if (material <= 2500)
                depth += 1;


            if (moveCount <= 10)
                depth += 2;

            return Math.Min(depth, Kenith.MAX_DEPTH + 2);
        }
    }
}