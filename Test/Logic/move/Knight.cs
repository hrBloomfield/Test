namespace Game.Logic
{
    public class Knight : Move
    {
        private bool isKnightWhite;
        private List<moveInfo> legalMoves = new List<moveInfo>();

        public Knight(bool isKnightWhite)
        {
            this.isKnightWhite = isKnightWhite;
        }

        public List<moveInfo> GenerateLegalMoves(int[] board, int currentPos)
        {
            legalMoves.Clear();
            int[] directions = { -17, -15, -10, -6, 6, 10, 15, 17 };

            int currentRow = currentPos / 8;
            int currentCol = currentPos % 8;

            bool IsOpponentPiece(int piece)
            {
                return isKnightWhite ? piece < Pieces.noPiece : piece > Pieces.noPiece;
            }

            foreach (int dir in directions)
            {
                int pos = currentPos + dir;
                
                if (pos < 0 || pos >= 64)
                    continue;
                int newRow = pos / 8;
                int newCol = pos % 8;
                
                int dRow = Math.Abs(newRow - currentRow);
                int dCol = Math.Abs(newCol - currentCol);
                
                if (!((dRow == 2 && dCol == 1) || (dRow == 1 && dCol == 2)))
                    continue;

                
                int piece = board[pos];

                if (board[pos] == Pieces.noPiece)
                {
                    legalMoves.Add(new moveInfo(currentPos, pos, MoveType.Normal));
                }
                else if (IsOpponentPiece(piece))
                {
                    legalMoves.Add(new moveInfo(currentPos, pos, MoveType.Capture));
                }
            }

            return legalMoves;
            
        }
    }
}
