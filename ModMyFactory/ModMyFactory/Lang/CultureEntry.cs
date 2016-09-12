using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace ModMyFactory.Lang
{
    /// <summary>
    /// Represents an entry in the language menu.
    /// </summary>
    class CultureEntry : NotifyPropertyChangedBase
    {
        static CultureEntry selectedEntry;

        readonly CultureInfo culture;
        bool isSelected;

        /// <summary>
        /// The name of the language.
        /// </summary>
        public string Name => culture.NativeName;

        /// <summary>
        /// The english name of the language.
        /// </summary>
        public string EnglishName => "(" + culture.EnglishName + ")";

        /// <summary>
        /// Indicates whether this language is currently selected.
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            private set
            {
                isSelected = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        /// <summary>
        /// The flag corresponding to the language.
        /// </summary>
        public BitmapImage Flag { get; }

        public RelayCommand SelectCommand { get; }

        public CultureEntry(CultureInfo culture)
        {
            this.culture = culture;
            Flag = new BitmapImage(new Uri($"Images/{culture.TwoLetterISOLanguageName}.png", UriKind.Relative));
            SelectCommand = new RelayCommand(Select);
        }

        /// <summary>
        /// Selects this language and deselects the previously selected language.
        /// </summary>
        public void Select()
        {
            if (selectedEntry != this)
            {
                if (selectedEntry != null) selectedEntry.IsSelected = false;
                selectedEntry = this;
                this.IsSelected = true;
                App.Instance.SelectCulture(culture);
            }
        }
    }
}
