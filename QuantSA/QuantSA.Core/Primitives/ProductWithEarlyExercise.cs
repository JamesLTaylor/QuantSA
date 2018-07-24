using System.Collections.Generic;
using QuantSA.General;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Primitives
{
    public abstract class ProductWithEarlyExercise : Product, IProductWithEarlyExercise
    {
        public abstract List<IProduct> GetPostExProducts();
        public abstract List<Date> GetExerciseDates();
        public abstract int GetPostExProductAtDate(Date exDate);
        public abstract bool IsLongOptionality(Date exDate);
    }
}