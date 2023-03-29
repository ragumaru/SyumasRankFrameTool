using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyumasTool;

internal class GenJogaiImage
{
    class JogaiRowData
    {
        DataRow Row { get; }

        /// <summary>除外理由</summary>
        string ExclusionReason => Row[0].ToString() ?? "";


        public JogaiRowData(DataRow row)
        {
            Row = row;
        }
    }

    internal void Gen(string outputPath, DataTable jogaiTable) { }
}
