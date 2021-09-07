using System;
using System.Text;

using LanguageExt;
using static LanguageExt.Prelude;

namespace crossword
{
    public enum BucketSymbol { Vowel, Consonant }

    public enum Direction { Horizontal, Vertical }

    public class FieldSymbol
    {
        public readonly Either<BucketSymbol, char> Value;

        private const char CONSONANT_CHAR = 'X';
        private const char VOWEL_CHAR = 'O';

        public FieldSymbol(char token)
        {
            switch(token)
            {
                case CONSONANT_CHAR: 
                    Value = BucketSymbol.Consonant;
                    break;
                case VOWEL_CHAR:
                    Value = BucketSymbol.Vowel;
                    break;
                default:
                    Value = token;
                    break;
            }
        }

        public override string ToString()
        {
            if (Value == BucketSymbol.Consonant) return CONSONANT_CHAR.ToString();
            if (Value == BucketSymbol.Vowel) return VOWEL_CHAR.ToString();
            if (Value.IsRight) return ((char)Value).ToString();

            return Value.ToString();
        }
    }

    public class FieldWord
    {
        public readonly Lst<FieldSymbol> Tokens;

        public FieldWord(string line)
        {
            Tokens = new Lst<FieldSymbol>(line.Map(token => new FieldSymbol(token)));
        }

        public FieldWord(Lst<FieldSymbol> tokens)
        {
            Tokens = tokens;
        }
    }

    public class RowIndex : Record<RowIndex>
    {
        public readonly int Value;

        public RowIndex(int value)
        {
            Value = value;
        }
    }

    public class BucketLength : Record<BucketLength>
    {
        public readonly int Value;

        public BucketLength(int value)
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
        public readonly BucketLength Length;

        public CrosswordBucket(CrosswordCell cell, Direction direction, BucketLength length)
        {
            Cell = cell;
            Direction = direction;
            Length = length;
        }
    }

    public class Field
    {
        private readonly Map<CrosswordCell, FieldSymbol> _field;
        private readonly RowIndex _fieldSize;

        public Field(Lst<FieldWord> lines)
        {
            _fieldSize = new RowIndex(lines.Count);

            _field = lines
                .Map((i, line) => (i, line.Tokens.Map((j, token) => (i, j, token))))
                .Fold(Lst<(int, int, FieldSymbol)>.Empty, (acc, elem) => acc.AddRange(elem.Item2))
                .ToDictionary(
                    trio => new CrosswordCell(new RowIndex(trio.Item1), new RowIndex(trio.Item2)), 
                    trio => trio.Item3)
                .ToMap();
        }

        public Field(Map<CrosswordCell, FieldSymbol> field, RowIndex fieldSize)
        {
            _fieldSize = fieldSize;
            _field = field;
        }

        public FieldWord GetWord(CrosswordBucket bucket)
        {
            if (bucket.Direction == Direction.Horizontal)
            {
                return new FieldWord(
                    new Lst<FieldSymbol>(
                        Range(0, bucket.Length.Value)
                        .Map(i => _field[new CrosswordCell(bucket.Cell.First, new RowIndex(bucket.Cell.Second.Value + i))])
                    )
                );
            }
            else
            {
                return new FieldWord(
                    new Lst<FieldSymbol>(
                        Range(0, bucket.Length.Value)
                        .Map(i => _field[new CrosswordCell(new RowIndex(bucket.Cell.Second.Value + i), bucket.Cell.First)])
                    )
                );
            }
        }

        public Field SetWord(Word word, CrosswordCell cell, Direction direction)
        {
            var newField = _field;
            if (direction == Direction.Horizontal)
            {
                return new Field(
                    _field.AddOrUpdateRange(
                        word.Value
                            .Map((i, token) => (
                                new CrosswordCell(cell.First, new RowIndex(cell.Second.Value + i)),
                                new FieldSymbol(token)
                            )
                        )
                    ),
                    _fieldSize
                );
            }
            else
            {
                return new Field(
                    _field.AddOrUpdateRange(
                        word.Value
                            .Map((i, token) => (
                                new CrosswordCell(new RowIndex(cell.Second.Value + i), cell.First),
                                new FieldSymbol(token)
                            )
                        )
                    ),
                    _fieldSize
                );
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < _fieldSize.Value; ++i)
            {
                for (int j = 0; j < _fieldSize.Value; ++j)
                {
                    builder.Append(_field[new CrosswordCell(new RowIndex(i), new RowIndex(j))].ToString());
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}