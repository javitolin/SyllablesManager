using System.Collections.Generic;
using System.IO;
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

        public void LoadNewSyllablesFromList(Dictionary<string, string> newSyllables)
        {
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

        public void SaveToFile()
        {
            StringBuilder toFile = new StringBuilder();
            foreach (KeyValuePair<string, string> keyValuePair in _knownSyllablesDictionary)
            {
                toFile.AppendLine($"{keyValuePair.Key}:{keyValuePair.Value}");
            }

            File.WriteAllText(_filename, toFile.ToString());
        }

        public string GetSyllablesForWord(string word)
        {
            if (!_knownSyllablesDictionary.TryGetValue(word, out string numberOfSyllables))
                return NotKnown;

            return numberOfSyllables;
        }
    }
}
