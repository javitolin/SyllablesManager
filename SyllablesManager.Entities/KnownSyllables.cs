using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace SyllablesManager.Entities
{
    public class KnownSyllables
    {
        private readonly Dictionary<string, string> _knownSyllablesDictionary;
        private readonly string _filename;
        public const string NotKnown = "";

        public KnownSyllables(string filename)
        {
            _filename = filename;
            _knownSyllablesDictionary = new Dictionary<string, string>();
        }

        public void ReadFromFile()
        {
            if (!File.Exists(_filename))
                throw new FileNotFoundException($"Couldn't find file [{_filename}]");

            _knownSyllablesDictionary.Clear();
            var lines = File.ReadLines(_filename);
            foreach (var line in lines)
            {
                var currentLineSplit = line.Split(':');
                var word = currentLineSplit[0].Trim();
                var syllablesNumber = currentLineSplit[1].Trim();
                if (!_knownSyllablesDictionary.ContainsKey(word))
                {
                    _knownSyllablesDictionary.Add(word, syllablesNumber);
                }
                else
                {
                    _knownSyllablesDictionary[word] = syllablesNumber;
                }
            }
        }

        private void LoadNewSyllablesFromList(Dictionary<string, string> newSyllables)
        {
            if (newSyllables == null)
                return;

            foreach (var newSyllable in newSyllables)
            {
                if (_knownSyllablesDictionary.ContainsKey(newSyllable.Key))
                {
                    _knownSyllablesDictionary[newSyllable.Key] = newSyllable.Value;
                    continue;
                }

                _knownSyllablesDictionary.Add(newSyllable.Key, newSyllable.Value);
            }
        }

        public void SaveToFile(Dictionary<string, string> newSyllables = null)
        {
            LoadNewSyllablesFromList(newSyllables);

            StringBuilder toFile = new StringBuilder();
            foreach (KeyValuePair<string, string> keyValuePair in _knownSyllablesDictionary)
            {
                toFile.AppendLine($"{keyValuePair.Key}:{keyValuePair.Value}");
            }

            File.WriteAllText(_filename, toFile.ToString());
        }

        public string GetSyllablesForWord(string word)
        {
            var wordToCheck = word.Trim();
            if (!_knownSyllablesDictionary.TryGetValue(wordToCheck, out string numberOfSyllables))
            {
                if (wordToCheck.Length == 2)
                {
                    _knownSyllablesDictionary.Add(wordToCheck, "1");
                    return "1";
                }
                else
                {
                    return NotKnown;
                }
            }

            return numberOfSyllables;
        }
    }
}
