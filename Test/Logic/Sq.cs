namespace Game.Logic
{
    public static class Sq
    {
        public static bool TryParse(string input, out int index)
        {
            index = -1;
            if (input.Length != 2) return false;

            char file = char.ToLower(input[0]);
            char rank = input[1];

            if (file < 'a' || file > 'h') return false;
            if (rank < '1' || rank > '8') return false;

            int col = file - 'a';
            int row = rank - '1';

            index = row * 8 + col;
            return true;
        }
        
        public static string ToAlgebraic(int index)
        {
            int row = index / 8;
            int col = index % 8;
            return $"{(char)('a' + col)}{row + 1}";
        }
    }
}