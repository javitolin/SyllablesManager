using System;
using System.Text;
using SyllablesManager.Entities;

namespace SyllablesManager.CLI
{
    class Program
    {
        private const string KnownWordsFile = "KnownSyllables.txt";
        static void Main(string[] args)
        {
            var knownSyllables = new KnownSyllables(KnownWordsFile);
            knownSyllables.ReadFromFile();
            Console.OutputEncoding = Encoding.GetEncoding("Windows-1255");

            var input = "";
            while (input != null && !input.Equals("q", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine("Enter a word, enter 'q' to exit");
                input = Console.ReadLine();
                var result = knownSyllables.GetSyllablesForWord(input);
                Console.WriteLine($"Syllables = [{result}]");
            }
        }
    }
}
