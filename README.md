# 形態素解析エンジンNMeCab
<img src="icon/NMeCab-icon-100.png">

## リポジトリ移転について

2010年から[OSDN](https://ja.osdn.net/projects/nmecab)で開発し公開してきたNMeCabですが、バージョン0.10.0からは、こちらGitHubで開発し公開していきます。

## これは何？

NMeCabは.NETで開発した日本語形態素解析エンジンです。.NET環境で簡単に使用できるライブラリとし、NuGetでのインストールも可能にしてあります。  
その名の通り、元はMeCabというC++で開発された形態素解析エンジンを参考にしており、同じ解析手法を組み込んであります。  
形態素解析には膨大な日本語情報を学習させた辞書データが必要になりますが、NMeCabではMeCabに対応した辞書をそのまま使用できます。

## NuGet

| NuGet ID | ステータス | 説明 |
| --- | --- | --- |
| [LibNMeCab](https://www.nuget.org/packages/LibNMeCab) | [![NuGet LibNMeCab](https://img.shields.io/nuget/v/LibNMeCab.svg)](https://www.nuget.org/packages/LibNMeCab) | NMeCabライブラリ単体パッケージ |
| [LibNMeCab.IpaDicBin](https://www.nuget.org/packages/LibNMeCab.IpaDicBin) | [![NuGet LibNMeCab.IpaDicBin](https://img.shields.io/nuget/v/LibNMeCab.IpaDicBin.svg)](https://www.nuget.org/packages/LibNMeCab.IpaDicBin) | IPA辞書パッケージ |

辞書パッケージをNuGetパッケージマネージャーでインストールすると、依存するNMeCabライブラリ単体パッケージも同時にインストールされます。

## 使い方

### 辞書を意識しないで使用する

`LibNMeCab.IpaDicBin` をNuGetでインストールするだけで、すぐに形態素解析を始めることができます。

サンプルコード:
```csharp
using System;
using NMeCab;

class Program
{
    static void Main()
    {
        using (var tagger = NMeCabIpaDic.CreateTagger()) // Taggerインスタンスを生成
        {
            var nodes = tagger.Parse("皇帝の新しい心"); // 形態素解析を実行
            foreach (var node in nodes) // 形態素ノード配列を順に処理
            {
                Console.WriteLine($"表層系：{node.Surface}");
                Console.WriteLine($"読み　：{node.Reading}");
                Console.WriteLine($"品詞　：{node.PartsOfSpeech}");
                Console.WriteLine();
            }
        }
    }
}
```

まず `NMeCabIpaDic.CreateTagger()` により、形態素解析処理の起点となるTaggerインスタンス（MeCabTagger継承クラスのインスタンス）を生成します。
このインスタンスは、使用後に必ず `Dispose()` メソッドを呼び出してリソース開放を行う必要があります（IDisposable実装クラスのインスタンスです）。
そのため、このサンプルでは `using` ステートメントを記述しています。
- 補足
  - Taggerインスタンスは生成時に読み込んだ辞書リソースを保持しているため、Dispose()を忘れるとそれが解放されないことに注意してください。
  usingステートメントを使うと確実に Dispose()を行うことができます。
  - .NETにある程度習熟している方であれば、「一度読み込んだ辞書リソースを使い続けたいので、すぐにはDispose()したくない」という場合にusingステートメントを記述せず、Taggerインスタンスを使いまわすこともできます。

そのTaggerインスタンスの `Parse(string sentence)` メソッドへ、任意の文字列を渡すと形態素解析が行われ、形態素に分割された結果を配列として取得することができます。
- 補足
  - 以前のNMeCabは実行パフォーマンスを重視し、オリジナルのMeCabと同様に先頭ノードが返却され、他のノードへは連結リストをたどってアクセスする方式でした。今のNMeCabでは、Linqなどから使いやすい、配列を返却する方式に変更してあります。
  - また、今でも必要に応じて、連結リスト（Prev、Nextプロパティ）により前後のノードへアクセスできるようにしてあります。

配列の中身は形態素ノード（MeCabNodeBase継承クラス）のインスタンスであり、形態素解析により得られた情報がプロパティに格納されています。このサンプルでは、表層系（ `Surface` ）、読み（ `Reading` ）、品詞（ `PartsOfSpeech` ）を取得しコンソールに出力しています。

他にもプロパティで取得できる情報があります。
VisualStudioのIntelliSenseなどにより閲覧できるよう、XML文書化コメントに記載してあるので確認してみてください。
- 補足
  - 使用する辞書により得られる情報（素性文字列）のフォーマットが異なるので、以前のNMeCabはオリジナルのMeCabと同様に全ての素性文字列をまとめて取得できるだけとしていましたが、今は個別の素性情報のプロパティを持たせてあります。
  - 辞書により異なるプロパティを持つ形態素ノードを使い分ける設計としたので、詳しくはMeCabNodeBaseクラスなどのソースコードを参照してください。別途解説も記載したいと思います。

サンプルコードの結果:
```
表層系：皇帝
読み　：コウテイ
品詞　：名詞

表層系：の
読み　：ノ
品詞　：助詞

表層系：新しい
読み　：アタラシイ
品詞　：形容詞

表層系：心
読み　：ココロ
品詞　：名詞
```

### 辞書を自分で用意して使用する

`LibNMeCab` だけをNuGetでインストールし、辞書は自分で用意したものを使うこともできます。

NMeCabで使う辞書は、MeCabの `mecab-dict-index` コマンドを使って 「解析用バイナリ辞書」 にしておく必要があります。
MeCabのインストール方法と使用方法については、[MeCabのサイト](https://taku910.github.io/mecab/)などを参照してください。
なお、解析用バイナリ辞書の文字コードが選べるときは、「utf-8」にしておくことが無難です。

結果として以下のファイルが必要になりますので、任意のディレクトリにまとめて配置してください。
- char.bin
- matrix.bin
- sys.dic
- uni.dic

上記以外のファイルは、同じディレクトリにあってもなくても、NMeCabの動作に影響しません。

サンプルコード：
```csharp
using System;
using NMeCab;

class Program
{
    static void Main()
    {
        var dicDir = @"C:\Program Files (x86)\MeCab\dic\ipadic"; // 辞書のパス

        using (var tagger = MeCabTagger.Create(dicDir)) // 汎用のTaggerインスタンスを生成
        {
            var nodes = tagger.Parse("皇帝の新しい心"); // 形態素解析を実行
            foreach (var node in nodes) // 形態素ノード配列を順に処理
            {
                Console.WriteLine($"表層系：{node.Surface}");
                Console.WriteLine($"素性　：{node.Feature}"); // 全ての素性文字列
                Console.WriteLine();
            }
        }
    }
}
```

このサンプルでは `MeCabTagger.Create(string dicDir, string[] userDics = null)` により、辞書ディレクトリパスを指定してTaggerインスタンスを生成しています。
- 補足
  - ここでは、WindowsでMeCabインストーラーを使いデフォルト設定でインストールしたときの辞書ディレクトリパスにしてみましたが、当然、任意のパスを指定できます。
  - このパス文字列は、その環境のランタイムの仕様に従っていれば大丈夫であり、相対パスなども指定可能です。
  - もし、パスが不正な時や、辞書ファイルが壊れている時などは、IO関係の例外がスローされます。

サンプルコード:
``` csharp
using System;
using NMeCab;

class Program
{
    static void Main()
    {
        var dicDir = @"C:\Program Files (x86)\MeCab\dic\ipadic"; // 辞書のパス

        using (var tagger = MeCabIpaDicTagger.Create(dicDir)) // IPAdic形式用のTaggerインスタンスを生成
        {
            var nodes = tagger.Parse("皇帝の新しい心"); // 形態素解析を実行
            foreach (var node in nodes) // 形態素ノード配列を順に処理
            {
                Console.WriteLine($"表層系：{node.Surface}");
                Console.WriteLine($"読み　：{node.Reading}"); // 個別の素性
                Console.WriteLine($"品詞　：{node.PartsOfSpeech}"); // 〃
                Console.WriteLine();
            }
        }
    }
}
```

指定する辞書の素性フォーマットがIPA辞書と同じであるならば、このサンプルのように、`MeCabIpaDicTagger.Create(string dicDir, string[] userDics = null)` により、IPA辞書用のTaggarインスタンスを生成し、個別の素性情報のプロパティを持った形態素ノードを形態素解析結果とすることができます。

### ユーザー辞書を使用する

辞書を自分で用意して使用するパターンの場合、まずはシステム辞書を用意しますが、そこに含まれていない単語を追加したくなったとき、ユーザー辞書を使うことが可能です。

やはりMeCabで解析用バイナリ辞書にしたファイルを、システム辞書と同じディレクトリに配置してください。
ファイル名は任意です。複数のユーザー辞書を使用することも可能です。

サンプルコード:
``` csharp
        var dicDir = @"C:\Program Files (x86)\MeCab\dic\ipadic"; // 辞書のパス
        var userDics = new[] { "usr1.dic", "usr2.dic" }; // ユーザー辞書ファイル名の一覧

        using (var tagger = MeCabIpaDicTagger.Create(dicDir, userDics)) // ユーザー辞書も指定してTaggerインスタンスを生成
        {
```

Taggerインスタンス生成メソッドの第2引数にユーザー辞書ファイル名の配列を渡すと、システム辞書と同時にユーザー辞書が読み込まれます。

### Nベスト解

ここまでで説明してきた `Parse(string sentence)` メソッドの場合、最も確からしい形態素解析結果だけが取得できました。
`ParseNBest(string sentence)` メソッドの場合は、確からしい順番に複数の形態素解析結果を取得できます。
下のサンプルでは、上位5件の結果を表示しています。

サンプルコード:
``` csharp
using System;
using System.Linq;
using NMeCab;

class Program
{
    static void Main()
    {
        using (var tagger = NMeCabIpaDic.CreateTagger())
        {
            var results = tagger.ParseNBest("皇帝の新しい心"); // Nベスト解を取得
            foreach (var nodes in results.Take(5)) // 上位から順に5件の結果を処理
            {
                foreach (var node in nodes) // 形態素ノード配列を順に処理
                {
                    Console.WriteLine("表層系：" + node.Surface);
                    Console.WriteLine("読み　：" + node.Reading);
                    Console.WriteLine("品詞　：" + node.PartsOfSpeech);
                    Console.WriteLine();
                }

                Console.WriteLine("----------------");
            }
        }
    }
}
```

### ソフトわかち書き

`ParseSoftWakachi(string, float)` メソッドの場合、その文章に含まれる可能性がある形態素を洗いざらい取得できます。
また取得した形態素ノードの `Prob` プロパティにより、その形態素の含まれる確率（周辺確率）も取得できます。

`ParseSoftWakachi(string, float)` の第2引数（theta）は、周辺確率のなめらかさを指定する温度パラメータです。
これを大きな値にすると、最も確からしい解の形態素の周辺確率が1または∞、その他は0となります。

- 補足
  - 日本語の性質上、形態素解析の正解は一意でなく曖昧なケースがあります。またもちろん形態素解析エンジンの精度の限界もあります。そのために考案された手法がソフトわかち書きです。検索エンジンのインデクシングや将来のNLPなどに応用できるはずです。
  - このサンプルの温度パラメータには「辞書のコストファクター800の逆数÷2」を指定していますが、どんな値が良いかを知るには試行錯誤が必要です。

サンプルコード：
```csharp
using System;
using System.Linq;
using NMeCab;

class Program
{
    static void Main()
    {
        using (var tagger = NMeCabIpaDic.CreateTagger())
        {
            var theta = 1f / 800f / 2f; // 温度パラメータ
            var nodes = tagger.ParseSoftWakachi("本部長", theta); // ソフトわかち解を取得
            foreach (var node in nodes.Where(n => n.Prob > 0.1)) // 周辺確率＞0.1の形態素ノードだけを処理
            {
                Console.WriteLine("表層系　：" + node.Surface);
                Console.WriteLine("読み　　：" + node.Reading);
                Console.WriteLine("品詞　　：" + node.PartsOfSpeech);
                Console.WriteLine("周辺確率：" + node.Prob);
                Console.WriteLine();
            }
        }
    }
}
```

サンプルコードの結果:
```
表層系　：本部
読み　　：ホンブ
品詞　　：名詞
周辺確率：0.5966396

表層系　：本
読み　　：ホン
品詞　　：接頭詞
周辺確率：0.2812245

表層系　：部長
読み　　：ブチョウ
品詞　　：名詞
周辺確率：0.3622776

表層系　：長
読み　　：チョウ
品詞　　：名詞
周辺確率：0.5903029
```

### ラティス出力

coming soon..

### 新しい素性フォーマットへの対応

coming soon..
