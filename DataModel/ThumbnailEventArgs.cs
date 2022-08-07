using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
サムネイル専用イベントの定義
  サムネイル中にマウスホバーが起きたときのためのイベント
  このイベントはThumbnailPanel::ThumbnailPanel_MouseMove()で発生している
  受ける側はこのEventArgsを使って受けるとアイテムが分かる。
*/
namespace Marmi
{
    public class ThumbnailEventArgs : EventArgs
    {
        /// <summary>Hover中のアイテム番号</summary>
        public int HoverItemNumber;

        /// <summary>Hover中のアイテム名</summary>
        public string HoverItemName;
    }
}
