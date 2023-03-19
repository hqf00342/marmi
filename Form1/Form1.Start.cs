using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
        private async Task StartAsync(string[] filenames)
        {
            Debug.WriteLine($"StartAsync()");

            //ファイルがすでに開いているかどうかチェック
            if (filenames.Length == 1 && filenames[0] == App.g_pi.PackageName)
            {
                const string text = "おなじフォルダ/ファイルを開こうとしています。開きますか？";
                const string title = "同一フォルダ/ファイルオープンの確認";
                if (MessageBox.Show(text, title, MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            //初期化
            await InitMarmiAsync();

            //ファイル一覧を生成
            bool needRecurse = MakePackageInfo(filenames);
            Debug.WriteLine($"StartAsync(): MakePackageInfo() 完了. {0} pages.",App.g_pi.Items.Count);
            if (App.g_pi.Items.Count == 0)
                throw new InvalidDataException("書庫内の画像がありません");

            //ver1.37 再帰構造だけでなくSolid書庫も展開
            //ver1.79 常に一時書庫に展開オプションに対応
            if (App.g_pi.PackType == PackageType.Archive)
            {
                if (needRecurse || App.g_pi.isSolid || App.Config.General.ExtractArchiveAlways)
                {
                    var success = ExtractToTempDir(filenames[0]);
                    if (!success)
                    {
                        MessageBox.Show("一時展開フォルダが作成できませんでした。設定を確認してください");
                        return;
                    }
                }
            }

            SortPackage();
            Debug.WriteLine($"StartAsync(): Sort 完了");

            //UIを初期化
            UpdateToolbar();
            Debug.WriteLine($"StartAsync(): UpdateToolbar() 完了");

            //pdfチェック
            if (App.g_pi.PackType == PackageType.Pdf
                && !App.susie.isSupportedExtentions("pdf"))
            {
                const string str = "pdfファイルはサポートしていません";
                _clearPanel.ShowAndClose(str, 1000);
                //App.g_pi.Initialize();
                App.g_pi = new PackageInfo();
                return;
            }

            if (App.g_pi.Items.Count == 0)
            {
                //画面をクリア、準備中の文字を消す
                Debug.WriteLine($"StartAsync(): 画像ファイル無し");
                const string str = "表示できるファイルがありませんでした";
                _clearPanel.ShowAndClose(str, 1000);
                SetStatusbarInfo(str);
                return;
            }

            //ページを初期化
            App.g_pi.NowViewPage = 0;
            //CheckAndStart();

            //書庫の場合、MRUからページ番号などを得る
            var mru = App.Config.Mru.FirstOrDefault(a => a.Name == App.g_pi.PackageName);
            if (mru != null)
            {
                //ブックマークを読み込み
                App.g_pi.LoadBookmarkString(mru.Bookmarks);
                //続きから読む
                if (App.Config.General.ContinueReading)
                {
                    App.g_pi.NowViewPage = mru.LastViewPage;
                }
            }

            //トラックバーを初期化
            InitTrackbar();

            //SideBarへ登録
            _sidebar.Init(App.g_pi);

            //タイトルバーの設定
            this.Text = $"{App.APPNAME} - {Path.GetFileName(App.g_pi.PackageName)}";

            //サムネイルの作成
            Uty.DebugPrint("PreloadAllImages()");
            PreloadAllImages();

            //画像を表示
            Uty.DebugPrint($"最初のページ表示。 = {App.g_pi.NowViewPage}page");
            PicPanel.Message = string.Empty;
            await SetViewPageAsync(App.g_pi.NowViewPage);

            Uty.DebugPrint("完了");
        }

        /// <summary>
        /// パッケージ情報をpiに取る
        /// </summary>
        /// <param name="files">対象ファイル</param>
        /// <returns>書庫内書庫がある場合はtrue</returns>
        private static bool MakePackageInfo(string[] files)
        {
            //初期化
            //App.g_pi.Initialize();
            App.g_pi = new PackageInfo();

            if (files.Length == 1)
            {
                //ドロップされたのは1つ.
                App.g_pi.PackageName = files[0];

                //画像1枚/zip/pdf/dirctory
                if (Directory.Exists(App.g_pi.PackageName))
                {
                    //ディレクトリ
                    App.g_pi.PackType = PackageType.Directory;
                    GetPiclistInDirectory(files[0], App.Config.RecurseSearchDir);
                }
                else if (Uty.IsSupportArchiveFile(App.g_pi.PackageName))
                {
                    // 書庫
                    App.g_pi.PackType = PackageType.Archive;
                    bool needRecurse = GetArchivedFileInfo(files[0]);
                    if (needRecurse)
                        return true;
                }
                else if (files[0].EndsWith(".pdf"))
                {
                    //pdf
                    return ListPdf(files[0]);
                }
                else if (Uty.IsPictureFilename(files[0]))
                {
                    //単一画像ファイル
                    App.g_pi.PackageName = string.Empty;
                    App.g_pi.PackType = PackageType.Pictures;
                        App.g_pi.Items.Add(new ImageInfo(0, files[0]));
                }
                else
                {
                    throw new System.Exception("ファイルがありませんでした");
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

        /// <summary>
        /// 初期化ルーチン。ファイル閲覧後に初期化するために利用
        /// ・サムネイルパネルの初期化
        /// ・サイドバーの初期化
        /// ・トラックナビの初期化
        /// ・メイン画面の消去
        /// ・MRU更新
        /// ・スクリーンキャッシュクリア
        /// ・PackageInfo初期化
        /// ・一時フォルダ削除
        /// ・非同期IO停止
        /// ・書庫パスワードをクリア
        /// </summary>
        private async Task InitMarmiAsync()
        {
            Debug.WriteLine("InitMarmiAsync()");

            //2022年9月17日 非同期IOを中止、書庫のclose
            await AsyncIO.ClearJobAndWaitAsync();

            //汎用キャッシュをクリア
            App.BmpCache.ClearAll();

            //サムネイルモードの解放
            if (ViewState.ThumbnailView)
                SetThumbnailView(false);

            //2011/08/19 サムネイル初期化
            _thumbPanel.Init();

            //2011年11月11日 ver1.24 サイドバー
            _sidebar.Init(null);   //ver1.37
            if (_sidebar.Visible)
                _sidebar.Invalidate();

            //ver1.25 trackNavi
            if (_trackNaviPanel != null)
            {
                _trackNaviPanel.Dispose();
                _trackNaviPanel = null;
            }

            //メインパネルの画像表示をやめる
            PicPanel.Clear();

            //ver1.73 MRUリストの更新
            App.Config.AddMRU(App.g_pi);

            //ver1.35スクリーンキャッシュをクリア
            ScreenCache.Clear();

            //パッケージ情報を初期化
            //App.g_pi.Initialize();
            App.g_pi = new PackageInfo();

            //一時フォルダの削除
            TempDirs.DeleteAll();

            //そのほか本体内の情報をクリア
            g_viewPages = 1;
            g_LastClickPoint = Point.Empty;
            App.g_pi.NowViewPage = 0;

            //書庫のパスワードをクリア
            SevenZipWrapper.ClearPassword();
        }

        /// <summary>
        /// アーカイブファイルを一時ファイルに展開する。
        /// </summary>
        /// <param name="archiveFilename"></param>
        /// <returns>フォルダの展開に失敗したときはfalse</returns>
        private bool ExtractToTempDir(string archiveFilename)
        {
            //ver1.73 一時フォルダを作成
            try
            {
                var tempDir = SuggestTempDirName();
                Directory.CreateDirectory(tempDir);
                TempDirs.AddDir(tempDir);
                App.g_pi.tempDirname = tempDir;
            }
            catch
            {
                //ver1.79 一時フォルダが作れなかった
                return false;
            }

            //ファイルを展開
            using (var ae = new AsyncExtractForm())
            {
                //ダイアログを表示
                ae.ArchivePath = archiveFilename;
                ae.ExtractDir = App.g_pi.tempDirname;
                ae.ShowDialog(this);
            }

            //展開終了
            //画像を App.g_pi.Items に読み込む
            App.g_pi.PackType = PackageType.Pictures;
            App.g_pi.Items.Clear();
            GetPiclistInDirectory(App.g_pi.tempDirname, true);
            return true;
        }

        /// <summary>
        /// 一時フォルダの名前を返す。
        /// 存在しないランダムなフォルダ名を作る
        /// </summary>
        /// <returns>一時フォルダのフルパス</returns>
        private static string SuggestTempDirName()
        {
            string root = App.Config.General.TmpFolder
                ?? Application.StartupPath;

            //ユニークなフォルダを探す
            string tempDir;
            do
            {
                tempDir = Path.Combine(
                    root,
                    "TEMP7Z" + Path.GetRandomFileName().Substring(0, 8));
            }
            while (Directory.Exists(tempDir));
            return tempDir;
        }

        /// <summary>
        /// ディレクトリ内の画像を App.g_pi.Items に追加する。
        /// </summary>
        /// <param name="dirName">追加対象のディレクトリ名</param>
        /// <param name="recurse">再帰走査する場合true</param>
        private static void GetPiclistInDirectory(string dirName, bool recurse)
        {
            //画像ファイルを追加
            int index = 0;
            foreach (string name in Directory.EnumerateFiles(dirName))
            {
                if (Uty.IsPictureFilename(name))
                {
                    App.g_pi.Items.Add(new ImageInfo(index++, name));
                }
            }

            //再帰取得する場合はサブディレクトリも処理
            if (recurse)
            {
                foreach (var name in Directory.GetDirectories(dirName))
                    GetPiclistInDirectory(name, recurse);
            }
        }

        /// <summary>
        /// 書庫情報を取得
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>書庫内書庫がある場合はtrye</returns>
        private static bool GetArchivedFileInfo(string filename)
        {
            var szw = new SevenZipWrapper();
            bool retval = false;

            if (!szw.Open(filename))
            {
                MessageBox.Show("エラーのため書庫は開けませんでした。");
                //App.g_pi.Initialize();
                App.g_pi = new PackageInfo();
                return false;
            }

            //Zipファイル情報を設定
            App.g_pi.PackageName = filename;
            App.g_pi.isSolid = szw.IsSolid;
            App.g_pi.PackType = PackageType.Archive;

            //ver1.31 7zファイルなのにソリッドじゃないことがある！？
            if (Path.GetExtension(filename) == ".7z")
                App.g_pi.isSolid = true;

            //ファイルをリストに追加
            App.g_pi.Items.Clear();
            foreach (var item in szw.Items)
            {
                if (item.IsDirectory)
                    continue;
                if (Uty.IsPictureFilename(item.FileName))
                {
                    App.g_pi.Items.Add(new ImageInfo(item.Index, item.FileName, item.CreationTime, (long)item.Size));
                }
                else if (Uty.IsSupportArchiveFile(item.FileName))
                {
                    retval = true;
                }
            }
            return retval;
        }

        //ファイルリストを並び替える
        private static void SortPackage()
        {
            if (App.g_pi.Items.Count > 0)
            {
                var comparer = new ImageInfoComparer(ImageInfoComparer.Target.Filename);
                App.g_pi.Items.Sort(comparer);
            }
            return;
        }
    }
}