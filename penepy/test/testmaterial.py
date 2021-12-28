import penepy
import numpy as np


def main():
    material_property_behavior()
    Calc_behavior()


def material_property_behavior():
    #Materialのプロパティ代入の挙動
    Mref = penepy.materialPropertyList["iron"]
    M = penepy.materialPropertyList["iron"]

    M.rho = Mref.rho * 2
    assert Mref.rho * 2 == M.rho
    print(M.rho)
    M.Y = Mref.Y * 2
    assert Mref.Y * 2 == M.Y
    print(M.Y)
    M.E = Mref.E * 8
    assert Mref.E * 8 == M.E
    assert Mref.c * 2 == M.c
    print(M.E, M.c)

    M.K0 = Mref.K0 * 8
    assert Mref.K0 * 8 == M.K0
    assert Mref.c0 * 2 == M.c0
    print(M.K0, M.c0)

    #Targetのプロパティを代入の挙動
    Tref = penepy.Target(Mref)
    T = penepy.Target(Mref)

    print("こっからTarget")
    T.rho = Tref.rho * 2
    assert Tref.rho * 2 == T.rho
    print(T.rho)
    T.Y0 = Tref.Y0 * 2
    assert Tref.Y0 * 2 == T.Y0
    print(T.Y0)
    T.E = Tref.E * 8
    assert Tref.E * 8 == T.E
    assert Tref.c * 2 == T.c
    T.E = Tref.E
    assert Tref.G == T.G
    assert Tref.Ginv == T.Ginv
    print(T.E, T.c, T.G)

    T.K0 = Tref.K0 * 8
    assert Tref.K0 * 8 == T.K0
    assert Tref.c0 * 2 == T.c0
    assert Tref.c0inv / 2 == T.c0inv
    T.K0 = Tref.K0
    T.rho = Tref.rho
    assert Tref.G == T.G
    assert Tref.Ginv == T.Ginv
    assert Tref.c0inv == T.c0inv
    print(T.K0, T.c0)

    Tref = penepy.Target(Mref, 2, 0.5, 1)
    T = penepy.Target(Mref, 2, 0.5, 1)

    T.Ys = Tref.Ys * 2
    x = np.linspace(0, 1)
    assert Tref.Ys * 2 == T.Ys
    T.Ys = Tref.Ys
    assert (Tref.Y(x) == T.Y(x)).all()

    T.ts = Tref.ts * 2
    T.th = Tref.th * 2
    x = np.linspace(0, 1)
    assert Tref.ts * 2 == T.ts
    assert Tref.th * 2 == T.th
    T.Ys = Tref.Ys
    T.ts = Tref.ts
    T.th = Tref.th
    assert (Tref.Y(x) == T.Y(x)).all()

    #Penetratorのプロパティ代入挙動
    L, D = 1, 1
    Tref = penepy.Penetrator(Mref, L, D)
    T = penepy.Penetrator(Mref, L, D)
    print("こっからPenetrator")
    T.rho = Tref.rho * 2
    assert Tref.rho * 2 == T.rho
    print(T.rho)

    T.Y = Tref.Y * 2
    assert Tref.Y * 2 == T.Y
    print(T.Y)

    T.E = Tref.E * 8
    assert Tref.E * 8 == T.E
    assert Tref.c * 2 == T.c
    assert Tref.cinv * 0.5 == T.cinv
    T.E = Tref.E
    T.rho = Tref.rho
    assert Tref.G == T.G
    assert Tref.cinv == T.cinv
    print(T.E, T.c, T.G)

    T.K0 = Tref.K0 * 8
    T.rho = Tref.rho * 2
    assert Tref.K0 * 8 == T.K0
    assert Tref.c0 * 2 == T.c0
    assert Tref.c0inv / 2 == T.c0inv
    T.K0 = Tref.K0
    T.rho = Tref.rho
    assert Tref.G == T.G
    assert Tref.c0inv == T.c0inv
    print(T.K0, T.c0)

    T.L = Tref.L * 2
    assert T.L == Tref.L * 2
    assert T.m == penepy.Penetrator(Mref, 2 * L, D).m

    T.D = Tref.D * 2
    assert T.D == Tref.D * 2
    assert T.R == Tref.R * 2
    assert T.m == penepy.Penetrator(Mref, 2 * L, 2 * D).m

    T.Crh = Tref.Crh * 2
    assert T.Crh == Tref.Crh * 2
    assert T.R == Tref.R * 2
    assert T.m == penepy.Penetrator(Mref, 2 * L, 2 * D, Tref.Crh * 2).m


def Calc_behavior():
    M = penepy.materialPropertyList["iron"]
    L, D = 1., 0.05
    T = penepy.Target(M)
    P = penepy.Penetrator(M, L, D)

    V_list = np.linspace(500, 2000)
    C = penepy.CalcAW(P, T, 2000)
    C.calc(1e-7, 1e-5)
    res = C.calc(1e-7, 1e-5)
    import pickle
    with open("CalcAW.calc.pickle", "rb") as f:
        pickleres = pickle.load(f)
        print(
            "difference",
            np.sqrt(np.sum(np.abs(res["DoP"] - pickleres["DoP"]))) /
            res["DoP"].values.size)
        assert (np.sqrt(np.sum(np.abs(res["DoP"] - pickleres["DoP"]))) /
                res["DoP"].values.size < 1e-5)

    res = C.calc_Vdependent(V_list)
    with open("CalcAW.calc_Vdependent.pickle", "rb") as f:
        pickleres = pickle.load(f)
        print(
            "difference",
            np.sqrt(np.sum(np.abs(res["DoP"] - pickleres["DoP"]))) /
            res["DoP"].values.size)

        assert (np.sqrt(np.sum(np.abs(res["DoP"] - pickleres["DoP"]))) /
                res["DoP"].values.size < 1e-2)

    C = penepy.CalcAWLV(P, T, 2000)
    res = C.calc(1e-7, 1e-5)
    with open("CalcAWLV.calc.pickle", "rb") as f:
        pickleres = pickle.load(f)
        print(
            "difference",
            np.sqrt(np.sum(
                (res["DoP"] - pickleres["DoP"]))) / res["DoP"].values.size)

        assert (np.sqrt(np.sum(np.abs(res["DoP"] - pickleres["DoP"]))) /
                res["DoP"].values.size < 1e-5)

    res = C.calc_Vdependent(V_list)
    with open("CalcAWLV.calc_Vdependent.pickle", "rb") as f:
        pickleres = pickle.load(f)
        print(
            "difference",
            np.sqrt(np.sum(np.abs(res["DoP"] - pickleres["DoP"]))) /
            res["DoP"].values.size)

        import matplotlib.pyplot as plt
        fig, ax = plt.subplots()

        ax.plot(res["t"], res["DoP"])
        ax.plot(pickleres["t"], pickleres["DoP"])
        fig.savefig("a.png")
        assert (np.sqrt(np.sum(np.abs(res["DoP"] - pickleres["DoP"]))) /
                res["DoP"].values.size < 1e-5)

    C = penepy.CalcAWHVLV(P, T, 2000)
    res = C.calc(1e-7, 1e-5)
    with open("CalcAWHVLV.calc.pickle", "rb") as f:
        pickleres = pickle.load(f)
        print(
            "difference",
            np.sqrt(np.sum(np.abs(res["DoP"] - pickleres["DoP"]))) /
            res["DoP"].values.size)

        assert (np.sqrt(np.sum(np.abs(res["DoP"] - pickleres["DoP"]))) /
                res["DoP"].values.size < 1e-5)

    res = C.calc_Vdependent(V_list)
    with open("CalcAWHVLV.calc_Vdependent.pickle", "rb") as f:
        pickleres = pickle.load(f)
        print(
            "difference",
            np.sqrt(np.sum(np.abs(res["DoP"] - pickleres["DoP"]))) /
            res["DoP"].values.size)

        assert True

    C = penepy.CalcForrLV(P, T, 2000)
    res = C.calc(1e-7, 1e-5)
    with open("CalcForrLV.calc.pickle", "rb") as f:
        assert (res["DoP"] == pickle.load(f)["DoP"]).all()

    res = C.calc_Vdependent(V_list)
    with open("CalcForrLV.calc_Vdependent.pickle", "rb") as f:
        assert (res["DoP"] == pickle.load(f)["DoP"]).all()

    C = penepy.CalcMBE(P, T, 2000)
    res = C.calc(1e-7, 1e-5)
    with open("CalcMBE.calc.pickle", "rb") as f:
        assert (res["DoP"] == pickle.load(f)["DoP"]).all()

    res = C.calc_Vdependent(V_list)
    with open("CalcMBE.calc_Vdependent.pickle", "rb") as f:
        assert (res["DoP"] == pickle.load(f)["DoP"]).all()


if __name__ == "__main__":
    main()