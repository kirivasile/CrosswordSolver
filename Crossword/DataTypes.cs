using System;
using System.Text;

using LanguageExt;
using static LanguageExt.Prelude;

namespace crossword
{
    public enum BucketSymbol { Vowel = '+', Consonant = '-' }

    public enum Direction { Horizontal, Vertical }

    public record Line(string Value) { }

    public record BucketWord(Lst<BucketSymbol> Symbols) { }

    public class RowIndex : Record<RowIndex>
    {
        public readonly int Value;

        public RowIndex(int value)
        {
            Value = value;
        }
    }

    public class WordLength : Record<WordLength>
    {
        public readonly int Value;

        public WordLength(int value)
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
        public readonly BucketWord Word;

        public CrosswordBucket(CrosswordCell cell, Direction direction, BucketWord word)
        {
            Cell = cell;
            Direction = direction;
            Word = word;
        }
    }

    public class Field
    {
        private readonly Map<CrosswordCell, char> _field;
        private readonly RowIndex _fieldSize;

        public Field(Lst<string> lines)
        {
            _fieldSize = new RowIndex(lines.Count);

            _field = lines
                .Map((i, line) => (i, line.Map((j, token) => (i, j, token))))
                .Fold(Lst<(int, int, char)>.Empty, (acc, elem) => acc.AddRange(elem.Item2))
                .ToDictionary(
                    trio => new CrosswordCell(new RowIndex(trio.Item1), new RowIndex(trio.Item2)), 
                    trio => trio.Item3)
                .ToMap();
        }

        public Field(Map<CrosswordCell, char> field, RowIndex fieldSize)
        {
            _fieldSize = fieldSize;
            _field = field;
        }

        public Word GetWord(CrosswordBucket bucket)
        {
            if (bucket.Direction == Direction.Horizontal)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < bucket.Word.Symbols.Count; ++i)
                {
                    sb.Append(_field[new CrosswordCell(bucket.Cell.First, new RowIndex(bucket.Cell.Second.Value + i))]);
                }
                return new Word(sb.ToString());
            }
            else
            {
                var sb = new StringBuilder();
                for (int i = 0; i < bucket.Word.Symbols.Count; ++i)
                {
                    sb.Append(_field[new CrosswordCell(new RowIndex(bucket.Cell.Second.Value + i), bucket.Cell.First)]);
                }
                return new Word(sb.ToString());
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
                                token
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
                                token
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
                    builder.Append(_field[new CrosswordCell(new RowIndex(i), new RowIndex(j))]);
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}