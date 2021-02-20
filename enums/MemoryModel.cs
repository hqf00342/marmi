namespace Marmi
{
    //メモリモデル
    public enum MemoryModel
    {
        Small,  //キャッシュしない
        Large,  //キャッシュ生成する。ver1.34までのデフォルト
        UserDefined
    }
}