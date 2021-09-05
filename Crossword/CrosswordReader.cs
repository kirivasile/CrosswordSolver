using System;
using System.IO;
using System.Linq;

using LanguageExt;
using static LanguageExt.Prelude;

namespace crossword
{
    class CrosswordReader
    {
        private readonly Set<Word> _words;
        private readonly Field _field;
        private readonly Set<CrosswordBucket> _buckets;

        public static char VOWEL_SYMBOL = '+';
        public static char CONSONANT_SYMBOL = '-';

        public Set<Word> Words => _words;
        public Field Field => _field;
        public Set<CrosswordBucket> Buckets => _buckets;

        public CrosswordReader(string filePath)
        {
            var lines = new Lst<string>(File.ReadAllLines(filePath));

            _words = new Set<Word>(lines[0].Split(" ").Select(word => new Word(word)));
            var bucketLines = new Lst<string>(lines.RemoveAt(0).RemoveAt(0));

            char[,] fieldChars = new char[bucketLines.Count, bucketLines.Count];
            for (int i = 0; i < bucketLines.Count; ++i)
            {
                for (int j = 0; j < bucketLines.Count; ++j)
                {
                    fieldChars[i, j] = bucketLines[i][j];
                }
            }
            _field = new Field(fieldChars);

            var horizontalBuckets = bucketLines
                .Map((idx, line) => 
                    GetBucketsFromLine(
                        new Line(bucketLines[idx]),
                        new Word(string.Empty),
                        new CrosswordCell(new RowIndex(idx), new RowIndex(0)),
                        Direction.Horizontal
                    ))
                .Fold(new Set<CrosswordBucket>(), (acc, buckets) => acc.AddRange(buckets));

            var transposedBucketLines = Range(0, bucketLines.Count)
                .Map(i => string.Join("", bucketLines.Map(line => line[i])))
                .ToList();

            var verticalBuckets = transposedBucketLines
                .Map((idx, line) =>
                    GetBucketsFromLine(
                        new Line(transposedBucketLines[idx]),
                        new Word(string.Empty),
                        new CrosswordCell(new RowIndex(idx), new RowIndex(0)),
                        Direction.Vertical
                    ))
                .Fold(new Set<CrosswordBucket>(), (acc, buckets) => acc.AddRange(buckets));

            _buckets = horizontalBuckets
                .AddRange(verticalBuckets);
        }

        Lst<CrosswordBucket> GetBucketsFromLine(Line lineSuffix, Word currentPrefix, CrosswordCell cell, Direction direction)
        {
            if (lineSuffix.Value.Length == 0)
            {
                if (currentPrefix.Value.Length <= 1)
                {
                    return Lst<CrosswordBucket>.Empty;
                }

                return List<CrosswordBucket>(
                    new CrosswordBucket(
                        new CrosswordCell(cell.First, new RowIndex(cell.Second.Value - currentPrefix.Value.Length)),
                        direction,
                        new Word(currentPrefix.Value.Replace('X', CONSONANT_SYMBOL).Replace('O', VOWEL_SYMBOL))
                    )
                );
            } 

            char currentSymbol = lineSuffix.Value[0];

            if (Char.IsWhiteSpace(currentSymbol)) {
                if (currentPrefix.Value.Length <= 1) {
                    return GetBucketsFromLine(
                        new Line(lineSuffix.Value.Substring(1)),
                        new Word(string.Empty),
                        new CrosswordCell(cell.First, new RowIndex(cell.Second.Value + 1)),
                        direction
                    );
                }
                
                var bucket = new CrosswordBucket(
                    new CrosswordCell(cell.First, new RowIndex(cell.Second.Value - currentPrefix.Value.Length)),
                    direction,
                    new Word(currentPrefix.Value.Replace('X', CONSONANT_SYMBOL).Replace('O', VOWEL_SYMBOL))
                );

                return GetBucketsFromLine(
                        new Line(lineSuffix.Value.Substring(1)),
                        new Word(string.Empty),
                        new CrosswordCell(cell.First, new RowIndex(cell.Second.Value + 1)),
                        direction
                    ).Add(bucket);
            }

            return GetBucketsFromLine(
                new Line(lineSuffix.Value.Substring(1)),
                new Word(currentPrefix.Value + currentSymbol),
                new CrosswordCell(cell.First, new RowIndex(cell.Second.Value + 1)),
                direction
            );
        }
    }
}