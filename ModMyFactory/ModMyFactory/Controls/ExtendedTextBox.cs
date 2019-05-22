using System.Windows;
using System.Windows.Controls;

namespace ModMyFactory.Controls
{
    class ExtendedTextBox : TextBox
    {
        const string DefaultDefaultText = "Default";

        public static readonly DependencyProperty AllowEmptyTextProperty = DependencyProperty.Register("AllowEmptyText", typeof(bool), typeof(ExtendedTextBox), new PropertyMetadata(true, OnAllowEmptyTextChanged));
        public static readonly DependencyProperty DefaultTextProperty = DependencyProperty.Register("DefaultText", typeof(string), typeof(ExtendedTextBox), new PropertyMetadata(DefaultDefaultText, OnDefaultTextChanged, CoerceDefaultText));

        private static void OnAllowEmptyTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var etb = (ExtendedTextBox)sender;
            if (!(bool)e.NewValue && string.IsNullOrEmpty(etb.Text)) etb.Text = etb.DefaultText;
            etb.OnAllowEmptyTextChanged(e);
        }

        private static void OnDefaultTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var etb = (ExtendedTextBox)sender;
            etb.OnDefaultTextChanged(e);
        }

        private static object CoerceDefaultText(DependencyObject sender, object value)
        {
            var s = (string)value;
            if (string.IsNullOrEmpty(s)) return DefaultDefaultText;
            else return s;
        }


        public event DependencyPropertyChangedEventHandler AllowEmptyTextChanged;
        public event DependencyPropertyChangedEventHandler DefaultTextChanged;

        public bool AllowEmptyText
        {
            get => (bool)GetValue(AllowEmptyTextProperty);
            set => SetValue(AllowEmptyTextProperty, value);
        }

        public string DefaultText
        {
            get => (string)GetValue(DefaultTextProperty);
            set => SetValue(DefaultTextProperty, value);
        }

        protected virtual void OnAllowEmptyTextChanged(DependencyPropertyChangedEventArgs e)
        {
            AllowEmptyTextChanged?.Invoke(this, e);
        }

        protected virtual void OnDefaultTextChanged(DependencyPropertyChangedEventArgs e)
        {
            DefaultTextChanged?.Invoke(this, e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (!AllowEmptyText && string.IsNullOrEmpty(Text)) Text = DefaultText;
        }
    }
}
