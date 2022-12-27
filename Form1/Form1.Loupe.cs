using System.Drawing;
using System.Windows.Forms;

namespace Marmi
{
    public partial class Form1 : Form
    {
        private void OpenLoope(Rectangle cRect)
        {
            //サムネイル作成を停止
            //PauseThumbnailMakerThread();

            //ルーペのサイズを決める。
            //HACK: 全画面ルーペ test code
            //ver0.990 2011/07/21全画面ルーペ
            int dx = cRect.Width;
            int dy = cRect.Height;
            int mag = App.Config.Loupe.LoupeMagnifcant;

            loupe = new Loupe(this, dx, dy, mag);
            this.Controls.Add(loupe);

            //ルーペの位置を決める。
            loupe.Top = cRect.Top;
            loupe.Left = cRect.Left;

            //表示させる
            if (!loupe.Visible)
                loupe.Visible = true;

            //ステータスバー表示
            if (App.Config.Loupe.OriginalSizeLoupe    //原寸ルーペ設定が有効
                && PicPanel.ZoomRatio < 1.0F)           //表示倍率が100%以下
            {
                SetStatubarRatio("ルーペ（100%表示）");
            }
            else
            {
                //%表示
                SetStatubarRatio(
                    $"ルーペ:{App.Config.Loupe.LoupeMagnifcant}倍（{(double)(PicPanel.ZoomRatio * App.Config.Loupe.LoupeMagnifcant),0:p1}表示）");
            }
        }

        private void CloseLoupe()
        {
            loupe.Close();
            this.Controls.Remove(loupe);
            loupe.Dispose();
            loupe = null;

            //サムネイル作成を再開する
            //ResumeThumbnailMakerThread();
        }

        /// <summary>
        /// ルーペコントロール内をアップデート表示する
        /// マウスの位置によってルーペの内容が変わるので位置を引数として渡す
        /// </summary>
        /// <param name="mouseX">マウス位置</param>
        /// <param name="mouseY">マウス位置</param>
        /// <param name="cRect">クライアントサイズを示す</param>
        private void UpdateLoopeView(int mouseX, int mouseY, Rectangle cRect)
        {
            //ver1.27 マウス位置を補正
            float ratio = PicPanel.ZoomRatio;   //拡縮率をローカルに取る

            //原寸ルーペ設定が有効 && 100%未満表示である.99%にしておく
            if (App.Config.Loupe.OriginalSizeLoupe && ratio < 0.99F)
            {
                //ver1.27 左上座標に補正
                //元画像サイズをローカルに取る
                Bitmap screenImage = PicPanel.Bmp;
                int _bmpHeight = screenImage.Height;
                int _bmpWidth = screenImage.Width;
                //縮尺画像の原点の表示位置を確認
                int x0 = (PicPanel.Width - (int)(_bmpWidth * ratio)) / 2;
                int y0 = (PicPanel.Height - (int)(_bmpHeight * ratio)) / 2;
                ////縮小画像内でのマウス位置を計算
                int x1 = mouseX - x0;
                int y1 = mouseY - y0;
                //始点を算出
                double sx = ((double)x1 / ratio) - mouseX;
                double sy = ((double)y1 / ratio) - mouseY;
                //左上始点指定版のルーペ
                loupe.DrawOriginalSizeLoupe2((int)sx, (int)sy, screenImage);
                loupe.Refresh();
            }
            else
            {
                //unsafe版高速n倍ルーペ
                //画面キャプチャに対するルーペを実施。
                //ツールバー分を補正
                //loupe.DrawLoupeFast2(mouseX - cRect.Left, mouseY - cRect.Top);	//ver0.986サイドバー補正

                //ver1.27 左上座標に補正
                double mag = App.Config.Loupe.LoupeMagnifcant;
                double x9 = ((mag - 1.0d) / mag) * (double)mouseX;
                double y9 = ((mag - 1.0d) / mag) * (double)mouseY;
                loupe.DrawLoupeFast3((int)x9, (int)y9);
                loupe.Refresh();
            }
        }
    }
}