using System.ComponentModel;
using System.Runtime.CompilerServices;
using SyllablesManager.UI.Annotations;

namespace SyllablesManager.UI
{
    public class FoundWordViewItem : INotifyPropertyChanged
    {
        private string _word;
        private int _syllables;

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

        public int Syllables
        {
            get => _syllables;
            set
            {
                if (value == _syllables) return;
                _syllables = value;
                OnPropertyChanged();
            }
        }

        public FoundWordViewItem(string word, int syllables)
        {
            Word = word;
            Syllables = syllables;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
