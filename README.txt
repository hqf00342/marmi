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
