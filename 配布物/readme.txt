=====================================================================
【タイトル】 アーカイブ画像ビューア Marmi
【作成月日】 2009年7月12日～
【制 作 者】 T.Nagashima
【動作環境】 日本語版 WindowsXP/Vista以降 (.NetFramework3.5以降)
【配布形態】 フリーウェア
【HomePage】 http://yk.tea-nifty.com/netdev/
【連 絡 先】 b4rsk.yk@gmail.com
【転　　載】 自由
【著 作 権】 Copyright (C) 2009-2021 by T.Nagashima
=====================================================================


■概要

  フォルダや圧縮ファイルに入っている画像ファイルを閲覧するソフトです。
  ２画像を並べて同時表示できる漫画ビューア機能があります。

  [特徴]
  ・2ページ同時閲覧可能。
  ・zip、lzh、rar、tar、7z、gz、tgz形式に対応
  ・書庫内に書庫がある場合でもその書庫内を走査します。
  ・書庫は次回開いた際に続きから表示します。
  ・SSDに優しい？。圧縮ファイルを展開せずに表示します。
  ・フォルダごと閲覧可、再帰的にサブフォルダまで検索
  ・サムネイル一覧表示機能
  ・pdfに対応(ver1.57より。susie pluginが必要）

■動作環境

 Windows10で動作確認。
 Windows 7～ 8.1で動作するには .NET Framework4.8が必要になります。


■対応書庫、対応画像形式

  書庫： zip, lzh, rar, tar, 7z, gz ,tgz
  画像： bmp, jpg, png, gif, ico, tiff, pdf
  アニメーション形式には対応していません。
  pdfは32bit環境のみ。64bit環境では閲覧できません。


■インストール・アンインストール

  展開したファイルを好きなフォルダにおいてください。
  この中のMarmi.exeが実行プログラムになります。
  設定ファイルはこのフォルダに保存されます。
  レジストリは使用しておりません。
  アンインストールはこのフォルダごと削除すれば完了です。

■pdf対応

  pdf閲覧にはsusie plugin axpdf.spiで動作確認しています。
  ただし64bit環境では動作しません。

  axpdf.spi (GPLv3)
  http://mimizunoapp.appspot.com/susie/

  同spiファイルをmarmi.exeと同じフォルダに入れてください。
  うまく読み込めていない場合はpdfはサポートされないと警告が出ます。

■使い方

  圧縮ファイルやフォルダ、画像をドラッグアンドドロップしてください。
  キーボード、マウス操作は主に次のようになります。
  オプションでカスタマイズ可能です。

　　画面右側クリック　次ページ
　　画面左側クリック　前ページ
　　右クリック　　　　コンテキストメニュー表示
　　右ドラッグ　　　　ルーペ起動
　　→矢印キー　　　　次ページ
　　↓矢印キー　　　　次ページ
　　スペース　　　　　次ページ
　　←矢印キー　　　　前ページ
　　↑矢印キー　　　　前ページ
　　ＥＳＣキー　　　　全画面モード中止
　　Ctrl+TAB　　　　　複数起動中のMarmiを切り替える


■利用、転載条件、ライセンス

  利用は個人利用、商用問わずご自由にご利用ください。
  ただし本ソフトウェアの使用に因るあらゆる事態に対し作者は一切責任を負いません。
  転載も自由ですが、下記連絡先へ記載いただけると助かります。
  また、本ソフトウェアは以下のライブラリを利用しています。作成、公開されている
  皆様に感謝いたします。

  SevenZipSharp（GNU LGPL3.0）
  http://sevenzipsharp.codeplex.com/
  
  7z.dll
  http://7-zip.org

  axpdf.spi (GPLv3)
  http://mimizunoapp.appspot.com/susie/

  アイコン
  一部のアイコンは以下のものを利用させていただいています。
  Fugue Icons - Copyright (C) 2010 Yusuke Kamiyamane. All rights reserved.
  The icons are licensed under a Creative Commons Attribution 3.0 license.
  http://creativecommons.org/licenses/by/3.0/
  http://p.yusukekamiyamane.com/



■連絡先

バグ・要望は以下のHPのコメント欄にお願いします。
http://yk.tea-nifty.com/netdev/2009/07/marmi-e771.html


■変更履歴

ver 1.97a 2023年1月22日
・ページの続きから読む機能が動作しないケースがある、を修正。
・設定画面の内部動作を修正

ver 1.95 2022年9月18日
  ・ Windows10に対応しました。
  ・.NET Framework4.8が必要です（Windows10では不要）
  ・高解像度ディスプレイに対応
  ・64bit環境に対応しました。
  ・rar5対応を除外しました。通常のrarは閲覧可能です。
  ・64bitモードの場合、プラグインの制約でpdfに未対応です。

ver1.82 2014年4月6日
  ・しおりメニューに画像を表示するように変更
  ・書庫を常に一時フォルダに展開するオプションを追加
  ・しおり位置が保存されないことがあるバグの修正
  ・強制的に2ページモードにするオプションを追加
  ・多重起動禁止時、ドロップしたファイルを前のMarmiに送るようにしました
  ・最終ページでの次の書庫を開く際1冊目の次が2ではなく10になっているバグを
    修正（並び替えの問題）
  ・キーボード：同じ機能を２つに割り当てられるように変更
  ・キーボード：Ctrl、Shiftを認識するように変更
  ・マウスのホイールボタン、戻る進むボタンも機能割り当てできるように変更
  ・サムネイルを必要になるまで作成しないように変更。
  ・サムネイル表示にスムーススクロールオプションを追加

ver1.78 2014年3月23日
  ・unrar.dllによるrar5書庫に対応しました。
  ・キーバインド設定に「終了」を追加しました。
  ・多重起動を禁止するオプションを付けました。（オプションメニュー）
  ・起動時の画面位置がおかしくことを修正。（オプションメニュー）
  ・最大化状態で終了したことを覚えるよう修正。（オプションメニュー）
  ・最近使ったファイルが保存されないことがあるバグを修正
  ・しおり（ブックマーク）も保存対象に変更
  ・しおり（ブックマーク）を１つのメニューに変更
  ・サムネイル画面にしおりがついているマークを追加
  ・画像ファイルを1枚だけ開くとき、そのディレクトリを参照するように変更
  ・ページ移動時に表示倍率を維持するモードを追加
  ・オプションメニューの下に簡易な設定を追加

ver1.75 2014年1月12日
  ・D&D起動時に時間がかかることがあるのを若干改善
  ・一時展開フォルダを指定出来るようにしました。
  ・最近利用したファイル（MRU）の数指定、消去を行えるようにしました。

ver1.72 2013年10月26日
  ・スムーススクロール関連のオプション表示が間違っていたのを修正
  ・画面中央表示オプションでウィンドウサイズはリセットしない仕様に変更

ver1.71 2013年10月13日
  ・2枚表示時の判定を緩くするオプションを追加
  ・サイドバーのスムーススクロールをoffするオプションを追加
  ・最終ページで次の書庫を探すオプションを追加
  ・上記に伴いオプションウィンドウに「表示」を追加

ver1.69 2013年8月21日
  ・[bug] gif画像のサムネイルが作れないバグを修正
  ・[bug] TIFFに対応していなかったのを修正
  ・[bug] 2ページモード時に最終ページを見た際に落ちる不具合を修正

ver1.67 2013年8月11日
  ・マウスのカスタマイズを増やした（画面クリック位置とツールバー動作の分離）
  ・ツールバーにある「全画面」「終了」の文字を消せるようにオプション追加
  ・表示メニューが長すぎるため「ページ移動」関連を別メニューに
  ・[bug] ツールバー下の状態でサムネイル画面の動作がオカシイのを修正
  ・[bug] サムネイル画面のまま全画面にすると普通の画面に戻るのを修正
  ・[bug] サムネイル画面のまま最小化すると例外終了するのを修正
  ・[bug] サムネイル作成中に別のzip/フォルダを開こうとすると異常終了するのを修正
  ・[bug]（特にpdfで）メモリ利用量が想定以上に増加するのを修正
  ・[bug] 起動直後トラックバーが表示されないことがあるバグを修正

ver1.63 2013年8月4日
  ・画面キャッシュが無い状態で画像を消す仕様を消さないように変更

ver1.62 2013年7月28日
  ・ツールバーを下に移動できるようにした。

ver1.61 2013年7月21日
  ・必要条件を.net Framework 3.5(ClientProfile)に変更
  ・Ctrl+TABによる複数起動のMarmiの切替に対応

ver1.60 2013年7月7日
  ・アニメーション効果を軽くした
  ・サムネイル作成中に次の書庫を見ると例外が発生するのを修正
  ・トラックバーのサイズがたまに反映されないのを修正
  ・キーコンフィグで設定無しの重複が出来ないバグを修正

ver1.57 2013年5月19日
  ・pdf対応（susie plugin経由）

ver1.56 2013年5月11日
  ・クリック連打対応（ver1.53の対応強化）
  ・サムネイルの事前読込を復活
  ・上記に伴う全体的な高速化

ver1.53 2013年5月6日
  ・画面中央に表示するオプションを追加
  ・クリック連打に対応（ただし、未完全で連打しすぎると前ページに戻ることあり）
  ・その他高速化のための処理追加

ver1.47 2013年3月31日
  ・平常時でもCPU使用率が高かったバグを修正。
  ・サムネイル表示のフェードインに対応

ver1.33 2013年1月20日
  ・メモリ利用量を圧縮（キャッシュの持ち方を変更）
  ・サムネイル作成を必要な時だけに変更
  ・上記に伴いDBファイルを作成しないように変更
  ・アニメーション関連のスピードを変更
  ・その他動作速度の微調整多数
  ・画像一枚表示の場合、DELキーで画像をゴミ箱に擦れられるようになった
  ・スライドショー機能追加

ver1.34 2012年4月15日
  ・書庫内書庫でフォルダ構造をしているときに動作しない場合があるのを修正

ver1.32 2012年3月11日
  ・書庫内書庫に対応（暫定）
  ・gz、bzip、tgz形式に対応
  ・左開きに暫定対応しました
  ・Ctrl+ホイールに強制的にズームを割り当て
  ・画面の切り替わり方法としてスライドを標準にしました。
  ・必要ないときにスクロールバーが表示される場合があったのを修正
  ・パスワード付き書庫に何度も確認が出ていたを修正

ver1.29 2012年1月19日
  ・2枚表示チェック判定を甘くしました

ver1.28 2011年12月14日
  ・サムネイル保存時にサムネイル作成中だとエラー終了していた問題を修正
  ・ツールバーを少し小さくしました
  ・動作速度を若干改善しました

ver1.26 2011年11月27日
  ・ズーム・回転機能を実装
  ・サムネイルサイズの変更機能を実装
  ・他オプションメニューの追加

ver1.23 2011年10月24日
  ・2枚表示の条件を改善
  ・サイドバー表示時にキー入力がダブることがあるのを修正
  ・画面の切り替え効果が正しく反映されないことがあるのを修正
  ・キーカスタマイズのドロップダウンの重複を修正
  ・起動直後に「表示」メニューを開くとエラー終了するのを修正
  ・オプションダイアログの表示項目を改善

ver1.21 2011年10月16日
  ・キーボードカスタマイズ機能を追加
  ・画面切り替わりアニメーションを選択可能に

ver1.20 2011年10月12日
  ・しおり機能追加
  ・サムネイル画面での右クリックメニューに対応
  ・２ページ表示判定を改善、ほぼ同じサイズの時のみ２ページ化
  ・サムネイル画面でマウス位置と異なる画像を選択する可能性があるのを修正
  ・サムネイル作成を効率化（ライブラリ最適化）
  ・100%表示時にマウスドラッグでスクロールできなかったバグを修正
  ・トラックバーでのプレビューを高速に
  ・メモリキャッシュを最適化しメモリ利用量を抑えました

ver1.13 2011年9月4日
  ・7z書庫正式対応
  ・画像切り替えにアニメーション効果を追加
  ・配布ファイル数を減らしました
  ・トラックバーを操作したあとマウスホイールが効かなくなるバグを修正
  ・ルーペ使用時に左クリックすると固まる現象を修正
  ・サイドバー等の画像サイズがおかしかったのを修正
  ・SharpZipLibから7z.dllに全面移行しました
    ほか微修正多数

ver1.01 2011年8月2日
  ・サムネイル作成中に不安定となるケールがあるのを改善

ver1.00 2011年8月2日
  ・zip以外の書庫に対応。（rar,tar,lzh,7z）
  ・ただし、7z形式はソリッド書庫のため、速度が著しく遅い状態
  ・ルーペの動作を変更。部分表示から全画面表示に変更。
  ・利用ライブラリをSharpZipからSevenZipSharpに変更。
  ・サムネイルサイズのデフォルトを160dotに変更
  ・キーボード操作が効かなくなることがある現象を若干改善（まだ不完全）
  ・マウスホイールが効かなくなることがある現象を若干改善（まだ不完全）
ver0.83  2009年7月12日
  ・初版リリース



■ライセンス

  7-Zip
  ~~~~~
  License for use and distribution
  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

  7-Zip Copyright (C) 1999-2011 Igor Pavlov.

  Licenses for files are:

    1) 7z.dll: GNU LGPL + unRAR restriction
    2) All other files:  GNU LGPL

  The GNU LGPL + unRAR restriction means that you must follow both 
  GNU LGPL rules and unRAR restriction rules.


  Note: 
    You can use 7-Zip on any computer, including a computer in a commercial 
    organization. You don't need to register or pay for 7-Zip.


  GNU LGPL information
  --------------------

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You can receive a copy of the GNU Lesser General Public License from 
    http://www.gnu.org/


  unRAR restriction
  -----------------

    The decompression engine for RAR archives was developed using source 
    code of unRAR program.
    All copyrights to original unRAR code are owned by Alexander Roshal.

    The license for original unRAR code has the following restriction:

      The unRAR sources cannot be used to re-create the RAR compression algorithm, 
      which is proprietary. Distribution of modified unRAR sources in separate form 
      or as a part of other software is permitted, provided that it is clearly
      stated in the documentation and source comments that the code may
      not be used to develop a RAR (WinRAR) compatible archiver.


  --
  Igor Pavlov



----------------------------------------------------------------------
                   GNU LESSER GENERAL PUBLIC LICENSE
                       Version 3, 29 June 2007

 Copyright (C) 2007 Free Software Foundation, Inc. <http://fsf.org/>
 Everyone is permitted to copy and distribute verbatim copies
 of this license document, but changing it is not allowed.


  This version of the GNU Lesser General Public License incorporates
the terms and conditions of version 3 of the GNU General Public
License, supplemented by the additional permissions listed below.

  0. Additional Definitions.

  As used herein, "this License" refers to version 3 of the GNU Lesser
General Public License, and the "GNU GPL" refers to version 3 of the GNU
General Public License.

  "The Library" refers to a covered work governed by this License,
other than an Application or a Combined Work as defined below.

  An "Application" is any work that makes use of an interface provided
by the Library, but which is not otherwise based on the Library.
Defining a subclass of a class defined by the Library is deemed a mode
of using an interface provided by the Library.

  A "Combined Work" is a work produced by combining or linking an
Application with the Library.  The particular version of the Library
with which the Combined Work was made is also called the "Linked
Version".

  The "Minimal Corresponding Source" for a Combined Work means the
Corresponding Source for the Combined Work, excluding any source code
for portions of the Combined Work that, considered in isolation, are
based on the Application, and not on the Linked Version.

  The "Corresponding Application Code" for a Combined Work means the
object code and/or source code for the Application, including any data
and utility programs needed for reproducing the Combined Work from the
Application, but excluding the System Libraries of the Combined Work.

  1. Exception to Section 3 of the GNU GPL.

  You may convey a covered work under sections 3 and 4 of this License
without being bound by section 3 of the GNU GPL.

  2. Conveying Modified Versions.

  If you modify a copy of the Library, and, in your modifications, a
facility refers to a function or data to be supplied by an Application
that uses the facility (other than as an argument passed when the
facility is invoked), then you may convey a copy of the modified
version:

   a) under this License, provided that you make a good faith effort to
   ensure that, in the event an Application does not supply the
   function or data, the facility still operates, and performs
   whatever part of its purpose remains meaningful, or

   b) under the GNU GPL, with none of the additional permissions of
   this License applicable to that copy.

  3. Object Code Incorporating Material from Library Header Files.

  The object code form of an Application may incorporate material from
a header file that is part of the Library.  You may convey such object
code under terms of your choice, provided that, if the incorporated
material is not limited to numerical parameters, data structure
layouts and accessors, or small macros, inline functions and templates
(ten or fewer lines in length), you do both of the following:

   a) Give prominent notice with each copy of the object code that the
   Library is used in it and that the Library and its use are
   covered by this License.

   b) Accompany the object code with a copy of the GNU GPL and this license
   document.

  4. Combined Works.

  You may convey a Combined Work under terms of your choice that,
taken together, effectively do not restrict modification of the
portions of the Library contained in the Combined Work and reverse
engineering for debugging such modifications, if you also do each of
the following:

   a) Give prominent notice with each copy of the Combined Work that
   the Library is used in it and that the Library and its use are
   covered by this License.

   b) Accompany the Combined Work with a copy of the GNU GPL and this license
   document.

   c) For a Combined Work that displays copyright notices during
   execution, include the copyright notice for the Library among
   these notices, as well as a reference directing the user to the
   copies of the GNU GPL and this license document.

   d) Do one of the following:

       0) Convey the Minimal Corresponding Source under the terms of this
       License, and the Corresponding Application Code in a form
       suitable for, and under terms that permit, the user to
       recombine or relink the Application with a modified version of
       the Linked Version to produce a modified Combined Work, in the
       manner specified by section 6 of the GNU GPL for conveying
       Corresponding Source.

       1) Use a suitable shared library mechanism for linking with the
       Library.  A suitable mechanism is one that (a) uses at run time
       a copy of the Library already present on the user's computer
       system, and (b) will operate properly with a modified version
       of the Library that is interface-compatible with the Linked
       Version.

   e) Provide Installation Information, but only if you would otherwise
   be required to provide such information under section 6 of the
   GNU GPL, and only to the extent that such information is
   necessary to install and execute a modified version of the
   Combined Work produced by recombining or relinking the
   Application with a modified version of the Linked Version. (If
   you use option 4d0, the Installation Information must accompany
   the Minimal Corresponding Source and Corresponding Application
   Code. If you use option 4d1, you must provide the Installation
   Information in the manner specified by section 6 of the GNU GPL
   for conveying Corresponding Source.)

  5. Combined Libraries.

  You may place library facilities that are a work based on the
Library side by side in a single library together with other library
facilities that are not Applications and are not covered by this
License, and convey such a combined library under terms of your
choice, if you do both of the following:

   a) Accompany the combined library with a copy of the same work based
   on the Library, uncombined with any other library facilities,
   conveyed under the terms of this License.

   b) Give prominent notice with the combined library that part of it
   is a work based on the Library, and explaining where to find the
   accompanying uncombined form of the same work.

  6. Revised Versions of the GNU Lesser General Public License.

  The Free Software Foundation may publish revised and/or new versions
of the GNU Lesser General Public License from time to time. Such new
versions will be similar in spirit to the present version, but may
differ in detail to address new problems or concerns.

  Each version is given a distinguishing version number. If the
Library as you received it specifies that a certain numbered version
of the GNU Lesser General Public License "or any later version"
applies to it, you have the option of following the terms and
conditions either of that published version or of any later version
published by the Free Software Foundation. If the Library as you
received it does not specify a version number of the GNU Lesser
General Public License, you may choose any version of the GNU Lesser
General Public License ever published by the Free Software Foundation.

  If the Library as you received it specifies that a proxy can decide
whether future versions of the GNU Lesser General Public License shall
apply, that proxy's public statement of acceptance of any version is
permanent authorization for you to choose that version for the
Library.
