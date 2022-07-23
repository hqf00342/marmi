/****************************************************************************
Form1.cs
ナビゲーション関連
****************************************************************************/

using System.Diagnostics;

namespace Marmi;

public partial class Form1
{
    private void NavigateToBack()
    {
        //前に戻る
        int prev = GetPrevPageIndex(App.CurrentPage);
        if (prev >= 0)
            SetViewPage(prev);
        else
            _clearPanel.ShowAndClose("先頭のページです", 1000);
    }

    private void NavigateToForword()
    {
        //ver1.35 ループ機能を実装
        int now = App.CurrentPage;
        int next = GetNextPageIndex(App.CurrentPage);
        Debug.WriteLine($"NavigateToForword() {now} -> {next}");
        if (next >= 0)
        {
            SetViewPage(next);
        }
        else if (App.Config.Draw.LastPage_toTop)
        {
            //先頭ページへループ
            SetViewPage(0);
            _clearPanel.ShowAndClose("先頭ページに戻りました", 1000);
        }
        //2022年5月20日 次の書庫へオプションを廃止
        //else if (App.Config.Draw.LastPage_toNextArchive)
        //{
        //    //ver1.70 最終ページで次の書庫を開く
        //    if (App.g_pi.PackType != PackageType.Directory)
        //    {
        //        //次の書庫を探す
        //        string filename = App.g_pi.PackageName;
        //        string dirname = Path.GetDirectoryName(filename);
        //        string[] files = Directory.GetFiles(dirname);

        //        //ファイル名でソートする
        //        //Array.Sort(files, Uty.Compare_unsafeFast);
        //        Array.Sort(files, NaturalStringComparer.CompareS);

        //        bool match = false;
        //        foreach (var s in files)
        //        {
        //            if (s == filename)
        //            {
        //                match = true;
        //                continue;
        //            }
        //            if (match)
        //            {
        //                if (Uty.IsAvailableFile(s))
        //                {
        //                    _clearPanel.ShowAndClose("次へ移動します：" + Path.GetFileName(s), 1000);
        //                    Start(new string[] { s });
        //                    return;
        //                }
        //            }
        //        }
        //        _clearPanel.ShowAndClose("最後のページです。次の書庫が見つかりませんでした", 1000);
        //    }
        //    else
        //    {
        //        //先頭ページへループ
        //        SetViewPage(0);
        //        _clearPanel.ShowAndClose("先頭ページに戻りました", 1000);
        //    }
        //}
        else //if(App.Config.lastPage_stay)
        {
            _clearPanel.ShowAndClose("最後のページです", 1000);
        }
    }

    /// <summary>
    /// 最終ページを見ているかどうか確認。２ページ表示に対応
    /// 先頭ページはそのまま０かどうかチェックするだけなので作成しない。
    /// </summary>
    /// <returns>最終ページであればtrue</returns>
    private static bool IsLastPageViewing()
    {
        if (string.IsNullOrEmpty(App.g_pi.PackageName))
            return false;
        if (App.TotalPages <= 1)
            return false;
        return App.CurrentPage + App.CurrentShowPages >= App.TotalPages;
    }

    //ver1.35 前のページ番号。すでに先頭ページなら-1
    private static int GetPrevPageIndex(int index)
    {
        if (index > 0)
        {
            int declimentPages = -1;
            //2ページ減らすことが出来るか
            if (CanDualView(App.CurrentPage - 2))
                declimentPages = -2;

            int ret = index + declimentPages;
            return ret >= 0 ? ret : 0;
        }
        else
        {
            //すでに先頭ページなので-1を返す
            return -1;
        }
    }

    //ver1.36次のページ番号。すでに最終ページなら-1
    private static int GetNextPageIndex(int index)
    {
        //int pages = CanDualView(index) ? 2 : 1;
        int pages = App.CurrentShowPages;

        if (index + pages <= App.TotalPages - 1)
        {
            return index + pages;
        }
        else
        {
            //最終ページ
            return -1;
        }
    }
}