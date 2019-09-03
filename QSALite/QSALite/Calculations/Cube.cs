using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSALite.Calculations
{
    public class Cube
    {
        private readonly List<CubeColumnType> _columnTypes = new List<CubeColumnType>();
        private readonly List<string> _headers = new List<string>();
        private readonly List<List<object>> _rows = new List<List<object>>();

        public void AddColumn(string header, CubeColumnType columnType)
        {
            _columnTypes.Add(columnType);
            _headers.Add(header);
        }

        public int AddRow()
        {
            _rows.Add(_headers.Select<string, object>(h => null).ToList());
            return _rows.Count - 1;
        }

        public void Set(int row, string header, object value)
        {
            var col = _headers.IndexOf(header);
            _rows[row][col] = value;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", _headers));
            foreach (var row in _rows) sb.AppendLine(string.Join(",", row));

            return sb.ToString();
        }
    }
}