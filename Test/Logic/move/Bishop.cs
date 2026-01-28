namespace Game.Logic
{
    public class Bishop : Move
    {
        private bool isWhite;

        public Bishop(bool isWhite)
        {
            this.isWhite = isWhite;
        }

        public List<moveInfo> GenerateLegalMoves(int[] board, int currentPos)
        {
            int[] directions = { moveUpRight, moveUpLeft, moveDownRight, moveDownLeft };
            return SlideMoves(board, currentPos, directions, isWhite);
        }
    }
}
