using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Marmi
{
    public partial class FormOption : Form
    {
        //static List<KeyConfig> keyConfigList = new List<KeyConfig>();
        private bool KeyDuplicationError = false;

        //AppGlobalConfig config = new AppGlobalConfig();

        public FormOption()
        {
            InitializeComponent();

            //TODOキーコンフィグ用ComboBoxを初期化したい
            //alwaysExtractArchive.DataBindings.Add("Checked", config, "AlwaysExtractArchive");
        }

        public void LoadConfig(AppGlobalConfig set)
        {
            //config = set.Clone();

            //全般タブ
            bSaveConfig.Checked = set.isSaveConfig;                     //設定の保存
                                                                        //bSaveThumbnailCache.Checked = set.isSaveThumbnailCache;		//キャッシュの保存
            bContinueZip.Checked = set.isContinueZipView;               //zipファイルは前回の続きから
                                                                        //bDeleteOldCache.Checked = set.isAutoCleanOldCache;			//古いキャッシュの削除
            bReplaceArrowButton.Checked = set.isReplaceArrowButton;     //矢印ボタンの入れ替え
            pictureBox_BackColor.BackColor = set.BackColor;             //背景色
            isFastDraw.Checked = set.isFastDrawAtResize;
            isWindowPosCenter.Checked = set.isWindowPosCenter;

            //高度な設定タブ
            bStopPaintingAtResize.Checked = set.isStopPaintingAtResize; //リサイズ描写

            //サムネイルタブ
            thumbnailSize.Text = set.ThumbnailSize.ToString();
            ThumbnailBackColor.BackColor = set.ThumbnailBackColor;
            fontDialog1.Font = set.ThumbnailFont;
            linkLabel1.Text = fontDialog1.Font.Name + ", " + fontDialog1.Font.Size;
            ThumbnailFontColor.BackColor = set.ThumbnailFontColor;
            isDrawThumbnailFrame.Checked = set.isDrawThumbnailFrame;
            isDrawThumbnailShadow.Checked = set.isDrawThumbnailShadow;
            isShowTPFileName.Checked = set.isShowTPFileName;
            isShowTPFileSize.Checked = set.isShowTPFileSize;
            isShowTPPicSize.Checked = set.isShowTPPicSize;
            isThumbFadein.Checked = set.isThumbFadein;

            //ルーペ関連
            isOriginalSizeLoupe.Checked = set.isOriginalSizeLoupe;
            loupeMag.Text = set.loupeMagnifcant.ToString();

            //ver1.09 書庫関連
            isExtractIfSolidArchive.Checked = set.isExtractIfSolidArchive;

            //ver1.09 クロスフェード
            //isCrossfadeTransition.Checked = set.isCrossfadeTransition;

            //ver1.21 キーコンフィグ
            //ver1.81コメントアウト
            //keyConfBookmark.Text = set.keyConfBookMark;
            //keyConfFullScr.Text = set.keyConfFullScr;
            //keyConfLastPage.Text = set.keyConfLastPage;
            //keyConfNextPage.Text = set.keyConfNextPage;
            //keyConfNextPageHalf.Text = set.keyConfNextPageHalf;
            //keyConfPrevPage.Text = set.keyConfPrevPage;
            //keyConfPrevPageHalf.Text = set.keyConfPrevPageHalf;
            //keyConfPrintMode.Text = set.keyConfPrintMode;
            //keyConfTopPage.Text = set.keyConfTopPage;
            //keyConfDualMode.Text = set.keyConfDualMode;
            //keyConfRecycleBin.Text = set.keyConfRecycleBin;
            //keyConfExitApp.Text = set.keyConfExitApp;

            //マウスコンフィグ
            mouseConfigWheel.Text = set.mouseConfigWheel;

            //画面切り替わり方法
            SwitchPicMode.SelectedIndex = (int)set.pictureSwitchMode;

            //拡大表示関連
            noEnlargeOver100p.Checked = set.noEnlargeOver100p;
            isDotByDotZoom.Checked = set.isDotByDotZoom;

            //ver1.35 ループ
            //isLoopToTopPage.Checked = set.isLoopToTopPage;

            //ver1.64 画面ナビ
            radioRightScrToNextPic.Checked = set.RightScrClickIsNextPic;
            radioLeftScrToNextPic.Checked = !set.RightScrClickIsNextPic;
            reverseClickPointWhenLeftBook.Checked = set.ReverseDirectionWhenLeftBook;

            //ver1.65ツールバーアイテムの文字消し
            eraseToolbarItemString.Checked = set.eraseToolbarItemString;

            //ver1.70 2枚表示の厳密チェック
            //dualview_exactCheck.Checked = set.dualview_exactCheck;

            //ver1.70 サイドバーのスムーススクロール機能
            sidebar_smoothscroll.Checked = set.sidebar_smoothScroll;

            //ver1.71 最終ページの動作
            lastPage_stay.Checked = set.lastPage_stay;
            lastPage_toTop.Checked = set.lastPage_toTop;
            lastPage_toNextArchive.Checked = set.lastPage_toNextArchive;

            //ver1.73 一時フォルダ
            tmpFolder.Text = set.tmpFolder;

            //ver1.73 MRU保持数
            numOfMru.Text = set.numberOfMru.ToString();

            //ver1.76多重起動
            disableMultipleStarts.Checked = set.disableMultipleStarts;
            //ver1.77 ウィンドウ表示位置を簡易にするか
            simpleCalcWindowPos.Checked = set.simpleCalcForWindowLocation;
            //ver1.77 フルスクリーン状態を復元できるようにする
            saveFullScreenMode.Checked = set.saveFullScreenMode;
            //ver1.78 倍率の保持
            keepMagnification.Checked = set.keepMagnification;
            //ver1.79 書庫は必ず展開
            alwaysExtractArchive.Checked = set.AlwaysExtractArchive;
            //ver1.79 2ページモードアルゴリズム
            dualView_Force.Checked = set.dualView_Force;
            dualView_Normal.Checked = set.dualView_Normal;
            dualView_withSizeCheck.Checked = set.dualView_withSizeCheck;

            //ver1.80 キーコンフィグ
            ka_exit1.keyData = set.ka_exit1;
            ka_exit2.keyData = set.ka_exit2;
            ka_bookmark1.keyData = set.ka_bookmark1;
            ka_bookmark2.keyData = set.ka_bookmark2;
            ka_fullscreen1.keyData = set.ka_fullscreen1;
            ka_fullscreen2.keyData = set.ka_fullscreen2;
            ka_dualview1.keyData = set.ka_dualview1;
            ka_dualview2.keyData = set.ka_dualview2;
            ka_viewratio1.keyData = set.ka_viewratio1;
            ka_viewratio2.keyData = set.ka_viewratio2;
            ka_recycle1.keyData = set.ka_recycle1;
            ka_recycle2.keyData = set.ka_recycle2;
            //1.80キーコンフィグナビゲーション関連;
            ka_nextpage1.keyData = set.ka_nextpage1;
            ka_nextpage2.keyData = set.ka_nextpage2;
            ka_prevpage1.keyData = set.ka_prevpage1;
            ka_prevpage2.keyData = set.ka_prevpage2;
            ka_prevhalf1.keyData = set.ka_prevhalf1;
            ka_prevhalf2.keyData = set.ka_prevhalf2;
            ka_nexthalf1.keyData = set.ka_nexthalf1;
            ka_nexthalf2.keyData = set.ka_nexthalf2;
            ka_toppage1.keyData = set.ka_toppage1;
            ka_toppage2.keyData = set.ka_toppage2;
            ka_lastpage1.keyData = set.ka_lastpage1;
            ka_lastpage2.keyData = set.ka_lastpage2;
            //ダブルクリックで全画面
            DoubleClickToFullscreen.Checked = set.DoubleClickToFullscreen;
            //ver1.81サムネイルのアニメーション効果
            ThumbnailPanelSmoothScroll.Checked = set.ThumbnailPanelSmoothScroll;
            //ver1.83 アンシャープマスク
            useUnsharpMask.Checked = set.useUnsharpMask;
            unsharpDepth.Value = (decimal)set.unsharpDepth;
        }

        public void SaveConfig(ref AppGlobalConfig set)
        {
            //全般タブ
            set.isSaveConfig = bSaveConfig.Checked;
            //set.isSaveThumbnailCache = bSaveThumbnailCache.Checked;
            set.isContinueZipView = bContinueZip.Checked;
            //set.isAutoCleanOldCache = bDeleteOldCache.Checked;
            set.isReplaceArrowButton = bReplaceArrowButton.Checked;
            set.BackColor = pictureBox_BackColor.BackColor;
            set.isFastDrawAtResize = isFastDraw.Checked;
            set.isWindowPosCenter = isWindowPosCenter.Checked;

            //高度な設定タブ
            set.isStopPaintingAtResize = bStopPaintingAtResize.Checked;

            //サムネイルタブ
            if (!int.TryParse(thumbnailSize.Text, out set.ThumbnailSize)) set.ThumbnailSize = 120;
            set.ThumbnailBackColor = ThumbnailBackColor.BackColor;
            set.ThumbnailFont = fontDialog1.Font;
            set.ThumbnailFontColor = ThumbnailFontColor.BackColor;
            set.isDrawThumbnailFrame = isDrawThumbnailFrame.Checked;
            set.isDrawThumbnailShadow = isDrawThumbnailShadow.Checked;
            set.isShowTPFileName = isShowTPFileName.Checked;
            set.isShowTPFileSize = isShowTPFileSize.Checked;
            set.isShowTPPicSize = isShowTPPicSize.Checked;
            set.isThumbFadein = isThumbFadein.Checked;

            //ルーペ関連
            set.isOriginalSizeLoupe = isOriginalSizeLoupe.Checked;
            if (!int.TryParse(loupeMag.Text, out set.loupeMagnifcant))
                set.loupeMagnifcant = 3;

            //ver1.09 書庫関連
            set.isExtractIfSolidArchive = isExtractIfSolidArchive.Checked;

            //ver1.09 クロスフェード
            //set.isCrossfadeTransition = isCrossfadeTransition.Checked;

            //ver1.21 キーコンフィグ
            //ver1.81コメントアウト
            //set.keyConfBookMark = keyConfBookmark.Text;
            //set.keyConfFullScr = keyConfFullScr.Text;
            //set.keyConfLastPage = keyConfLastPage.Text;
            //set.keyConfNextPage = keyConfNextPage.Text;
            //set.keyConfNextPageHalf = keyConfNextPageHalf.Text;
            //set.keyConfPrevPage = keyConfPrevPage.Text;
            //set.keyConfPrevPageHalf = keyConfPrevPageHalf.Text;
            //set.keyConfPrintMode = keyConfPrintMode.Text;
            //set.keyConfTopPage = keyConfTopPage.Text;
            //set.keyConfDualMode = keyConfDualMode.Text;
            //set.keyConfRecycleBin = keyConfRecycleBin.Text;
            //set.keyConfExitApp = keyConfExitApp.Text;

            //マウスコンフィグ
            set.mouseConfigWheel = mouseConfigWheel.Text;

            //画面切り替わり方法
            set.pictureSwitchMode =
                //(AppGlobalConfig.AnimateMode)SwitchPicMode.SelectedIndex;
                (AnimateMode)SwitchPicMode.SelectedIndex;
            //拡大表示関連
            set.noEnlargeOver100p = noEnlargeOver100p.Checked;
            set.isDotByDotZoom = isDotByDotZoom.Checked;

            //ver1.35 ループ
            //set.isLoopToTopPage = isLoopToTopPage.Checked;

            //ver1.64 画面ナビ
            set.RightScrClickIsNextPic = radioRightScrToNextPic.Checked;
            set.ReverseDirectionWhenLeftBook = reverseClickPointWhenLeftBook.Checked;

            //ver1.65ツールバーアイテムの文字消し
            set.eraseToolbarItemString = eraseToolbarItemString.Checked;

            //ver1.70 2枚表示の厳密チェック
            //set.dualview_exactCheck = dualview_exactCheck.Checked;

            //ver1.70 サイドバーのスムーススクロール機能
            set.sidebar_smoothScroll = sidebar_smoothscroll.Checked;

            //ver1.71 最終ページの動作
            set.lastPage_stay = lastPage_stay.Checked;
            set.lastPage_toTop = lastPage_toTop.Checked;
            set.lastPage_toNextArchive = lastPage_toNextArchive.Checked;

            //ver1.73 一時フォルダ
            set.tmpFolder = tmpFolder.Text;
            //ver1.73 MRU保持数
            if (!int.TryParse(numOfMru.Text, out set.numberOfMru))
                set.numberOfMru = 10;   //デフォルト値

            //ver1.76多重起動
            set.disableMultipleStarts = disableMultipleStarts.Checked;
            //ver1.77 ウィンドウ表示位置を簡易にするか
            set.simpleCalcForWindowLocation = simpleCalcWindowPos.Checked;
            //ver1.77 フルスクリーン状態を復元できるようにする
            set.saveFullScreenMode = saveFullScreenMode.Checked;
            //ver1.78 倍率の保持
            set.keepMagnification = keepMagnification.Checked;
            //ver1.79 書庫は必ず展開
            set.AlwaysExtractArchive = alwaysExtractArchive.Checked;
            //ver1.79 2ページモードアルゴリズム
            set.dualView_Force = dualView_Force.Checked;
            set.dualView_Normal = dualView_Normal.Checked;
            set.dualView_withSizeCheck = dualView_withSizeCheck.Checked;

            //ver1.80 キーコンフィグ
            set.ka_exit1 = ka_exit1.keyData;
            set.ka_exit2 = ka_exit2.keyData;
            set.ka_bookmark1 = ka_bookmark1.keyData;
            set.ka_bookmark2 = ka_bookmark2.keyData;
            set.ka_fullscreen1 = ka_fullscreen1.keyData;
            set.ka_fullscreen2 = ka_fullscreen2.keyData;
            set.ka_dualview1 = ka_dualview1.keyData;
            set.ka_dualview2 = ka_dualview2.keyData;
            set.ka_viewratio1 = ka_viewratio1.keyData;
            set.ka_viewratio2 = ka_viewratio2.keyData;
            set.ka_recycle1 = ka_recycle1.keyData;
            set.ka_recycle2 = ka_recycle2.keyData;
            //1.80キーコンフィグナビゲーション関連;
            set.ka_nextpage1 = ka_nextpage1.keyData;
            set.ka_nextpage2 = ka_nextpage2.keyData;
            set.ka_prevpage1 = ka_prevpage1.keyData;
            set.ka_prevpage2 = ka_prevpage2.keyData;
            set.ka_prevhalf1 = ka_prevhalf1.keyData;
            set.ka_prevhalf2 = ka_prevhalf2.keyData;
            set.ka_nexthalf1 = ka_nexthalf1.keyData;
            set.ka_nexthalf2 = ka_nexthalf2.keyData;
            set.ka_toppage1 = ka_toppage1.keyData;
            set.ka_toppage2 = ka_toppage2.keyData;
            set.ka_lastpage1 = ka_lastpage1.keyData;
            set.ka_lastpage2 = ka_lastpage2.keyData;
            //1.80 ダブルクリックで全画面
            set.DoubleClickToFullscreen = DoubleClickToFullscreen.Checked;
            //ver1.81サムネイルのアニメーション効果
            set.ThumbnailPanelSmoothScroll = ThumbnailPanelSmoothScroll.Checked;
            //ver1.83 アンシャープマスク
            set.useUnsharpMask = useUnsharpMask.Checked;
            set.unsharpDepth = (int)unsharpDepth.Value;
        }

        private void InitButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                    "全ての設定が初期値に戻りますが実行しますか？",
                    "確認",
                    MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                LoadConfig(new AppGlobalConfig());      //初期値を作り出す
                this.Refresh();
            }
        }

        private void bSaveConfig_CheckedChanged(object sender, EventArgs e)
        {
            //全てのTabPage1内のアイテムの動作設定
            //設定を保存するときはEnable、そうでないときはDisable
            foreach (Control o in General.Controls)
            {
                switch (o.Name)
                {
                    case "bSaveConfig":
                        break;

                    case "loupeUserSetting":
                        break;

                    default:
                        o.Enabled = bSaveConfig.Checked;
                        break;
                }
            }
        }

        private void pictureBoxBackColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                pictureBox_BackColor.BackColor = colorDialog1.Color;
            }
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog(this) == DialogResult.OK)
            {
                linkLabel1.Text = fontDialog1.Font.Name + ", " + fontDialog1.Font.Size;
            }
        }

        private void ThumbnailFontColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                ThumbnailFontColor.BackColor = colorDialog1.Color;
            }
        }

        private void ThumbnailBackColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                ThumbnailBackColor.BackColor = colorDialog1.Color;
            }
        }

        private void OnFocus_Enter(object sender, EventArgs e)
        {
            HelpBox.Text = (string)(((Control)sender).Tag);
            //toolTip1.Show((string)(((Control)sender).Tag), this);
            toolTip1.Show("hai", this, 5000);
            toolTip1.SetToolTip((Control)sender, (string)((Control)sender).Tag);
        }

        //ver1.81 KeyAccelerator 利用に伴い Validating に移行
        /// <summary>
        /// キー重複チェックルーチン
        /// コントロールの値を比較する
        /// </summary>
        /// <returns>重複していた場合はtrue</returns>
        //private bool CheckKeyDuplicate()
        //{
        //	//キーコンフィグに重複がないことをチェック
        //	List<string> checkkey = new List<string>();

        //	foreach (Control c in keyConfigGroupBox.Controls)
        //	{
        //		if (c is ComboBox)
        //		{
        //			if (c.Text.Contains("なし"))
        //				continue;
        //			if (checkkey.Contains(c.Text))
        //				return true;
        //			else
        //				checkkey.Add(c.Text);
        //		}
        //	}
        //	return false;
        //}

        /// <summary>
        /// このままフォームを閉じていいかチェック
        /// ver1.21のキーコンフィグ重複チェックのため追加
        /// </summary>
        private void FormOption_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (KeyDuplicationError)
                e.Cancel = true;
        }

        /// <summary>
        /// OKボタンを押した際に不具合がなかったかチェック
        /// ver1.21ではキーコンフィグ重複チェック
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            //if (CheckKeyDuplicate())
            //{
            //	MessageBox.Show("キー設定が重複しています");
            //	KeyDuplicationError = true;
            //}
            //else
            KeyDuplicationError = false;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //Cancelボタンを押した時はエラー無し
            KeyDuplicationError = false;
        }

        private void radioRightScrToNextPic_CheckedChanged(object sender, EventArgs e)
        {
            //クリック画面表示を更新
            pictureBoxRightScr.Image = Properties.Resources.ScrNext;
            pictureBoxLeftScr.Image = Properties.Resources.ScrPrev;
        }

        private void radioLeftScrToNextPic_CheckedChanged(object sender, EventArgs e)
        {
            //クリック画面表示を更新
            pictureBoxRightScr.Image = Properties.Resources.ScrPrev;
            pictureBoxLeftScr.Image = Properties.Resources.ScrNext;
        }

        private void pictureBoxRightScr_Click(object sender, EventArgs e)
        {
            //ラジオボックスを連動->画像も変わる
            radioRightScrToNextPic.Checked = true;
            //radioLeftScrToNextPic.Checked = false;
        }

        private void pictureBoxLeftScr_Click(object sender, EventArgs e)
        {
            //ラジオボックスを連動->画像も変わる
            //radioRightScrToNextPic.Checked = false;
            radioLeftScrToNextPic.Checked = true;
        }

        private void tmpFolderBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tmpFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void label35_Click(object sender, EventArgs e)
        {
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPageIndex == 7)
                e.Cancel = true;
        }

        /// <summary>
        /// KeyAcceleratorの入力値検証
        /// 同一の入力値がある場合はキャンセルする。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ka_Validating(object sender, CancelEventArgs e)
        {
            //調査元となるKeyAccelerator
            var org = sender as KeyAccelerator;

            //設定がないのなら何もしない。
            if (org.keyData == Keys.None)
                return;

            //コントロールを列挙
            foreach (var c in tableLayoutPanel1.Controls)
                if (c is KeyAccelerator)
                {
                    KeyAccelerator testing = c as KeyAccelerator;
                    if (testing == org)
                        //自分自身はチェック対象外
                        continue;
                    else
                        //チェック
                        if (testing.keyData == org.keyData)
                    {
                        //重複している
                        var ret = MessageBox.Show(
                            string.Format("「{0}」と設定が重複しています。上書きしますか？", testing.Tag),
                            "キー設定確認",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);
                        if (ret == DialogResult.Yes)
                        {
                            //ほかのコントロールを変更する
                            testing.keyData = Keys.None;
                            testing.Invalidate();
                        }
                        else
                        {
                            //Cancelする。
                            e.Cancel = true;
                        }
                    }
                }
        }
    }//class
}//namespace