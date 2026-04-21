# ⏱️ UniTask って何？

## 🎯 一言で言うと

**ずんだもん**：「一言で言うと...**時間がかかる処理を超高速＆楽に書けるツール**なのだ！」

**つむぎ**：「時間がかかる処理...？」

**めたん**：「まずは『非同期処理』について説明しますね」

---

## 🤔 なぜ必要なの？「非同期処理」とは

**つむぎ**：「非同期処理...？なんか難しそう...」

**めたん**：「身近な例で説明しましょう」

**ずんだもん**：「わかりやすく頼むのだ！」

---

### 📝 身近な例で説明

**めたん**：「ゲームでこんな経験ありませんか？」

**つむぎ**：「どんな？」

**めたん**：「例えば...」

- **ローディング画面** - データを読み込んでる間、画面が表示される
- **フェードアウト/イン** - 画面がゆっくり暗くなったり明るくなる
- **アニメーション** - キャラが歩いたり攻撃したり
- **ネットワーク通信** - サーバーからデータを取得

**つむぎ**：「あー、よくあるやつだ！」

**ずんだもん**：「どれも時間がかかるのだ！」

**めたん**：「こういう『時間がかかる処理』を扱うのが非同期処理なんです」

---

### 😓 時間がかかる処理の問題

**めたん**：「普通に書くとこうなります」

```csharp
void Start()
{
    // データ読み込み（3秒かかる）
    LoadData();  // ←ここで3秒止まる！

    // 次の処理
    Debug.Log("読み込み完了！");
}
```

**つむぎ**：「何が問題なの？」

**めたん**：「LoadData()が終わるまで、ゲームが**完全に止まってしまう**んです」

**ずんだもん**：「え！？ゲームが固まるのだ！？」

**つむぎ**：「それヤバくない！？」

**めたん**：「ええ。画面も真っ白、操作もできず、フリーズしたように見えます」

---

### 🎬 実際の問題シーン

```csharp
void Start()
{
    Debug.Log("ゲーム開始！");

    // 3秒かかる処理
    LoadHugeData();  // ←3秒間ゲームが止まる！

    Debug.Log("読み込み完了！");  // ←3秒後にようやく表示
}
```

**つむぎ**：「プレイヤーから見たら、バグってるように見えるわ...」

**ずんだもん**：「ローディング画面も表示できないのだ！」

---

## 🔄 Unityの従来の解決策：コルーチン

**めたん**：「Unityには『コルーチン』という機能があります」

**ずんだもん**：「コルーチン！聞いたことあるのだ！」

### 📖 コルーチンの例

```csharp
IEnumerator Start()
{
    Debug.Log("ゲーム開始！");

    // 3秒待つ
    yield return new WaitForSeconds(3f);

    Debug.Log("3秒経った！");
}
```

**つむぎ**：「yield return って何？」

**めたん**：「『ここで一旦止まって、後で再開する』という意味です」

**ずんだもん**：「なるほど、一時停止ボタンみたいなものなのだ！」

---

### 😰 コルーチンの問題点

**めたん**：「でも、コルーチンにはいくつか問題があるんです」

**つむぎ**：「どんな問題？」

#### 問題1：戻り値が受け取れない

```csharp
// ❌ これはできない
int result = StartCoroutine(LoadData());  // エラー！

IEnumerator LoadData()
{
    // データ読み込み...
    yield return null;
    return 123;  // ←返せない！
}
```

**ずんだもん**：「え、値を返せないのだ！？」

**つむぎ**：「それ不便じゃん...」

---

#### 問題2：エラー処理が難しい

```csharp
IEnumerator LoadData()
{
    // try-catchが使いにくい...
    yield return StartCoroutine(LoadFromServer());

    // エラーハンドリングが複雑になる
}
```

**めたん**：「try-catchでエラーを捕まえるのが難しいんです」

**つむぎ**：「エラー処理できないのはマズいよね...」

---

#### 問題3：処理の連鎖が複雑

```csharp
IEnumerator Start()
{
    yield return StartCoroutine(Step1());
    yield return StartCoroutine(Step2());
    yield return StartCoroutine(Step3());
}

IEnumerator Step1()
{
    Debug.Log("Step1");
    yield return new WaitForSeconds(1f);
}

IEnumerator Step2()
{
    Debug.Log("Step2");
    yield return new WaitForSeconds(1f);
}

IEnumerator Step3()
{
    Debug.Log("Step3");
    yield return new WaitForSeconds(1f);
}
```

**つむぎ**：「うわ、長い...」

**ずんだもん**：「読みにくいのだ...」

---

#### 問題4：遅い＆メモリ食う

**めたん**：「コルーチンは内部でイテレータを使うので...」

- **GC（ガベージ）が発生** - メモリのゴミが出る
- **パフォーマンスが悪い** - 大量に使うと重くなる

**つむぎ**：「スマホゲームには厳しそう...」

---

## ✨ UniTask の登場！

**ずんだもん**：「そこでUniTaskの出番なのだ！」

**つむぎ**：「これで全部解決するの？」

**めたん**：「ええ、全ての問題を解決してくれます」

### 🚀 UniTaskの特徴

**めたん**：「主に5つの強みがあります」

#### 1️⃣ めちゃくちゃ速い

**つむぎ**：「どれくらい速いの？」

**めたん**：「ベンチマークでは...」

```
【1000個の非同期処理を実行】
コルーチン: 50ms
UniTask: 0.5ms  （100倍速い！）
```

**ずんだもん**：「爆速なのだ！」

**つむぎ**：「100倍！？ヤバすぎでしょ！」

---

#### 2️⃣ Zero Allocation（ゴミが出ない）

**めたん**：「メモリのゴミが出ないんです」

```
【GC Allocation（ゴミの量）】
コルーチン: 毎回40B
UniTask: 0B  （ゴミゼロ！）
```

**ずんだもん**：「メモリに優しいのだ！」

**つむぎ**：「スマホゲームには最高じゃん！」

---

#### 3️⃣ 戻り値が受け取れる

**めたん**：「async/awaitで自然に書けます」

```csharp
// ✅ UniTaskなら簡単！
async UniTask<int> LoadData()
{
    await UniTask.Delay(1000);  // 1秒待つ
    return 123;  // 返せる！
}

async UniTaskVoid Start()
{
    int result = await LoadData();  // 受け取れる！
    Debug.Log(result);  // 123
}
```

**つむぎ**：「お！普通に値を返せるんだ！」

**ずんだもん**：「わかりやすいのだ！」

---

#### 4️⃣ エラー処理が簡単

**めたん**：「try-catchが普通に使えます」

```csharp
async UniTaskVoid Start()
{
    try
    {
        await LoadFromServer();
        Debug.Log("成功！");
    }
    catch (Exception e)
    {
        Debug.LogError($"エラー: {e.Message}");
    }
}
```

**つむぎ**：「これは見やすい！」

**ずんだもん**：「エラー処理もバッチリなのだ！」

---

#### 5️⃣ コードがシンプル

**めたん**：「同じことをする場合の比較です」

**コルーチン**
```csharp
IEnumerator Start()
{
    yield return StartCoroutine(Step1());
    yield return StartCoroutine(Step2());
    yield return StartCoroutine(Step3());
    Debug.Log("完了！");
}
```

**UniTask**
```csharp
async UniTaskVoid Start()
{
    await Step1();
    await Step2();
    await Step3();
    Debug.Log("完了！");
}
```

**つむぎ**：「UniTaskの方が短くて読みやすい！」

**ずんだもん**：「スッキリしてるのだ！」

---

## 💻 基本的な使い方

**つむぎ**：「じゃあ、実際にどう使うか教えて！」

**ずんだもん**：「コード見せるのだ！」

**めたん**：「ステップバイステップで説明しますね」

---

### ステップ1：パッケージをインストール

**めたん**：「まず、UniTaskをインストールします」

```
1. Window > Package Manager を開く
2. + ボタン > Add package from git URL
3. 以下を入力：
   https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
```

**つむぎ**：「Package Managerから簡単に入れられるんだ！」

---

### ステップ2：基本的な待機処理

**めたん**：「まずは単純な待機処理から」

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Example : MonoBehaviour
{
    async UniTaskVoid Start()
    {
        Debug.Log("開始！");

        // 3秒待つ
        await UniTask.Delay(3000);  // ミリ秒で指定

        Debug.Log("3秒経った！");
    }
}
```

**ずんだもん**：「await UniTask.Delay()で待てるのだ！」

**つむぎ**：「シンプルでわかりやすい！」

---

### ステップ3：次のフレームまで待つ

**めたん**：「1フレーム待つこともできます」

```csharp
async UniTaskVoid Start()
{
    Debug.Log("フレーム1");

    // 次のフレームまで待つ
    await UniTask.Yield();

    Debug.Log("フレーム2");
}
```

**つむぎ**：「yield return null みたいなやつね！」

**めたん**：「ええ、同じことができます」

---

### ステップ4：フレーム数で待つ

```csharp
async UniTaskVoid Start()
{
    Debug.Log("開始");

    // 10フレーム待つ
    await UniTask.DelayFrame(10);

    Debug.Log("10フレーム後");
}
```

**ずんだもん**：「フレーム単位でも待てるのだ！」

---

## 🎮 実際の使用例

**つむぎ**：「もっと実践的な例も見せてよ！」

**めたん**：「いくつか紹介しますね」

---

### 例1：フェードイン/アウト

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    [SerializeField] private Image fadeImage;

    // フェードアウト
    async UniTask FadeOut(float duration)
    {
        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / duration);
            fadeImage.color = color;

            await UniTask.Yield();  // 次のフレームまで待つ
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    // フェードイン
    async UniTask FadeIn(float duration)
    {
        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsed / duration);
            fadeImage.color = color;

            await UniTask.Yield();
        }

        color.a = 0f;
        fadeImage.color = color;
    }

    // 使い方
    async UniTaskVoid Start()
    {
        await FadeOut(1f);  // 1秒でフェードアウト
        await UniTask.Delay(2000);  // 2秒待つ
        await FadeIn(1f);  // 1秒でフェードイン
    }
}
```

**つむぎ**：「おー、フェード処理がスッキリ書けてる！」

**ずんだもん**：「await で順番に実行されるのだ！」

**めたん**：「コルーチンより読みやすいでしょう？」

---

### 例2：シーンロード

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    async UniTaskVoid LoadSceneAsync(string sceneName)
    {
        // ローディング画面を表示
        ShowLoadingScreen();

        // シーンを非同期で読み込み
        await SceneManager.LoadSceneAsync(sceneName);

        // ローディング画面を非表示
        HideLoadingScreen();

        Debug.Log($"{sceneName} 読み込み完了！");
    }

    void ShowLoadingScreen()
    {
        // ローディング画面表示処理...
    }

    void HideLoadingScreen()
    {
        // ローディング画面非表示処理...
    }
}
```

**つむぎ**：「シーン読み込みも await できるんだ！」

**ずんだもん**：「便利なのだ！」

---

### 例3：ネットワーク通信

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    async UniTask<string> FetchDataFromServer(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // サーバーにリクエスト送信
            await request.SendWebRequest();

            // エラーチェック
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new System.Exception($"通信エラー: {request.error}");
            }

            // データを返す
            return request.downloadHandler.text;
        }
    }

    async UniTaskVoid Start()
    {
        try
        {
            string data = await FetchDataFromServer("https://api.example.com/data");
            Debug.Log($"取得データ: {data}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"エラー: {e.Message}");
        }
    }
}
```

**つむぎ**：「通信処理もスッキリ！エラー処理もちゃんとできてる！」

**めたん**：「try-catchで簡単にエラーハンドリングできるんです」

---

### 例4：複数の処理を同時実行

```csharp
async UniTaskVoid Start()
{
    // 並列実行
    await UniTask.WhenAll(
        LoadTexture("texture1.png"),
        LoadTexture("texture2.png"),
        LoadTexture("texture3.png")
    );

    Debug.Log("全部のテクスチャ読み込み完了！");
}

async UniTask LoadTexture(string path)
{
    Debug.Log($"{path} 読み込み開始");
    await UniTask.Delay(1000);  // 1秒かかる
    Debug.Log($"{path} 読み込み完了");
}
```

**ずんだもん**：「複数の処理を同時に実行できるのだ！」

**つむぎ**：「これなら3つ合わせて1秒で終わるね！」

**めたん**：「順番に実行すると3秒かかるところが、並列実行なら1秒で済みます」

---

### 例5：タイムアウト設定

```csharp
async UniTaskVoid Start()
{
    try
    {
        // 5秒でタイムアウト
        await FetchDataFromServer("https://api.example.com/data")
            .Timeout(TimeSpan.FromSeconds(5));

        Debug.Log("成功！");
    }
    catch (TimeoutException)
    {
        Debug.LogError("タイムアウト！");
    }
}
```

**つむぎ**：「タイムアウトも簡単に設定できるんだ！」

**ずんだもん**：「通信が遅い時に便利なのだ！」

---

### 例6：キャンセル処理

```csharp
using System.Threading;

public class Example : MonoBehaviour
{
    private CancellationTokenSource cts;

    async UniTaskVoid Start()
    {
        cts = new CancellationTokenSource();

        try
        {
            await LongRunningTask(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("キャンセルされました");
        }
    }

    async UniTask LongRunningTask(CancellationToken token)
    {
        for (int i = 0; i < 100; i++)
        {
            // キャンセルされたかチェック
            token.ThrowIfCancellationRequested();

            Debug.Log($"処理中: {i}");
            await UniTask.Delay(100, cancellationToken: token);
        }
    }

    void OnDestroy()
    {
        // オブジェクト破棄時にキャンセル
        cts?.Cancel();
        cts?.Dispose();
    }
}
```

**つむぎ**：「処理を途中でキャンセルできるんだ！」

**めたん**：「シーン切り替え時などに重要です」

**ずんだもん**：「メモリリーク防止にも役立つのだ！」

---

## 🆚 コルーチンとの比較

**つむぎ**：「結局、コルーチンとどう違うの？」

**めたん**：「詳しく比較しましょう」

### 📊 比較表

| 項目 | コルーチン | UniTask |
|------|-----------|---------|
| **速度** | 遅い | 100倍速い |
| **メモリ** | GC発生 | Zero Allocation |
| **戻り値** | ❌ 返せない | ✅ 返せる |
| **エラー処理** | △ 難しい | ✅ try-catch使える |
| **並列実行** | △ 難しい | ✅ WhenAll簡単 |
| **タイムアウト** | ❌ 自分で実装 | ✅ Timeout()で簡単 |
| **キャンセル** | △ 難しい | ✅ CancellationToken |
| **コードの見た目** | △ 長くなる | ✅ シンプル |

**つむぎ**：「UniTaskが圧勝じゃん！」

**ずんだもん**：「コルーチンを使う理由がないのだ！」

---

### 🔍 具体的なコード比較

**めたん**：「同じことをする場合の比較です」

#### やりたいこと
1. フェードアウト（1秒）
2. データ読み込み（2秒）
3. フェードイン（1秒）

**コルーチン版**
```csharp
IEnumerator LoadSequence()
{
    // フェードアウト
    yield return StartCoroutine(FadeOut(1f));

    // データ読み込み
    yield return StartCoroutine(LoadData());

    // フェードイン
    yield return StartCoroutine(FadeIn(1f));

    Debug.Log("完了！");
}

IEnumerator FadeOut(float duration)
{
    // 実装...
    yield return new WaitForSeconds(duration);
}

IEnumerator LoadData()
{
    // 実装...
    yield return new WaitForSeconds(2f);
}

IEnumerator FadeIn(float duration)
{
    // 実装...
    yield return new WaitForSeconds(duration);
}
```

**UniTask版**
```csharp
async UniTask LoadSequence()
{
    // フェードアウト
    await FadeOut(1f);

    // データ読み込み
    await LoadData();

    // フェードイン
    await FadeIn(1f);

    Debug.Log("完了！");
}

async UniTask FadeOut(float duration)
{
    // 実装...
    await UniTask.Delay((int)(duration * 1000));
}

async UniTask LoadData()
{
    // 実装...
    await UniTask.Delay(2000);
}

async UniTask FadeIn(float duration)
{
    // 実装...
    await UniTask.Delay((int)(duration * 1000));
}
```

**つむぎ**：「StartCoroutine()がいらないからスッキリ！」

**ずんだもん**：「読みやすさが全然違うのだ！」

---

## 🎯 UniTask の便利機能

**めたん**：「UniTaskには他にも便利な機能がたくさんあります」

### 1️⃣ Unityのイベントをawaitできる

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ButtonExample : MonoBehaviour
{
    [SerializeField] private Button button;

    async UniTaskVoid Start()
    {
        // ボタンがクリックされるまで待つ
        await button.OnClickAsync();

        Debug.Log("ボタンがクリックされた！");
    }
}
```

**つむぎ**：「ボタンクリックを await できるの！？」

**ずんだもん**：「便利すぎるのだ！」

---

### 2️⃣ Time.timeScale の影響を受けない待機

```csharp
// Time.timeScaleの影響を受ける（ゲーム時間）
await UniTask.Delay(1000);

// Time.timeScaleの影響を受けない（実時間）
await UniTask.Delay(1000, ignoreTimeScale: true);
```

**つむぎ**：「ポーズ中でも時間を進められるんだ！」

**めたん**：「ポーズメニューのアニメーションなどに便利です」

---

### 3️⃣ プログレス（進捗）報告

```csharp
async UniTask DownloadFile(IProgress<float> progress)
{
    for (int i = 0; i <= 100; i++)
    {
        await UniTask.Delay(50);

        // 進捗を報告
        progress?.Report(i / 100f);
    }
}

async UniTaskVoid Start()
{
    var progress = Progress.Create<float>(value =>
    {
        Debug.Log($"進捗: {value * 100}%");
    });

    await DownloadFile(progress);
}
```

**つむぎ**：「ローディングバーの実装に使えそう！」

**ずんだもん**：「進捗が見えるのは嬉しいのだ！」

---

### 4️⃣ AsyncReactiveProperty（値の変化を待つ）

```csharp
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public class Example : MonoBehaviour
{
    private AsyncReactiveProperty<int> hp = new AsyncReactiveProperty<int>(100);

    async UniTaskVoid Start()
    {
        // HPが0になるまで待つ
        await hp.FirstAsync(x => x <= 0);

        Debug.Log("HP が 0 になった！ゲームオーバー！");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            hp.Value -= 10;  // HPを減らす
        }
    }
}
```

**つむぎ**：「値が変わるのを待てるんだ！すごい！」

**めたん**：「Reactive Programming との組み合わせです」

---

## ⚠️ 注意点

**つむぎ**：「注意点も教えて！」

**めたん**：「いくつかあります」

### 注意点1：UniTaskVoid と UniTask の違い

**めたん**：「戻り値を使わない場合は`UniTaskVoid`を使います」

```csharp
// ✅ 良い例：戻り値を使わない
async UniTaskVoid Start()
{
    await UniTask.Delay(1000);
    Debug.Log("完了");
}

// ⚠️ これでもOKだけど警告が出る
async UniTask Start()
{
    await UniTask.Delay(1000);
    Debug.Log("完了");
}

// ✅ 良い例：戻り値を使う
async UniTask<int> GetScore()
{
    await UniTask.Delay(1000);
    return 100;
}
```

**ずんだもん**：「使い分けが大事なのだ！」

**つむぎ**：「戻り値いらないなら UniTaskVoid ってことね！」

---

### 注意点2：OnDestroy でキャンセルする

**めたん**：「オブジェクトが破棄されたら、UniTaskもキャンセルしましょう」

```csharp
public class Example : MonoBehaviour
{
    async UniTaskVoid Start()
    {
        // このオブジェクトが破棄されたらキャンセル
        await LongTask(this.GetCancellationTokenOnDestroy());
    }

    async UniTask LongTask(CancellationToken token)
    {
        for (int i = 0; i < 1000; i++)
        {
            await UniTask.Delay(100, cancellationToken: token);
            Debug.Log(i);
        }
    }
}
```

**つむぎ**：「メモリリーク防止ってやつ？」

**めたん**：「ええ、とても重要です」

---

### 注意点3：例外は必ず処理する

```csharp
// ❌ 悪い例：例外を無視
async UniTaskVoid BadExample()
{
    await SomethingThatMightFail();  // エラーが握りつぶされる
}

// ✅ 良い例：例外を処理
async UniTaskVoid GoodExample()
{
    try
    {
        await SomethingThatMightFail();
    }
    catch (Exception e)
    {
        Debug.LogError($"エラー: {e.Message}");
    }
}
```

**ずんだもん**：「エラー処理は大事なのだ！」

---

## 🎓 こんな人におすすめ

**つむぎ**：「結局、どんな人が使うといいの？」

**めたん**：「整理しましょう」

### ✅ おすすめな人

**ずんだもん**：「こんな人におすすめなのだ！」

✅ Unity でゲーム・アプリを作っている
✅ コルーチンを使ったことがある
✅ 非同期処理をたくさん書く
✅ パフォーマンスを重視したい
✅ スマホゲームを作っている
✅ async/await を学びたい

### ⚠️ まだ早い人

**めたん**：「こういう場合は、まだ使わなくてもいいかもしれません」

❌ Unity 完全初心者
❌ コルーチンを使ったことがない
❌ C# の基礎がわかっていない
❌ 非同期処理を使わないゲーム

**つむぎ**：「まずはコルーチンから学んで、慣れたら UniTask に移行するのが良さそうね」

**ずんだもん**：「段階的に学ぶのだ！」

---

## 📝 まとめ

**つむぎ**：「じゃあ最後にまとめるよ～！」

**ずんだもん**：「UniTask のまとめなのだ！」

### UniTask とは？

**めたん**：「整理しましょう」

> **Unity 向けの超高速・Zero Allocation な非同期処理ライブラリ**

### ✨ 特徴

**ずんだもん**：「こんな特徴があるのだ！」

- ⚡ **爆速** - コルーチンの100倍速い
- 💾 **Zero Allocation** - GC発生なし
- 📝 **シンプル** - async/await で読みやすい
- 🎯 **戻り値** - 値を返せる
- 🛡️ **エラー処理** - try-catch が使える
- 🔧 **豊富な機能** - タイムアウト、キャンセル、並列実行など

### 👍 こんな時に使おう

**つむぎ**：「チェックリスト！」

✅ フェードイン/アウト
✅ ローディング処理
✅ シーン遷移
✅ ネットワーク通信
✅ アニメーション制御
✅ コルーチンの置き換え

### 🆚 コルーチンとの違い

| 項目 | コルーチン | UniTask |
|------|-----------|---------|
| 速度 | 遅い | 100倍速い |
| メモリ | GC発生 | Zero Allocation |
| 戻り値 | ❌ | ✅ |
| エラー処理 | △ | ✅ |

### 📚 学習リソース

**めたん**：「公式リソースを紹介します」

- 📚 **公式GitHub**: https://github.com/Cysharp/UniTask
- 📖 **公式ドキュメント**: READMEが充実しています
- 💻 **サンプルコード**: GitHubのサンプルフォルダ

**ずんだもん**：「Cysharp ファミリーなのだ！」
- **UniTask** - 非同期処理 ←今ここ！
- **MessagePack** - データ保存
- **MasterMemory** - マスターデータ管理
- **VContainer** - 依存性注入

**つむぎ**：「全部合わせて使うと最強！」

---

## 🎬 おわりに

**つむぎ**：「というわけで、UniTask の解説でした～！」

**めたん**：「理解できましたか？」

**ずんだもん**：「UniTask を使えば、コルーチンより速くて楽に非同期処理が書けるのだ！」

**つむぎ**：「あーし的には、もうコルーチンには戻れないわ。UniTask マジで便利！」

**めたん**：「async/await の概念は最初は難しいかもしれませんが、一度覚えれば強力な武器になりますよ」

**ずんだもん**：「UniTask で快適な Unity 開発なのだ～！」

**つむぎ**：「また会おうね～！」
