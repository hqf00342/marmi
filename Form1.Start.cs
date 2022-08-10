using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
Marmiの閲覧開始ポイント
Start()

*/

namespace Marmi
{
    public partial class Form1 : Form
    {
        private async Task Start(string[] filenames)
        {
            //ver1.73 MRUリストの更新
            //今まで見ていたものを登録する
            UpdateMRUList();

            //ファイルがすでに開いているかどうかチェック
            if (filenames.Length == 1 && filenames[0] == App.g_pi.PackageName)
            {
                const string text = "おなじフォルダ/ファイルを開こうとしています。開きますか？";
                const string title = "同一フォルダ/ファイルオープンの確認";
                if (MessageBox.Show(text, title, MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            //ver1.41 非同期IOを停止
            AsyncIO.ClearJob();
            AsyncIO.AddJob(-1, null);
            App.g_pi.Initialize();

            //v1.92 画面を一度すべて消す
            PicPanel.Clear();
            SetStatusbarInfo("準備中・・・" + filenames[0]);

            //ver1.35スクリーンキャッシュをクリア
            ScreenCache.Clear();

            //コントロールの初期化
            InitControls();
            //この時点ではg_pi.PackageNameはできていない＝MRUがつくられない。

            //ver1.78単一ファイルの場合、そのディレクトリを対象とする
            string onePicFile = string.Empty;
            if (filenames.Length == 1 && Uty.IsPictureFilename(filenames[0]))
            {
                onePicFile = filenames[0];
                filenames[0] = Path.GetDirectoryName(filenames[0]);
            }

            //書庫のパスワードをクリア
            SevenZipWrapper.ClearPassword();

            //ファイル一覧を生成
            bool needRecurse = SetPackageInfo(filenames);

            //ver1.37 再帰構造だけでなくSolid書庫も展開
            //ver1.79 常に一時書庫に展開オプションに対応
            //if (needRecurse )
            if (needRecurse || App.g_pi.isSolid || App.Config.General.AlwaysExtractArchive)
            {
                using (AsyncExtractForm ae = new AsyncExtractForm())
                {
                    SetStatusbarInfo("書庫を展開中です" + filenames[0]);

                    //ver1.73 一時フォルダ作成
                    try
                    {
                        var tempDir = SuggestTempDirName();
                        Directory.CreateDirectory(tempDir);
                        DeleteDirList.Add(tempDir);
                        App.g_pi.tempDirname = tempDir;
                    }
                    catch
                    {
                        //ver1.79 一時フォルダが作れないときの対応
                        MessageBox.Show("一時展開フォルダが作成できませんでした。設定を確認してください");
                        App.g_pi.Initialize();
                        return;
                    }

                    //ダイアログを表示
                    ae.ArchivePath = filenames[0];
                    ae.ExtractDir = App.g_pi.tempDirname;
                    ae.ShowDialog(this);

                    //ダイアログの表示が終了
                    //ディレクトリをすべてg_piに読み込む
                    this.Cursor = Cursors.WaitCursor;
                    App.g_pi.PackType = PackageType.Pictures;
                    App.g_pi.Items.Clear();
                    GetDirPictureList(App.g_pi.tempDirname, true);
                    this.Cursor = Cursors.Arrow;
                }
            }
            SortPackage();
            //UIを初期化
            UpdateToolbar();

            //ver1.73 MRUリストの更新
            //ここではだめ.最終ページを保存できない。
            //UpdateMRUList();

            //pdfチェック
            if (App.g_pi.PackType == PackageType.Pdf)
            {
                if (!App.susie.isSupportedExtentions("pdf"))
                {
                    const string str = "pdfファイルはサポートしていません";
                    _clearPanel.ShowAndClose(str, 1000);
                    SetStatusbarInfo(str);
                    App.g_pi.Initialize();
                    return;
                }
            }

            if (App.g_pi.Items.Count == 0)
            {
                //画面をクリア、準備中の文字を消す
                const string str = "表示できるファイルがありませんでした";
                _clearPanel.ShowAndClose(str, 1000);
                SetStatusbarInfo(str);
                return;
            }

            //ページを初期化
            App.g_pi.NowViewPage = 0;
            //CheckAndStart();

            //サムネイルDBがあれば読み込む
            //loadThumbnailDBFile();
            if (App.Config.General.IsContinueZipView)
            {
                //読み込み値を無視し、０にセット
                //g_pi.NowViewPage = 0;
                foreach (var mru in App.Config.Mru)
                {
                    if (mru == null)
                    {
                        continue;
                    }
                    else if (mru.Name == App.g_pi.PackageName
                        //ver1.79 コメントアウト
                        //&& g_pi.packType == PackageType.Archive)
                        )
                    {
                        //最終ページを設定する。
                        App.g_pi.NowViewPage = mru.LastViewPage;
                        //Bookmarkを設定する
                        App.g_pi.LoadBookmarkString(mru.Bookmarks);
                        break;
                    }
                }
            }

            //１ファイルドロップによるディレクトリ参照の場合
            //最初に見るページをドロップしたファイルにする。
            if (!string.IsNullOrEmpty(onePicFile))
            {
                int i = App.g_pi.Items.FindIndex(c => c.Filename == onePicFile);
                if (i < 0) i = 0;
                App.g_pi.NowViewPage = i;
            }

            //トラックバーを初期化
            InitTrackbar();

            //SideBarへ登録
            _sidebar.Init(App.g_pi);

            //タイトルバーの設定
            this.Text = $"{App.APPNAME} - {Path.GetFileName(App.g_pi.PackageName)}";

            //サムネイルの作成
            AsyncLoadImageInfo();

            //画像を表示
            PicPanel.Message = string.Empty;
            await SetViewPageAsync(App.g_pi.NowViewPage);
        }

        /// <summary>
        /// パッケージ情報をpiに取る
        /// </summary>
        /// <param name="files">対象ファイル</param>
        /// <returns>書庫内書庫がある場合はtrue</returns>
        private static bool SetPackageInfo(string[] files)
        {
            //初期化
            App.g_pi.Initialize();

            if (files.Length == 1)
            {
                //ドロップされたのは1つ
                App.g_pi.PackageName = files[0];    //ディレクトリかZipファイル名を想定

                //ドロップされたファイルの詳細を探る
                if (Directory.Exists(App.g_pi.PackageName))
                {
                    //ディレクトリの場合
                    App.g_pi.PackType = PackageType.Directory;
                    GetDirPictureList(files[0], App.Config.IsRecurseSearchDir);
                }
                else if (App.unrar.dllLoaded && files[0].EndsWith(".rar", StringComparison.OrdinalIgnoreCase))
                {
                    //
                    //unrar.dllを使う。
                    //
                    App.g_pi.PackType = PackageType.Archive;
                    App.g_pi.isSolid = true;

                    //ファイルリストを構築
                    ListRar(files[0]);

                    //展開が必要なのでtrueを返す
                    return true;
                }
                else if (Uty.IsSupportArchiveFile(App.g_pi.PackageName))
                {
                    // 書庫ファイル
                    App.g_pi.PackType = PackageType.Archive;
                    bool needRecurse = GetArchivedFileInfo(files[0]);
                    if (needRecurse)
                        return true;
                }
                else if (files[0].EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    //pdfファイル
                    return ListPdf(files[0]);
                }
                else
                {
                    //単一画像ファイル
                    App.g_pi.PackageName = string.Empty;
                    App.g_pi.PackType = PackageType.Pictures;
                    if (Uty.IsPictureFilename(files[0]))
                    {
                        App.g_pi.Items.Add(new ImageInfo(0, files[0]));
                    }
                }
            }
            else //if (files.Length == 1)
            {
                //複数ファイルパターン
                App.g_pi.PackageName = string.Empty;
                App.g_pi.PackType = PackageType.Pictures;

                //ファイルを追加する
                int index = 0;
                foreach (string filename in files)
                {
                    if (Uty.IsPictureFilename(filename))
                    {
                        App.g_pi.Items.Add(new ImageInfo(index++, filename));
                    }
                }
            }//if (files.Length == 1)
            return false;

            /// <summary>unrarを使ってリスト化</summary>
            void ListRar(string file)
            {
                App.unrar.Open(file, Unrar.OpenMode.List);
                int num = 0;
                while (App.unrar.ReadHeader())
                {
                    if (!App.unrar.CurrentFile.IsDirectory)
                    {
                        App.g_pi.Items.Add(new ImageInfo(
                            num++,
                            App.unrar.CurrentFile.FileName,
                            App.unrar.CurrentFile.FileTime,
                            App.unrar.CurrentFile.UnpackedSize
                            ));
                    }
                    App.unrar.Skip();
                }
                App.unrar.Close();
            }

            bool ListPdf(string file)
            {
                App.g_pi.PackType = PackageType.Pdf;
                if (App.susie.isSupportPdf())
                {
                    foreach (var e in App.susie.GetArchiveInfo(file))
                    {
                        App.g_pi.Items.Add(new ImageInfo((int)e.position, e.filename, e.timestamp, e.filesize));
                    }
                    return false;
                }
                else
                {
                    //pdfは未サポート
                    //g_pi.PackageName = string.Empty;
                    return false;
                }
            }
        }

    }
}
