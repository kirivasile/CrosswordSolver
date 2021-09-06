using System;
using System.IO;
using System.Diagnostics;

namespace crossword
{
    class CrosswordTest
    {
        static void Main(string[] args)
        {
            var st = new Stopwatch();

            st.Start();

            Console.WriteLine($"Reading crossword..");
            var reader = new CrosswordReader(args[0]);

            Console.WriteLine($"Filling crossword..");
            var filler = new CrosswordFiller(reader.Words, reader.Buckets, reader.Field);

            st.Stop();

            using (var writer = new StreamWriter(args[1]))
            {
                var solutions = filler.GetSolutions();
                Console.WriteLine($"{solutions.Count} solutions found! Have written them in {args[1]}");

                for (int i = 0; i < solutions.Count; ++i)
                {
                    writer.WriteLine($"Solution {i + 1}/{solutions.Count}:");
                    writer.WriteLine(solutions[i]);
                }
            }

            Console.WriteLine($"Working time of the algorithm {st.Elapsed}");
        }
    }
}
