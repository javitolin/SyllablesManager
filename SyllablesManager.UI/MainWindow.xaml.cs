using Microsoft.Win32;
using SyllablesManager.DataAccess;
using SyllablesManager.Entities;
using SyllablesManager.UI.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private ObservableCollection<FoundWordViewItem> _unknownSyllablesWords = new ObservableCollection<FoundWordViewItem>();
        private bool _knownSyllablesLoaded;
        private bool _wasSaved = false;
        private string _syllablesCount;

        public string SyllablesCount
        {
            get => _syllablesCount;
            set
            {
                if (value == _syllablesCount) return;
                _syllablesCount = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FoundWordViewItem> UnknownSyllablesWords
        {
            get => _unknownSyllablesWords;
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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
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
            }
            catch (Exception e1)
            {
                MessageBox.Show($"There was an error loading file [{chosenFile}]. [{e1}]", Caption);
            }
        }

        private void LoadTextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_knownSyllables == null)
            {
                MessageBox.Show("Please load known syllables first");
                return;
            }

            var chosenFile = GetFileFromUser();
            if (chosenFile == null)
                return;

            if (UnknownSyllablesWords.Count > 0)
            {
                var deleteLoadedFiles = MessageBox.Show("Delete loaded files?", Caption, MessageBoxButton.YesNo);
                if (deleteLoadedFiles == MessageBoxResult.Yes)
                    UnknownSyllablesWords.Clear();
            }

            var newWords = new List<FoundWordViewItem>();
            var wordsFromFile = _fileReader.GetWordsFromFile(chosenFile);
            foreach (var wordFromFile in wordsFromFile)
            {
                var syllables = _knownSyllables.GetSyllablesForWord(wordFromFile);
                if (syllables != KnownSyllables.NotKnown)
                {
                    SyllablesCount += syllables;
                }

                var foundWord = new FoundWordViewItem(wordFromFile, "" + syllables);
                newWords.Add(foundWord);
            }

            if (newWords.Count == 0)
            {
                GetSyllablesCount_Click(sender, e);
                return;
            }

            newWords.ForEach(UnknownSyllablesWords.Add);
            OnPropertyChanged(nameof(UnknownSyllablesWords));
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
                MessageBox.Show("Please load known syllables first");
                return;
            }

            _knownSyllables.LoadNewSyllablesFromList(UnknownSyllablesWords.ToDictionary(f => f.Word, f => f.Syllables));
            _knownSyllables.SaveToFile();
            _wasSaved = true;
        }

        private void GetSyllablesCount_Click(object sender, RoutedEventArgs e)
        {
            if (_knownSyllables == null)
            {
                MessageBox.Show("Please load known syllables first", Caption);
                return;
            }

            if (UnknownSyllablesWords.Count == 0)
            {
                MessageBox.Show("Please load a text file first", Caption);
                return;
            }

            if (UnknownSyllablesWords.Any(f => f.Syllables == KnownSyllables.NotKnown))
            {
                MessageBox.Show("There are words without syllable number", Caption);
                return;
            }

            int totalSyllables = UnknownSyllablesWords.Sum(f => int.Parse(f.Syllables));
            SyllablesCount += totalSyllables;
            MessageBox.Show($"Total syllables: [{SyllablesCount}]");
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
            if (!_wasSaved)
            {
                var userAnswer = MessageBox.Show("Forgot to save?", Caption, MessageBoxButton.YesNo);
                if (userAnswer == MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }

            base.OnClosing(e);
        }
    }
}
