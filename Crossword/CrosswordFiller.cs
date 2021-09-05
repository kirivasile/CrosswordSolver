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
        private readonly Set<CrosswordBucket> _buckets;
        private readonly Field _field;

        private readonly static Set<char> VOWELS = Set('a', 'ą', 'e', 'ę', 'ė', 'i', 'į', 'y', 'o', 'u', 'ų', 'ū');

        public CrosswordFiller(Set<Word> words, Set<CrosswordBucket> buckets, Field field)
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
            Set<CrosswordBucket> openBuckets,
            Set<Word> notFilledWords)
        {
            if (openBuckets.IsEmpty || notFilledWords.IsEmpty) return List(currentField);

            var nextWord = notFilledWords.First();

            return openBuckets
                .Filter(bucket => Match(nextWord, bucket))
                .Map(bucket => GetRecursiveSolution(
                    currentField.SetWord(nextWord, bucket.Cell, bucket.Direction),
                    openBuckets.Remove(bucket),
                    notFilledWords.Remove(nextWord)
                ))
                .Fold(Lst<Field>.Empty, (acc, field) => acc.AddRange(field));
        }

        private static bool MatchChar(char wordChar, char bucketSymbol)
        {
            return bucketSymbol == CrosswordReader.VOWEL_SYMBOL && VOWELS.Contains(wordChar) ||
                bucketSymbol == CrosswordReader.CONSONANT_SYMBOL && !VOWELS.Contains(wordChar) && Char.IsLetter(wordChar);
        }

        private static bool Match(Word word, CrosswordBucket bucket)
        {
            if (word.Value.Length != bucket.Word.Value.Length)
            {
                return false;
            }

            return word.Value.ToLower()
                    .Zip(bucket.Word.Value.ToLower())
                    .Map(charPair => MatchChar(charPair.Item1, charPair.Item2))
                    .ForAll(val => val);
        }
    }
}