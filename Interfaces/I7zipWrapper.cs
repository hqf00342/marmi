using System.IO;
/*
7zip用に必要なインターフェースを定義
*/

namespace Marmi
{
    public interface I7zipWrapper
    {
        /// <summary>書庫を開いているかどうか</summary>
        bool IsOpen { get; }

        /// <summary>書庫にあるアイテム数</summary>
        int ItemCount { get; }

        /// <summary>ソリッド書庫か</summary>
        bool IsSolid { get; }

        //ArchivedFiles Items { get; set; }

        /// <summary>書庫名</summary>
        string Filename { get; }

        //static void ClearPassword();

        /// <summary>ファイルを開く</summary>
        bool Open(string ArchiveName);

        /// <summary>書庫を閉じる</summary>
        void Close();

        /// <summary>Streamを取得</summary>
        Stream GetStream(string filename);

        /// <summary>全展開</summary>
        void ExtractAll(string ExtractDir);

        /// <summary>全展開をキャンセル。動作していない</summary>
        void CancelExtractAll();
    }
}
