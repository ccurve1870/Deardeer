import clr
clr.AddReference("awlib")
clr.AddReference("System.Collections")
import awcsc as aw
from System.Collections.Generic import List, Dictionary
from System import *
import numpy as np
import System
from typing import Dict, List, Tuple
import pandas as pd
import ctypes
from System.Runtime.InteropServices import GCHandle, GCHandleType


def netArraytonpArray(a):
    length = a.Length
    npArray = np.empty(length, dtype=np.float64)
    try:

        sourceHandle = GCHandle.Alloc(a, GCHandleType.Pinned)
        sourcePtr = sourceHandle.AddrOfPinnedObject().ToInt64()
        destPtr = npArray.__array_interface__['data'][0]
        ctypes.memmove(destPtr, sourcePtr, npArray.nbytes)
    finally:
        if sourceHandle.IsAllocated: sourceHandle.Free()
    return npArray


def get_constant(C1: float, C2: float) -> Tuple[float, float]:
    """Cavity expansion analysisの一般形
    P/Y = C1 Y(1+log(C2 E/Y))から
    CalcForrLVで使う P/Y = K1 Log(E/Y)+K2に変換。

    Parameters
    ----------
    C1 : float
        Cavity expansion analysisの定数C1
    C2 : float
        Cavity expansion analysisの定数C1

    Returns
    -------
    K1 : float
        CalcForrLV用の定数K1
    K2 : float
        CalcForrLV用の定数K2
    """
    K1 = C1
    K2 = C1 * (1 + np.log(C2))
    return K1, K2


def dicconverter(d):
    if (type(d) == type(())):
        d = d[0]

    if type(d) == type({}):
        dret = Dictionary[String, List[Double]]()
        for k, v in d.items():
            r = System.Array[Double](v)
            V_list = System.Collections.Generic.List[Double]()
            V_list.AddRange(r)
            dret[k] = V_list
    else:
        dret = {}

        for k in d.Keys:
            dret[k] = netArraytonpArray(d[k])
        dret = pd.DataFrame(dret)
    return dret


def calc(C: aw.Calc, dt: float, dt_log: float) -> Dict[str, np.ndarray]:
    r"""awcscのCalc.calcで計算されたDictionary[String, List<double>]をpythonの辞書に変換して返すためのラッパー

    python側で使う分にはpenepy.Calcクラスのcalcを使えば問題ない(penepy.Calc.calcがこの関数を使う)
    
    Parameters
    ----------
    C : aw.Calc
        awcscで定義されるCalcを継承したクラス
    dt : float
        計算時間ステップ[s]
    dt_log : float
        記録時間ステップ
    
    Returns
    -------
    Dict[str,np.ndarray]
        Calc.calcで得られた計算結果
    """
    return dicconverter(C.calcPyInterop(float(dt), float(dt_log)))


def calc_Vdependent(C: aw.Calc, V_list: np.ndarray) -> Dict[str, np.ndarray]:
    r"""awcscのCalc.calc_Vdependentで計算されたDictionary[String, List<double>]をpythonの辞書に変換して返すためのラッパー

    python側で使う分にはpenepy.Calcクラスのcalcを使えば問題ない(penepy.Calc.calc_Vdependentがこの関数を使う)

    Parameters
    ----------
    C : aw.Calc
        awcscで定義されるCalcを継承したクラス
    V_list : np.ndarray
        衝突速度のリスト
    
    Returns
    -------
    Dict[str, np.ndarray]
        各衝突速度で衝突した際の、侵徹終了時点での各パラメータを格納した辞書
    """
    if type(V_list) == type([]):
        V_list = np.array(V_list)
    return dicconverter(C.calc_VdependentPyInterop(V_list))
