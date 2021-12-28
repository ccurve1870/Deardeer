import material
import materialList

from typing import List, Union


def getMaterials(*args) -> List[material.Material]:
    """penepy.materialPropertyList 中のMaterialを取得
    

    Parameters
    ----------
    *args: str
        materialPropertyListのkey

    Returns
    -------
    List[material.Material]
        Material

    Example
    -------
    
    ::

        i, W = penepy.getMaterials("iron", "WHA")
    
    何個でも行ける

    ::

        materialkey = ["iron", "WHA", "DU"]
        materialList = penepy.getMaterials(*materialkey)

    """
    lst = []
    for m in args:
        lst.append(materialList.materialPropertyList[m])
    return lst


def getTandP(mT: material.Material,
             mP: material.Material,
             L: float,
             D: float,
             Crh: float = 0.5,
             Ys: float = 0.,
             ts: float = 0.,
             th: float = 0.
             ) -> List[Union[material.Target, material.Penetrator]]:
    """ :any:`Target <penepy.material.Target>` と :any:`Penetrator <material.Penetrator>` を1行で取得するための関数
    

    Parameters
    ----------
    mT : material.Material
        Targetに使うMaterial
    mP : material.Material
        Penetratorに使うMaterial
    L : float
        L[m]
    D : float
        D[m]
    Crh : float, optional
        Crh, by default 0.5
    Ys : float, optional
        表面硬化領域の降伏強度[GPa], by default 0.
    ts : float, optional
        完全に焼きが入った層の厚み[m], by default 0.
    th : float, optional
        硬化層全体の厚み[m], by default 0.
    
    Returns
    -------
    List[Union[material.Target, material.Penetrator]]
        [:any:`Target <penepy.material.Target>` , :any:`Penetrator <material.Penetrator>` ]
    """
    T = material.Target(mT, Ys, ts, th)
    P = material.Penetrator(mP, L, D, Crh)
    return [T, P]
