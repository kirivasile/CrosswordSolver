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
        private readonly Map<WordLength, Set<CrosswordBucket>> _buckets;

        public static char VOWEL_SYMBOL = '+';
        public static char CONSONANT_SYMBOL = '-';

        public Set<Word> Words => _words;
        public Field Field => _field;
        public Map<WordLength, Set<CrosswordBucket>> Buckets => _buckets;

        public CrosswordReader(string filePath)
        {
            var lines = new Lst<string>(File.ReadAllLines(filePath));

            _words = new Set<Word>(lines[0].Split(" ").Select(word => new Word(word)));
            var bucketLines = lines
                .RemoveAt(0)
                .RemoveAt(0)
                .Map(line => line.Replace('X', CONSONANT_SYMBOL).Replace('O', VOWEL_SYMBOL));

            _field = new Field(bucketLines);

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
                .AddRange(verticalBuckets)
                .GroupBy(bucket => new WordLength(bucket.Word.Value.Length))
                .ToDictionary(group => group.Key, group => new Set<CrosswordBucket>(group.ToList()))
                .ToMap(); 
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
                        new Word(currentPrefix.Value)
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
                    new Word(currentPrefix.Value)
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