def main():
    import penepy
    import numpy as np
    M = penepy.materialPropertyList["iron"]
    L, D = 1., 0.05
    T = penepy.Target(M)
    P = penepy.Penetrator(M, L, D)

    V_list = np.linspace(500, 2000)
    C = penepy.CalcAW(P, T, 2000)
    C.calc(1e-7, 1e-5)
    res = C.calc(1e-7, 1e-5)
    import pickle
    with open("CalcAW.calc.pickle", "wb") as f:
        pickle.dump(res, f)

    res = C.calc_Vdependent(V_list)
    with open("CalcAW.calc_Vdependent.pickle", "wb") as f:
        pickle.dump(res, f)

    C = penepy.CalcAWLV(P, T, 2000)
    res = C.calc(1e-7, 1e-5)
    with open("CalcAWLV.calc.pickle", "wb") as f:
        pickle.dump(res, f)

    res = C.calc_Vdependent(V_list)
    with open("CalcAWLV.calc_Vdependent.pickle", "wb") as f:
        pickle.dump(res, f)

    C = penepy.CalcAWHVLV(P, T, 2000)
    res = C.calc(1e-7, 1e-5)
    with open("CalcAWHVLV.calc.pickle", "wb") as f:
        pickle.dump(res, f)

    res = C.calc_Vdependent(V_list)
    with open("CalcAWHVLV.calc_Vdependent.pickle", "wb") as f:
        pickle.dump(res, f)

    C = penepy.CalcForrLV(P, T, 2000)
    res = C.calc(1e-7, 1e-5)
    with open("CalcForrLV.calc.pickle", "wb") as f:
        pickle.dump(res, f)

    res = C.calc_Vdependent(V_list)
    with open("CalcForrLV.calc_Vdependent.pickle", "wb") as f:
        pickle.dump(res, f)

    C = penepy.CalcMBE(P, T, 2000)
    res = C.calc(1e-7, 1e-5)
    with open("CalcMBE.calc.pickle", "wb") as f:
        pickle.dump(res, f)

    res = C.calc_Vdependent(V_list)
    with open("CalcMBE.calc_Vdependent.pickle", "wb") as f:
        pickle.dump(res, f)