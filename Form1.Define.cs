using System;
using System.Drawing;					// Size, Bitmap, Font , Point, Graphics
using System.Windows.Forms;				// UserControl
using System.Collections.Generic;		// Dictionary

namespace Marmi
{
	//ver1.21画像切り替え方法
	public enum AnimateMode : int
	{
		none = 0,
		Slide,
		CrossFade,
		FastDrawWithThumbnail
	}

	//スレッドの状態を表す
	enum ThreadStatus
	{
		STOP,			//スレッドは動作していない
		RUNNING,		//スレッドは動作中
		REQUEST_STOP,	//スレッドに停止要求中。STOPに変わる
		PAUSE,			//スレッドに中断要求。次にRUNNINGになるまで待つ。
	}

	//各種情報の表示位置
	enum DiagLocate
	{
		Left,
		Right,
		Top,
		Bottom,
		Middle
	};

	//サムネイルモード
	enum ThumnailMode
	{
		Tile,           // 通常のタイル表示
		SquareTile,    // 正方形
		HeightFix	    // 高さだけ合わせる
	}
	//サムネイルサイズ
	enum DefaultThumbSize
	{
		minimum = 64,
		small = 128,
		normal = 160,
		large = 256,
		big = 400
	}

	//メモリモデル
	public enum MemoryModel
	{
		Small,	//キャッシュしない
		Large,	//キャッシュ生成する。ver1.34までのデフォルト
		UserDefined
	}

	/// <summary>
	/// PackageInfoのパッケージタイプ
	/// </summary>
	public enum PackageType
	{
		None,		//なにもない、未定義
		Archive,	//書庫ファイルであることを表す
		Pictures,	//画像ファイル集であることを表す
		Directory,	//１ディレクトリ内の画像集であることを表す
		Pdf			//pdfファイル
	}

		//    #region キーコンフィグリスト
		//public static Dictionary<string, Keys> keyConfigList
		//    = new Dictionary<string, Keys>()
		//{
		//    {"(なし)", Keys.None},
		//    {"→", Keys.Right},
		//    {"←", Keys.Left},
		//    {"↑", Keys.Up},
		//    {"↓", Keys.Down},
		//    {"ESC", Keys.Escape},
		//    {"Space", Keys.Space},
		//    {"Enter", Keys.Enter},
		//    {"Tab", Keys.Tab},
		//    {"PageUp", Keys.PageUp},
		//    {"PageDown", Keys.PageDown},
		//    {"Home", Keys.Home},
		//    {"End", Keys.End},
		//    {"Insert", Keys.Insert},
		//    {"Delete", Keys.Delete},
		//    {"BackSpace", Keys.Back},
		//    {"1", Keys.D1},
		//    {"2", Keys.D2},
		//    {"3", Keys.D3},
		//    {"4", Keys.D4},
		//    {"5", Keys.D5},
		//    {"6", Keys.D6},
		//    {"7", Keys.D7},
		//    {"8", Keys.D8},
		//    {"9", Keys.D9},
		//    {"0", Keys.D0},
		//    {"A", Keys.A},
		//    {"B", Keys.B},
		//    {"C", Keys.C},
		//    {"D", Keys.D},
		//    {"E", Keys.E},
		//    {"F", Keys.F},
		//    {"G", Keys.G},
		//    {"H", Keys.H},
		//    {"I", Keys.I},
		//    {"J", Keys.J},
		//    {"K", Keys.K},
		//    {"L", Keys.L},
		//    {"M", Keys.M},
		//    {"N", Keys.N},
		//    {"O", Keys.O},
		//    {"P", Keys.P},
		//    {"Q", Keys.Q},
		//    {"R", Keys.R},
		//    {"S", Keys.S},
		//    {"T", Keys.T},
		//    {"U", Keys.U},
		//    {"V", Keys.V},
		//    {"W", Keys.W},
		//    {"X", Keys.X},
		//    {"Y", Keys.Y},
		//    {"Z", Keys.Z}
		//};
		//#endregion

}