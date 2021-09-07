using System;
using System.IO;
using System.Linq;

using LanguageExt;
using static LanguageExt.Prelude;

namespace crossword
{
    class CrosswordFiller
    {
        private readonly Set<Word> _words;
        private readonly Map<BucketLength, Set<CrosswordBucket>> _buckets;
        private readonly Field _field;

        private readonly static Set<char> VOWELS = Set('a', 'ą', 'e', 'ę', 'ė', 'i', 'į', 'y', 'o', 'u', 'ų', 'ū');

        public CrosswordFiller(Set<Word> words, Map<BucketLength, Set<CrosswordBucket>> buckets, Field field)
        {
            _words = words;
            _buckets = buckets;
            _field = field;
        }

        public Lst<Field> GetSolutions()
        {
            return GetRecursiveSolution(_field, _buckets, _words);
        }

        private Lst<Field> GetRecursiveSolution(
            Field currentField,
            Map<BucketLength, Set<CrosswordBucket>> openBuckets,
            Set<Word> notFilledWords)
        {
            if (openBuckets.IsEmpty || notFilledWords.IsEmpty) return List(currentField);

            var nextWord = notFilledWords.First();
            var wordLength = new BucketLength(nextWord.Value.Length);
            var possibleBuckets = openBuckets[wordLength];

            return possibleBuckets
                .Filter(bucket => Match(nextWord, currentField.GetWord(bucket)))
                .Map(bucket => GetRecursiveSolution(
                    currentField.SetWord(nextWord, bucket.Cell, bucket.Direction),
                    openBuckets.AddOrUpdate(wordLength, possibleBuckets.Remove(bucket)),
                    notFilledWords.Remove(nextWord)
                ))
                .Fold(Lst<Field>.Empty, (acc, field) => acc.AddRange(field));
        }

        private static bool MatchChar(char wordChar, FieldSymbol bucketSymbol)
        {
            return bucketSymbol.Value.IsRight && Char.ToLower((char)bucketSymbol.Value) == wordChar ||
                bucketSymbol.Value == BucketSymbol.Vowel && VOWELS.Contains(wordChar) ||
                bucketSymbol.Value == BucketSymbol.Consonant && !VOWELS.Contains(wordChar) && Char.IsLetter(wordChar);
        }

        private static bool Match(Word word, FieldWord bucketWord)
        {
            if (word.Value.Length != bucketWord.Tokens.Count)
            {
                return false;
            }

            return word.Value.ToLower()
                    .Zip(bucketWord.Tokens)
                    .Map(charPair => MatchChar(charPair.Item1, charPair.Item2))
                    .ForAll(val => val);
        }
    }
}