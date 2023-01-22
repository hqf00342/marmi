using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Marmi
{
	class KeyConfigControl : ComboBox
	{
		public KeyConfigControl()
		{
			//ドロップダウンスタイルに
			this.DropDownStyle = ComboBoxStyle.DropDownList;

			//アイテムをDictionaryのKeyで追加
			//this.Items.Clear();
			//foreach (string s in Form1.keyConfigList.Keys)
			//    this.Items.Add(s);
		}

	}
}
