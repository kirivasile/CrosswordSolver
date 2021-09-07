using System;
using System.IO;
using System.Linq;

using LanguageExt;
using static LanguageExt.Prelude;

namespace crossword
{
    class CrosswordReader
    {
        public readonly Set<Word> Words;
        public readonly Field Field;
        public readonly Map<BucketLength, Set<CrosswordBucket>> Buckets;

        public CrosswordReader(string filePath)
        {
            var lines = new Lst<string>(File.ReadAllLines(filePath));

            Words = new Set<Word>(lines[0].Split(" ").Select(word => new Word(word)));
            var bucketLines = lines
                .RemoveAt(0)
                .RemoveAt(0)
                .Map(line => new FieldWord(line));

            Field = new Field(bucketLines);

            var horizontalBuckets = bucketLines
                .Map((idx, line) => 
                    GetBucketsFromLine(
                        bucketLines[idx],
                        new BucketLength(0),
                        new CrosswordCell(new RowIndex(idx), new RowIndex(0)),
                        Direction.Horizontal
                    ))
                .Fold(new Set<CrosswordBucket>(), (acc, buckets) => acc.AddRange(buckets));

            var transposedBucketLines = Range(0, bucketLines.Count)
                .Map(i => new FieldWord(bucketLines.Map(line => line.Tokens[i])))
                .ToList();

            var verticalBuckets = transposedBucketLines
                .Map((idx, line) =>
                    GetBucketsFromLine(
                        transposedBucketLines[idx],
                        new BucketLength(0),
                        new CrosswordCell(new RowIndex(idx), new RowIndex(0)),
                        Direction.Vertical
                    ))
                .Fold(new Set<CrosswordBucket>(), (acc, buckets) => acc.AddRange(buckets));

            Buckets = horizontalBuckets
                .AddRange(verticalBuckets)
                .GroupBy(bucket => bucket.Length)
                .ToDictionary(group => group.Key, group => new Set<CrosswordBucket>(group.ToList()))
                .ToMap(); 
        }

        Lst<CrosswordBucket> GetBucketsFromLine(FieldWord lineSuffix, BucketLength currentBucketLength, CrosswordCell cell, Direction direction)
        {
            if (lineSuffix.Tokens.Count == 0)
            {
                if (currentBucketLength.Value <= 1)
                {
                    return Lst<CrosswordBucket>.Empty;
                }

                return List<CrosswordBucket>(
                    new CrosswordBucket(
                        new CrosswordCell(cell.First, new RowIndex(cell.Second.Value - currentBucketLength.Value)),
                        direction,
                        currentBucketLength
                    )
                );
            } 

            var currentSymbol = lineSuffix.Tokens[0];

            if (currentSymbol.Value.IsLeft)
            {
                return GetBucketsFromLine(
                    new FieldWord(lineSuffix.Tokens.RemoveAt(0)),
                    new BucketLength(currentBucketLength.Value + 1),
                    new CrosswordCell(cell.First, new RowIndex(cell.Second.Value + 1)),
                    direction
                );
            }

            if (currentBucketLength.Value <= 1) {
                return GetBucketsFromLine(
                    new FieldWord(lineSuffix.Tokens.RemoveAt(0)),
                    new BucketLength(0),
                    new CrosswordCell(cell.First, new RowIndex(cell.Second.Value + 1)),
                    direction
                );
            }
            
            var bucket = new CrosswordBucket(
                new CrosswordCell(cell.First, new RowIndex(cell.Second.Value - currentBucketLength.Value)),
                direction,
                currentBucketLength
            );

            return GetBucketsFromLine(
                    new FieldWord(lineSuffix.Tokens.RemoveAt(0)),
                    new BucketLength(0),
                    new CrosswordCell(cell.First, new RowIndex(cell.Second.Value + 1)),
                    direction
                ).Add(bucket);
        }
    }
}