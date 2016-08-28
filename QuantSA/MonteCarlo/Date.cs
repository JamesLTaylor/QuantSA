using System;

namespace MonteCarlo
{
    public class Date
    {
        public DateTime date { get; private set; }

        public Date(DateTime date)
        {
            this.date = date.Date;
        }
    }
}