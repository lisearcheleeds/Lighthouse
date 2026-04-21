# 🔧 VContainer って何？

## 🎯 一言で言うと

**ずんだもん**：「一言で言うと...**部品を自動で組み立ててくれる超便利なツール**なのだ！」

**つむぎ**：「部品を組み立てる...？なんか抽象的じゃない？」

**めたん**：「まずは基礎から説明しますね」

---

## 🤔 なぜ必要なの？「依存性注入（DI）」とは

**つむぎ**：「依存性注入...？また難しそうな言葉が...」

**めたん**：「英語だと Dependency Injection、略してDIと言います」

**ずんだもん**：「よけい分からないのだ！」

**つむぎ**：「わかる〜！」

---

### 📝 身近な例で説明

**めたん**：「RPGゲームで説明しましょうか」

**ずんだもん**：「RPG！」

**つむぎ**：「それはわかりやすそう！」

**めたん**：「プレイヤーキャラクターを作ることを考えてみてください」

```csharp
// プレイヤーキャラクター
public class Player
{
    private Weapon weapon;      // 武器
    private Armor armor;        // 防具
    private Inventory inventory; // インベントリ

    public Player()
    {
        // ここで全部作る？
    }
}
```

**つむぎ**：「プレイヤーは武器とか防具とか、いろんなものを持ってるよね」

**めたん**：「ええ。これらを『依存関係』と呼びます」

---

### 😓 従来の方法の問題点

**めたん**：「普通に書くと、こうなります」

```csharp
// ❌ 悪い例：全部自分で作る
public class Player
{
    private Weapon weapon;
    private Armor armor;
    private Inventory inventory;

    public Player()
    {
        // 自分で全部作る
        weapon = new Sword();           // 剣を作る
        armor = new IronArmor();        // 鎧を作る
        inventory = new Inventory(20);  // インベントリを作る
    }
}
```

**ずんだもん**：「普通に見えるけど...何が問題なのだ？」

**つむぎ**：「確かに、これで動きそうだけど？」

---

### 🔥 実際の問題

**めたん**：「いくつか問題があるんです」

#### 問題1：変更が大変

**めたん**：「武器を剣から弓に変えたいとき、どうしますか？」

```csharp
public Player()
{
    weapon = new Sword();  // ←これをBowに変えたい！
    // でも、コードを書き換えないといけない...
}
```

**つむぎ**：「あー、毎回コード変えるのめんどくさそう...」

**ずんだもん**：「もっと柔軟にできないのだ？」

---

#### 問題2：テストが難しい

**めたん**：「テストを書くとき、こんな問題が起きます」

```csharp
// Playerをテストしたいけど...
public void TestPlayer()
{
    var player = new Player();
    // ←武器もアーマーも全部本物が作られちゃう！
    // テスト用の偽物（モック）を使いたいのに...
}
```

**つむぎ**：「テストだけしたいのに、全部作られちゃうんだ」

**めたん**：「ええ、無駄が多いんです」

---

#### 問題3：クラスが密結合

**めたん**：「`Player`クラスが`Sword`クラスに直接依存しています」

```csharp
weapon = new Sword();  // ←Swordクラスを知っている必要がある
```

**ずんだもん**：「何が悪いのだ？」

**めたん**：「Swordクラスが変わったら、Playerクラスも変更が必要になるかもしれません」

**つむぎ**：「あー、影響範囲が広がるってこと？」

**めたん**：「その通りです」

---

## ✨ 依存性注入（DI）の解決策

**ずんだもん**：「じゃあどうすればいいのだ？」

**めたん**：「『外から渡してもらう』んです」

### ✅ 良い例：外から受け取る

```csharp
// ✅ 良い例：コンストラクタで受け取る
public class Player
{
    private Weapon weapon;
    private Armor armor;
    private Inventory inventory;

    // 外から渡してもらう！
    public Player(Weapon weapon, Armor armor, Inventory inventory)
    {
        this.weapon = weapon;
        this.armor = armor;
        this.inventory = inventory;
    }
}

// 使うとき
var sword = new Sword();
var armor = new IronArmor();
var inventory = new Inventory(20);

var player = new Player(sword, armor, inventory);  // ←外から渡す！
```

**つむぎ**：「あ、自分で作るんじゃなくて、もらうんだ！」

**ずんだもん**：「これなら武器を変えるのも簡単なのだ！」

```csharp
// 剣を持ったプレイヤー
var player1 = new Player(new Sword(), armor, inventory);

// 弓を持ったプレイヤー
var player2 = new Player(new Bow(), armor, inventory);
```

**めたん**：「これが依存性注入（DI）の基本です」

---

### 💡 DIのメリット

**めたん**：「DIを使うとこんな良いことがあります」

1. **柔軟性UP** - 武器や防具を簡単に差し替えられる
2. **テストしやすい** - テスト用の偽物を渡せる
3. **疎結合** - クラス同士の依存が弱くなる
4. **再利用しやすい** - 同じクラスを別の設定で使える

**つむぎ**：「なるほどね！でも...」

---

### 😰 でも、問題が...

**つむぎ**：「これ、毎回全部自分で作って渡すの？めんどくさくない？」

```csharp
// 毎回これを書くの...？
var sword = new Sword();
var armor = new IronArmor();
var inventory = new Inventory(20);
var player = new Player(sword, armor, inventory);

var enemyAI = new EnemyAI();
var enemyWeapon = new Club();
var enemy = new Enemy(enemyAI, enemyWeapon);

// もっと増えたら...
```

**ずんだもん**：「確かに大変そうなのだ...」

**めたん**：「そこで**DIコンテナ**の登場です！」

---

## 🎁 DIコンテナ = 自動組み立て機

**ずんだもん**：「DIコンテナって何なのだ？」

**めたん**：「必要なものを**自動で作って渡してくれる**便利なツールです」

**つむぎ**：「自動組み立て機みたいな？」

**めたん**：「まさにその通りです！」

### 🏭 DIコンテナのイメージ

```
【DIコンテナなし】
自分で全部作る
↓
Sword sword = new Sword();
Armor armor = new IronArmor();
Inventory inventory = new Inventory(20);
Player player = new Player(sword, armor, inventory);

【DIコンテナあり】
「Playerください！」と言うだけ
↓
Player player = container.Resolve<Player>();
↓
必要なもの（Sword, Armor, Inventory）を自動で作って
自動でPlayerに渡してくれる！
```

**ずんだもん**：「すごいのだ！全部自動なのだ！」

**つむぎ**：「マジで楽じゃん！」

---

## 🚀 VContainer の登場！

**めたん**：「そして、UnityでDIコンテナを使うなら**VContainer**が最適なんです」

**ずんだもん**：「VContainerって何が凄いのだ？」

### ✨ VContainerの特徴

**めたん**：「主に5つの強みがあります」

#### 1️⃣ めちゃくちゃ速い

**つむぎ**：「どれくらい速いの？」

**めたん**：「他のDIコンテナと比べて...」

```
【ベンチマーク】
Zenject: 100ms
VContainer: 5ms  （20倍速い！）
```

**ずんだもん**：「爆速なのだ！」

**めたん**：「起動時間の短縮に直結します」

---

#### 2️⃣ メモリ効率が良い

**つむぎ**：「メモリも節約できるの？」

**めたん**：「ええ、GC（ガベージコレクション）の発生を最小限に抑えています」

**ずんだもん**：「スマホゲームには嬉しいのだ！」

---

#### 3️⃣ Unity向けに最適化

**めたん**：「Unityの機能と相性抜群です」

- **MonoBehaviourのインジェクション** - Unityのコンポーネントにも使える
- **シーンスコープ** - シーンごとにコンテナを分けられる
- **ライフサイクル管理** - IInitializable, IDisposableなど

**つむぎ**：「Unity専用に作られてるってことね！」

---

#### 4️⃣ シンプルなAPI

**めたん**：「他のDIコンテナより書きやすいんです」

```csharp
// VContainer
builder.Register<Player>(Lifetime.Singleton);

// 他のDIコンテナ（例）
Container.Bind<Player>().AsSingle().NonLazy();
```

**つむぎ**：「確かに上の方がシンプル！」

**ずんだもん**：「読みやすいのだ！」

---

#### 5️⃣ IL2CPP完全対応

**めたん**：「UnityのIL2CPPビルドでも問題なく動きます」

**つむぎ**：「IL2CPP...？」

**めたん**：「Unityでスマホ向けにビルドするときの仕組みです。これに対応してるのは重要なんです」

**ずんだもん**：「安心して使えるのだ！」

---

## 💻 基本的な使い方

**つむぎ**：「じゃあ、実際にどう使うか教えて！」

**ずんだもん**：「コード見せるのだ！」

**めたん**：「ステップバイステップで説明しますね」

---

### ステップ1：パッケージをインストール

**めたん**：「まず、VContainerをインストールします」

```
1. Window > Package Manager を開く
2. + ボタン > Add package from git URL
3. 以下を入力：
   https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer
```

**つむぎ**：「Package Managerから入れられるんだ！」

---

### ステップ2：クラスを定義

**めたん**：「普通のC#クラスを書きます」

```csharp
// 武器インターフェース
public interface IWeapon
{
    void Attack();
}

// 剣
public class Sword : IWeapon
{
    public void Attack()
    {
        Debug.Log("剣で攻撃！");
    }
}

// プレイヤー
public class Player
{
    private readonly IWeapon weapon;

    // コンストラクタで受け取る
    public Player(IWeapon weapon)
    {
        this.weapon = weapon;
    }

    public void DoAttack()
    {
        weapon.Attack();
    }
}
```

**ずんだもん**：「普通のクラスなのだ！」

**つむぎ**：「特別な属性とかいらないの？」

**めたん**：「基本的には不要です。シンプルでしょう？」

---

### ステップ3：LifetimeScopeを作る

**めたん**：「VContainerの設定を書くクラスを作ります」

```csharp
using VContainer;
using VContainer.Unity;
using UnityEngine;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Swordを登録
        builder.Register<IWeapon, Sword>(Lifetime.Singleton);

        // Playerを登録
        builder.Register<Player>(Lifetime.Singleton);

        // エントリーポイント
        builder.RegisterEntryPoint<GameController>();
    }
}
```

**つむぎ**：「これが設定ファイルみたいなもの？」

**めたん**：「ええ。ここで『何を作るか』を登録します」

**ずんだもん**：「レシピを書いてる感じなのだ！」

---

### ステップ4：使う

**めたん**：「あとは自動で注入されます」

```csharp
using VContainer.Unity;

public class GameController : IStartable
{
    private readonly Player player;

    // 自動で注入される！
    public GameController(Player player)
    {
        this.player = player;
    }

    public void Start()
    {
        // Playerを使う
        player.DoAttack();  // "剣で攻撃！"
    }
}
```

**つむぎ**：「え、これだけ！？」

**ずんだもん**：「Playerを自分で作ってないのだ！」

**めたん**：「VContainerが自動で作って渡してくれるんです」

---

### 📋 仕組みの解説

**めたん**：「流れを整理しますね」

```
1. GameLifetimeScopeで登録
   ├─ Sword を IWeapon として登録
   ├─ Player を登録
   └─ GameController を登録

2. ゲーム開始時、VContainerが自動で...
   ├─ Sword を作成
   ├─ Player を作成（Swordを渡す）
   └─ GameController を作成（Playerを渡す）

3. GameController.Start() が呼ばれる
   └─ player.DoAttack() を実行
```

**つむぎ**：「なるほど！全部自動で繋げてくれるんだ！」

**ずんだもん**：「便利なのだ！」

---

## 🎮 実際の使用例

**つむぎ**：「もっと実践的な例も見せてよ！」

**めたん**：「いくつか紹介しますね」

---

### 例1：シンプルなゲームシステム

```csharp
// スコア管理
public class ScoreManager
{
    private int score = 0;

    public void AddScore(int points)
    {
        score += points;
        Debug.Log($"Score: {score}");
    }
}

// 敵
public class Enemy
{
    private readonly ScoreManager scoreManager;

    public Enemy(ScoreManager scoreManager)
    {
        this.scoreManager = scoreManager;
    }

    public void Die()
    {
        scoreManager.AddScore(100);
    }
}

// 登録
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<ScoreManager>(Lifetime.Singleton);
        builder.Register<Enemy>(Lifetime.Transient);
    }
}
```

**ずんだもん**：「Enemyが自動でScoreManagerをもらえるのだ！」

**つむぎ**：「Singletonとか Transient って何？」

---

### 📝 Lifetimeの種類

**めたん**：「オブジェクトの寿命を決めるものです」

| Lifetime | 意味 | 使い所 |
|----------|------|--------|
| **Singleton** | 1個だけ作る | マネージャー系（ScoreManager等） |
| **Transient** | 毎回新しく作る | 敵キャラ、弾丸など |
| **Scoped** | スコープ内で1個 | シーンごとのデータなど |

**つむぎ**：「あー、マネージャーは1個でいいけど、敵は何体も作るもんね！」

**ずんだもん**：「使い分けが大事なのだ！」

---

### 例2：MonoBehaviourへの注入

**めたん**：「Unityのコンポーネントにも注入できます」

```csharp
using UnityEngine;
using VContainer;

public class PlayerController : MonoBehaviour
{
    private ScoreManager scoreManager;

    // [Inject]属性をつける
    [Inject]
    public void Construct(ScoreManager scoreManager)
    {
        this.scoreManager = scoreManager;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            scoreManager.AddScore(10);
        }
    }
}

// 登録
public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerController playerController;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<ScoreManager>(Lifetime.Singleton);

        // MonoBehaviourを登録
        builder.RegisterComponent(playerController);
    }
}
```

**つむぎ**：「MonoBehaviourにも使えるんだ！」

**ずんだもん**：「Unityと相性抜群なのだ！」

---

### 例3：インターフェースを使った柔軟な設計

**めたん**：「インターフェースを使うと、実装を簡単に差し替えられます」

```csharp
// インターフェース
public interface ISaveService
{
    void Save(string data);
    string Load();
}

// ローカル保存
public class LocalSaveService : ISaveService
{
    public void Save(string data)
    {
        PlayerPrefs.SetString("SaveData", data);
    }

    public string Load()
    {
        return PlayerPrefs.GetString("SaveData", "");
    }
}

// クラウド保存
public class CloudSaveService : ISaveService
{
    public void Save(string data)
    {
        // クラウドに保存...
    }

    public string Load()
    {
        // クラウドから読み込み...
        return "";
    }
}

// 使う側
public class GameManager
{
    private readonly ISaveService saveService;

    public GameManager(ISaveService saveService)
    {
        this.saveService = saveService;
    }

    public void SaveGame()
    {
        saveService.Save("game data");
    }
}

// 登録（どっちを使うか選べる！）
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // ローカル保存を使う場合
        builder.Register<ISaveService, LocalSaveService>(Lifetime.Singleton);

        // クラウド保存を使う場合
        // builder.Register<ISaveService, CloudSaveService>(Lifetime.Singleton);

        builder.Register<GameManager>(Lifetime.Singleton);
    }
}
```

**つむぎ**：「お！登録を変えるだけで、ローカルとクラウドを切り替えられるんだ！」

**ずんだもん**：「GameManagerのコードは変えなくていいのだ！」

**めたん**：「これがDIの真骨頂です」

---

### 例4：ライフサイクルイベント

**めたん**：「VContainerにはライフサイクル用のインターフェースがあります」

```csharp
using VContainer.Unity;
using UnityEngine;

public class GameInitializer : IStartable, ITickable, IDisposable
{
    private readonly ScoreManager scoreManager;

    public GameInitializer(ScoreManager scoreManager)
    {
        this.scoreManager = scoreManager;
    }

    // ゲーム開始時に1回呼ばれる
    public void Start()
    {
        Debug.Log("ゲーム初期化！");
        scoreManager.AddScore(0);  // スコアリセット
    }

    // 毎フレーム呼ばれる
    public void Tick()
    {
        // 何か更新処理...
    }

    // 破棄時に呼ばれる
    public void Dispose()
    {
        Debug.Log("クリーンアップ！");
    }
}

// 登録
builder.RegisterEntryPoint<GameInitializer>();
```

**つむぎ**：「UnityのStart()やUpdate()みたいなものがあるんだ！」

**めたん**：「ええ、MonoBehaviourなしでもライフサイクル管理できます」

---

## 🆚 他のDIコンテナとの比較

**つむぎ**：「他にもDIコンテナあるの？」

**めたん**：「ええ、Unity向けだといくつかあります」

### 📊 比較表

| DIコンテナ | 速度 | メモリ | Unity対応 | 学習コスト |
|-----------|------|--------|----------|----------|
| **VContainer** | ◎ 超高速 | ◎ 軽量 | ◎ 最適化 | ○ 低い |
| **Zenject** | △ 遅い | △ 重い | ○ 対応 | △ 高い |
| **Extenject** | △ 遅い | △ 重い | ○ 対応 | △ 高い |
| **純正DI** | ○ 速い | ○ 軽量 | △ Unity非対応 | ○ 低い |

**ずんだもん**：「VContainerが一番速いのだ！」

**つむぎ**：「学習コストも低いんだ！」

**めたん**：「Zenjectは機能が多い分、複雑で重いんです」

---

### 🔍 具体的な違い

**めたん**：「同じことをする場合の比較です」

```csharp
// VContainer
builder.Register<Player>(Lifetime.Singleton);

// Zenject
Container.Bind<Player>().AsSingle().NonLazy();
```

**つむぎ**：「VContainerの方がシンプルだね」

**ずんだもん**：「読みやすいのだ！」

---

## ⚠️ 注意点

**つむぎ**：「いいことばっかりじゃなさそうだよね？」

**めたん**：「ええ、注意点もあります」

### 注意点1：学習コストがある

**ずんだもん**：「DIの概念を理解する必要があるのだ」

**つむぎ**：「最初は難しそう...」

**めたん**：「でも、一度理解すれば強力な武器になります」

---

### 注意点2：小規模プロジェクトにはオーバースペック

**めたん**：「クラスが10個以下の小さいプロジェクトだと、逆に複雑になることも」

```csharp
// 小規模なら、普通に書いた方が早い
var player = new Player();
player.Initialize();
```

**つむぎ**：「確かに、簡単なゲームならわざわざ使わなくてもいいかも」

**ずんだもん**：「規模に応じて判断するのだ！」

---

### 注意点3：エラーが実行時に発生する

**めたん**：「登録忘れなどは、実行するまでわかりません」

```csharp
// 登録し忘れ
builder.Register<Player>(Lifetime.Singleton);
// builder.Register<Weapon>(Lifetime.Singleton);  // ←これを忘れた！

// 実行時にエラー
// "Weapon is not registered!"
```

**つむぎ**：「コンパイルエラーにならないんだ」

**めたん**：「ええ、注意が必要です」

---

## 🎓 こんな人におすすめ

**つむぎ**：「結局、どんな人が使うといいの？」

**めたん**：「整理しましょう」

### ✅ おすすめな人

**ずんだもん**：「こんな人におすすめなのだ！」

✅ 中〜大規模なゲーム・アプリを作っている
✅ クラスが増えて管理が大変になってきた
✅ テストを書きたい
✅ コードの柔軟性を高めたい
✅ チーム開発している

### ⚠️ まだ早い人

**めたん**：「こういう場合は、まだ使わなくてもいいかもしれません」

❌ Unity完全初心者
❌ 作るゲームが超小規模（クラス10個以下）
❌ とにかく今すぐ動くものを作りたい
❌ DIの概念がまだ理解できていない

**つむぎ**：「ある程度Unity慣れてから使うのが良さそうね」

---

## 📝 まとめ

**つむぎ**：「じゃあ最後にまとめるよ～！」

**ずんだもん**：「VContainerのまとめなのだ！」

### VContainer とは？

**めたん**：「整理しましょう」

> **Unity向けの超高速・軽量DIコンテナ。部品を自動で組み立ててくれる便利ツール**

### ✨ 特徴

**ずんだもん**：「こんな特徴があるのだ！」

- ⚡ **爆速** - 他のDIコンテナの20倍速い
- 💾 **軽量** - メモリ効率が良い
- 🎮 **Unity最適化** - MonoBehaviour、IL2CPP完全対応
- 📝 **シンプル** - 学習コストが低い
- 🔧 **柔軟** - インターフェースで実装を切り替え可能

### 👍 こんな時に使おう

**つむぎ**：「チェックリスト！」

✅ クラスが増えて管理が大変
✅ テストを書きたい
✅ 柔軟な設計にしたい
✅ チーム開発している
✅ 中〜大規模プロジェクト

### 📚 学習リソース

**めたん**：「公式リソースを紹介します」

- 📚 **公式GitHub**: https://github.com/hadashiA/VContainer
- 📖 **公式ドキュメント**: https://vcontainer.hadashikick.jp/
- 💻 **サンプルプロジェクト**: GitHubのSamplesフォルダ

**ずんだもん**：「Cysharpの仲間たちなのだ！」
- **UniTask** - 非同期処理
- **MessagePack** - データ保存
- **MasterMemory** - マスターデータ管理
- **VContainer** - 依存性注入 ←今ここ！

**つむぎ**：「全部合わせて使うとマジで強力！」

---

## 🎬 おわりに

**つむぎ**：「というわけで、VContainerの解説でした～！」

**めたん**：「理解できましたか？」

**ずんだもん**：「最初は難しく感じるけど、慣れたら手放せなくなるのだ！」

**つむぎ**：「あーし的には、ある程度プロジェクト大きくなってきたら絶対使った方がいいと思う！マジで楽になる！」

**めたん**：「DIの概念を理解するのに時間がかかるかもしれませんが、一度覚えれば強力な武器になりますよ」

**ずんだもん**：「VContainerで快適なUnity開発なのだ～！」

**つむぎ**：「また会おうね～！」
