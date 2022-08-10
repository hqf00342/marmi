using System;
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
            //ファイルがすでに開いているかどうかチェック
            if (filenames.Length == 1 && filenames[0] == App.g_pi.PackageName)
            {
                const string text = "おなじフォルダ/ファイルを開こうとしています。開きますか？";
                const string title = "同一フォルダ/ファイルオープンの確認";
                if (MessageBox.Show(text, title, MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            //初期化
            InitMarmi();

            //ver1.78単一ファイルの場合、そのディレクトリを対象とする
            string onePicFile = string.Empty;
            if (filenames.Length == 1 && Uty.IsPictureFilename(filenames[0]))
            {
                onePicFile = filenames[0];
                filenames[0] = Path.GetDirectoryName(filenames[0]);
            }


            //ファイル一覧を生成
            bool needRecurse = SetPackageInfo(filenames);

            //ver1.37 再帰構造だけでなくSolid書庫も展開
            //ver1.79 常に一時書庫に展開オプションに対応
            //if (needRecurse )
            if (needRecurse || App.g_pi.isSolid || App.Config.General.AlwaysExtractArchive)
            {
                var success = ExtractToTempDir(filenames[0]);
                if (!success)
                {
                    MessageBox.Show("一時展開フォルダが作成できませんでした。設定を確認してください");
                    return;

                }
            }
            SortPackage();
            //UIを初期化
            UpdateToolbar();

            //pdfチェック
            if (App.g_pi.PackType == PackageType.Pdf
                && !App.susie.isSupportedExtentions("pdf"))
            {
                const string str = "pdfファイルはサポートしていません";
                _clearPanel.ShowAndClose(str, 1000);
                App.g_pi.Initialize();
                return;
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
        private void InitMarmi()
        {
            //サムネイルモードの解放
            if (App.Config.isThumbnailView)
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
            UpdateMRUList();

            //ver1.35スクリーンキャッシュをクリア
            ScreenCache.Clear();

            //パッケージ情報を初期化
            App.g_pi.Initialize();
            //App.g_pi = new PackageInfo();

            //一時フォルダの削除
            foreach (string dir in DeleteDirList)
            {
                Uty.DeleteTempDir(dir);
            }
            DeleteDirList.Clear();

            //2012/09/04 非同期IOを中止
            AsyncIO.ClearJob();
            AsyncIO.AddJob(-1, null);

            //そのほか本体内の情報をクリア
            g_viewPages = 1;
            g_LastClickPoint = Point.Empty;
            App.g_pi.NowViewPage = 0;

            //書庫のパスワードをクリア
            SevenZipWrapper.ClearPassword();

            //GC: 2021年2月26日 前の書庫のガベージを消すためここでやっておく。
            //Uty.ForceGC();
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
                DeleteDirList.Add(tempDir);
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
            GetDirPictureList(App.g_pi.tempDirname, true);
            return true;
        }

        /// <summary>
        /// ディレクトリ内の画像を App.g_pi.Items に追加する。
        /// </summary>
        /// <param name="dirName">追加対象のディレクトリ名</param>
        /// <param name="recurse">再帰走査する場合true</param>
        private static void GetDirPictureList(string dirName, bool recurse)
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
                    GetDirPictureList(name, recurse);
            }
        }
    }
}