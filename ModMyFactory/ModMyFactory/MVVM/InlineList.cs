using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ModMyFactory.MVVM
{
    class InlineList : Control
    {
        private class InlineCollectionWrapper : ObservableCollection<Inline>
        { }


        private object anchor;

        public ICollection<Inline> Inlines { get; }

        public InlineList()
        {
            var inlines = new InlineCollectionWrapper();
            inlines.CollectionChanged += OnInlineCollectionChanged;
            Inlines = inlines;
        }

        void OnInlineCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TextBlock textBlock = anchor as TextBlock;
            if (textBlock != null)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        textBlock.Inlines.AddRange(e.NewItems);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems)
                            textBlock.Inlines.Remove((Inline)item);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        if (e.NewItems != null)
                        {
                            textBlock.Inlines.AddRange(e.NewItems);
                        }
                        if (e.OldItems != null)
                        {
                            foreach (var item in e.OldItems)
                                textBlock.Inlines.Remove((Inline)item);
                        }
                        break;
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            anchor = Template?.FindName("PART_ContentHost", this);
            TextBlock textBlock = anchor as TextBlock;
            textBlock?.Inlines.AddRange(Inlines);
        }
    }
}
