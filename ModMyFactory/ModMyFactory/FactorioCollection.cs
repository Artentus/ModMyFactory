using ModMyFactory.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModMyFactory
{
    sealed class FactorioCollection : ObservableCollection<FactorioVersion>
    {
        public FactorioCollection()
            : base()
        {
            this.Add(new LatestFactorioVersion(this));
        }

        public FactorioCollection(IEnumerable<FactorioVersion> collection)
            : base(collection)
        {
            this.Add(new LatestFactorioVersion(this));
        }

        public FactorioCollection(List<FactorioVersion> list)
            : base(list)
        {
            this.Add(new LatestFactorioVersion(this));
        }
    }
}
