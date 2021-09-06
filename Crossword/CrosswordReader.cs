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
                .Map(line => line.Replace('X', (char)BucketSymbol.Consonant).Replace('O', (char)BucketSymbol.Vowel));

            _field = new Field(bucketLines);

            var horizontalBuckets = bucketLines
                .Map((idx, line) => 
                    GetBucketsFromLine(
                        new Line(bucketLines[idx]),
                        new BucketWord(Lst<BucketSymbol>.Empty),
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
                        new BucketWord(Lst<BucketSymbol>.Empty),
                        new CrosswordCell(new RowIndex(idx), new RowIndex(0)),
                        Direction.Vertical
                    ))
                .Fold(new Set<CrosswordBucket>(), (acc, buckets) => acc.AddRange(buckets));

            _buckets = horizontalBuckets
                .AddRange(verticalBuckets)
                .GroupBy(bucket => new WordLength(bucket.Word.Symbols.Count))
                .ToDictionary(group => group.Key, group => new Set<CrosswordBucket>(group.ToList()))
                .ToMap(); 
        }

        Lst<CrosswordBucket> GetBucketsFromLine(Line lineSuffix, BucketWord currentPrefix, CrosswordCell cell, Direction direction)
        {
            if (lineSuffix.Value.Length == 0)
            {
                if (currentPrefix.Symbols.Count <= 1)
                {
                    return Lst<CrosswordBucket>.Empty;
                }

                return List<CrosswordBucket>(
                    new CrosswordBucket(
                        new CrosswordCell(cell.First, new RowIndex(cell.Second.Value - currentPrefix.Symbols.Count)),
                        direction,
                        new BucketWord(currentPrefix.Symbols)
                    )
                );
            } 

            char currentSymbol = lineSuffix.Value[0];

            if (currentSymbol == (char)BucketSymbol.Vowel || currentSymbol == (char)BucketSymbol.Consonant)
            {
                return GetBucketsFromLine(
                    new Line(lineSuffix.Value.Substring(1)),
                    new BucketWord(currentPrefix.Symbols.Add((BucketSymbol)currentSymbol)),
                    new CrosswordCell(cell.First, new RowIndex(cell.Second.Value + 1)),
                    direction
                );
            }

            if (currentPrefix.Symbols.Count <= 1) {
                return GetBucketsFromLine(
                    new Line(lineSuffix.Value.Substring(1)),
                    new BucketWord(Lst<BucketSymbol>.Empty),
                    new CrosswordCell(cell.First, new RowIndex(cell.Second.Value + 1)),
                    direction
                );
            }
            
            var bucket = new CrosswordBucket(
                new CrosswordCell(cell.First, new RowIndex(cell.Second.Value - currentPrefix.Symbols.Count)),
                direction,
                new BucketWord(currentPrefix.Symbols)
            );

            return GetBucketsFromLine(
                    new Line(lineSuffix.Value.Substring(1)),
                    new BucketWord(Lst<BucketSymbol>.Empty),
                    new CrosswordCell(cell.First, new RowIndex(cell.Second.Value + 1)),
                    direction
                ).Add(bucket);
        }
    }
}