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
        private KnownSyllables _knownSyllables;
        private readonly FileReader _fileReader = new FileReader();
        private ObservableCollection<FoundWordViewItem> _foundWords = new ObservableCollection<FoundWordViewItem>();
        private bool _knownSyllablesLoaded;

        public ObservableCollection<FoundWordViewItem> FoundWords
        {
            get => _foundWords;
            set
            {
                if (Equals(value, _foundWords)) return;
                _foundWords = value;
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
                MessageBox.Show($"There was an error loading file [{chosenFile}]. [{e1}]");
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

            if (FoundWords.Count > 0)
            {
                var deleteLoadedFiles = MessageBox.Show("Delete loaded files?", "", MessageBoxButton.YesNo);
                if (deleteLoadedFiles == MessageBoxResult.Yes)
                    FoundWords.Clear();
            }

            var newWords = new List<FoundWordViewItem>();
            newWords.AddRange(FoundWords);
            var wordsFromFile = _fileReader.GetWordsFromFile(chosenFile);
            foreach (var wordFromFile in wordsFromFile)
            {
                var syllables = _knownSyllables.GetSyllablesForWord(wordFromFile);
                newWords.Add(new FoundWordViewItem(wordFromFile, syllables));
            }

            var newWordsOrdered = newWords.OrderBy(nw => nw.Syllables).ToList();

            newWordsOrdered.ForEach(FoundWords.Add);
            OnPropertyChanged(nameof(FoundWords));
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

            _knownSyllables.LoadNewSyllablesFromList(FoundWords.ToDictionary(f => f.Word, f => f.Syllables));
            _knownSyllables.SaveToFile();
        }
        private void GetSyllablesCount_Click(object sender, RoutedEventArgs e)
        {
            if (_knownSyllables == null)
            {
                MessageBox.Show("Please load known syllables first");
                return;
            }

            if (FoundWords.Count == 0)
            {
                MessageBox.Show("Please load a text file first");
                return;
            }

            if (FoundWords.Any(f => f.Syllables == -1))
            {
                MessageBox.Show("There are words without syllable number");
                return;
            }

            int totalSyllables = FoundWords.Sum(f => f.Syllables);
            MessageBox.Show($"Total syllables: [{totalSyllables}]");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
