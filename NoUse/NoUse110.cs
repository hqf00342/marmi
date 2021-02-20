using System;
using System.Collections.Generic;
using System.Text;

namespace Marmi.NoUse
{
    /// <summary>
    /// ver1.10時点で使わなくなったForm1のメソッド
    /// 消さずに一応取っておく
    /// </summary>
    class NoUse110
    {
        //ver1.10コメントアウト
        //利用者がなくなった
        /// <summary>
        /// 画像を読み込みBitmapを返す。
        /// 読み込む最中に様々な情報を取得し、サムネイル画像１つを作り出す。
        /// callbackMakeThumbNails()から呼び出されることを想定しているが
        /// CanDualView()やGetBitmap()など各画像のサイズだけを得たいときに
        /// キャッシュされてない場合直接呼び出されることもある
        /// </summary>
        /// <param name="index">読み込み対象の画像インデックス</param>
        //private Bitmap LoadImage(int index)
        //{
        //    if (index < 0 || index > g_pi.Items.Count - 1)
        //        return null;

        //    string filename = g_pi.Items[index].filename;
        //    try
        //    {
        //        if (!g_pi.isZip)
        //        {
        //            //通常ファイル
        //            using (FileStream fs = File.OpenRead(filename))	//これも正解
        //            {
        //                return g_pi.Items[index].loadImage(fs);
        //            }
        //        }
        //        else
        //        {
        //            //圧縮ファイルの場合
        //            //案2 2011年8月2日
        //            //Invokeせずにこの中でstreamを生成する
        //            using (SevenZipWrapper szw = new SevenZipWrapper())
        //            {
        //                szw.Open(g_pi.PackageName);
        //                return g_pi.Items[index].loadImage(szw.GetStream(filename));
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        //g_zw.GetStream(g_filelist[i])が発生する例外
        //        //ここでは何もしないでnullを返す。
        //        Debug.WriteLine(e.Message, "Form1::LoadImage()");
        //        return null;
        //    }
        //}

        // ver1.09 2011年8月16日
        //　GetBitmapSize(int index)
        // 使われていないのでコメントアウト
        /// <summary>
        /// Bitmapのサイズを取得する。
        /// 単純に取得するとまだデーターベース化されていない場合があるので
        /// 読み込みを確実にする。
        /// これでまた、遅くなる要因が増えたかも
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        //private Size GetBitmapSize(int index)
        //{
        //    //最後のページになっていないか確認
        //    //if (index >= g_pi.Items.Count - 1 || index < 0) //これだと最後のページが対象外。2011年7月21日
        //    if (index > g_pi.Items.Count - 1 || index < 0)
        //        return Size.Empty;

        //    //ImageInfoに読み込まれていない場合は読む
        //    if (g_pi.Items[index].ThumbImage == null)
        //        //LoadThumbnail(index);
        //        LoadImage(index);

        //    return new Size(g_pi.Items[index].originalWidth, g_pi.Items[index].originalHeight);
        //}
    }
}