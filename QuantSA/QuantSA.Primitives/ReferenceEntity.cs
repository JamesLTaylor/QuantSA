using System;

namespace QuantSA.Primitives
{
    /// <summary>
    /// A company that may be used in some credit derivatives and whose default time will be required.
    /// </summary>
    [Serializable]
    public class ReferenceEntity
    {
        private string name;

        /// <summary>
        /// Creates a new ReferenceEntity from the name of a company.
        /// </summary>
        /// <param name="name">The name of the company.  Keep this short and consistent.</param>
        public ReferenceEntity(string name)
        { this.name = name;        }

        public override string ToString()
        {
            return name.ToUpper();
        }
    }
}
