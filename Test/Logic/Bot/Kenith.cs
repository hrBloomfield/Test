namespace Game.Logic.Bot
{
    public class Kenith
    {
        private const int MAX_DEPTH = 6;
        private const int CHECKMATE_SCORE = 100000;
        private const int STALEMATE_SCORE = 0;

        // TT
        private static Dictionary<ulong, TranspositionEntry> transpositionTable = new Dictionary<ulong, TranspositionEntry>();
        private const int MAX_TT_SIZE = 1000000;

        // heuristics for move ordering
        private static int[,] historyTable = new int[64, 64];

        private class TranspositionEntry
        {
            public int depth;
            public int score;
            public NodeType nodeType;
            public Move.moveInfo bestMove;
        }

        private enum NodeType
        {
            Exact,      // PV node - exact score
            LowerBound, // Beta cutoff - score is at least this value
            UpperBound  // Alpha cutoff - score is at most this value
        }

        // Piece values for evaluation
        private static readonly Dictionary<int, int> pieceValues = new Dictionary<int, int>
        {
            { Pieces.pawn, 100 },
            { Pieces.knight, 320 },
            { Pieces.bishop, 330 },
            { Pieces.rook, 500 },
            { Pieces.queen, 900 },
            { Pieces.king, 20000 }
        };

        // Position tables (same as before but keeping for reference)
        private static readonly int[] pawnTable = {
            0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10,-20,-20, 10, 10,  5,
            5, -5,-10,  0,  0,-10, -5,  5,
            0,  0,  0, 20, 20,  0,  0,  0,
            5,  5, 10, 25, 25, 10,  5,  5,
            10, 10, 20, 30, 30, 20, 10, 10,
            50, 50, 50, 50, 50, 50, 50, 50,
            0,  0,  0,  0,  0,  0,  0,  0
        };

        private static readonly int[] knightTable = {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50
        };

        private static readonly int[] bishopTable = {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -20,-10,-10,-10,-10,-10,-10,-20
        };

        private static readonly int[] rookTable = {
            0,  0,  0,  5,  5,  0,  0,  0,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            5, 10, 10, 10, 10, 10, 10,  5,
            0,  0,  0,  0,  0,  0,  0,  0
        };

        // IMPROVED: King table with strong castling bonus
        private static readonly int[] kingMiddleGameTable = {
            30, 40, 20,  0,  0, 20, 40, 30,  // Rank 1 - encourage castling
            20, 20,  0,  0,  0,  0, 20, 20,
            -10,-20,-20,-20,-20,-20,-20,-10,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30
        };

        public static Move.moveInfo FindBestMove(char sideToMove, Board board)
        {
            // Clear history table at start of search
            Array.Clear(historyTable, 0, historyTable.Length);

            Move.moveInfo bestMove = null;
            int bestScore = int.MinValue;
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            var allMoves = Game.GenerateAllLegalMoves(sideToMove, board);
            if (allMoves.Count == 0) return null;

            // IMPROVED: Adaptive depth based on position complexity
            int searchDepth = GetAdaptiveDepth(board, sideToMove, allMoves.Count);

            // IMPROVED: Better move ordering from the start
            var orderedMoves = OrderMovesAdvanced(allMoves, board, null);

            // Opening book for variety
            if (board.IsStartingPosition())
            {
                int topN = Math.Min(5, orderedMoves.Count);
                Random rng = new Random();
                bestMove = orderedMoves[rng.Next(topN)];
            }
            else
            {
                foreach (var move in orderedMoves)
                {
                    Board tempBoard = board.Clone();
                    ApplyMove(tempBoard, move);

                    char opponentSide = sideToMove == 'w' ? 'b' : 'w';
                    int score = -Minimax(tempBoard, searchDepth - 1, -beta, -alpha, opponentSide);
                    
                    // DEBUG: displays useful progress
                    Console.WriteLine($"depth = {searchDepth,3} | score = {score,3} | move = {move.from} -> {move.to}");

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }

                    alpha = Math.Max(alpha, score);
                }
            }

            return bestMove;
        }

        // IMPROVED: Better adaptive depth logic
        private static int GetAdaptiveDepth(Board board, char sideToMove, int moveCount)
        {
            int material = CalculateMaterial(board);
            int depth = MAX_DEPTH;

            // In endgame (low material), search deeper
            if (material <= 1500) depth += 2;
            else if (material <= 2500) depth += 1;

            // In tactical positions (few moves), search deeper
            if (moveCount <= 10) depth += 2;

            // Don't go too deep as it gets exponentially slower
            return Math.Min(depth, MAX_DEPTH + 2);
        }

        // NEW: Advanced move ordering using multiple heuristics
        private static List<Move.moveInfo> OrderMovesAdvanced(List<Move.moveInfo> moves, Board board, Move.moveInfo pvMove)
        {
            return moves.OrderByDescending(m => {
                int score = 0;

                // 1. PV move from transposition table (best move from previous search)
                if (pvMove != null && m.from == pvMove.from && m.to == pvMove.to)
                    score += 10000;

                // 2. IMPROVED: Castling bonus - strongly encourage castling
                if (m.moveType == Move.MoveType.Castle)
                    score += 700; // High priority for castling

                // 3. MVV-LVA (Most Valuable Victim - Least Valuable Attacker)
                if (m.moveType == Move.MoveType.Capture || m.moveType == Move.MoveType.PromotionCapture)
                {
                    int capturedPiece = Math.Abs(board.gameBoard[m.to]);
                    int attackingPiece = Math.Abs(board.gameBoard[m.from]);
                    
                    int victimValue = pieceValues.ContainsKey(capturedPiece) ? pieceValues[capturedPiece] : 0;
                    int attackerValue = pieceValues.ContainsKey(attackingPiece) ? pieceValues[attackingPiece] : 0;
                    
                    // Prioritize capturing valuable pieces with less valuable pieces
                    score += victimValue * 10 - attackerValue;
                }

                // 4. Promotions
                if (m.moveType == Move.MoveType.Promotion || m.moveType == Move.MoveType.PromotionCapture)
                    score += 800;

                // 5. History heuristic (moves that caused cutoffs before)
                score += historyTable[m.from, m.to] / 10;

                // 6. Center control
                if (m.to == 27 || m.to == 28 || m.to == 35 || m.to == 36)
                    score += 20;

                return score;
            }).ToList();
        }

        private static int CalculateMaterial(Board board)
        {
            int total = 0;
            for (int i = 0; i < 64; i++)
            {
                int piece = board.gameBoard[i];
                if (piece == Pieces.noPiece || Math.Abs(piece) == Pieces.enPassantMarker) continue;

                int pieceType = Math.Abs(piece);
                if (pieceValues.ContainsKey(pieceType))
                    total += pieceValues[pieceType];
            }
            return total;
        }

        // NEW: Hash function for transposition table
        private static ulong HashBoard(Board board)
        {
            ulong hash = 0;
            for (int i = 0; i < 64; i++)
            {
                int piece = board.gameBoard[i];
                if (piece != Pieces.noPiece)
                {
                    // Simple hash combining position and piece
                    hash ^= (ulong)((piece + 10) * (i + 1) * 31);
                }
            }
            return hash;
        }

        // IMPROVED: Minimax with transposition table
        private static int Minimax(Board board, int depth, int alpha, int beta, char sideToMove)
        {
            int alphaOrig = alpha;
            
            // NEW: Check transposition table
            ulong boardHash = HashBoard(board);
            if (transpositionTable.TryGetValue(boardHash, out TranspositionEntry entry))
            {
                if (entry.depth >= depth)
                {
                    if (entry.nodeType == NodeType.Exact)
                        return entry.score;
                    else if (entry.nodeType == NodeType.LowerBound)
                        alpha = Math.Max(alpha, entry.score);
                    else if (entry.nodeType == NodeType.UpperBound)
                        beta = Math.Min(beta, entry.score);

                    if (alpha >= beta)
                        return entry.score;
                }
            }

            // Check for game over
            string gameState = Game.CheckGameState(sideToMove, board);
            if (gameState != "null")
            {
                if (gameState.Contains("wins")) return -CHECKMATE_SCORE + (MAX_DEPTH - depth);
                return STALEMATE_SCORE;
            }

            // Reached depth limit - use quiescence search
            if (depth == 0) 
                return Quiescence(board, alpha, beta, sideToMove);

            var allMoves = Game.GenerateAllLegalMoves(sideToMove, board);
            
            // Use PV move from TT if available
            Move.moveInfo pvMove = entry?.bestMove;
            var orderedMoves = OrderMovesAdvanced(allMoves, board, pvMove);

            int maxScore = int.MinValue;
            Move.moveInfo bestMoveFound = null;

            foreach (var move in orderedMoves)
            {
                Board tempBoard = board.Clone();
                ApplyMove(tempBoard, move);

                char opponentSide = sideToMove == 'w' ? 'b' : 'w';
                int score = -Minimax(tempBoard, depth - 1, -beta, -alpha, opponentSide);

                if (score > maxScore)
                {
                    maxScore = score;
                    bestMoveFound = move;
                }

                alpha = Math.Max(alpha, score);

                // Beta cutoff - this move is too good, opponent won't allow it
                if (alpha >= beta)
                {
                    // NEW: Update history table for good cutoff moves
                    if (move.moveType == Move.MoveType.Normal)
                        historyTable[move.from, move.to] += depth * depth;
                    break;
                }
            }

            // NEW: Store in transposition table
            NodeType nodeType;
            if (maxScore <= alphaOrig)
                nodeType = NodeType.UpperBound;
            else if (maxScore >= beta)
                nodeType = NodeType.LowerBound;
            else
                nodeType = NodeType.Exact;

            // Limit table size
            if (transpositionTable.Count > MAX_TT_SIZE)
                transpositionTable.Clear();

            transpositionTable[boardHash] = new TranspositionEntry
            {
                depth = depth,
                score = maxScore,
                nodeType = nodeType,
                bestMove = bestMoveFound
            };

            return maxScore;
        }

        // IMPROVED: Evaluation function with castling bonus
        private static int EvaluatePosition(Board board, char sideToMove)
        {
            int score = 0;
            int whiteMaterial = 0;
            int blackMaterial = 0;
            int whitePieceCount = 0;
            int blackPieceCount = 0;

            for (int i = 0; i < 64; i++)
            {
                int piece = board.gameBoard[i];
                if (piece == Pieces.noPiece || Math.Abs(piece) == Pieces.enPassantMarker) continue;

                int pieceType = Math.Abs(piece);
                int pieceValue = pieceValues.ContainsKey(pieceType) ? pieceValues[pieceType] : 0;
                int positionBonus = GetPositionBonus(pieceType, i, piece > 0);
                int totalValue = pieceValue + positionBonus;

                if (piece > 0) // White piece
                {
                    score += totalValue;
                    whiteMaterial += pieceValue;
                    whitePieceCount++;
                }
                else // Black piece
                {
                    score -= totalValue;
                    blackMaterial += pieceValue;
                    blackPieceCount++;
                }
            }

            // REMOVED: Mobility bonus (too expensive to calculate every position)
            // Instead we rely on move ordering and transposition table

            // Development bonus (only check in opening)
            int totalPieces = whitePieceCount + blackPieceCount;
            if (totalPieces > 24) // Opening phase
                score += EvaluateDevelopment(board);

            // King safety
            score += EvaluateKingSafety(board, 'w') - EvaluateKingSafety(board, 'b');

            // Pawn structure
            score += EvaluatePawnStructure(board);

            // NEW: Castling bonus - check if king has castled
            score += EvaluateCastling(board);

            return sideToMove == 'w' ? score : -score;
        }

        // NEW: Evaluate castling status
        private static int EvaluateCastling(Board board)
        {
            int score = 0;
            
            // Check if white has castled (king on g1 or c1)
            if (board.gameBoard[6] == Pieces.king) // Kingside castle
                score += 70;
            else if (board.gameBoard[2] == Pieces.king) // Queenside castle
                score += 60;
            
            // Check if black has castled (king on g8 or c8)
            if (board.gameBoard[62] == -Pieces.king) // Kingside castle
                score -= 70;
            else if (board.gameBoard[58] == -Pieces.king) // Queenside castle
                score -= 60;
            
            return score;
        }

        private static int EvaluateDevelopment(Board board)
        {
            int score = 0;
            
            // Penalize pieces still on starting squares
            if (board.gameBoard[1] == Pieces.knight) score -= 20;
            if (board.gameBoard[6] == Pieces.knight) score -= 20;
            if (board.gameBoard[57] == -Pieces.knight) score += 20;
            if (board.gameBoard[62] == -Pieces.knight) score += 20;
            
            if (board.gameBoard[2] == Pieces.bishop) score -= 15;
            if (board.gameBoard[5] == Pieces.bishop) score -= 15;
            if (board.gameBoard[58] == -Pieces.bishop) score += 15;
            if (board.gameBoard[61] == -Pieces.bishop) score += 15;
            
            return score;
        }

        private static int EvaluateKingSafety(Board board, char side)
        {
            int kingPiece = (side == 'w') ? Pieces.king : -Pieces.king;
            int kingPos = Array.IndexOf(board.gameBoard, kingPiece);
            
            if (kingPos == -1) return -10000;
            
            int safety = 0;
            int[] adjacentSquares = { -9, -8, -7, -1, 1, 7, 8, 9 };
            
            // Count friendly pieces around king
            foreach (int offset in adjacentSquares)
            {
                int pos = kingPos + offset;
                if (pos >= 0 && pos < 64)
                {
                    int piece = board.gameBoard[pos];
                    if (side == 'w' && piece > 0 && piece != Pieces.enPassantMarker)
                        safety += 5;
                    else if (side == 'b' && piece < 0 && piece != -Pieces.enPassantMarker)
                        safety += 5;
                }
            }
            
            return safety;
        }

        private static int EvaluatePawnStructure(Board board)
        {
            int score = 0;
            
            // Penalize doubled pawns
            for (int file = 0; file < 8; file++)
            {
                int whitePawns = 0;
                int blackPawns = 0;
                
                for (int rank = 0; rank < 8; rank++)
                {
                    int pos = rank * 8 + file;
                    if (board.gameBoard[pos] == Pieces.pawn) whitePawns++;
                    if (board.gameBoard[pos] == -Pieces.pawn) blackPawns++;
                }
                
                if (whitePawns > 1) score -= (whitePawns - 1) * 15;
                if (blackPawns > 1) score += (blackPawns - 1) * 15;
            }
            
            return score;
        }

        private static int GetPositionBonus(int pieceType, int position, bool isWhite)
        {
            int index = isWhite ? position : (63 - position);

            switch (pieceType)
            {
                case Pieces.pawn:
                    return pawnTable[index];
                case Pieces.knight:
                    return knightTable[index];
                case Pieces.bishop:
                    return bishopTable[index];
                case Pieces.rook:
                    return rookTable[index];
                case Pieces.king:
                    return kingMiddleGameTable[index];
                default:
                    return 0;
            }
        }
        
        // Quiescence search to avoid horizon effect
        private static int Quiescence(Board board, int alpha, int beta, char sideToMove)
        {
            int standPat = EvaluatePosition(board, sideToMove);

            if (standPat >= beta) return beta;
            if (standPat > alpha) alpha = standPat;

            // Only search captures to stabilize the position
            var captures = Game.GenerateAllLegalMoves(sideToMove, board)
                .Where(m => m.moveType == Move.MoveType.Capture || m.moveType == Move.MoveType.PromotionCapture)
                .OrderByDescending(m => {
                    int capturedPiece = Math.Abs(board.gameBoard[m.to]);
                    return pieceValues.ContainsKey(capturedPiece) ? pieceValues[capturedPiece] : 0;
                });

            foreach (var move in captures)
            {
                Board tempBoard = board.Clone();
                ApplyMove(tempBoard, move);
                char opponent = sideToMove == 'w' ? 'b' : 'w';
                int score = -Quiescence(tempBoard, -beta, -alpha, opponent);

                if (score >= beta) return beta;
                if (score > alpha) alpha = score;
            }

            return alpha;
        }

        private static void ApplyMove(Board board, Move.moveInfo move)
        {
            int movingPiece = board.gameBoard[move.from];

            // Clear en passant markers
            for (int i = 0; i < 64; i++)
            {
                if (Math.Abs(board.gameBoard[i]) == Pieces.enPassantMarker)
                    board.gameBoard[i] = Pieces.noPiece;
            }

            // Handle castling
            if (move.moveType == Move.MoveType.Castle)
            {
                bool kingSide = move.to > move.from;
                board.gameBoard[move.to] = movingPiece;
                board.gameBoard[move.from] = Pieces.noPiece;

                if (kingSide)
                {
                    int rookFrom = move.from + 3;
                    int rookTo = move.from + 1;
                    board.gameBoard[rookTo] = board.gameBoard[rookFrom];
                    board.gameBoard[rookFrom] = Pieces.noPiece;
                }
                else
                {
                    int rookFrom = move.from - 4;
                    int rookTo = move.from - 1;
                    board.gameBoard[rookTo] = board.gameBoard[rookFrom];
                    board.gameBoard[rookFrom] = Pieces.noPiece;
                }
                return;
            }

            // Handle en passant
            if (move.moveType == Move.MoveType.EnPassant)
            {
                int capturedPawnSquare = PieceHelpers.IsWhite(movingPiece) ? move.to - 8 : move.to + 8;
                board.gameBoard[capturedPawnSquare] = Pieces.noPiece;
            }

            // Handle promotion
            if (move.moveType == Move.MoveType.Promotion || move.moveType == Move.MoveType.PromotionCapture)
            {
                int colour = PieceHelpers.IsWhite(movingPiece) ? Pieces.white : Pieces.black;
                board.gameBoard[move.to] = Pieces.queen * colour;
                board.gameBoard[move.from] = Pieces.noPiece;
                return;
            }

            // Normal move
            board.gameBoard[move.to] = movingPiece;
            board.gameBoard[move.from] = Pieces.noPiece;

            // Create en passant marker
            if (move.moveType == Move.MoveType.DoubleMove)
            {
                int enPassantSquare = PieceHelpers.IsWhite(movingPiece) ? move.to - 8 : move.to + 8;
                board.gameBoard[enPassantSquare] = PieceHelpers.IsWhite(movingPiece) ? 
                    Pieces.enPassantMarker : Pieces.black * Pieces.enPassantMarker;
            }
        }
    }
}