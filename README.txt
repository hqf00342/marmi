# DPI aware

app.manifest を参照。

# TODO

画像が多いときにキャッシュを作らないようにする。条件を確定する必要がある。

・書庫でない場合は作成しない
・ソリッド書庫は作成したい
・サムネイルを作るだけのループはありうる
・再生空での○○MBを超えたらやめる
・キャッシュがなかった場合の処理
・キャッシュをGCさせる措置


# 画像保持の構造

App.g_pi        // PackageInfo
    .Items      // List<ImageInfo>

ImageInfo
    .CacheImage // RawImage
    ._rawimage  // byte[](private)


# v1.8.8時点のキャッシュの作り方

AsyncIOで作成されている。
画像を読み込みを要求しているのはキューへの積み込みを呼び出している部分
キャッシュに積み込んでいるのはAsyncIOのみ。

AsyncIO.AddJobLow()
    Form1.AsyncLoadImageInfo()  //全画像よみこみ
    Navibar3.DrawItem()         //画像がないときに読み込み依頼
    Sidebar.DrawItem()          

AsyncIO.AddJob()
    Bmp.AsyncGetBitmap()        汎用メソッド。１画像読み込み
    Form1.Start()               キャッシュクリア信号送信
    Form1.InitControls()        キャッシュクリア信号送信（ダブり）


# サムネイルの作成

AsyncIO内で実行。キャッシュを読み込んだ後に画像サイズ、サムネイル作成を行っている。

# ジョブ作成の整理

・サムネイル作成
・終了信号、クリア命令
・キャッシュ生成
・キャッシュ生成。今すぐ使いたい
・画像サイズ取得


# キャッシュイメージの区分

・オリジナル
・圧縮（高品質）
・圧縮（低品質）

# 確認したい点

・PackageType.Directoryは必要か      --> 除外できるレベルまで持ってきた
・ThumbnailPanelの SaveThumbnailImageAsync(int thumbSize, int numX, string FilenameCandidate)がnumXを使ってない。
