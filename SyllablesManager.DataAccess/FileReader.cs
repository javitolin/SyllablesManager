using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SyllablesManager.DataAccess
{
    public class FileReader
    {
        public IEnumerable<string> GetWordsFromFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException($"File [{filename}] was not found");

            var lines = File.ReadLines(filename);
            foreach (var line in lines)
            {
                var words = line.Split(' ');
                foreach (var word in words)
                {
                    var currentWord = RemoveSpecialCharacters(word).Trim();
                    if (string.IsNullOrWhiteSpace(currentWord))
                        continue;

                    yield return currentWord;
                }
            }
        }

        private string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Zא-ת]+", "", RegexOptions.Compiled);
        }
    }
}
