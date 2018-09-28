using ModMyFactory.Helpers;
using System.Collections.Specialized;
using System.Linq;

namespace ModMyFactory.Models
{
    sealed class LatestFactorioVersion : SpecialFactorioVersion
    {
        readonly FactorioCollection collection;

        protected override string LoadName()
        {
            return App.Instance.GetLocalizedResourceString("LatestFactorioName");
        }

        private void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            WrappedVersion = collection.Where(item => !(item is SpecialFactorioVersion)).MaxBy(item => item.Version);
        }

        public LatestFactorioVersion(FactorioCollection collection)
            : base(null)
        {
            this.collection = collection;
            this.collection.CollectionChanged += CollectionChangedHandler;
        }
    }
}
