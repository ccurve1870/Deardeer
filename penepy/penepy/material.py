import clr
clr.AddReference("awlib")
import awcsc as aw
import numpy as np
from typing import Union


class Material:
    r"""侵徹及び標的の材料の特性を設定するクラス。

    awcsc.Materialのラッパー
    
    Attributes
    ----------
    rho : float
        密度[ :math:`\mathrm{kg/m^3}` ]
    Y : float
        降伏強度[GPa]
    E : float
        ヤング率[GPa]
    K0 : float
        静的な体積弾性率[GPa]
    k : float
        衝撃波速度の粒子速度依存性[-]
    G : float
        剛性率[GPa]
    c : float
        縦波の速度[m/s]
    c0 : float
        静的な体積弾性波の速度[m/s]
    """
    def __init__(self, rho: float, Y: float, E: float, K0: float, k: float):
        r"""Materialのコンストラクタ。
        
        Parameters
        ----------
        rho : float
            密度[ :math:`\mathrm{kg/m^3}` ]
        Y : float
            降伏強度[GPa]
        E : float
            ヤング率[GPa]
        K0 : float
            静的な体積弾性率[GPa]
        k : float
            衝撃波速度の粒子速度依存性[-]
        """
        self._M = aw.Material(float(rho), float(Y), float(E), float(K0),
                              float(k))

    @property
    def rho(self) -> float:
        r"""密度[ :math:`\mathrm{kg/m^3}` ]
        
        Returns
        -------
        float
            密度[ :math:`\mathrm{kg/m^3}` ]
        """
        return self._M.rho

    @rho.setter
    def rho(self, v: float):
        self._M.rho = float(v)

    @property
    def Y(self) -> float:
        r"""降伏強度
        
        Returns
        -------
        float
            降伏強度[GPa]
        """
        return self._M.Y * 1e-9

    @Y.setter
    def Y(self, v: float):
        self._M.Y = float(v)

    @property
    def E(self) -> float:
        r"""ヤング率
        
        Returns
        -------
        float
            ヤング率[GPa]
        """
        return self._M.E * 1e-9

    @E.setter
    def E(self, v: float):
        self._M.E = float(v)

    @property
    def K0(self) -> float:
        r"""静的な体積弾性率
        
        Returns
        -------
        float
            静的な体積弾性率[GPa]
        """
        return self._M.K0 * 1e-9

    @K0.setter
    def K0(self, v: float):
        self._M.K0 = float(v)

    @property
    def k(self) -> float:
        r"""衝撃波速度の粒子速度依存性
        
        Returns
        -------
        float
            衝撃波速度の粒子速度依存性[-]
        """
        return self._M.k

    @k.setter
    def k(self, v: float):
        self._M.k = float(v)

    @property
    def G(self) -> float:
        r"""剛性率
        
        Returns
        -------
        float
            剛性率[GPa]
        """
        return self._M.G * 1e-9

    @property
    def c(self) -> float:
        r"""縦波の音速
        
        Returns
        -------
        float
            縦波のの音速[m/s]
        """
        return self._M.c

    @property
    def c0(self) -> float:
        r"""静的な体積弾性波の音速[m/s]
        
        Returns
        -------
        float
            静的な体積弾性波の音速[m/s]
        """
        return self._M.c0


class Target:
    r"""標的の材料特性を定めるクラス。

    awcsc.Targetのpython側のラッパー
    
    Attributes
    ----------
    rho : float
        密度[ :math:`\mathrm{kg/m^3}` ]
    Y0 : float
        均質な部分の降伏強度[GPa]
    Ys : float
        表面の完全に焼きが入った領域の降伏強度[GPa]
    E : float
        ヤング率[GPa]
    K0 : float
        静的な体積弾性率[GPa]
    k : float
        衝撃波速度の粒子速度依存性[-]
    G : float
        剛性率[GPa]
    c : float
        縦波の速度[m/s]
    c0 : float
        静的な体積弾性波の速度[m/s]
    ts : float
        完全に焼きが入った層の厚み[m]
    th : float
        硬化層全体の厚み[m]
    c0inv : float
        1/c0[s/m]
    Ginv : float
        1/G[1/Pa]
    """
    def __init__(self,
                 M: Material,
                 Ys: float = 0.,
                 ts: float = 0.,
                 th: float = 0.):
        r"""Targetクラスのコンストラクタ。

        Materialのみを引数に取る場合は均質な標的として設定される。
        一方で、表面硬化部の硬さYs、完全に焼きが入った層の厚みts、硬化層全体の厚みthを設定することで擬似的に表面硬化装甲にも対応している。
        
        Parameters
        ----------
        M : Material
            penepy.Material
        Ys : float, optional
            表面硬化領域の降伏強度[GPa], by default 0
        ts : float, optional
            完全に焼きが入った層の厚み[m], by default 0.
        th : float, optional
            硬化層全体の厚み[m], by default 0.
        """
        if Ys == 0.:
            Ys = M._M.Y
            
        if type(M) == Material:
            self._T = aw.Target(M._M, float(Ys), float(ts), float(th))
        else:
            self._T = aw.Target(M, float(Ys), float(ts), float(th))

    @property
    def c(self) -> float:
        r"""縦波の音速
        
        Returns
        -------
        float
            縦波のの音速[m/s]
        """
        return self._T.c

    @property
    def c0(self) -> float:
        r"""静的な体積弾性波の音速[m/s]
        
        Returns
        -------
        float
            静的な体積弾性波の音速[m/s]
        """
        return self._T.c0

    @property
    def rho(self) -> float:
        r"""密度[ :math:`\mathrm{kg/m^3}` ]
        
        Returns
        -------
        float
            密度[ :math:`\mathrm{kg/m^3}` ]
        """
        return self._T.rho

    @rho.setter
    def rho(self, v: float):
        self._T.rho = float(v)

    @property
    def E(self) -> float:
        r"""ヤング率
        
        Returns
        -------
        float
            ヤング率[GPa]
        """
        return self._T.E * 1e-9

    @E.setter
    def E(self, v: float):
        self._T.E = float(v)

    @property
    def K0(self) -> float:
        r"""静的な体積弾性率
        
        Returns
        -------
        float
            静的な体積弾性率[GPa]
        """
        return self._T.K0 * 1e-9

    @K0.setter
    def K0(self, v: float):
        self._T.K0 = float(v)

    @property
    def k(self) -> float:
        r"""衝撃波速度の粒子速度依存性
        
        Returns
        -------
        float
            衝撃波速度の粒子速度依存性[-]
        """
        return self._T.k

    @k.setter
    def k(self, v: float):
        self._T.k = float(v)

    @property
    def G(self) -> float:
        r"""剛性率
        
        Returns
        -------
        float
            剛性率[GPa]
        """
        return self._T.G * 1e-9

    @property
    def Y0(self) -> float:
        r"""均質部の降伏強度
        
        Returns
        -------
        float
            降伏強度[GPa]
        """
        return self._T.Y0 * 1e-9

    @Y0.setter
    def Y0(self, v: float):
        r"""均質部の降伏強度のセッター
        
        Parameters
        ----------
        v : float
            降伏強度[GPa]
        """
        self._T.Y0 = float(v)

    @property
    def Ys(self) -> float:
        r"""完全に焼きが入った表面硬化部の降伏強度
        
        Returns
        -------
        float
            降伏強度[GPa]
        """
        return self._T.Ys * 1e-9

    @Ys.setter
    def Ys(self, v: float):
        r"""完全に焼きが入った表面硬化部の降伏強度
        
        Parameters
        ----------
        v : float
            完全に焼きが入った表面硬化部の降伏強度[GPa]
        """
        self._T.Ys = float(v)

    @property
    def ts(self) -> float:
        r"""完全に焼きが入った厚み[m]
        
        Returns
        -------
        float
            完全に焼きが入った厚み[m]
        """
        return self._T.ts

    @ts.setter
    def ts(self, v: float):
        r"""完全に焼きが入った厚みのセッター
        
        Parameters
        ----------
        v : float
            完全に焼きが入った厚み[m]
        """
        self._T.ts = float(v)

    @property
    def th(self) -> float:
        r"""硬化層全体の厚み[m]
        
        Returns
        -------
        float
            硬化層全体の厚み[m]
        """
        return self._T.th

    @th.setter
    def th(self, v: float):
        r"""硬化層全体の厚み[m]のセッター
        
        Parameters
        ----------
        v : float
            硬化層全体の厚み[m]
        """
        self._T.th = float(v)

    @property
    def c0inv(self) -> float:
        return self._T.c0inv

    @property
    def Ginv(self) -> float:
        return self._T.Ginv

    def Y(self, x: Union[float, np.ndarray]) -> Union[float, np.ndarray]:
        if type(x) == float or type(x)==int:
            return self._T.Y(float(x)) * 1e-9
        elif type(x) == np.ndarray:
            return np.array([self._T.Y(float(_)) * 1e-9 for _ in x])


class Penetrator:
    r"""侵徹体の材料特性を定めるクラス。

    awcsc.Penetratorのpython側のラッパー

    Attributes
    ----------
    rho : float
        密度[ :math:`\mathrm{kg/m^3}` ]
    Y : float
        降伏強度[GPa]
    E : float
        ヤング率[GPa]
    K0 : float
        静的な体積弾性率[GPa]
    k : float
        衝撃波速度の粒子速度依存性[-]
    G : float
        剛性率[GPa]
    c : float
        縦波の速度[m/s]
    c0 : float
        静的な体積弾性波の速度[m/s]
    L : float
        侵徹体長さ[m]
    D : float
        侵徹体直径[m]
    R : float
        侵徹体半径
    LD : float
        L/D比[-]
    Crh : float
        CRH[-]
    cv : float
        侵徹体先端部の半球状領域の重さ :math:`m` を与える係数 

        :math:`m = (L-l+cv\times R)\times \rho_P\times \pi R^2`

    theta0 : float
        侵徹体の先端部を半球状に近似したときの侵徹先端部の開き角
    l : float
        侵徹体先端部の半球状領域の長さ
    m : float
        侵徹体の質量[kg] 

        :math:`m = (L-l+cv\times R)\times \rho_P \times \pi R^2`
    
    c0inv : float
        1/c0[s/m]
    cinv : float
        1/c[s/m]
    """
    def __init__(self, M: Material, L: float, D: float, Crh: float = 0.5):
        r"""Penetratorのコンストラクタ。
        
        Parameters
        ----------
        M : Material
            penepy.Material
        L : float
            侵徹体長さ[m]
        D : float
            侵徹体直径[m]
        Crh : float, optional
            CRH, by default 0.5
        """
        if type(M) == Material:
            self._P = aw.Penetrator(M._M, float(L), float(D), float(Crh))
        else:
            self._P = aw.Penetrator(M, float(L), float(D), float(Crh))

    @property
    def Crh(self) -> float:
        r"""CRH
        
        Returns
        -------
        float
            CRH[-]
        """
        return self._P.Crh

    @Crh.setter
    def Crh(self, v: float):
        r"""CRHのセッター
        
        Parameters
        ----------
        v : float
            CRH[-]
        """
        self._P.Crh = float(v)

    @property
    def cv(self) -> float:
        r"""侵徹体先端部の半球状領域の重さ :math:`m` を与える係数
        
        :math:`m = (L-l+cv \times R) \times \rho_P \times \pi R^2`

        
        Returns
        -------
        float
            cv[-]
        """
        return self._P.cv

    @property
    def theta0(self) -> float:
        r"""侵徹体の先端部を半球状に近似したときの侵徹先端部の開き角[rad]
        
        Returns
        -------
        float
            侵徹体の先端部を半球状に近似したときの侵徹先端部の開き角[rad]
        """
        return self._P.theta0

    @property
    def c(self) -> float:
        r"""縦波の音速
        
        Returns
        -------
        float
            縦波のの音速[m/s]
        """
        return self._P.c

    @property
    def c0(self) -> float:
        r"""静的な体積弾性波の音速[m/s]
        
        Returns
        -------
        float
            静的な体積弾性波の音速[m/s]
        """
        return self._P.c0

    @property
    def rho(self) -> float:
        r"""密度[ :math:`\mathrm{kg/m^3}`]
        
        Returns
        -------
        float
            密度[ :math:`\mathrm{kg/m^3}` ]
        """
        return self._P.rho

    @rho.setter
    def rho(self, v: float):
        self._P.rho = float(v)

    @property
    def E(self) -> float:
        r"""ヤング率
        
        Returns
        -------
        float
            ヤング率[GPa]
        """
        return self._P.E * 1e-9

    @E.setter
    def E(self, v: float):
        self._P.E = float(v)

    @property
    def K0(self) -> float:
        r"""静的な体積弾性率
        
        Returns
        -------
        float
            静的な体積弾性率[GPa]
        """
        return self._P.K0 * 1e-9

    @K0.setter
    def K0(self, v: float):
        self._P.K0 = float(v)

    @property
    def k(self) -> float:
        r"""衝撃波速度の粒子速度依存性
        
        Returns
        -------
        float
            衝撃波速度の粒子速度依存性[-]
        """
        return self._P.k

    @k.setter
    def k(self, v: float):
        self._P.k = float(v)

    @property
    def G(self) -> float:
        r"""剛性率
        
        Returns
        -------
        float
            剛性率[GPa]
        """
        return self._P.G * 1e-9

    @property
    def Y(self) -> float:
        r"""降伏強度
        
        Returns
        -------
        float
            降伏強度[GPa]
        """
        return self._P.Y * 1e-9

    @Y.setter
    def Y(self, v: float):
        r"""降伏強度のセッター
        
        Parameters
        ----------
        v : float
            降伏強度[GPa]
        """
        self._P.Y = float(v)

    @property
    def L(self) -> float:
        r"""侵徹体長さ[m]
        
        Returns
        -------
        float
            侵徹体長さ[m]
        """
        return self._P.L

    @L.setter
    def L(self, v: float):
        r"""侵徹体長さのセッター
        
        Parameters
        ----------
        v : float
            侵徹体長さ[m]
        """
        self._P.L = float(v)

    @property
    def D(self) -> float:
        r"""侵徹体直径[m]
        
        Returns
        -------
        float
            侵徹体直径[m]
        """
        return self._P.D

    @D.setter
    def D(self, v: float):
        r"""侵徹体直径のセッター
        
        Parameters
        ----------
        v : float
            侵徹体直径[m]
        """
        self._P.D = float(v)

    @property
    def LD(self) -> float:
        r"""L/D比[-]
        
        Returns
        -------
        float
            L/D比
        """
        return self._P.LD

    @property
    def R(self) -> float:
        r"""侵徹体半径
        
        Returns
        -------
        float
            侵徹体半径
        """
        return self._P.R

    @property
    def l(self) -> float:
        r"""侵徹体先端部の半球状領域の長さ[m]
        
        Returns
        -------
        float
            侵徹体先端部の半球状領域の長さ[m]
        """
        return self._P.l

    @property
    def m(self) -> float:
        r"""侵徹体の質量[kg]
        
        :math:`m = (L-l+cv\times R)\times \rho_P \times \pi R^2`
        
        Returns
        -------
        float
            侵徹体の質量[kg]
        """
        return self._P.m

    @property
    def c0inv(self) -> float:
        return self._P.c0inv

    @property
    def cinv(self) -> float:
        return self._P.cinv