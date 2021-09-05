using System.Text;

using LanguageExt;

namespace crossword
{
    public enum Direction { Horizontal, Vertical }

    public record Line(string Value) { }

    public class RowIndex : Record<RowIndex>
    {
        public readonly int Value;

        public RowIndex(int value)
        {
            Value = value;
        }
    }

    public class CrosswordCell : Record<CrosswordCell>
    {
        public readonly RowIndex First;
        public readonly RowIndex Second;

        public CrosswordCell(RowIndex first, RowIndex second)
        {
            First = first;
            Second = second;
        }
    }

    public class Word : Record<Word>
    {
        public readonly string Value;

        public Word(string value)
        {
            Value = value;
        }
    }

    public class CrosswordBucket : Record<CrosswordBucket>
    {
        public readonly CrosswordCell Cell;
        public readonly Direction Direction;

        [NonOrd]
        public readonly Word Word;

        public CrosswordBucket(CrosswordCell cell, Direction direction, Word word)
        {
            Cell = cell;
            Direction = direction;
            Word = word;
        }
    }

    public struct Field
    {
        private readonly char[,] _field;

        public Field(char[,] field)
        {
            _field = field;
        }

        public Field SetWord(Word word, CrosswordCell cell, Direction direction)
        {
            var field = _field.Clone() as char[,];

            if (direction == Direction.Horizontal)
            {
                for (int i = 0; i < word.Value.Length; ++i)
                {
                    field[cell.First.Value, cell.Second.Value + i] = word.Value[i];
                }
            }
            else
            {
                for (int j = 0; j < word.Value.Length; ++j)
                {
                    field[cell.Second.Value + j, cell.First.Value] = word.Value[j];
                }
            }

            return new Field(field);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < _field.GetLength(0); ++i)
            {
                for (int j = 0; j < _field.GetLength(1); ++j)
                {
                    builder.Append(_field[i, j]);
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}