using System.Collections.Generic;

namespace QSALite.Products
{
    public class Portfolio
    {
        public List<NettingSet> NettingSets;
    }

    public class NettingSet
    {
        public List<CollateralSet> CollateralSets;
    }

    public class CollateralSet
    {
        public List<Structure> Structures;
    }

    public class Structure
    {
        public List<IProduct> Products;
    }
}