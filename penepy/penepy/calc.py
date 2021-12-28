import clr
clr.AddReference("awlib")
import awcsc as aw
import numpy as np
from typing import Dict, List
from core import calc, calc_Vdependent


class Calc:
    """計算を実行するCalcクラスの基底クラス。

    実体はないのでこれは使わない。
    計算用のクラスはすべて、

    ::

        Calc(P:Penetrator, T:Target, V0:float)

    の形で使用可能。

    Attributes
    ----------
    V0 : float
        衝突速度[m/s]
    Rc : float
        クレーター半径[m]
    Dc : float
        クレーター直径[m]

    Methods
    -------
    calc(dt, dt_log)
        衝突速度V0における侵徹過程の時間変化を計算する
    calc_Vdependent(V_list)
        種々の衝突速度について、侵徹終了時点での状態を取得する
    """
    def __init__(self):
        """コンストラクタ。実体はない
        """
        self._C: aw.Calc = None

    def calc(self, dt: float, dt_log: float) -> Dict[str, np.ndarray]:
        """衝突速度V0における侵徹過程の時間変化を計算する。
        
        Parameters
        ----------
        dt : float
            計算時間ステップ[s]
        dt_log : float
            記録時間ステップ[s]
        
        Returns
        -------
        Dict[str, np.ndarray]
            侵徹過程の時間変化を記録した辞書
        """
        return calc(self._C, float(dt), (dt_log))

    def calc_Vdependent(self, V_list: np.ndarray) -> Dict[str, np.ndarray]:
        """種々の衝突速度について、侵徹終了時点での状態を取得する。

        np.ndarrayに格納されている値は、calcと異なりV_listに対応した値が記録されている。
        
        Parameters
        ----------
        V_list : np.ndarray
            衝突速度のリスト。
        
        Returns
        -------
        Dict[str, np.ndarray]
            侵徹終了時点での状態を記録した辞書
        """
        return calc_Vdependent(self._C, V_list)

    @property
    def V0(self) -> float:
        """衝突速度[m/s]
        
        Returns
        -------
        float
            衝突速度
        """
        return self._C.V0

    @V0.setter
    def V0(self, value: float):
        """衝突速度のセッター
        
        Parameters
        ----------
        value : float
            衝突速度[m/s]
        """
        self._C.V0 = float(value)

    @property
    def Rc(self) -> float:
        """クレーター半径[m]

        
        Returns
        -------
        float
            クレーター半径[m]
        """
        return self._C.Rc

    @property
    def Dc(self) -> float:
        """クレーター直径[m]
        
        Returns
        -------
        float
            クレーター直径[m]
        """
        return self._C.Dc


class CalcAW(Calc):
    """高速度Anderson-Walkerモデル用のCalcクラス

    ::

        CalcAW(P:Penetrator, T:Target, V0:float)

    の形で使用可能。

    Parameters
    ----------
    P : Penetrator
        侵徹体の材料特性、寸法
    T : Target
        標的の材料特性
    V0 : float
        衝突速度
    fit_param : np.ndarray, optional
        衝突時に形成されるCrater径を求める際の速度依存性。わからなければ触れないこと, by default np.array([0.000287, 1.48e-07])

    Attributes
    ----------
    V0 : float
        衝突速度[m/s]
    Rc : float
        クレーター半径[m]
    Dc : float
        クレーター直径[m]

    Methods
    -------
    calc(dt, dt_log)
        衝突速度V0における侵徹過程の時間変化を計算する
    calc_Vdependent(V_list)
        種々の衝突速度について、侵徹終了時点での状態を取得する
    """
    def __init__(self,
                 P,
                 T,
                 V0,
                 fit_param: np.ndarray = np.array([0.000287, 1.48e-07])):
        """CalcAWのコンストラクタ
        
        Parameters
        ----------
        P : Penetrator
            侵徹体の材料特性、寸法
        T : Target
            標的の材料特性
        V0 : float
            衝突速度
        fit_param : np.ndarray, optional
            衝突時に形成されるCrater径を求める際の速度依存性。わからなければ触れないこと, by default np.array([0.000287, 1.48e-07])
        """
        self._C = aw.CalcAW(P._P, T._T, float(V0), fit_param)


class CalcAWLV(Calc):
    """低速度Anderson-Walkerモデル用のCalcクラス

    ::

        CalcAWLV(P:Penetrator, T:Target, V0:float)

    の形で使用可能。

    Parameters
    ----------
    P : Penetrator
        侵徹体の材料特性、寸法
    T : Target
        標的の材料特性
    V0 : float
        衝突速度
    fit_param : np.ndarray, optional
        衝突時に形成されるCrater径を求める際の速度依存性。わからなければ触れないこと, by default np.array([0.000287, 1.48e-07])

    Attributes
    ----------
    V0 : float
        衝突速度[m/s]
    Rc : float
        クレーター半径[m]
    Dc : float
        クレーター直径[m]

    Methods
    -------
    calc(dt, dt_log)
        衝突速度V0における侵徹過程の時間変化を計算する
    calc_Vdependent(V_list)
        種々の衝突速度について、侵徹終了時点での状態を取得する
    """
    def __init__(self,
                 P,
                 T,
                 V0,
                 fit_param: np.ndarray = np.array([0.000287, 1.48e-07])):
        """CalcAWLVのコンストラクタ
        
        Parameters
        ----------
        P : Penetrator
            侵徹体の材料特性、寸法
        T : Target
            標的の材料特性
        V0 : float
            衝突速度
        fit_param : np.ndarray, optional
            衝突時に形成されるCrater径を求める際の速度依存性。わからなければ触れないこと, by default np.array([0.000287, 1.48e-07])
        """
        self._C = aw.CalcAWLV(P._P, T._T, float(V0), fit_param)


class CalcAWHVLV(Calc):
    """高速度-低速度一貫Anderson-Walkerモデル用のCalcクラス

    ::

        CalcAWHVLV(P:Penetrator, T:Target, V0:float)

    の形で使用可能。

    Parameters
    ----------
    P : Penetrator
        侵徹体の材料特性、寸法
    T : Target
        標的の材料特性
    V0 : float
        衝突速度
    fit_param : np.ndarray, optional
        衝突時に形成されるCrater径を求める際の速度依存性。わからなければ触れないこと, by default np.array([0.000287, 1.48e-07])

    Attributes
    ----------
    V0 : float
        衝突速度[m/s]
    Rc : float
        クレーター半径[m]
    Dc : float
        クレーター直径[m]

    Methods
    -------
    calc(dt, dt_log)
        衝突速度V0における侵徹過程の時間変化を計算する
    calc_Vdependent(V_list)
        種々の衝突速度について、侵徹終了時点での状態を取得する
    """
    def __init__(self,
                 P,
                 T,
                 V0,
                 fit_param: np.ndarray = np.array([0.000287, 1.48e-07])):
        """CalcAWHVLVのコンストラクタ
        
        Parameters
        ----------
        P : Penetrator
            侵徹体の材料特性、寸法
        T : Target
            標的の材料特性
        V0 : float
            衝突速度
        fit_param : np.ndarray, optional
            衝突時に形成されるCrater径を求める際の速度依存性。わからなければ触れないこと, by default np.array([0.000287, 1.48e-07])
        """
        self._C = aw.CalcAWHVLV(P._P, T._T, float(V0), fit_param)


class CalcForrLV(Calc):
    """低速度Forrestal-Warrenモデル用のCalcクラス

    ::

        CalcForrLV(P:Penetrator, T:Target, V0:float)

    の形で使用可能。

    Parameters
    ----------
    P : Penetrator
        侵徹体の材料特性、寸法
    T : Target
        標的の材料特性
    V0 : float
        衝突速度
    fit_param : np.ndarray, optional
        衝突時に形成されるCrater径を求める際の速度依存性。わからなければ触れないこと, by default np.array([0.000287, 1.48e-07])

    Attributes
    ----------
    V0 : float
        衝突速度[m/s]
    Rc : float
        クレーター半径[m]
    Dc : float
        クレーター直径[m]

    Methods
    -------
    calc(dt, dt_log)
        衝突速度V0における侵徹過程の時間変化を計算する
    calc_Vdependent(V_list)
        種々の衝突速度について、侵徹終了時点での状態を取得する
    """
    def __init__(self,
                 P,
                 T,
                 V0,
                 fit_param: np.ndarray = np.array([0.000287, 1.48e-07]),
                 K1=0.6666666666666666,
                 K2=0.3963565945945571):
        """CalcForrLVのコンストラクタ
        
        Parameters
        ----------
        P : Penetrator
            侵徹体の材料特性、寸法
        T : Target
            標的の材料特性
        V0 : float
            衝突速度
        fit_param : np.ndarray, optional
            衝突時に形成されるCrater径を求める際の速度依存性。わからなければ触れないこと, by default np.array([0.000287, 1.48e-07])
        K1 : double, optional
            P/Y = K1 Log(E/Y)+K2のK1, by default 0.6666666666666666
        K2 : double, optional
            P/Y = K1 Log(E/Y)+K2のK2, by default 0.3963565945945571(=2/3*(1+log(2/3)))
        """
        self._C = aw.CalcForrLV(P._P, T._T, float(V0), fit_param, float(K1),
                                float(K2))


class CalcMBE(Calc):
    r"""高速度-低速度一貫Alekseevski-Tateモデル用のCalcクラス

    ::

        CalcMBE(P:Penetrator, T:Target, V0:float)

    

    の形で使用可能。

    Parameters
    ----------
    P : Penetrator
        侵徹体の材料特性、寸法
    T : Target
        標的の材料特性
    V0 : float
        衝突速度
    fit_param : np.ndarray, optional
        衝突時に形成されるCrater径を求める際の速度依存性。わからなければ触れないこと, by default np.array([0.000287, 1.48e-07])

    Attributes
    ----------
    V0 : float
        衝突速度[m/s]
    Rc : float
        クレーター半径[m]
    Dc : float
        クレーター直径[m]
    hydro_lim : float
         :math:`\sqrt{\rho_P / \rho_T}` 

    Methods
    -------
    calc(dt, dt_log)
        衝突速度V0における侵徹過程の時間変化を計算する
    calc_Vdependent(V_list)
        種々の衝突速度について、侵徹終了時点での状態を取得する
    """
    def __init__(self,
                 P,
                 T,
                 V0,
                 fit_param: np.ndarray = np.array([0.000287, 1.48e-07])):
        """CalcMBEのコンストラクタ
        
        Parameters
        ----------
        P : Penetrator
            侵徹体の材料特性、寸法
        T : Target
            標的の材料特性
        V0 : float
            衝突速度
        fit_param : np.ndarray, optional
            衝突時に形成されるCrater径を求める際の速度依存性。わからなければ触れないこと, by default np.array([0.000287, 1.48e-07])
        """
        self._C = aw.CalcMBE(P._P, T._T, float(V0), fit_param)

    @property
    def hydro_lim(self) -> float:
        return self._C.hydro_lim
