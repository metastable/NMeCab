using NMeCab;
using System;
using System.Linq;

class Program
{
    static void Main()
    {
        Program.UseWithoutConsciousDictionaly();
        Console.WriteLine("--------------------------------");
        Program.UseWithUserPreparedDictionaly1();
        Console.WriteLine("--------------------------------");
        Program.UseWithUserPreparedDictionaly2();
        Console.WriteLine("--------------------------------");
        Program.UseNBest();
        Console.WriteLine("--------------------------------");
        Program.UseSoftWakachi();
    }

    static void UseWithoutConsciousDictionaly()
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

    static void UseWithUserPreparedDictionaly1()
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

    static void UseWithUserPreparedDictionaly2()
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

    static void UseNBest()
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

    static void UseSoftWakachi()
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