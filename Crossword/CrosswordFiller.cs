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
        private readonly Map<WordLength, Set<CrosswordBucket>> _buckets;
        private readonly Field _field;

        private readonly static Set<char> VOWELS = Set('a', 'ą', 'e', 'ę', 'ė', 'i', 'į', 'y', 'o', 'u', 'ų', 'ū');

        public CrosswordFiller(Set<Word> words, Map<WordLength, Set<CrosswordBucket>> buckets, Field field)
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
            Map<WordLength, Set<CrosswordBucket>> openBuckets,
            Set<Word> notFilledWords)
        {
            if (openBuckets.IsEmpty || notFilledWords.IsEmpty) return List(currentField);

            var nextWord = notFilledWords.First();
            var wordLength = new WordLength(nextWord.Value.Length);
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

        private static bool MatchChar(char wordChar, char bucketChar)
        {
            return bucketChar == wordChar ||
                bucketChar == (char)BucketSymbol.Vowel && VOWELS.Contains(wordChar) ||
                bucketChar == (char)BucketSymbol.Consonant && !VOWELS.Contains(wordChar) && Char.IsLetter(wordChar);
        }

        private static bool Match(Word word, Word bucketWord)
        {
            if (word.Value.Length != bucketWord.Value.Length)
            {
                return false;
            }

            return word.Value.ToLower()
                    .Zip(bucketWord.Value.ToLower())
                    .Map(charPair => MatchChar(charPair.Item1, charPair.Item2))
                    .ForAll(val => val);
        }
    }
}