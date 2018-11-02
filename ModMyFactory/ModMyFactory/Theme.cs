using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory
{
    sealed class Theme : NotifyPropertyChangedBase
    {
        static readonly List<Theme> themes;

        public static IReadOnlyCollection<Theme> AvailableThemes { get; }

        public static Theme Light { get; }

        public static Theme Dark { get; }

        static Theme()
        {
            themes = new List<Theme>();

            Light = new Theme("light");
            Dark = new Theme("dark");

            AvailableThemes = new ReadOnlyCollection<Theme>(themes);
        }

        bool selected;

        public string Name { get; }

        public bool Selected
        {
            get => selected;
            set
            {
                if (value != selected)
                {
                    selected = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Selected)));

                    if (selected)
                    {
                        themes.ForEach(theme => { if (theme != this) theme.Selected = false; });
                        App.Instance.SetTheme(Name);
                    }
                }
            }
        }

        public BitmapImage Image { get; }

        public ICommand SelectCommand { get; }

        private Theme(string name)
        {
            Name = name;
            Image = new BitmapImage(new Uri($"../Images/Themes/{name}.png", UriKind.Relative));
            SelectCommand = new RelayCommand(() => Selected = true);
            themes.Add(this);

            if (App.Instance.Settings.Theme == Name)
            {
                selected = true;
                App.Instance.SetTheme(Name);
            }
        }

        public void OnLocalizedNameChanged()
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Name)));
        }
    }
}
