# クリーンアーキテクチャって何なのだ？～Unity初心者でもわかる設計の話～

LighthouseSample
https://github.com/lisearcheleeds/LighthouseSample

## 登場人物
- **ずんだもん**：東北ずん子の精霊。元気いっぱいで好奇心旺盛
- **四国めたん**：落ち着いた性格で丁寧な話し方をする。難しいことも優しく説明してくれる。
- **春日部つむぎ**：埼玉県内の高校に通うギャルの女の子。やんちゃに見えて実は真面目

---

## 第1話：クリーンアーキテクチャって何？

### ずんだもん
ねえねえめたん！このプロジェクトのコードを見ていたら、`Presenter`とか`View`とか、なんか色んなファイルに分かれているのだ！これって何なのだ？

### 四国めたん
あ、それは「クリーンアーキテクチャ」という設計方法を使っているからですね。プログラムを整理整頓するための考え方なんです。

### 春日部つむぎ
クリーンアーキテクチャ？なんかカタカナばっかで難しそうじゃん...あーしにもわかる？

### 四国めたん
大丈夫ですよ！身近な例で説明しますね。つむぎさん、お家の部屋って、用途ごとに分かれてますよね？

### 春日部つむぎ
うん！リビングとか、寝室とか、キッチンとか！

### 四国めたん
その通りです！それぞれの部屋には役割があって、ごちゃ混ぜにならないように分けてありますよね。プログラムも同じで、「役割ごとに分けて整理しよう」というのがクリーンアーキテクチャの基本的な考え方なんです。

### ずんだもん
なるほどなのだ！じゃあ、プログラムの「部屋」って何があるのだ？

---

## 第2話：プログラムの「層（レイヤー）」

### 四国めたん
クリーンアーキテクチャでは、プログラムを「層（レイヤー）」に分けて考えます。よく「同心円」に例えられるんですが...

### 春日部つむぎ
同心円？バウムクーヘンみたいな感じ？

### 四国めたん
そうです！まさにそんな感じです。中心から外側に向かって、いくつかの層があります。このプロジェクトでわかりやすい例を見てみましょう。

```
外側の層（画面表示）
    ↓
中間の層（処理の指示）
    ↓
内側の層（ルール・データ）
```

### ずんだもん
これがバウムクーヘンなのだ？もうちょっと詳しく教えてほしいのだ！

### 四国めたん
では、実際の例で説明しますね。このプロジェクトの「Homeシーン」を見てみましょう。

---

## 第3話：具体例で見る層の分け方

### 四国めたん
Homeシーンには、こんなファイルがあります：

1. **HomeView.cs** - 画面に表示するボタンの情報
2. **HomePresenter.cs** - ボタンが押されたときの処理
3. **HomeScene.cs** - シーン全体の管理

### 春日部つむぎ
それぞれ何が違うの？

### 四国めたん
例えば、レストランで考えてみましょう。

- **HomeView（ビュー）**：お客さんが見るメニュー表やテーブル
- **HomePresenter（プレゼンター）**：注文を受けて、厨房に指示を出すウェイター
- **その他のサービス**：実際に料理を作る厨房

### ずんだもん
お客さんは厨房の中を見ないし、厨房の人は直接お客さんと話さないのだ！ウェイターが間に入っているのだ！

### 四国めたん
その通りです！それぞれの役割をはっきり分けることで、変更しやすくなるんです。

---

## 第4話：実際のコードを見てみよう

### 四国めたん
実際のコードを見てみましょう。まずは**HomeView**から。

```csharp
public class HomeView : MonoBehaviour, IHomeView
{
    [SerializeField] LHButton guideButton;
    [SerializeField] LHButton sampleButton;

    IDisposable IHomeView.SubscribeGuideButtonClick(Action action)
        => guideButton.SubscribeOnClick(action);
}
```

### 春日部つむぎ
なんか`LHButton`とか`SerializeField`とか書いてあるけど...

### 四国めたん
これは「画面にボタンがあるよ」「ボタンが押されたら教えてね」という情報だけを持っているんです。**押されたらどうするか**は書いてありません。

### ずんだもん
レストランで言うと「テーブルにメニュー表があります」「お客さんが呼び出しボタンを押しました」って報告するだけなのだ！

### 四国めたん
そうです！では次に**HomePresenter**を見てみましょう。

```csharp
public class HomePresenter : IHomePresenter
{
    ISceneManager sceneManager;
    IScreenStackModule screenStackModule;

    void OnClickSampleButton()
    {
        sceneManager.TransitionScene(
            new SampleTopScene.SampleTopTransitionData(),
            TransitionType.Cross
        ).Forget();
    }
}
```

### 春日部つむぎ
あ、こっちには「サンプルボタンが押されたらSampleTopシーンに移動する」って書いてある！

### 四国めたん
その通り！PresenterはViewから「ボタンが押されました」って報告を受けて、「じゃあシーン移動をお願いします」って指示を出すんです。

### ずんだもん
ウェイターが「ハンバーグ一つお願いします！」って厨房に注文を通すのと同じなのだ！

---

## 第5話：なんで分けるの？

### 春日部つむぎ
でもさ、全部一つのファイルに書いた方が楽じゃない？なんでわざわざ分けるの？

### 四国めたん
良い質問ですね！分ける理由はいくつかあります。

#### 理由1：変更しやすい

### 四国めたん
例えば、ボタンのデザインを変えたいとき。Viewだけを変更すれば良くて、Presenterは触らなくていいんです。

### ずんだもん
レストランで「メニュー表のデザインを変える」ときに、厨房のレシピは変えなくていいのと同じなのだ！

#### 理由2：テストしやすい

### 四国めたん
Presenterは画面表示の部分と分かれているので、画面を開かなくてもテストできるんです。

### 春日部つむぎ
あー、「このボタンを押したらこの処理が動く」ってことだけ確認できるってこと？

### 四国めたん
その通りです！実際に画面を作らなくても、処理だけをテストできるんです。

#### 理由3：複数人で作業しやすい

### 四国めたん
Aさんが画面のデザイン（View）を作って、Bさんがボタンを押したときの処理（Presenter）を作る、というように分担できます。

### ずんだもん
お互いの作業がぶつからないから、同時に作業できるのだ！

---

## 第6話：依存の方向

### 四国めたん
クリーンアーキテクチャでもう一つ大事なのが「依存の方向」です。

### 春日部つむぎ
依存？なにそれ？

### 四国めたん
「どっちがどっちを知っているか」という関係性のことです。例えば、このプロジェクトでは...

```
HomeView (画面) → IHomePresenter (約束事)
HomePresenter (処理) → IHomePresenter (約束事)
```

### ずんだもん
`I`が付いてる`IHomePresenter`って何なのだ？

### 四国めたん
これは「インターフェース」といって、「こういう機能があるよ」という約束事だけを書いたものです。

```csharp
public interface IHomePresenter
{
    void Setup();
    void OnEnter();
}
```

### 春日部つむぎ
「Setup()とOnEnter()っていう機能があります」って宣言してるだけ？

### 四国めたん
そうです！ViewとPresenterは、お互いを直接見ないで、この「約束事（インターフェース）」だけを見るんです。

### ずんだもん
これって何が嬉しいのだ？

### 四国めたん
例えば、Presenterの中身を全部書き換えても、インターフェースさえ守っていればViewは何も変更しなくていいんです。

---

## 第7話：料理で例える依存の方向

### 四国めたん
もう一度レストランの例で説明しますね。

### 四国めたん
お客さん（View）は「ハンバーグを注文する」というメニュー（インターフェース）しか知りません。実際に誰が作るか、どうやって作るかは知らなくていいんです。

### 春日部つむぎ
あー！だから厨房のシェフが変わっても、お客さんは同じように注文できるんだ！

### ずんだもん
「ハンバーグください」って言えば、新人シェフでもベテランシェフでも、ハンバーグが出てくるのだ！

### 四国めたん
その通りです！これを「依存性の逆転」と言います。お客さんが厨房の詳細を知らなくていいように、ViewがPresenterの中身を知らなくていいんです。

---

## 第8話：DI（依存性注入）って何？

### 春日部つむぎ
コードに`[Inject]`とか`IContainerBuilder`とか書いてあったけど、あれは何？

### 四国めたん
それは「DI（Dependency Injection：依存性注入）」という仕組みです。

### ずんだもん
イゾンセイチュウニュウ...？難しい言葉なのだ！

### 四国めたん
簡単に言うと「必要なものを自動で渡してくれる仕組み」です。

```csharp
public class HomeLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<HomePresenter>(Lifetime.Singleton)
               .AsImplementedInterfaces();
        builder.RegisterComponent(homeView)
               .AsImplementedInterfaces();
    }
}
```

### 春日部つむぎ
これって何やってるの？

### 四国めたん
「HomePresenterが必要になったら、自動で作って渡してあげるよ」という登録をしているんです。

### ずんだもん
自分で`new HomePresenter()`って書かなくていいのだ？

### 四国めたん
そうです！VContainerというツールが、必要なときに自動で作って渡してくれます。

---

## 第9話：DIのメリット

### 四国めたん
DIを使うメリットを、学校で例えてみますね。

### 四国めたん
**DIを使わない場合**：
- 授業のたびに、生徒が自分で教科書を印刷して、ノートを作って、ペンを買いに行く

**DIを使う場合**：
- 学校が必要な教科書やノートを用意してくれて、授業のときに配ってくれる

### 春日部つむぎ
あー！確かに毎回自分で全部用意するの大変じゃん！

### ずんだもん
それに、教科書が変わったときも、学校が新しいのを配ってくれるから、生徒は何もしなくていいのだ！

### 四国めたん
その通り！プログラムでも同じで：

```csharp
public class HomePresenter : IHomePresenter
{
    ISceneManager sceneManager;
    IScreenStackModule screenStackModule;

    [Inject]
    public void Construct(
        ISceneManager sceneManager,
        IScreenStackModule screenStackModule)
    {
        this.sceneManager = sceneManager;
        this.screenStackModule = screenStackModule;
    }
}
```

### 四国めたん
この`Construct`メソッドに`[Inject]`って書いてあると、VContainerが自動で`sceneManager`と`screenStackModule`を作って渡してくれるんです。

### 春日部つむぎ
自分で`new`する必要ないんだ！便利！

---

## 第10話：このプロジェクトの構造

### 四国めたん
このLighthouseプロジェクトの構造を整理すると、こうなります。

```
【外側の層】View（画面表示）
  ↓ 依存
【中間の層】Presenter（処理の指示）
  ↓ 依存
【内側の層】サービス（実際の機能）
  - SceneManager（シーン管理）
  - ScreenStackModule（ダイアログ管理）
  - LanguageService（多言語対応）
```

### ずんだもん
依存は外から内に向かっているのだ！

### 四国めたん
そうです！外側が内側を使うけど、内側は外側のことを知りません。

### 春日部つむぎ
お客さん（View）がウェイター（Presenter）に注文して、ウェイターが厨房（サービス）に指示を出す。でも厨房はお客さんのことを直接知らない、みたいな？

### 四国めたん
完璧な理解です！

---

## 第11話：ファイル構成を見てみよう

### 四国めたん
Homeシーンのファイル構成を見てみましょう。

```
Home/
├── HomeScene.cs           # シーンの管理（Unityと連携）
├── HomePresenter.cs       # ボタンの処理ロジック
├── IHomePresenter.cs      # Presenterの約束事
├── HomeView.cs            # 画面のボタン（Unity要素）
├── IHomeView.cs           # Viewの約束事
└── HomeLifetimeScope.cs   # DIの設定
```

### ずんだもん
それぞれの役割がはっきり分かれているのだ！

### 四国めたん
そうなんです。そして、このパターンは他のシーンでも同じです。

```
SampleTop/
├── SampleTopScene.cs
├── SampleTopPresenter.cs
├── ISampleTopPresenter.cs
├── SampleTopView.cs
├── ISampleTopView.cs
└── SampleTopLifetimeScope.cs

Title/
├── TitleScene.cs
├── TitlePresenter.cs
├── ITitlePresenter.cs
├── TitleView.cs
├── ITitleView.cs
└── TitleLifetimeScope.cs
```

### 春日部つむぎ
どのシーンも同じ構造なんだ！これなら新しいシーンを作るときも、同じパターンで作ればいいってことだね！

---

## 第12話：クリーンアーキテクチャの原則

### 四国めたん
ここまでの内容を、クリーンアーキテクチャの原則としてまとめますね。

### 原則1：役割ごとに層を分ける

### 四国めたん
- **View（ビュー）**：画面表示、ボタン、Unity特有の要素
- **Presenter（プレゼンター）**：ボタンが押されたときの処理
- **Service（サービス）**：実際の機能（シーン管理、データ管理など）

### 原則2：依存は外から内へ

### 四国めたん
- 外側の層は内側の層を使える
- 内側の層は外側の層を知らない
- 内側ほど変わりにくい、重要な部分

### ずんだもん
バウムクーヘンの中心（内側）ほど、大事なルールがあるのだ！

### 原則3：インターフェースで約束する

### 四国めたん
- 直接クラスを見るのではなく、インターフェースを見る
- これにより、中身を変えても外側に影響しない

### 原則4：依存性は注入する（DI）

### 四国めたん
- 必要なものは自分で作らず、外から渡してもらう
- これにより、テストしやすく、変更しやすくなる

---

## 第13話：実際の処理の流れ

### 春日部つむぎ
じゃあ、実際にボタンが押されたとき、どういう流れで処理されるの？

### 四国めたん
HomeシーンでSampleボタンが押されたときの流れを見てみましょう。

```
1. ユーザーがSampleボタンをタップ
   ↓
2. HomeView が「ボタンが押された」ことを検知
   ↓
3. HomeView が HomePresenter の OnClickSampleButton を呼ぶ
   ↓
4. HomePresenter が SceneManager に「SampleTopシーンに移動して」と指示
   ↓
5. SceneManager が実際にシーン移動を実行
   ↓
6. 画面がSampleTopシーンに切り替わる
```

### ずんだもん
それぞれが自分の役割だけをやっているのだ！

### 四国めたん
コードで見るとこうなります。

```csharp
// 1. HomeView - ボタンクリックを検知
IDisposable SubscribeSampleButtonClick(Action action)
    => sampleButton.SubscribeOnClick(action);

// 2. HomePresenter - Setup時に購読を登録
homeView.SubscribeSampleButtonClick(OnClickSampleButton);

// 3. HomePresenter - クリック時の処理
void OnClickSampleButton()
{
    // 4. SceneManagerに指示
    sceneManager.TransitionScene(
        new SampleTopScene.SampleTopTransitionData(),
        TransitionType.Cross
    ).Forget();
}
```

### 春日部つむぎ
おー！ちゃんと役割が分かれてる！

---

## 第14話：テストのしやすさ

### 四国めたん
クリーンアーキテクチャのもう一つの大きなメリットは「テストのしやすさ」です。

### ずんだもん
テスト？学校のテストなのだ？

### 四国めたん
プログラムのテストです。「このボタンを押したら、ちゃんと動くかな？」って確認する作業ですね。

### 四国めたん
**クリーンアーキテクチャを使わない場合**：

```csharp
// 全部一緒に書いてある
public class HomeScene : MonoBehaviour
{
    [SerializeField] Button sampleButton;

    void Start()
    {
        sampleButton.onClick.AddListener(() => {
            SceneManager.LoadScene("SampleTop");
        });
    }
}
```

### 春日部つむぎ
これ、テストするの大変そう...

### 四国めたん
そうなんです。Unityのシーンを実際に開いて、ボタンを押して、確認しないといけません。

### 四国めたん
**クリーンアーキテクチャを使う場合**：

```csharp
// テストコード（例）
[Test]
public void SampleButtonClick_CallsTransitionScene()
{
    // 偽物のViewとSceneManagerを用意
    var mockView = new MockHomeView();
    var mockSceneManager = new MockSceneManager();

    // Presenterを作成
    var presenter = new HomePresenter(mockView, mockSceneManager);
    presenter.Setup();

    // ボタンクリックをシミュレート
    mockView.SimulateClick();

    // SceneManagerが呼ばれたか確認
    Assert.IsTrue(mockSceneManager.WasCalled);
}
```

### ずんだもん
Unityを起動しなくても、処理だけをテストできるのだ！

### 四国めたん
その通り！Presenterは画面から分離されているので、画面を作らなくてもテストできるんです。

---

## 第15話：変更に強い設計

### 春日部つむぎ
他にどんなメリットがあるの？

### 四国めたん
「変更に強い」というのも大きなメリットです。例えば...

#### 例1：ボタンのデザインを変える

### 四国めたん
ボタンの色を変えたい、位置を変えたい、アニメーションを付けたい...

→ **HomeViewだけ変更すればOK**

### ずんだもん
HomePresenterは触らなくていいのだ！

#### 例2：シーン遷移の方法を変える

### 四国めたん
フェードアウト→フェードインじゃなくて、スライドで切り替えたい...

→ **SceneManagerだけ変更すればOK**

### 春日部つむぎ
ViewもPresenterも変えなくていいんだ！

#### 例3：Unityから別のゲームエンジンに移行

### 四国めたん
これは極端な例ですが...もしUnityから別のゲームエンジンに変えることになったら？

### ずんだもん
えええ！？全部書き直しなのだ！？

### 四国めたん
クリーンアーキテクチャなら、Viewの部分だけ書き直せばいいんです。Presenterのビジネスロジックはそのまま使えます。

### 春日部つむぎ
マジで！？すごくない！？

---

## 第16話：初心者がやりがちな間違い

### 四国めたん
クリーンアーキテクチャを学び始めた人がよくやる間違いを紹介しますね。

### 間違い1：PresenterからViewのMonoBehaviourメソッドを直接呼ぶ

```csharp
// ❌ ダメな例
public class HomePresenter
{
    HomeView homeView;

    void ShowButton()
    {
        homeView.gameObject.SetActive(true);  // Unity依存！
    }
}
```

### ずんだもん
何がダメなのだ？

### 四国めたん
PresenterがUnity特有の`gameObject`を直接触っています。これだとPresenterがUnityに依存してしまって、テストしにくくなります。

```csharp
// ✅ 良い例
public class HomePresenter
{
    IHomeView homeView;

    void ShowButton()
    {
        homeView.Show();  // インターフェース経由！
    }
}

// IHomeView
public interface IHomeView
{
    void Show();
}

// HomeView
public class HomeView : MonoBehaviour, IHomeView
{
    void IHomeView.Show()
    {
        gameObject.SetActive(true);  // ViewならOK
    }
}
```

### 春日部つむぎ
あー、インターフェースを経由すれば、Presenterは Unity のこと知らなくていいんだ！

### 間違い2：Viewにビジネスロジックを書く

```csharp
// ❌ ダメな例
public class HomeView : MonoBehaviour
{
    void OnClickButton()
    {
        if (PlayerPrefs.GetInt("Level") >= 10)
        {
            SceneManager.LoadScene("Advanced");
        }
        else
        {
            SceneManager.LoadScene("Beginner");
        }
    }
}
```

### 四国めたん
Viewが「レベル10以上ならAdvanced」という判断をしています。これはビジネスロジックなので、Presenterに書くべきです。

```csharp
// ✅ 良い例
public class HomeView : MonoBehaviour
{
    Action onButtonClick;

    void OnClickButton()
    {
        onButtonClick?.Invoke();  // 通知するだけ
    }
}

public class HomePresenter
{
    void OnButtonClick()
    {
        // ビジネスロジックはPresenterに
        if (playerLevel >= 10)
        {
            sceneManager.TransitionScene(new AdvancedScene());
        }
        else
        {
            sceneManager.TransitionScene(new BeginnerScene());
        }
    }
}
```

### ずんだもん
Viewは「ボタンが押された」って報告するだけで、どうするかはPresenterが決めるのだ！

### 間違い3：循環参照

```csharp
// ❌ ダメな例
public class HomePresenter
{
    HomeView homeView;  // PresenterがViewを知っている
}

public class HomeView : MonoBehaviour
{
    HomePresenter homePresenter;  // ViewもPresenterを知っている
}
```

### 四国めたん
お互いがお互いを直接知っている状態です。これだと変更が難しくなります。

```csharp
// ✅ 良い例
public class HomePresenter
{
    IHomeView homeView;  // インターフェース経由
}

public class HomeView : MonoBehaviour, IHomeView
{
    // ViewはPresenterを知らない
    // イベントで通知するだけ
}
```

### 春日部つむぎ
インターフェースを使って、一方向の関係にするんだね！

---

## 第17話：このプロジェクトで使われている技術まとめ

### 四国めたん
このLighthouseプロジェクトで使われている技術をまとめますね。

### VContainer（DIコンテナ）

### 四国めたん
依存性を自動で注入してくれるツールです。

```csharp
[Inject]
public void Construct(ISceneManager sceneManager)
{
    this.sceneManager = sceneManager;
}
```

### ずんだもん
`[Inject]`って書くと、必要なものを自動で渡してくれるのだ！

### UniTask（非同期処理）

### 四国めたん
シーン遷移やダイアログ表示など、時間のかかる処理を扱うための仕組みです。

```csharp
await sceneManager.TransitionScene(data);
await screenStackModule.Open(dialogData);
```

### 春日部つむぎ
`await`って書くと、処理が終わるまで待ってくれるんだよね！

### Interface（インターフェース）

### 四国めたん
「こういう機能がありますよ」という約束事です。

```csharp
public interface IHomePresenter
{
    void Setup();
    void OnEnter();
}
```

### ずんだもん
実際の中身は別のファイルに書いて、約束事だけをここに書くのだ！

---

## 第18話：もっと詳しく知りたい人へ

### 四国めたん
クリーンアーキテクチャをもっと深く学びたい人のために、キーワードを紹介しますね。

### レイヤードアーキテクチャ

### 四国めたん
プログラムを層に分ける考え方の総称です。クリーンアーキテクチャもその一つです。

### SOLID原則

### 四国めたん
良いプログラムを書くための5つの原則です。

- **S**ingle Responsibility（単一責任）：一つのクラスは一つの責任だけ
- **O**pen/Closed（開放閉鎖）：拡張には開いて、修正には閉じる
- **L**iskov Substitution（リスコフの置換）：親子関係を正しく使う
- **I**nterface Segregation（インターフェース分離）：大きなインターフェースは分ける
- **D**ependency Inversion（依存性逆転）：具体ではなく抽象に依存する

### ずんだもん
この`D`（依存性逆転）が、さっき勉強したインターフェースを使う理由なのだ！

### MVP パターン

### 四国めたん
このプロジェクトで使われているパターンです。

- **M**odel（モデル）：データやビジネスロジック
- **V**iew（ビュー）：画面表示
- **P**resenter（プレゼンター）：ViewとModelの橋渡し

### 春日部つむぎ
このプロジェクトの Presenter と View がまさにこれだね！

---

## 第19話：まとめ

### ずんだもん
クリーンアーキテクチャについて色々学んだのだ！まとめるのだ！

### 四国めたん
はい、まとめますね。

### クリーンアーキテクチャとは？

- プログラムを「役割ごとに層に分ける」設計方法
- 外側から内側に向かって依存する（内側は外側を知らない）
- インターフェースを使って、具体的な実装から切り離す

### このプロジェクトの構造

```
View（画面表示）
  ↓ インターフェース経由で依存
Presenter（処理の指示）
  ↓ インターフェース経由で依存
Service（実際の機能）
```

### メリット

1. **変更しやすい**：一部を変えても、他に影響しない
2. **テストしやすい**：画面を作らなくてもロジックをテストできる
3. **分担しやすい**：役割が明確なので、複数人で作業しやすい
4. **再利用しやすい**：ロジックを別のプロジェクトでも使える

### 春日部つむぎ
最初は難しそうだったけど、レストランとか部屋の例えで分かりやすかったよ！

### ずんだもん
バウムクーヘンみたいな層になっていて、それぞれに役割があるのだ！

### 四国めたん
クリーンアーキテクチャは、大きなプロジェクトになればなるほど、その価値がわかってきます。最初は「面倒くさい」と思うかもしれませんが、長期的には必ず役に立ちますよ。

---

## 第20話：実際に作ってみよう

### 春日部つむぎ
じゃあ実際に、クリーンアーキテクチャで簡単な画面を作るとしたら？

### 四国めたん
例えば「設定画面」を作るとしましょう。必要なファイルはこうなります。

### ステップ1：インターフェースを決める

```csharp
// ISettingsView.cs
public interface ISettingsView
{
    IDisposable SubscribeSoundToggle(Action<bool> onToggle);
    IDisposable SubscribeBackButton(Action onClick);
    void SetSoundEnabled(bool enabled);
}

// ISettingsPresenter.cs
public interface ISettingsPresenter
{
    void Setup();
    void OnEnter();
}
```

### ずんだもん
まず「どんな機能があるか」を約束事として書くのだ！

### ステップ2：Viewを作る

```csharp
// SettingsView.cs
public class SettingsView : MonoBehaviour, ISettingsView
{
    [SerializeField] Toggle soundToggle;
    [SerializeField] Button backButton;

    IDisposable ISettingsView.SubscribeSoundToggle(Action<bool> onToggle)
    {
        return soundToggle.OnValueChangedAsObservable()
            .Subscribe(onToggle);
    }

    IDisposable ISettingsView.SubscribeBackButton(Action onClick)
    {
        return backButton.OnClickAsObservable()
            .Subscribe(_ => onClick());
    }

    void ISettingsView.SetSoundEnabled(bool enabled)
    {
        soundToggle.isOn = enabled;
    }
}
```

### 春日部つむぎ
Unity の UI 部品（Toggle とか Button）を扱うのは View の仕事だね！

### ステップ3：Presenterを作る

```csharp
// SettingsPresenter.cs
public class SettingsPresenter : ISettingsPresenter
{
    ISettingsView settingsView;
    ISceneManager sceneManager;
    ISoundService soundService;

    [Inject]
    public void Construct(
        ISettingsView settingsView,
        ISceneManager sceneManager,
        ISoundService soundService)
    {
        this.settingsView = settingsView;
        this.sceneManager = sceneManager;
        this.soundService = soundService;
    }

    void ISettingsPresenter.Setup()
    {
        // 初期値を設定
        settingsView.SetSoundEnabled(soundService.IsSoundEnabled);

        // イベントを購読
        settingsView.SubscribeSoundToggle(OnSoundToggle);
        settingsView.SubscribeBackButton(OnBackButton);
    }

    void ISettingsPresenter.OnEnter()
    {
        // 画面に入ったときの処理
    }

    void OnSoundToggle(bool enabled)
    {
        soundService.SetSoundEnabled(enabled);
    }

    void OnBackButton()
    {
        sceneManager.Back().Forget();
    }
}
```

### ずんだもん
「音のON/OFFが変わったら SoundService に伝える」「戻るボタンが押されたら SceneManager に伝える」という処理を書くのだ！

### ステップ4：DIコンテナに登録

```csharp
// SettingsLifetimeScope.cs
public class SettingsLifetimeScope : LifetimeScope
{
    [SerializeField] SettingsScene settingsScene;
    [SerializeField] SettingsView settingsView;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(settingsScene);

        builder.Register<SettingsPresenter>(Lifetime.Singleton)
               .AsImplementedInterfaces();
        builder.RegisterComponent(settingsView)
               .AsImplementedInterfaces();
    }
}
```

### 四国めたん
これで、VContainer が自動的に必要なものを注入してくれます。

### 春日部つむぎ
あー！さっき見た Home シーンと同じ構造だ！パターンがわかれば簡単だね！

---

## 第21話：最後に

### 四国めたん
クリーンアーキテクチャについて、基本的なことを説明しました。

### ずんだもん
最初は難しそうに見えたけど、理解すると「なるほど！」ってなるのだ！

### 春日部つむぎ
レストランの例えとか、部屋の例えとか、身近なもので説明してくれたから分かりやすかった！

### 四国めたん
クリーンアーキテクチャは「銀の弾丸」ではありません。小さなプロジェクトでは逆に複雑になりすぎることもあります。

### 四国めたん
でも、チームで開発する場合や、長期間メンテナンスするプロジェクトでは、とても役に立つ考え方です。

### ずんだもん
このLighthouseプロジェクトも、クリーンアーキテクチャを使って、綺麗に整理されているのだ！

### 春日部つむぎ
実際のコードを見ながら学べたから、理解しやすかったよ！

### 四国めたん
ぜひ、自分のプロジェクトでも試してみてくださいね。最初は慣れないかもしれませんが、慣れてくると「これなしでは開発できない！」と思えるようになりますよ。

---

## おまけ：用語集（専門用語を覚えたい人向け）

### クリーンアーキテクチャ（Clean Architecture）
プログラムを層に分けて、依存の方向を内側に向ける設計手法。

### レイヤー（Layer / 層）
プログラムを役割ごとに分けたときの、それぞれの階層。

### ビュー（View）
画面表示を担当する部分。UI要素を持つ。

### プレゼンター（Presenter）
画面とビジネスロジックを繋ぐ部分。Viewからの通知を受けて、適切な処理を実行する。

### インターフェース（Interface）
「こういう機能があります」という約束事だけを定義したもの。実際の処理は書かない。

### 依存性（Dependency）
「AがBを使う」という関係性。Aは「Bに依存している」と言う。

### 依存性の逆転（Dependency Inversion）
具体的なクラスではなく、抽象（インターフェース）に依存させること。

### 依存性注入（Dependency Injection / DI）
必要なものを外から渡してもらう仕組み。自分で`new`しない。

### DIコンテナ（DI Container）
依存性注入を自動でやってくれるツール。VContainerなど。

### ビジネスロジック（Business Logic）
アプリの核となる処理。「レベル10以上なら上級コースに進む」のような判断。

### 単一責任の原則（Single Responsibility Principle）
一つのクラスは一つの責任だけを持つべきという原則。

### MVP パターン（Model-View-Presenter）
画面設計のパターンの一つ。Model（データ）、View（表示）、Presenter（橋渡し）に分ける。

---

## 参考リンク

### このプロジェクトで使われている技術

- **Lighthouse**：このフレームワーク自体
- **VContainer**：DIコンテナ（依存性注入）
- **UniTask**：非同期処理（async/await）
- **R3**：リアクティブプログラミング

### クリーンアーキテクチャをもっと学ぶ

- 「Clean Architecture 達人に学ぶソフトウェアの構造と設計」（書籍）
- Uncle Bob（Robert C. Martin）のブログ

---

**このプロジェクト：LighthouseSample**

これで、クリーンアーキテクチャの基本が理解できたのだ！✨
