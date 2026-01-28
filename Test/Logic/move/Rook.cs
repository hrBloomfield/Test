namespace Game.Logic
{
    public class Rook : Move
    {
        private bool isWhite;

        public Rook(bool isWhite)
        {
            this.isWhite = isWhite;
        }

        public List<moveInfo> GenerateLegalMoves(int[] board, int currentPos)
        {
            int[] directions = { moveUp, moveDown, moveRight, moveLeft };
            return SlideMoves(board, currentPos, directions, isWhite);
        }
    }
}