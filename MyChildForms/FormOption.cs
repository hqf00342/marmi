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
            bSaveConfig.Checked = set.IsSaveConfig;                     //設定の保存
                                                                        //bSaveThumbnailCache.Checked = set.isSaveThumbnailCache;		//キャッシュの保存
            bContinueZip.Checked = set.IsContinueZipView;               //zipファイルは前回の続きから
                                                                        //bDeleteOldCache.Checked = set.isAutoCleanOldCache;			//古いキャッシュの削除
            bReplaceArrowButton.Checked = set.IsReplaceArrowButton;     //矢印ボタンの入れ替え
            pictureBox_BackColor.BackColor = set.BackColor;             //背景色
            isFastDraw.Checked = set.IsFastDrawAtResize;
            isWindowPosCenter.Checked = set.IsWindowPosCenter;

            //高度な設定タブ
            bStopPaintingAtResize.Checked = set.IsStopPaintingAtResize; //リサイズ描写

            //サムネイルタブ
            thumbnailSize.Text = set.ThumbnailSize.ToString();
            ThumbnailBackColor.BackColor = set.ThumbnailBackColor;
            fontDialog1.Font = set.ThumbnailFont;
            linkLabel1.Text = fontDialog1.Font.Name + ", " + fontDialog1.Font.Size;
            ThumbnailFontColor.BackColor = set.ThumbnailFontColor;
            isDrawThumbnailFrame.Checked = set.IsDrawThumbnailFrame;
            isDrawThumbnailShadow.Checked = set.IsDrawThumbnailShadow;
            isShowTPFileName.Checked = set.IsShowTPFileName;
            isShowTPFileSize.Checked = set.IsShowTPFileSize;
            isShowTPPicSize.Checked = set.IsShowTPPicSize;
            isThumbFadein.Checked = set.IsThumbFadein;

            //ルーペ関連
            isOriginalSizeLoupe.Checked = set.IsOriginalSizeLoupe;
            loupeMag.Text = set.loupeMagnifcant.ToString();

            //ver1.09 書庫関連
            isExtractIfSolidArchive.Checked = set.IsExtractIfSolidArchive;

            //マウスコンフィグ
            mouseConfigWheel.Text = set.MouseConfigWheel;

            //画面切り替わり方法
            SwitchPicMode.SelectedIndex = (int)set.PictureSwitchMode;

            //拡大表示関連
            noEnlargeOver100p.Checked = set.NoEnlargeOver100p;
            isDotByDotZoom.Checked = set.IsDotByDotZoom;

            //ver1.35 ループ
            //isLoopToTopPage.Checked = set.isLoopToTopPage;

            //ver1.64 画面ナビ
            radioRightScrToNextPic.Checked = set.RightScrClickIsNextPic;
            radioLeftScrToNextPic.Checked = !set.RightScrClickIsNextPic;
            reverseClickPointWhenLeftBook.Checked = set.ReverseDirectionWhenLeftBook;

            //ver1.65ツールバーアイテムの文字消し
            eraseToolbarItemString.Checked = set.EraseToolbarItemString;

            //ver1.70 2枚表示の厳密チェック
            //dualview_exactCheck.Checked = set.dualview_exactCheck;

            //ver1.70 サイドバーのスムーススクロール機能
            sidebar_smoothscroll.Checked = set.Sidebar_smoothScroll;

            //ver1.71 最終ページの動作
            lastPage_stay.Checked = set.LastPage_stay;
            lastPage_toTop.Checked = set.LastPage_toTop;
            //lastPage_toNextArchive.Checked = set.LastPage_toNextArchive;

            //ver1.73 一時フォルダ
            tmpFolder.Text = set.TmpFolder;

            //ver1.73 MRU保持数
            numOfMru.Text = set.NumberOfMru.ToString();

            //ver1.76多重起動
            disableMultipleStarts.Checked = set.DisableMultipleStarts;
            //ver1.77 ウィンドウ表示位置を簡易にするか
            simpleCalcWindowPos.Checked = set.SimpleCalcForWindowLocation;
            //ver1.77 フルスクリーン状態を復元できるようにする
            saveFullScreenMode.Checked = set.SaveFullScreenMode;
            //ver1.78 倍率の保持
            keepMagnification.Checked = set.KeepMagnification;
            //ver1.79 書庫は必ず展開
            alwaysExtractArchive.Checked = set.AlwaysExtractArchive;
            //ver1.79 2ページモードアルゴリズム
            dualView_Force.Checked = set.DualView_Force;
            dualView_Normal.Checked = set.DualView_Normal;
            dualView_withSizeCheck.Checked = set.DualView_withSizeCheck;

            //ver1.91 キーコンフィグ分離
            LoadKeyConfig(set);

            //ダブルクリックで全画面
            DoubleClickToFullscreen.Checked = set.DoubleClickToFullscreen;
            //ver1.81サムネイルのアニメーション効果
            ThumbnailPanelSmoothScroll.Checked = set.ThumbnailPanelSmoothScroll;
            //ver1.83 アンシャープマスク
            useUnsharpMask.Checked = set.UseUnsharpMask;
            unsharpDepth.Value = (decimal)set.UnsharpDepth;
        }

        private void LoadKeyConfig(AppGlobalConfig set)
        {
            ka_exit1.keyData = set.Keys.Key_Exit1;
            ka_exit2.keyData = set.Keys.Key_Exit2;
            ka_bookmark1.keyData = set.Keys.Key_Bookmark1;
            ka_bookmark2.keyData = set.Keys.Key_Bookmark2;
            ka_fullscreen1.keyData = set.Keys.Key_Fullscreen1;
            ka_fullscreen2.keyData = set.Keys.Key_Fullscreen2;
            ka_dualview1.keyData = set.Keys.Key_Dualview1;
            ka_dualview2.keyData = set.Keys.Key_Dualview2;
            ka_viewratio1.keyData = set.Keys.Key_ViewRatio1;
            ka_viewratio2.keyData = set.Keys.Key_ViewRatio2;
            ka_recycle1.keyData = set.Keys.Key_Recycle1;
            ka_recycle2.keyData = set.Keys.Key_Recycle2;
            ka_rotate1.keyData = set.Keys.Key_Rotate1;
            ka_rotate2.keyData = set.Keys.Key_Rotate2;
            ka_nextpage1.keyData = set.Keys.Key_Nextpage1;
            ka_nextpage2.keyData = set.Keys.Key_Nextpage2;
            ka_prevpage1.keyData = set.Keys.Key_Prevpage1;
            ka_prevpage2.keyData = set.Keys.Key_Prevpage2;
            ka_prevhalf1.keyData = set.Keys.Key_Prevhalf1;
            ka_prevhalf2.keyData = set.Keys.Key_Prevhalf2;
            ka_nexthalf1.keyData = set.Keys.Key_Nexthalf1;
            ka_nexthalf2.keyData = set.Keys.Key_Nexthalf2;
            ka_toppage1.keyData = set.Keys.Key_Toppage1;
            ka_toppage2.keyData = set.Keys.Key_Toppage2;
            ka_lastpage1.keyData = set.Keys.Key_Lastpage1;
            ka_lastpage2.keyData = set.Keys.Key_Lastpage2;
        }

        public void SaveConfig(ref AppGlobalConfig set)
        {
            //全般タブ
            set.IsSaveConfig = bSaveConfig.Checked;
            //set.isSaveThumbnailCache = bSaveThumbnailCache.Checked;
            set.IsContinueZipView = bContinueZip.Checked;
            //set.isAutoCleanOldCache = bDeleteOldCache.Checked;
            set.IsReplaceArrowButton = bReplaceArrowButton.Checked;
            set.BackColor = pictureBox_BackColor.BackColor;
            set.IsFastDrawAtResize = isFastDraw.Checked;
            set.IsWindowPosCenter = isWindowPosCenter.Checked;

            //高度な設定タブ
            set.IsStopPaintingAtResize = bStopPaintingAtResize.Checked;

            //サムネイルタブ
            if (!int.TryParse(thumbnailSize.Text, out set.ThumbnailSize)) set.ThumbnailSize = 120;
            set.ThumbnailBackColor = ThumbnailBackColor.BackColor;
            set.ThumbnailFont = fontDialog1.Font;
            set.ThumbnailFontColor = ThumbnailFontColor.BackColor;
            set.IsDrawThumbnailFrame = isDrawThumbnailFrame.Checked;
            set.IsDrawThumbnailShadow = isDrawThumbnailShadow.Checked;
            set.IsShowTPFileName = isShowTPFileName.Checked;
            set.IsShowTPFileSize = isShowTPFileSize.Checked;
            set.IsShowTPPicSize = isShowTPPicSize.Checked;
            set.IsThumbFadein = isThumbFadein.Checked;

            //ルーペ関連
            set.IsOriginalSizeLoupe = isOriginalSizeLoupe.Checked;
            if (!int.TryParse(loupeMag.Text, out set.loupeMagnifcant))
                set.loupeMagnifcant = 3;

            //ver1.09 書庫関連
            set.IsExtractIfSolidArchive = isExtractIfSolidArchive.Checked;


            //マウスコンフィグ
            set.MouseConfigWheel = mouseConfigWheel.Text;

            //画面切り替わり方法
            set.PictureSwitchMode =
                //(AppGlobalConfig.AnimateMode)SwitchPicMode.SelectedIndex;
                (AnimateMode)SwitchPicMode.SelectedIndex;
            //拡大表示関連
            set.NoEnlargeOver100p = noEnlargeOver100p.Checked;
            set.IsDotByDotZoom = isDotByDotZoom.Checked;

            //ver1.35 ループ
            //set.isLoopToTopPage = isLoopToTopPage.Checked;

            //ver1.64 画面ナビ
            set.RightScrClickIsNextPic = radioRightScrToNextPic.Checked;
            set.ReverseDirectionWhenLeftBook = reverseClickPointWhenLeftBook.Checked;

            //ver1.65ツールバーアイテムの文字消し
            set.EraseToolbarItemString = eraseToolbarItemString.Checked;

            //ver1.70 2枚表示の厳密チェック
            //set.dualview_exactCheck = dualview_exactCheck.Checked;

            //ver1.70 サイドバーのスムーススクロール機能
            set.Sidebar_smoothScroll = sidebar_smoothscroll.Checked;

            //ver1.71 最終ページの動作
            set.LastPage_stay = lastPage_stay.Checked;
            set.LastPage_toTop = lastPage_toTop.Checked;
            //set.LastPage_toNextArchive = lastPage_toNextArchive.Checked;

            //ver1.73 一時フォルダ
            set.TmpFolder = tmpFolder.Text;
            //ver1.73 MRU保持数
            if (!int.TryParse(numOfMru.Text, out set.NumberOfMru))
                set.NumberOfMru = 10;   //デフォルト値

            //ver1.76多重起動
            set.DisableMultipleStarts = disableMultipleStarts.Checked;
            //ver1.77 ウィンドウ表示位置を簡易にするか
            set.SimpleCalcForWindowLocation = simpleCalcWindowPos.Checked;
            //ver1.77 フルスクリーン状態を復元できるようにする
            set.SaveFullScreenMode = saveFullScreenMode.Checked;
            //ver1.78 倍率の保持
            set.KeepMagnification = keepMagnification.Checked;
            //ver1.79 書庫は必ず展開
            set.AlwaysExtractArchive = alwaysExtractArchive.Checked;
            //ver1.79 2ページモードアルゴリズム
            set.DualView_Force = dualView_Force.Checked;
            set.DualView_Normal = dualView_Normal.Checked;
            set.DualView_withSizeCheck = dualView_withSizeCheck.Checked;

            //ver1.91 キーコンフィグ
            SaveKeyConfig(set);

            //1.80 ダブルクリックで全画面
            set.DoubleClickToFullscreen = DoubleClickToFullscreen.Checked;
            //ver1.81サムネイルのアニメーション効果
            set.ThumbnailPanelSmoothScroll = ThumbnailPanelSmoothScroll.Checked;
            //ver1.83 アンシャープマスク
            set.UseUnsharpMask = useUnsharpMask.Checked;
            set.UnsharpDepth = (int)unsharpDepth.Value;
        }

        private void SaveKeyConfig(AppGlobalConfig set)
        {
            set.Keys.Key_Exit1 = ka_exit1.keyData;
            set.Keys.Key_Exit2 = ka_exit2.keyData;
            set.Keys.Key_Bookmark1 = ka_bookmark1.keyData;
            set.Keys.Key_Bookmark2 = ka_bookmark2.keyData;
            set.Keys.Key_Fullscreen1 = ka_fullscreen1.keyData;
            set.Keys.Key_Fullscreen2 = ka_fullscreen2.keyData;
            set.Keys.Key_Dualview1 = ka_dualview1.keyData;
            set.Keys.Key_Dualview2 = ka_dualview2.keyData;
            set.Keys.Key_ViewRatio1 = ka_viewratio1.keyData;
            set.Keys.Key_ViewRatio2 = ka_viewratio2.keyData;
            set.Keys.Key_Recycle1 = ka_recycle1.keyData;
            set.Keys.Key_Recycle2 = ka_recycle2.keyData;
            set.Keys.Key_Rotate1 = ka_rotate1.keyData;
            set.Keys.Key_Rotate2 = ka_rotate2.keyData;
            //1.80キーコンフィグナビゲーション関連;
            set.Keys.Key_Nextpage1 = ka_nextpage1.keyData;
            set.Keys.Key_Nextpage2 = ka_nextpage2.keyData;
            set.Keys.Key_Prevpage1 = ka_prevpage1.keyData;
            set.Keys.Key_Prevpage2 = ka_prevpage2.keyData;
            set.Keys.Key_Prevhalf1 = ka_prevhalf1.keyData;
            set.Keys.Key_Prevhalf2 = ka_prevhalf2.keyData;
            set.Keys.Key_Nexthalf1 = ka_nexthalf1.keyData;
            set.Keys.Key_Nexthalf2 = ka_nexthalf2.keyData;
            set.Keys.Key_Toppage1 = ka_toppage1.keyData;
            set.Keys.Key_Toppage2 = ka_toppage2.keyData;
            set.Keys.Key_Lastpage1 = ka_lastpage1.keyData;
            set.Keys.Key_Lastpage2 = ka_lastpage2.keyData;
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

        private void SaveConfig_CheckedChanged(object sender, EventArgs e)
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

        private void PictureBoxBackColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                pictureBox_BackColor.BackColor = colorDialog1.Color;
            }
        }

        private void TextBox1_Click(object sender, EventArgs e)
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
        private void BtnOK_Click(object sender, EventArgs e)
        {
            //if (CheckKeyDuplicate())
            //{
            //	MessageBox.Show("キー設定が重複しています");
            //	KeyDuplicationError = true;
            //}
            //else
            KeyDuplicationError = false;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            //Cancelボタンを押した時はエラー無し
            KeyDuplicationError = false;
        }

        private void RadioRightScrToNextPic_CheckedChanged(object sender, EventArgs e)
        {
            //クリック画面表示を更新
            pictureBoxRightScr.Image = Properties.Resources.ScrNext;
            pictureBoxLeftScr.Image = Properties.Resources.ScrPrev;
        }

        private void RadioLeftScrToNextPic_CheckedChanged(object sender, EventArgs e)
        {
            //クリック画面表示を更新
            pictureBoxRightScr.Image = Properties.Resources.ScrPrev;
            pictureBoxLeftScr.Image = Properties.Resources.ScrNext;
        }

        private void PictureBoxRightScr_Click(object sender, EventArgs e)
        {
            //ラジオボックスを連動->画像も変わる
            radioRightScrToNextPic.Checked = true;
            //radioLeftScrToNextPic.Checked = false;
        }

        private void PictureBoxLeftScr_Click(object sender, EventArgs e)
        {
            //ラジオボックスを連動->画像も変わる
            //radioRightScrToNextPic.Checked = false;
            radioLeftScrToNextPic.Checked = true;
        }

        private void TmpFolderBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tmpFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        //private void Label35_Click(object sender, EventArgs e)
        //{
        //}

        //private void TableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        //{
        //}

        private void TabControl1_Selecting(object sender, TabControlCancelEventArgs e)
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
        private void KeyAcc_Validating(object sender, CancelEventArgs e)
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