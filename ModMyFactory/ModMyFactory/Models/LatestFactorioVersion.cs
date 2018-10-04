using ModMyFactory.Helpers;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ModMyFactory.Models
{
    sealed class LatestFactorioVersion : SpecialFactorioVersion
    {
        readonly FactorioCollection collection;

        public override string DisplayName => App.Instance.GetLocalizedResourceString("LatestFactorioName");

        protected override string LoadName()
        {
            return "Latest";
        }

        private void SetWrappedVersion()
        {
            WrappedVersion = collection.Where(item => !(item is SpecialFactorioVersion)).MaxBy(item => item.Version);
        }

        private void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (FactorioVersion item in e.NewItems)
                {
                    if (!(item is SpecialFactorioVersion))
                        item.PropertyChanged += ItemPropertyChangedHandler;
                }
            }

            if ((e.Action == NotifyCollectionChangedAction.Remove) || (e.Action == NotifyCollectionChangedAction.Reset))
            {
                foreach (FactorioVersion item in e.OldItems)
                {
                    if (!(item is SpecialFactorioVersion))
                        item.PropertyChanged -= ItemPropertyChangedHandler;
                }
            }

            SetWrappedVersion();
        }

        private void ItemPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FactorioVersion.Version))
                SetWrappedVersion();
        }

        public LatestFactorioVersion(FactorioCollection collection)
            : base(null)
        {
            this.collection = collection;
            this.collection.CollectionChanged += CollectionChangedHandler;
        }
    }
}
