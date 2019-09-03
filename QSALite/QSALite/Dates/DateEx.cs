namespace QSALite.Dates
{
    public static class DateEx
    {
        /// <summary>
        /// is <paramref name="date1" /> on before <paramref name="date2" />.
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static bool OnOrBefore(this Date date1, Date date2)
        {
            return date1.SerialNumber <= date2.SerialNumber;
        }
    }
}