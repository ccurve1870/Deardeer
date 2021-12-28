""""頻繁に使うpenepy.MaterialについてはmaterialPropertyListを準備した。

現在、Al、鉄、WHA、タングステン、劣化ウランについて用意している。
具体的なパラメーターとして

.. highlight:: python

::

    Al = Material(2700, 0.443, 72, 78,1.27)
    iron = Material(7900, 1, 299, 172, 1)
    WHA = Material(17600, 1, 200, 172, 1.23)
    Tungsten = Material(19.2e3, 1, 411, 311, 1.23)
    W = Material(19.2e3, 1, 411, 311, 1.23) #Tungstenの短縮形
    DU = Material(18.6e3, 1, 193, 104, 1.51)

を用意している。使用する際には
:: 

    iron = penepy.materialPropertyList["iron"]

などとして使うと便利。
"""


from material import Material
from typing import Dict, List


class materialdic(dict):
    def __init__(self):
        dict.__init__(self)

    def __getitem__(self, key):
        a = self.get(key)
        return Material(a.rho, a.Y, a.E, a.K0, a.k)


materialPropertyList: Dict[str, Material] = materialdic()
materialPropertyList["Al"] = Material(**{
    "rho": float(2700),
    "E": float(72),
    "K0": float(78),
    "k": float(1.27),
    "Y": float(0.443)
})
materialPropertyList["iron"] = Material(**{
    "rho": float(7900),
    "E": float(200),
    "K0": float(172),
    "k": float(1.),
    "Y": float(1)
})
materialPropertyList["WHA"] = Material(**{
    "rho": float(17600),
    "E": float(411),
    "K0": float(311),
    "k": float(1.23),
    "Y": float(1)
})
materialPropertyList["Tungsten"] = Material(**{
    "rho": float(19.2e3),
    "E": float(411),
    "K0": float(311),
    "k": float(1.23),
    "Y": float(1)
})
materialPropertyList["W"] = Material(**{
    "rho": float(19.2e3),
    "E": float(411),
    "K0": float(311),
    "k": float(1.23),
    "Y": float(1)
})
materialPropertyList["DU"] = Material(**{
    "rho": float(18.6e3),
    "E": float(193),
    "K0": float(104),
    "k": float(1.51),
    "Y": float(1)
})