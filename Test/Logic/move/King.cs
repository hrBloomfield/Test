namespace Game.Logic
{
    public class King : Move
    {
        private bool isKingWhite;
        private List<moveInfo> legalMoves = new List<moveInfo>();

        public King(bool isKingWhite)
        {
            this.isKingWhite = isKingWhite;
        }

       public List<moveInfo> GenerateLegalMoves(int[] board, int currentPos)
        {
            legalMoves.Clear();
            int[] directions =
                { moveDownRight, moveDownLeft, moveUpRight, moveUpLeft, moveUp, moveDown, moveRight, moveLeft };

            bool IsOpponentPiece(int piece)
            {
                return isKingWhite ? piece < Pieces.noPiece : piece > Pieces.noPiece;
            }

            // castling
            int whiteKingStart = 4;
            int blackKingStart = 60;

            if (currentPos == (isKingWhite ? whiteKingStart : blackKingStart))
            {
                // Check if king can castle (king hasn't moved + rook hasn't moved)
                bool canCastleKingside = MakingMoves.CanCastleKingside(isKingWhite);
                bool canCastleQueenside = MakingMoves.CanCastleQueenside(isKingWhite);

                char opponentSide = isKingWhite ? 'b' : 'w';
                Board tempBoard = new Board();
                tempBoard.gameBoard = (int[])board.Clone();

                // king-side castling
                if (canCastleKingside)
                {
                    int kSide1 = currentPos + moveRight;
                    int kSide2 = currentPos + moveRight * 2;
                    int kRookSquare = currentPos + moveRight * 3;
                    
                    if (kSide1 >= 0 && kSide2 < 64 && kRookSquare < 64)
                    {
                        // No pieces between king and rook
                        if (board[kSide1] == Pieces.noPiece && board[kSide2] == Pieces.noPiece && Math.Abs(board[kRookSquare]) == Pieces.rook)
                        {
                            // King not in check
                            if (!Game.IsSquareUnderAttack(currentPos, opponentSide, tempBoard))
                            {
                                // King doesn't pass through check
                                if (!Game.IsSquareUnderAttack(kSide1, opponentSide, tempBoard))
                                {
                                    // King doesn't end in check (already handled by GetLegalMovesForPiece)
                                    legalMoves.Add(new moveInfo(currentPos, kSide2, MoveType.Castle));
                                }
                            }
                        }
                    }
                }

                // queen-side castling
                if (canCastleQueenside)
                {
                    int qSide1 = currentPos + moveLeft;
                    int qSide2 = currentPos + moveLeft * 2;
                    int qSide3 = currentPos + moveLeft * 3;
                    int qRookSquare = currentPos + moveLeft * 4;
                    
                    if (qSide3 >= 0 && qRookSquare >= 0)
                    {
                        // No pieces between king and rook
                        if (board[qSide1] == Pieces.noPiece && board[qSide2] == Pieces.noPiece && board[qSide3] == Pieces.noPiece && Math.Abs(board[qRookSquare]) == Pieces.rook)
                        {
                            // King not in check
                            if (!Game.IsSquareUnderAttack(currentPos, opponentSide, tempBoard))
                            {
                                // King doesn't pass through check
                                if (!Game.IsSquareUnderAttack(qSide1, opponentSide, tempBoard))
                                {
                                    // King doesn't end in check (already handled by GetLegalMovesForPiece)
                                    legalMoves.Add(new moveInfo(currentPos, qSide2, MoveType.Castle));
                                }
                            }
                        }
                    }
                }
            }

            // normal king moves
            foreach (int dir in directions)
            {
                int pos = currentPos + dir;

                if (pos < 0 || pos >= 64) continue;

                int currentRow = currentPos / 8;
                int newRow = pos / 8;
                
                if (Math.Abs(newRow - currentRow) > 1 && (dir == moveRight || dir == moveLeft)) continue;

                int piece = board[pos];
                if (piece == Pieces.noPiece)
                {
                    legalMoves.Add(new moveInfo(currentPos, pos, MoveType.Normal));
                }
                else
                {
                    if (IsOpponentPiece(piece))
                    {
                        legalMoves.Add(new moveInfo(currentPos, pos, MoveType.Capture));
                    }
                }
            }

            return legalMoves;
        }

    }
}