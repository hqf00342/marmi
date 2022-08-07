using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marmi.DataModel
{
    public class AdvanceConfig
    {
        //memModel == userDefinedのときのキャッシュサイズ[MB]
        public int CacheSize { get; set; }                       


        public void Init()
        {
            //ver1.53 100MB
            CacheSize = 100;

        }
    }
}
