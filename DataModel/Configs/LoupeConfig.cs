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
        public int loupeMagnifcant;

        // ルーペを原寸表示とするかどうか。
        public bool IsOriginalSizeLoupe { get; set; }

        public void Init()
        {
            loupeMagnifcant = 3;
            IsOriginalSizeLoupe = true;
        }
    }
}
