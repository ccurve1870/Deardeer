using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
namespace awcsc
{

    /// <summary>
    /// 高速度と低速度AWモデルを扱うためのモデル
    /// 侵徹体強度が十分高い高速度侵徹では、侵徹終了後に、残存侵徹体長さが低速度侵徹を起こしうる。
    /// そこでCalcAW→CalcAWLVを続けて行うことで高速度侵徹終了後の侵徹を計算する。
    /// 基本的な使い方は<see cref="Material"/>により材料定数を設定し、
    /// <see cref="Target"/>,<see cref="Penetrator"/>で標的、侵徹体の特徴を決める。
    /// 最も簡単な例を以下に示す。
    /// <example>
    ///   <code>
    ///   Material M = new Material(rho, YS, E, K0, k);
    ///   Target T = new Target(M);
    ///   Penetrator P = new Penetrator(M, L,D);
    ///   CalcAW C = new CalcAW(P,T, V0, cfp);
    ///   CalcAW C = new CalcAW(P,T, V0); //cfpは省略可能
    ///   var  res = C.calc(dt_log, dt);
    /// </code>
    /// </example>
    /// これは衝突速度V0での侵徹挙動を計算する。
    /// <c>var res = C.calc_Vdependent(V_list);</c>
    /// とすることで、速度が変化したときの侵徹終了時の各種値を返す。
    /// </summary>
    public class CalcAWHVLV : Calc
    {
        //double V0_;
        //private double Rc_;
        //public double Rc { get { return Rc_; } }

        /// <summary>
        /// 衝突速度[m/s]。Rcも再設定する
        /// </summary>
        public override double V0
        {
            get
            {
                return V0_;
            }
            set
            {
                V0_ = value;
                Rc_ = Crater_radius(V0_, fit_param_);
            }
        }
        /// <summary>
        /// クレーター直径。2*Rc
        /// </summary>
        public double Dc { get { return 2d * Rc_; } }
        //Penetrator P_;
        //Target T_;

        /// <summary>
        /// Calcが計算に使用するPenetrator
        /// </summary>
        /// <seealso cref="Penetrator"/>
        public override Penetrator P
        {
            get { return P_; }
            set
            {
                P_ = new Penetrator(value);
                Rc_ = Crater_radius(V0, fit_param_);
            }
        }

        /// <summary>
        /// Calcが計算に使用するTarget
        /// </summary>
        /// <seealso cref="Target"/>
        public override Target T
        {
            get { return T_; }
            set
            {
                T_ = new Target(value);
            }
        }
        CalcAW cAW;
        CalcAWLV cAWLV;


        /// <summary>
        /// CalcAWを初期化。
        /// </summary>
        /// <param name="P0">Penetrator</param>
        /// <param name="T0">Target</param>
        /// <param name="V00">衝突速度[m/s]</param>
        /// <param name="fit_param0">クレーター径の衝突速度依存性。
        /// 
        /// わからなかったらこの引数は省略可能。その場合は[0.000287, 1.48e-07]が用いられる</param>
        public CalcAWHVLV(in Penetrator P0, in Target T0, double V00,
       double[] fit_param0=null)
        {
            if (fit_param0 == null)
            {
                fit_param0 = new double[2] { 0.000287, 1.48e-07 };
            }
            P_ = new Penetrator(P0);
            T_ = new awcsc.Target(T0);
            fit_param_.AddRange(fit_param0);
            V0 = V00;
        }


        /// <summary>
        /// 速度V0で衝突する侵徹挙動を計算を実行する関数。
        /// ここでは一度CalcAWが終了するまで計算を行い、その後
        /// CalcAWLVを再度投げている
        /// 
        /// 計算はdtごとに行い、時間がdt_log経過するごとにその時の計算結果を保存するという形式。
        /// 結果を格納した辞書の中身は<see cref="State"/>参照
        /// </summary>
        /// <param name="dt0">計算時間ステップ[s]</param>
        /// <param name="dt_log0">計算結果取得ステップ[s]</param>
        /// <returns>結果を格納した辞書。引数の詳細は<see cref="State"/>参照</returns>
        /// <seealso cref="State"/>

        public override Dictionary<string, List<double>> calc(in double dt0, in double dt_log0)
        {
            Dictionary<string, List<double>> result;
            Dictionary<string, List<double>> resultLV;
            double dt = dt0;
            double dt_log = dt_log0;
            double[] fit_param0 = fit_param_.ToArray();
            cAW = new CalcAW(P, T, V0, fit_param0);
            result = cAW.calc(dt, dt_log);
            Penetrator Pres = new Penetrator(P);

            int size = result["L"].Count - 1;
            Pres.L = result["L"][size];
            double V00 = result["v"][size];
            cAWLV = new CalcAWLV(Pres, T, V00, fit_param0);
            resultLV = cAWLV.calc(dt, dt_log);
            int sizeLV = resultLV["t"].Count - 1;
            double tendHV = result["t"][size];
            double dopHV = result["DoP"][size];
            foreach (var key in result.Keys)
            {
                if (key == "t")
                {
                    var tl = resultLV["t"];
                    for (int i = 0; i <= sizeLV; i++)
                    {
                        tl[i] += tendHV;
                    }
                }
                else if (key == "DoP")
                {
                    var dopl = resultLV["DoP"];
                    for (int i = 0; i <= sizeLV; i++)
                    {
                        dopl[i] += dopHV;
                        
                    }
                }
                result[key].AddRange(resultLV[key]);
                //MessageBox.Show(result[key].Count.ToString());
            }

            return result;
        }

        /// <summary>
        /// 衝突速度V0を変化させながら、衝突終了後の値を取得する関数。
        /// 各種モデルについて統一的にこの関数を用いて計算を行い、各パラメータが格納された辞書を返す。
        /// SoAを返さないのはpythonとの相互作用を考慮。 
        /// 
        /// と書いていたけどよく考えると普通にSoAで返したほうが幸せかもしれない(検討
        /// 
        /// 結果を格納した辞書の中身は<see cref="State"/>参照。
        /// また、この関数では衝突速度V0も格納した辞書が返される
        /// </summary>
        /// <seealso cref="State"/>
        /// <param name="V0_list">V0のリスト[m/s]。10000 m/sくらいから結果が不安定になる(この辺は割と難しくて放置)。</param>
        /// <returns>結果を格納した辞書。引数の詳細は<see cref="State"/>参照。この関数ではV0のリストも格納されている</returns>
        public override Dictionary<string, List<double>> calc_Vdependent(in List<double> V0_list)
        {
            var result = new Dictionary<string, List<double>>();
            int size = V0_list.Count;

            result["t"] = new List<double>() { Capacity = size };
            result["DoP"] = new List<double>() { Capacity = size };
            result["v"] = new List<double>() { Capacity = size };
            result["u"] = new List<double>() { Capacity = size };
            result["L"] = new List<double>() { Capacity = size };
            result["Le"] = new List<double>() { Capacity = size };
            result["vdot"] = new List<double>() { Capacity = size };
            result["Ldot"] = new List<double>() { Capacity = size };
            result["s"] = new List<double>() { Capacity = size };
            result["alpha"] = new List<double>() { Capacity = size };

            result["udot"] = new List<double>() { Capacity = size };
            result["sdot"] = new List<double>() { Capacity = size };
            result["vu_sdot"] = new List<double>() { Capacity = size };
            result["alphadot"] = new List<double>() { Capacity = size };
            result["Y"] = new List<double>() { Capacity = size };
            result["Rt"] = new List<double>() { Capacity = size };

            var keylist = result.Keys;

            double V0_or = Double.Parse((this.V0).ToString());
            int i = 0;
            foreach (var V0 in V0_list)
            {
                this.V0 = V0;
                var r = calc(1e-7, 1);
                foreach (var key in keylist)
                {
                    result[key].Add(r[key][3]);
                }

            }
            result["V0"] = V0_list;
            this.V0 = V0_or;

            return result;
        }

        double Crater_radius(in double v, in List<double> fit)
        {
            double ret;
            ret = P.R * (1d + fit[0] * v + fit[1] * v * v);
            return ret;
        }

        /// <summary>
        ///計算モデルのt=0sにおける状態を取得するための関数だけど必要なし
        /// </summary>
        /// <param name="dt">dt[s]。まあ気にしなくていいと思います</param>
        /// <returns>初期化されたStateを返します</returns>
        protected override State init_State(in double dt)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 計算を終了するための判定基準。CalcAw,CalcAWLVのものを使う
        /// </summary>
        /// <param name="st">Statenew</param>
        /// <param name="st0">Stateold</param>
        /// <returns></returns>
        protected override bool cond_endcalc(in State st, in State st0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 書く時間の状態を計算する。CalcAw,CalcAWLVのものを使う
        /// </summary>
        /// <param name="dt">time step[s]</param>
        /// <param name="st0">Stateold</param>
        /// <returns></returns>
        protected override State cycle(in double dt, in State st0)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 標的強度項Rtは常に変化するので、それを取得して記録することを矯正するための関数。
        /// CalcAW、CalcAWLVを使う
        /// </summary>
        /// <param name="Y">Material.Y[GPa]</param>
        /// <param name="stold">State</param>
        /// <returns></returns>
        protected override double getRt(double Y, in State stold)
        {
            throw new NotImplementedException();
        }
    }
}
