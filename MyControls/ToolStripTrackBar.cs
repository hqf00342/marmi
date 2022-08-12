using System;
using System.Drawing;
using System.Windows.Forms;

/*
TrackBarをToolStripに載せるためのクラス。

利用方法：
Form1()やForm1_Load()で実装させる。
  private ToolStripTrackBar g_trackbar;
  g_trackbar = new ToolStripTrackBar();
  g_trackbar.Minimum = 0;
  g_trackbar.Maximum = 0;		//両方０にすると動かないバーになる。
  g_trackbar.ValueChanged += new EventHandler(g_trackbar_ValueChanged);
  toolStrip1.Items.Add(g_trackbar);
*/

namespace Marmi
{
    public class ToolStripTrackBar : ToolStripControlHost
    {
        /// <summary>
        /// Trackbar用コンストラクタ
        /// </summary>
        public ToolStripTrackBar() : base(new TrackBar())
        {
            this.BackColor = SystemColors.Control;
            Initialize();   //2011年8月19日 追加
        }

        /// <summary>
        /// TrackBarコントロールを返すプロパティ
        /// </summary>
        public TrackBar TrackBar => (TrackBar)Control;

        /// <summary>
        /// TickFrequencyをset/get用プロパティ
        /// </summary>
        public int TickFrequency
        {
            get => TrackBar.TickFrequency;
            set => TrackBar.TickFrequency = value;
        }

        /// <summary>
        /// 最小値のプロパティ
        /// </summary>
        public int Minimum
        {
            get => TrackBar.Minimum;
            set => TrackBar.Minimum = value;
        }

        /// <summary>
        /// 最大値のプロパティ
        /// </summary>
        public int Maximum
        {
            get => TrackBar.Maximum;
            set => TrackBar.Maximum = value;
        }

        /// <summary>
        /// 現在の値
        /// </summary>
        public int Value
        {
            get => TrackBar.Value;
            set => TrackBar.Value = value;
        }

        //TODO: ここにnewは必要？
        //　new はプロパティをオーバーライドするときに必要
        //　どうやらsealedで隠蔽されているものを上書きするときに必要？
        //　親クラスのメソッドを"オーバーライド"(≠隠蔽)する時はoverrideキーワード。
        //　親クラスのメソッドを"隠蔽"する時はnewキーワード。
        public new int Width
        {
            get => TrackBar.Width;
            set => TrackBar.Width = value;
        }

        public int SmallChange
        {
            get => TrackBar.SmallChange;
            set => TrackBar.SmallChange = value;
        }

        public int LargeChange
        {
            get => TrackBar.LargeChange;
            set => TrackBar.LargeChange = value;
        }

        //ValueChangedイベントをサブスクライブ（登録）する。
        protected override void OnSubscribeControlEvents(Control control)
        {
            base.OnSubscribeControlEvents(control);
            TrackBar tb = (TrackBar)Control;
            tb.ValueChanged += new EventHandler(Tb_ValueChanged);
            tb.MouseWheel += new MouseEventHandler(Tb_MouseWheel);
        }

        //ValueChangedイベントをアンサブスクライブ（登録解除）する。
        protected override void OnUnsubscribeControlEvents(Control control)
        {
            base.OnUnsubscribeControlEvents(control);
            TrackBar tb = (TrackBar)Control;
            tb.ValueChanged -= new EventHandler(Tb_ValueChanged);
            tb.MouseWheel -= new MouseEventHandler(Tb_MouseWheel);
        }

        //イベント定義。ValueChanged()
        public event EventHandler ValueChanged;

        //サブスクライブで自動生成されたコード。
        //ValueChaned()を起こすように追記
        private void Tb_ValueChanged(object sender, EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        //2011/09/01
        //マウスホイールイベントの実装
        public event EventHandler<MouseEventArgs> MouseWheel;

        private void Tb_MouseWheel(object sender, MouseEventArgs e)
        {
            if (MouseWheel != null)
                MouseWheel(null, e);
        }

        public void Initialize()
        {
            Minimum = 0;
            Value = 0;
            Maximum = 0;
        }
    }
}