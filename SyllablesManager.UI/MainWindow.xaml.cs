using Microsoft.Win32;
using SyllablesManager.DataAccess;
using SyllablesManager.Entities;
using SyllablesManager.UI.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SyllablesManager.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string Caption = "Syllables Helper";
        private KnownSyllables _knownSyllables;
        private readonly FileReader _fileReader = new FileReader();
        private List<FoundWordViewItem> _unknownSyllablesWords = new List<FoundWordViewItem>();
        private bool _knownSyllablesLoaded;
        private bool _wasSaved = false;
        private int _syllablesCount;
        private readonly Dictionary<string, int> _loadedWordsRepetitions = new Dictionary<string, int>();
        private string _logContent;
        public string WantedFileToLoad = "";

        public int SyllablesCount
        {
            get => _syllablesCount;
            set
            {
                if (value == _syllablesCount) return;
                _syllablesCount = value;
                OnPropertyChanged();
            }
        }

        public List<FoundWordViewItem> UnknownSyllablesWords
        {
            get => _unknownSyllablesWords.Where(s => s.ShowToUser).OrderBy(s => s.Word).ToList();
            set
            {
                if (Equals(value, _unknownSyllablesWords)) return;
                _unknownSyllablesWords = value;
                OnPropertyChanged();
            }
        }

        public bool KnownSyllablesLoaded
        {
            get => _knownSyllablesLoaded;
            set
            {
                if (value == _knownSyllablesLoaded) return;
                _knownSyllablesLoaded = value;
                OnPropertyChanged();
            }
        }

        public string LogContent
        {
            get => _logContent;
            set
            {
                if (value == _logContent) return;
                _logContent = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            WriteToLog("Started");
        }

        private void LoadKnowSyllablesBtn_Click(object sender, RoutedEventArgs e)
        {
            var chosenFile = GetFileFromUser();
            if (chosenFile == null)
                return;

            _knownSyllables = new KnownSyllables(chosenFile);
            try
            {
                _knownSyllables.ReadFromFile();
                KnownSyllablesLoaded = true;
                WriteToLog($"Known syllables loaded from file [{chosenFile}]");
            }
            catch (Exception e1)
            {
                WriteToLog($"There was an error loading file [{chosenFile}]. [{e1}]");
            }
        }

        private void LoadTextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_knownSyllables == null)
            {
                WriteToLog("Please load known syllables first");
                return;
            }

            var chosenFile = GetFileFromUser();
            if (chosenFile == null)
                return;

            LoadWordsFromFile(chosenFile);
        }

        private void LoadWordsFromFile(string? chosenFile)
        {
            var wordsFromFile = _fileReader.GetWordsFromFile(chosenFile);
            AddWords(wordsFromFile);
        }

        private void AddWords(IEnumerable<string> wordsFromFile)
        {
            var newWords = new List<FoundWordViewItem>();

            if (_unknownSyllablesWords.Count > 0)
            {
                var deleteLoadedFiles = MessageBox.Show("Delete loaded files?", Caption, MessageBoxButton.YesNo);
                if (deleteLoadedFiles == MessageBoxResult.Yes)
                {
                    _unknownSyllablesWords.Clear();
                    _loadedWordsRepetitions.Clear();
                }
                else
                {
                    _unknownSyllablesWords.Clear();
                    newWords.AddRange(_unknownSyllablesWords);
                }
            }

            foreach (var wordFromFile in wordsFromFile)
            {
                if (!_loadedWordsRepetitions.ContainsKey(wordFromFile))
                {
                    _loadedWordsRepetitions.Add(wordFromFile, 1);
                }
                else
                {
                    _loadedWordsRepetitions[wordFromFile]++;
                }
            }

            foreach (var loadedWordsRepetition in _loadedWordsRepetitions)
            {
                var currentWord = loadedWordsRepetition.Key;
                var repetitions = loadedWordsRepetition.Value;
                var syllables = _knownSyllables.GetSyllablesForWord(currentWord);
                FoundWordViewItem foundWord = null;
                if (syllables != KnownSyllables.NotKnown)
                {
                    foundWord = new FoundWordViewItem(currentWord, syllables, false, repetitions);
                    WriteToLog($"Word [{currentWord}] is already known. It has [{syllables}] syllables. It appears [{repetitions}] times in the text");
                }
                else
                {
                    foundWord = new FoundWordViewItem(currentWord, KnownSyllables.NotKnown, true, repetitions);
                }

                newWords.Add(foundWord);
            }

            if (newWords.Count == 0)
            {
                WriteToLog("Found no new words");
                GetSyllablesCount_Click(null, null);
                return;
            }

            WriteToLog("Input text loaded");

            UnknownSyllablesWords = newWords;
            var knownSyllablesSum = _unknownSyllablesWords.Where(f => f.ShowToUser == false).Sum(f => int.Parse(f.Syllables) * f.Repetitions);
            WriteToLog($"Known syllables are: [{knownSyllablesSum}]");
        }

        private string? GetFileFromUser()
        {
            OpenFileDialog opf = new OpenFileDialog { Multiselect = false };
            var isSelected = opf.ShowDialog();
            if (isSelected == null || !isSelected.Value)
                return null;

            return opf.FileName;
        }

        private void SaveLoaded_Click(object sender, RoutedEventArgs e)
        {
            if (_knownSyllables == null)
            {
                WriteToLog("Please load known syllables first");
                return;
            }

            _knownSyllables.SaveToFile(_unknownSyllablesWords
                .Where(f => f.Syllables != KnownSyllables.NotKnown)
                .ToDictionary(f => f.Word, f => f.Syllables));

            WriteToLog("File was saved successfully");
            _wasSaved = true;
        }

        private bool CheckInput()
        {
            if (_knownSyllables == null)
            {
                WriteToLog("Please load known syllables first");
                return false;
            }

            if (_unknownSyllablesWords.Count == 0)
            {
                WriteToLog("Please load a text file first");
                return false;
            }

            if (_unknownSyllablesWords.Any(f => f.Syllables == KnownSyllables.NotKnown))
            {
                WriteToLog("There are words without syllable number");
                return false;
            }

            return true;
        }
        private void GetSyllablesCount_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckInput())
                return;

            SyllablesCount = _unknownSyllablesWords.Sum(f => int.Parse(f.Syllables) * f.Repetitions);
            WriteToLog($"Total syllables: [{SyllablesCount}]");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_wasSaved && _knownSyllables != null)
            {
                var userAnswer = MessageBox.Show("Save before closing?", Caption, MessageBoxButton.YesNo);
                if (userAnswer == MessageBoxResult.Yes)
                {
                    SaveLoaded_Click(null, null);
                }
            }

            base.OnClosing(e);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            string knownSyllablesFilepath = "KnownSyllables.txt";
            if (File.Exists(knownSyllablesFilepath))
            {
                _knownSyllables = new KnownSyllables(knownSyllablesFilepath);
                _knownSyllables.ReadFromFile();
                KnownSyllablesLoaded = true;
                WriteToLog($"Known syllables loaded automatically from file [{knownSyllablesFilepath}]");
            }

            if (KnownSyllablesLoaded && !string.IsNullOrWhiteSpace(WantedFileToLoad))
            {
                LoadWordsFromFile(WantedFileToLoad);
            }
        }

        private void WriteToLog(string message)
        {
            LogContent += message + Environment.NewLine;
            LogScrollViewer.ScrollToEnd();
        }

        private void CalculateWithTimeBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CheckInput())
                return;

            SyllablesCount = _unknownSyllablesWords.Sum(f => int.Parse(f.Syllables) * f.Repetitions);
            WriteToLog($"Total syllables: [{SyllablesCount}]");

            if (!int.TryParse(InputTimeTxt.Text, out var seconds))
            {
                MessageBox.Show($"Wrong number of seconds: [{InputTimeTxt.Text}]", Caption);
                return;
            }

            WriteToLog($"Calculation is: [{(double)SyllablesCount / seconds}]");
        }

        private void UseCopiedText_OnClick(object sender, RoutedEventArgs e)
        {
            var text = Clipboard.GetText();
            var words = _fileReader.GetWordsFromText(text);
            AddWords(words);
        }
    }
}
