import clr
clr.AddReference("awlib")
import awcsc as aw
import numpy as np
import matplotlib
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation
from typing import Dict, List
from core import dicconverter, calc
from calc import Calc


class Animate:
    r"""侵徹過程のアニメーションを出力するためのクラス

    深いことを気にしなかったら

    .. highlight:: python

    ::

        A = Animate(C)
        f = A.makeAnimation()
        f.save(~~)

    でなんとかなる。

    Parameters
    ----------
    C : Calc
        Calcを継承したクラス
    dt : float, optional
        計算時間ステップ, by default 1e-7
    dt_log : float, optional
        記録時間ステップ, by default 1e-5
    numsplit : int, optional
        plotの分割数(多いほど曲面がなめらかに分割される), by default 100
    figkwargs: Dict, optional
        plt.figureのオプションとして渡される辞書 plt.figure(**kwargs), by default {"figsize":(8, 6),"dpi": 300}

    Attributes
    ----------
    res : Dict[str, np.ndarray]
        計算結果
    plot : List[Dict[str,List[double]]
        awcsc.AnimateUtilで取得されたアニメーションを出力するに必要な座標のリスト
    xmin : float
        プロットする際に適切と考えられるxmin
    xmax : float
        プロットする際に適切と考えられるxmax
    ymin : float
        プロットする際に適切と考えられるymin
    ymax : float
        プロットする際に適切と考えられるymax
    fig : plt.figure
        plt.figure
    ax : plt.Axes
        plt.Axes
    im_lst : List[plt.Axes]
        各座標をプロットしたplt.AxesのList
    
    Methods
    -------
    update(i, step)
        アニメーションを作成する時に使う、各時間ごとのプロットを行う関数
    makeAnimation(step=1, animatekwargs={"interval":100})
        アニメーション作成用関数
    """
    def __init__(self,
                 C: Calc,
                 dt: float = 1e-7,
                 dt_log: float = 1e-5,
                 numsplit: int = 100,
                 figkwargs: Dict = {"figsize":(8, 6),"dpi": 300}):
        """Animateクラスのコンストラクタ
        
        Parameters
        ----------
        C : Calc
            Calcを継承したクラス
        dt : float, optional
            計算時間ステップ, by default 1e-7
        dt_log : float, optional
            記録時間ステップ, by default 1e-5
        numsplit : int, optional
            plotの分割数(多いほど曲面がなめらかに分割される), by default 100
        dpi : float, optional
            figのdpi, by default 300
        """
        if "dpi" not in figkwargs:
            figkwargs["dpi"] = 300
        if "figsize" not in figkwargs:
            figkwargs["figsize"] = (8,6)
        A = aw.AnimateUtil(C._C, dt, dt_log, numsplit)
        self.res = A.result
        self.plot = A.plot
        self.xmin = A.xmin
        self.xmax = A.xmax
        self.ymin = A.ymin
        self.ymax = A.ymax
        self.keys = list(self.plot[0].Keys)
        _ = plt.subplots(**figkwargs)
        self.fig, self.ax = _
        self.ax.set_xlim(self.xmin, self.xmax)
        self.ax.set_ylim(self.ymin, self.ymax)
        self.ax.set_xlabel("Position, $x$ / m")
        self.ax.set_ylabel("Position, $y$ / m")
        self.fig.tight_layout()
        self.im_lst = []
        p = self.plot[0]
        res = self.res
        for k in self.keys:
            self.im_lst.append(self.ax.plot(p[k].x, p[k].y)[0])
        self.txtax = self.ax.text(A.xmax * 0.98,
                                  A.ymax * 0.98,
                                  f"",
                                  va="top",
                                  ha="right",
                                  fontsize=18)

    def update(self, i, step):
        """アニメーションを作成する時に使う、各時間ごとのプロットを行う関数
        
        Parameters
        ----------
        i : int
            現ステップ
        step : int
            アニメーションをstep数ごとに記録するための引数
        """
        p = self.plot[::step]
        res = self.res
        for im, k in zip(self.im_lst, self.keys):
            im.set_data(p[i][k].x, p[i][k].y)
        self.txtax.set_text(
            f"t:{res['t'][::step][i]*1e3:.0f}μs\nv:{res['v'][::step][i]:.0f}m/s\nu:{res['u'][::step][i]:.0f}m/s"
        )
        return self.im_lst

    def makeAnimation(self, step: int = 1,
                      animatekwargs:Dict ={"interval":100}) -> FuncAnimation:
        """アニメーション作成用関数
        
        Parameters
        ----------
        step : int, optional
            アニメーションをstep数ごとに記録するための引数, by default 1
        animatekwargs : Dict, optional
            FuncAnimationの**kwargs
            by default animatekwargs:Dict ={"interval":100}
        
        Returns
        -------
        FuncAnimation
            matplotlib.Animation.FuncAnimation
        """
        p = self.plot[::step]
        if "interval" not in animatekwargs:
            animatekwargs["interval"] = 100
        return FuncAnimation(self.fig,
                             self.update,
                             frames=len(p),
                             fargs=(step, ),
                             blit=True, **animatekwargs)
