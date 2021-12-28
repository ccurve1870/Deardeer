using System;
using System.Collections.Generic;
using System.Text;

namespace awcsc
{


    /// <summary>
    /// 低速度Forrestal-Warrenモデルを扱うためのモデル
    /// 添字のVCはVarious Constantを意味しており、Cavity expansion analysisの定数項を変更可能であることを意味している。
    /// 
    /// 基本的な使い方は<see cref="Material"/>により材料定数を設定し、
    /// <see cref="Target"/>,<see cref="Penetrator"/>で標的、侵徹体の特徴を決める。
    /// 
    /// このモデルでは侵徹体先端形状CRHを考慮した計算が可能。
    /// 最も簡単な例を以下に示す。
    /// <example>
    ///   <code>
    ///   Material M = new Material(rho, YS, E, K0, k);
    ///   Target T = new Target(M);
    ///   Penetrator P = new Penetrator(M, L,D, CRH); //ここでCRHを設定
    ///   CalcAWLV C = new CalcAWLV(P,T, V0, cfp);
    ///   CalcAWLV C = new CalcAWLV(P,T, V0); //cfpは省略可能
    ///   var  res = C.calc(dt_log, dt);
    /// </code>
    /// </example>
    /// 上記例は衝突速度V0での侵徹挙動を計算する。
    /// <c>var res = C.calc_Vdependent(V_list);</c>
    /// とすることで、速度が変化したときの侵徹終了時の各種値を返す。
    /// </summary>
    public class CalcForrLV : Calc
    {

        /// <summary>
        /// P/Y = K1 log(E/Y)+K2のK1
        /// </summary>
        public double K1 { get; set; }
        /// <summary>
        /// P/Y = K1 log(E/Y)+K2のK2
        /// </summary>
        public double K2 { get; set; }

        //double V0_;
        //private double Rc_;
        //private List<double> fit_param_ = new List<double>() { 1.0, 0d };
        //public double Rc { get { return Rc_; } }

        /// <summary>
        /// 衝突速度[m/s]
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

            }
        }
        /// <summary>
        /// クレーター直径。侵徹体径と同一
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
                Rc_ = P.R;

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
        //List<double> fit_param_;

        /// <summary>
        /// CalcForrLVを初期化。
        /// </summary>
        /// <param name="P0">Penetrator</param>
        /// <param name="T0">Target</param>
        /// <param name="V00">衝突速度[m/s]</param>
        /// <param name="fit_param0">クレーター径の衝突速度依存性。
        /// 
        /// CalcForrLVでは使用しないため省略可能。
        /// CalcAWなどと同一の引数を取れるよう用意している</param>
        /// <param name="k1"> Cavity Expansion Analysisで求められるPの一般形P/Y = K1 log(E/Y)+K2のK1 </param>
        /// <param name="k2"> Cavity Expansion Analysisで求められるPの一般形P/Y = K1 log(E/Y)+K2のK2 </param>
        public CalcForrLV(in Penetrator P0, in Target T0, double V00,
               double[] fit_param0=null, double k1= 0.6666666666666666, double k2= 0.3963565945945571)
        {
            P_ = new Penetrator(P0);
            T_ = new awcsc.Target(T0);
            Rc_ = P.R;
            V0 = V00;
            K1 = k1;
            K2 = k2;
        }

        /// <summary>
        /// 侵徹体先端の加速度を求める。
        /// </summary>
        /// <param name="st">State</param>
        /// <returns>udot[m/s^2]</returns>
        double calc_udot(in State st)
        {
            double ret, sigmas, rt, rhs1, rhs2, rhv1, rhv2, dv;
            //double alphacubic = 2d * T.E / 3d / T.Y(st.DoP);
            double y = T.Y(st.DoP);
            //alphacubic = Math.Pow(calc_alpha(st.u, st), 3d);
            //Console.WriteLine($"{st.u},{2d/3d*Math.Log(alphacubic)}, {7d/3d*Math.Log(calc_alpha(st.u, st))}");
            //rt = 2d / 3d * T.Y(st.DoP) * (1d + Math.Log(alphacubic));
            rt = getRt(y, st);
            //rt = T.Y(st.DoP) * 7d / 3d * Math.Log(calc_alpha(st.u, st));
            dv = 1.5d * T.rho * st.u * st.u;
            rhs1 = rt * (1d - Math.Pow(Math.Sin(P.theta0), 2d)) * 0.5;
            rhs2 = -rt * (1d - 0.5 / P.Crh) * (1d - Math.Sin(P.theta0));
            rhv1 = dv * Math.Pow(Math.Cos(P.theta0), 4d) * 0.25;
            rhv2 = -dv * (1d - 0.5 / P.Crh) * (2d / 3d - Math.Sin(P.theta0) + 1d / 3d * Math.Pow(Math.Sin(P.theta0), 3d));
            sigmas = 8d * P.Crh * P.Crh * (rhs1 + rhs2 + rhv1 + rhv2);

            ret = -sigmas / (P.L - P.l + P.cv * Rc_) / P.rho;
            return ret;
        }

        /// <summary>
        /// 侵徹体後ろ端の加速度を求める。
        /// 
        /// Rigidな侵徹体なので侵徹体先端と同じ
        /// </summary>
        /// <param name="st">State</param>
        /// <returns>udot[m/s^2]</returns>
        double calc_vdot(in State st)
        {
            double ret = calc_udot(st);
            return ret;
        }
        //double func_p(double const u);
        //double func_pprime(double const u);
        //double calc_initu();

        /// <summary>
        /// 書く時間における侵徹体の状態を計算する。
        /// 詳細はAnderson, Walker参照
        /// </summary>
        /// <param name="dt">時間刻み[s]</param>
        /// <param name="st0">State</param>
        /// <returns>次の時間でのState</returns>
        protected override State cycle(in double dt, in State st0)
        {
            double alphan, _udot, _vdot;
            double _alpha, _u, _v;
            double _alphadot;
            State st = new State();

            alphan = st0.alpha;

            _udot = 0.5 * (st0.udot + calc_udot(st0));
            _vdot = 0.5 * (st0.vdot + calc_vdot(st0));

            _alpha = Math.Pow(2d * T.E / 3d / T.Y(st0.DoP), 1d/3d);

            _u = st0.u + _udot * dt;
            _v = st0.v + _vdot * dt;

            _alphadot = 0.5 * (st0.alphadot + (_alpha - alphan) / dt);


            st.udot = _udot;
            st.vdot = _vdot;
            st.alphadot = _alphadot;

            st.u = _u;
            st.v = _v;
            st.alpha = _alpha;
            st.L = P.L;

            st.t = st0.t + dt;
            st.DoP = st0.DoP + _u * dt;

            return st;
        }

        /// <summary>
        ///計算モデルのt=0sにおける状態を取得するための関数
        /// </summary>
        /// <param name="dt">dt[s]。まあ気にしなくていいと思います</param>
        /// <returns>初期化されたStateを返します</returns>
        protected override State init_State(in double dt)
        {
            State st0 = new State();
            st0.u = V0;
            st0.v = V0;
            st0.alpha = Math.Pow(2d * T.E / 3d / T.Y(st0.DoP), 1d / 3d);



            st0.alphadot = 0d;

            st0.DoP = 0d;
            st0.udot = calc_udot(st0);
            st0.vdot = calc_vdot(st0);

            st0.L = P.L;

            st0.t = 0d;
            return st0;
        }

        /// <summary>
        /// 計算を終了するための判定基準。
        /// がんばろう。
        /// </summary>
        /// <param name="st">Statenew</param>
        /// <param name="st0">Stateold</param>
        /// <returns>bool</returns>
        protected override bool cond_endcalc(in State st, in State st0)
        {
            //bool p_cond = st.v > sqrt(2*(T.Y*7/3.*log(st.alpha) - P.Y)/P.rho);
            bool u_cond = st.udot / st0.udot > 0.2;


            bool ret = (st.u > 0) && (Double.IsNaN(st.u) == false) && u_cond;
            return ret;
        }
        /// <summary>
        /// 標的強度項Rtは常に変化するので、それを取得して記録することを矯正するための関数。
        /// </summary>
        /// <param name="Y">Material.Y[GPa]</param>
        /// <param name="stold">State</param>
        /// <returns>Rt[Pa]</returns>
        protected override double getRt(double Y, in State stold)
        {
            //double alphacubic = 2d * T.E / 3d / Y*1e-9;

           double  rt = Y * (K1 * Math.Log(T.E/ Y) + K2);
            return rt;
        }

    };

}
