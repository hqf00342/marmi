namespace Marmi
{
    /// <summary>
    /// PackageInfoのパッケージタイプ
    /// </summary>
    public enum PackageType
    {
        None,       //なにもない、未定義
        Archive,    //書庫ファイルであることを表す
        Pictures,   //画像ファイル集であることを表す
        Directory,  //１ディレクトリ内の画像集であることを表す
        Pdf         //pdfファイル
    }
}