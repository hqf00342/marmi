/****************************************************************************
Form1.cs
static methods
****************************************************************************/

using System;
using System.Windows.Forms;

namespace Marmi;

public partial class Form1
{
    /// <summary>
    /// 指定されたインデックスから２枚表示できるかチェック
    /// チェックはImageInfoに取り込まれた値を利用、縦横比で確認する。
    /// </summary>
    /// <param name="index">インデックス値</param>
    /// <returns>2画面表示できるときはtrue</returns>
    public static bool CanDualView(int index)
    {
        //これ以上大きなサイズの画像は2枚並べしない
        const int MAX_WIDTH = 3000;
        const int MAX_HEIGHT = 3000;

        //コンフィグ条件を確認
        if (!App.Config.DualView)
            return false;

        //最後のページか確認
        if (index == App.TotalPages - 1)
            return false;

        //マイナスかどうか
        if (index < 0)
            return false;

        //強制2ページ表示
        if (App.Config.Draw.DualView_Force)
            return true;

        //1枚目チェック
        if (_check1page(index) == false)
            return false;

        //２枚目チェック
        if (_check1page(index + 1) == false)
            return false;

        //2枚とも縦長だった

        //縦長2枚でOKの場合
        if (App.Config.Draw.DualView_Normal)
            return true;

        //2画像の高さがほぼ一緒ならrue
        const int ACCEPTABLE_RANGE = 200;
        return Math.Abs(App.g_pi.Items[index].ImgSize.Height - App.g_pi.Items[index + 1].ImgSize.Height) < ACCEPTABLE_RANGE;

        //1ページ分チェックする
        //2枚並べられないときはfalseを返す
        static bool _check1page(int index)
        {
            //読み込み出来ていなければ待つ
            if (!App.g_pi.Items[index].HasImage)
            {
                //Bmp.GetBitmapForce(index);
                return false;
            }

            //横長だった
            if (App.g_pi.Items[index].IsFat)
                return false;

            //大きい画像
            if ((App.g_pi.Items[index].ImgSize.Width > MAX_WIDTH
                || App.g_pi.Items[index].ImgSize.Height > MAX_HEIGHT))
                return false;

            return true;
        }
    }

    private static string[] OpenDialog()
    {
        var of = new OpenFileDialog
        {
            DefaultExt = "zip",
            FileName = "",
            Filter = "対応ファイル形式(書庫ファイル;画像ファイル)|*.zip;*.lzh;*.tar;*.rar;*.7z;*.jpg;*.bmp;*.gif;*.ico;*.png;*.jpeg|"
            + "書庫ファイル|*.zip;*.lzh;*.tar;*.rar;*.7z|"
            + "画像ファイル|*.jpg;*.bmp;*.gif;*.ico;*.png;*.jpeg|"
            + "すべてのファイル|*.*",
            FilterIndex = 1,
            CheckFileExists = true,
            Multiselect = true,
            RestoreDirectory = true
        };

        if (of.ShowDialog() == DialogResult.OK)
        {
            //ver1.09 OpenFileAndStart()とりやめに伴い展開
            //OpenFileAndStart(of.FileName);
            return of.FileNames;
        }
        return null;
    }

    private static void ToggleBookmark()
    {
        if (App.TotalPages > 0 && App.CurrentPage >= 0)
        {
            App.g_pi.Items[App.CurrentPage].IsBookMark
                = !App.g_pi.Items[App.CurrentPage].IsBookMark;

            // 2枚表示なら2枚目もブックマーク
            if (App.CurrentShowPages == 2)
            {
                App.g_pi.Items[App.CurrentPage + 1].IsBookMark
                    = !App.g_pi.Items[App.CurrentPage + 1].IsBookMark;
            }
        }
    }
}