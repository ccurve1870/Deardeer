
# penepyの使い方


## 計算の始め方

```eval_rst
計算には材料物性クラスである :any:`Material <penepy.material.Material>` と、
標的の特性を決める :any:`Target <penepy.material.Target>` クラス、
侵徹体の特性を決める :any:`Penetraotr <penepy.material.Penetrator>` クラス、
を準備した後、 :any:`Calc <penepy.calc.Calc>` を継承したクラスに対して、
```
 
```python
C = penepy.Calc(P,T, V0)
```

```eval_rst
とすることで計算を行うことが可能なインスタンスを生成できる。

頻繁に使う材料物性クラスは :ref:`materialPropertyList <penepy.materialList module>` に準備したので、普通に使用する際には
```
```python
import penepy

iron = penepy.materialPropertyList["iron"]
WHA = penepy.materialPropertyList["WHA"]
T = penepy.Target(iron)

L = 0.6 #侵徹体長さ
D = 0.025 #侵徹体径
P = penepy.Penetrator(WHA, L, D)

V0 = 2000 #衝突速度[m/s]
C = penepy.Calc(P, T, V0) #Calcを継承したクラスを使う。Calcで書いても動かない。

dt = 1e-7 #計算時間ステップ[s]
dt_log = 1e-6 #計算記録時間ステップ[s]
result = C.calc(dt, dt_log)
```

```eval_rst
とすることで使用可能。resultはDict[str, np.ndarray]であり、 :ref:`Resultの説明` に示す結果を保持している。
```
上記のコード中CalcをCalcAWにして計算した結果を実際に結果を以下のコードによりプロットしてみると、

```python
import matplotlib.pyplot as plt
fig, ax = plt.subplots(figsize=(8,6))
ax.plot(result["t"], result["u"])
ax.plot(result["t"], result["v"])
ax.set_xlabel(r"Time, $t$ / ms")
ax.set_ylabel(r"Veolicty, $u,v$ / m/s")
```

<img src=_images/u,v-t.png width=400>

が描かれます。
これを使っていろいろ遊びましょうというのがこのモジュールの意図するところです。

```eval_rst
:any:`Calcクラス <penepy.calc.Calc>` はもう一つ、:any:`calc_Vdependent <penepy.calc.Calc.calc_Vdependent>` という関数を持っています。
これは種々の衝突速度について、侵徹終了時点での状態を取得する関数で、例えば、上のCについて書いてみると
```

```python
import numpy as np
V_list = np.linspace(500, 2000, 50) #500 m/sから2000 m/sまで50分割した衝突速度のリスト。
result_Vdependent = C.calc_Vdependent(V_list)
fig, ax = plt.subplots(figsize=(6,4))
ax.plot(result_Vdependent["V0"], result_Vdependent["DoP"])
ax.set_xlabel(r"Striking velocity, $V_0$ / m/s")
ax.set_ylabel(r"Penetration depth, $P$ / m")
```
<img src=_images/P-V0.png width=400>

が得られます。result_Vdependentは侵徹終了時のresultを保持しており、また、V_listの値をV0として保持しています。

以上が基本的な使い方です。

## 侵徹アニメーションの作製
```eval_rst
あまり有益なことはわからないのでお遊びではありますが、 :any:`Animateクラス <penepy.animate.Animate>` を用いることで、侵徹過程をアニメーションとして出力することが出来ます。
上記のCを使いまわします。単純には、
```

```python
A = penepy.Animate(C)
f = A.makeAnimation()
f.savefig(path)
```

と書くことで、以下のようなアニメーションを得ることが出来ます。

<img src=_images/animate_ex.gif width=400>

```eval_rst
:any:`Animateクラス <penepy.animate.Animate>` はfig,axを保持しているので、Animateインスタンスを作製した後に軸の設定なども比較的自由にできます。
```

ところで、上図は侵徹過程を見るときに最低限必要な時間、侵徹体先端速度、侵徹体後端速度を図中に示していますが、これらの文字がいらない、あるいは他の文字に変更したいということは往々にしてあります。
```eval_rst
そのようなときは :any:`Animateクラス <penepy.animate.Animate>` を継承し、
```
```python
class Anim(penepy.Animate):
    def update(self, i, step):

        p = self.plot[::step]
        res = self.res
        for im, k in zip(self.im_lst, self.keys):
            im.set_data(p[i][k].x, p[i][k].y)
        self.txtax.set_text(f"t:{res['t'][::step][i]*1e-3:.4f}s", ) #ここを変える
        return self.im_lst

A = .Anim(C)
f = A.makeAnimation()
f.savefig(path)
```

と書くことで、任意の情報に書き換えることが出来ます。


## Calcクラスの詳細

```eval_rst

:any:`Calcクラス <penepy.calc.Calc>` は関数とプロパティのみを実装したAbstract class的なクラスであり、計算の実態はありません。そこで、計算を実際に行うには :any:`Calcクラス <penepy.calc.Calc>` を継承したクラスを用いる必要があります。ここではCalcクラスを継承したクラスを列挙し、その特徴を比較します。
普通に計算するならCalcAW周りか、CalcForrLVがお勧めです。CalcAWLVとCalcForrLVは知りたいことに合わせて使います。

.. csv-table:: Calcの種類
   :file: Calc.csv
   :encoding: UTF-8
   :header-rows: 1
   :stub-columns: 1
   :widths: 1,2,1,1,2,2

```

