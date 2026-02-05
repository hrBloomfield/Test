namespace Game.Logic
{
    public class Pawn : Move
    {
        private bool isPawnWhite;
        private List<moveInfo> legalMoves = new List<moveInfo>();

        public Pawn(bool isPawnWhite)
        {
            this.isPawnWhite = isPawnWhite;
        }
        


        public List<moveInfo> GenerateLegalMoves(int[] board, int currentPos)
        {
            legalMoves.Clear();
            
            bool IsEnPassantMarker(int piece)
            {
                return isPawnWhite ? piece == Pieces.black * Pieces.enPassantMarker : piece == Pieces.enPassantMarker;
            }

            int direction = isPawnWhite ? moveUp : moveDown;
            
            int startRank = isPawnWhite ? 1 : 6;
            int promotionRank = isPawnWhite ? 7 : 0;
            int enPassantRank = isPawnWhite ? 5 : 4;
            
            int currentFile = currentPos % 8;
            int currentRank = currentPos / 8;
            
            int diaganalLeft = isPawnWhite ? moveUpLeft : moveDownLeft;
            int diaganalRight = isPawnWhite ? moveUpRight : moveDownRight;
            
            int captureLeft = currentPos + diaganalLeft;
            int captureRight = currentPos + diaganalRight;

            bool IsOpponentPiece(int piece)
            {
                return isPawnWhite ? piece < 0 : piece > 0;
            }

            int forwardOne = currentPos + direction;
            if (forwardOne < 0 || forwardOne >= 64) 
                return legalMoves; 
            
            // Move forward one square
            if (board[forwardOne] == Pieces.noPiece)
            {
                int forwardRank = forwardOne / 8;
                
                if (forwardRank == promotionRank)
                    legalMoves.Add(new moveInfo(currentPos, forwardOne, MoveType.Promotion));
                else
                    legalMoves.Add(new moveInfo(currentPos, forwardOne, MoveType.Normal));
            }

            
            //enpassant
            // en passant
            if (currentRank == enPassantRank)
            {
                if (currentFile > 0 && captureLeft >= 0 && captureLeft < 64 && IsEnPassantMarker(board[captureLeft]))
                {
                    legalMoves.Add(new moveInfo(currentPos, captureLeft, MoveType.EnPassant));
                }

                if (currentFile < 7 && captureRight >= 0 && captureRight < 64 && IsEnPassantMarker(board[captureRight]))
                {
                    legalMoves.Add(new moveInfo(currentPos, captureRight, MoveType.EnPassant));
                }
            }


            //promotion
            if (currentFile > 0 && captureLeft >= 0 && captureLeft < 64 && IsOpponentPiece(board[captureLeft]))
            {
                int targetRank = captureLeft / 8;
                
                if (targetRank == promotionRank)
                    legalMoves.Add(new moveInfo(currentPos, captureLeft, MoveType.PromotionCapture));
                else
                    legalMoves.Add(new moveInfo(currentPos, captureLeft, MoveType.Capture));

            }

            if (currentFile < 7 && captureRight >= 0 && captureRight < 64 && IsOpponentPiece(board[captureRight]))
            {
                int targetRank = captureRight / 8;
                
                if (targetRank == promotionRank)
                    legalMoves.Add(new moveInfo(currentPos, captureRight, MoveType.PromotionCapture));
                else
                    legalMoves.Add(new moveInfo(currentPos, captureRight, MoveType.Capture));
            }



            // Double move
            if (currentRank == startRank)
            {
                int moveTwice = currentPos + direction * 2;
                if (moveTwice >= 0 && moveTwice < 64 &&
                    board[currentPos + direction] == Pieces.noPiece &&
                    board[moveTwice] == Pieces.noPiece)
                {
                    legalMoves.Add(new moveInfo(currentPos, moveTwice, MoveType.DoubleMove));
                }
            }
            
            return legalMoves;
        }
    }
}
