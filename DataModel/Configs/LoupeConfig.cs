using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marmi.DataModel
{
    public class LoupeConfig
    {
        //ルーペ倍率
        public int LoupeMagnifcant { get; set; }

        // ルーペを原寸表示とするかどうか。
        public bool OriginalSizeLoupe { get; set; }

        public void Init()
        {
            LoupeMagnifcant = 3;
            OriginalSizeLoupe = true;
        }
    }
}
