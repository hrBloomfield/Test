namespace Game.Logic
{
    public class Queen : Move
    {
        private bool isWhite;

        public Queen(bool isWhite)
        {
            this.isWhite = isWhite;
        }

        public List<moveInfo> GenerateLegalMoves(int[] board, int currentPos)
        {
            int[] directions = { moveUp, moveDown, moveRight, moveLeft, moveUpRight, moveUpLeft, moveDownRight, moveDownLeft };
            return SlideMoves(board, currentPos, directions, isWhite);
        }

    }
}
