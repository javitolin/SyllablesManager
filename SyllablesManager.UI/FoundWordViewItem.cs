using SyllablesManager.UI.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SyllablesManager.UI
{
    public class FoundWordViewItem : INotifyPropertyChanged
    {
        private string _word;
        private string _syllables;
        private bool _showToUser;
        private int _repetitions;

        public string Word
        {
            get => _word;
            set
            {
                if (value == _word) return;
                _word = value;
                OnPropertyChanged();
            }
        }

        public string Syllables
        {
            get => _syllables;
            set
            {
                if (value == _syllables) return;
                _syllables = value;
                OnPropertyChanged();
            }
        }

        public bool ShowToUser
        {
            get => _showToUser;
            set
            {
                if (value == _showToUser) return;
                _showToUser = value;
                OnPropertyChanged();
            }
        }

        public int Repetitions
        {
            get => _repetitions;
            set
            {
                if (value == _repetitions) return;
                _repetitions = value;
                OnPropertyChanged();
            }
        }

        public FoundWordViewItem(string word, string syllables, bool showToUser, int repetitions)
        {
            Word = word;
            Syllables = syllables;
            ShowToUser = showToUser;
            Repetitions = repetitions;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
