using System;
using System.Windows;
using System.Windows.Controls;

namespace ModMyFactory.Controls
{
    /// <summary>
    /// Interaktionslogik für NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata(decimal.Zero, OnValueChanged, CoerceValue));
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata(decimal.MinValue, OnMinimumChanged, CoerceMinimum));
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata(decimal.MaxValue, OnMaximumChanged, CoerceMaximum));
        public static readonly DependencyProperty StepProperty = DependencyProperty.Register("Step", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata(decimal.One, OnStepChanged));

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var nud = (NumericUpDown)sender;
            nud.TextBox.Text = ((decimal)e.NewValue).ToString();
            nud.OnValueChanged(e);
        }

        private static void OnMinimumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => ((NumericUpDown)sender).OnMinimumChanged(e);

        private static void OnMaximumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => ((NumericUpDown)sender).OnMaximumChanged(e);

        private static void OnStepChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => ((NumericUpDown)sender).OnStepChanged(e);

        private static object CoerceValue(DependencyObject sender, object value)
        {
            var nud = (NumericUpDown)sender;
            return Math.Min(nud.Maximum, Math.Max(nud.Minimum, (decimal)value));
        }

        private static object CoerceMinimum(DependencyObject sender, object value)
        {
            var nud = (NumericUpDown)sender;
            return Math.Min(nud.Maximum, (decimal)value);
        }

        private static object CoerceMaximum(DependencyObject sender, object value)
        {
            var nud = (NumericUpDown)sender;
            return Math.Max(nud.Minimum, (decimal)value);
        }


        public event DependencyPropertyChangedEventHandler ValueChanged;
        public event DependencyPropertyChangedEventHandler MinimumChanged;
        public event DependencyPropertyChangedEventHandler MaximumChanged;
        public event DependencyPropertyChangedEventHandler StepChanged;

        public decimal Value
        {
            get => (decimal)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public decimal Minimum
        {
            get => (decimal)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public decimal Maximum
        {
            get => (decimal)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public decimal Step
        {
            get => (decimal)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        protected virtual void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        protected virtual void OnMinimumChanged(DependencyPropertyChangedEventArgs e)
        {
            MinimumChanged?.Invoke(this, e);
        }

        protected virtual void OnMaximumChanged(DependencyPropertyChangedEventArgs e)
        {
            MaximumChanged?.Invoke(this, e);
        }

        protected virtual void OnStepChanged(DependencyPropertyChangedEventArgs e)
        {
            StepChanged?.Invoke(this, e);
        }

        public NumericUpDown()
        {
            InitializeComponent();
            TextBox.Text = Value.ToString();
        }

        private void UpButtonClickHandler(object sender, RoutedEventArgs e)
        {
            Value += Step;
        }

        private void DownButtonClickHandler(object sender, RoutedEventArgs e)
        {
            Value -= Step;
        }

        private void TextBoxLostFocusHandler(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TextBox.Text, out var result)) Value = result;
            TextBox.Text = Value.ToString();
        }
    }
}
