using System;
using System.Collections.Generic;
using System.Text;

namespace awcsc
{


    /// <summary>
    /// MBEモデルを扱うためのモデル
    /// 
    /// 基本的な使い方は<see cref="Material"/>により材料定数を設定し、
    /// <see cref="Target"/>,<see cref="Penetrator"/>で標的、侵徹体の特徴を決める。
    /// 最も簡単な例を以下に示す。
    /// <example>
    ///   <code>
    ///   Material M = new Material(rho, YS, E, K0, k);
    ///   Target T = new Target(M);
    ///   Penetrator P = new Penetrator(M, L,D);
    ///   CalcMBE C = new CalcMBE(P,T, V0, cfp);
    ///   CalcMBE C = new CalcMBE(P,T, V0); //cfpは省略可能
    ///   var  res = C.calc(dt_log, dt);
    /// </code>
    /// </example>
    /// これは衝突速度V0での侵徹挙動を計算する。
    /// <c>var res = C.calc_Vdependent(V_list);</c>
    /// とすることで、速度が変化したときの侵徹終了時の各種値を返す。
    /// </summary>
    public class CalcMBE : Calc
    {
        double mu_;
        //double V0_;
        //public double Rc { get { return Rc_; } }

        /// <summary>
        /// 衝突速度[m/s]。
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
        //Penetrator P_;
        //Target T_;
        /// <summary>
        /// Calcが計算に使用するPenetrator
        /// </summary>
        /// <seealso cref="Penetrator"/>
        public override Penetrator P
        {
            get
            {
                return P_;
            }
            set
            {
                P_ = new Penetrator(value);
                mu_ = Math.Sqrt(T.rho / P.rho);

                hydro_lim_ = Math.Sqrt(P.rho / T.rho);
                Rc_ = P.R;
            }
        }
        /// <summary>
        /// Calcが計算に使用するTarget
        /// </summary>
        /// <seealso cref="Target"/>
        public override Target T
        {
            get
            {
                return T_;
            }
            set
            {
                T_ = new Target(value);
                mu_ = Math.Sqrt(T.rho / P.rho);
                
                hydro_lim_ = Math.Sqrt(P.rho / T.rho);
            }
        }
        double hydro_lim_;
        /// <summary>
        /// MBEモデルから決定される、侵徹体と標的の密度の比 
        /// $\sqrt{\rho_P / \rho_T}$
        /// </summary>
        public double hydro_lim { get { return hydro_lim_; } }

        /// <summary>
        /// CalcMBEを初期化。
        /// </summary>
        /// <param name="P0">Penetrator</param>
        /// <param name="T0">Target</param>
        /// <param name="V00">衝突速度[m/s]</param>
        /// <param name="fit_param0">クレーター径の衝突速度依存性。
        /// 
        /// CalcMBEでは使用しないため省略可能。
        /// CalcAWなどと同一の引数を取れるよう用意している</param>
        public CalcMBE(in Penetrator P0, in Target T0, double V00,
              double[] fit_param0=null)
        {
            V0_ = V00;
            P_ = new Penetrator(P0);
            T_ = new awcsc.Target(T0);
            mu_ = Math.Sqrt(T.rho / P.rho);
            
            hydro_lim_ = Math.Sqrt(P.rho / T.rho);

        }
        /*
        public CalcMBE set_v(in double V00)
        {
            V0_ = V00;
            mu_ = Math.Sqrt(T.rho / P.rho);

            hydro_lim_ = Math.Sqrt(P.rho / T.rho);
            return this;
        }
        public CalcMBE set_P(in Penetrator P0)
        {
            P = new Penetrator(P0);
            mu_ = Math.Sqrt(T.rho / P.rho);

            hydro_lim_ = Math.Sqrt(P.rho / T.rho);
            return this;
        }
        public CalcMBE set_T(in Target T0)
        {
            T = new Target(T0);
            mu_ = Math.Sqrt(T.rho / P.rho);

            hydro_lim_ = Math.Sqrt(P.rho / T.rho);
            return this;
        }*/
        /// <summary>
        /// MBEモデルにより侵徹が開始する速度。
        /// </summary>
        /// <param name="x">侵徹深さ。標的が強度の深さ依存性を持っているときを考慮するため。</param>
        /// <returns></returns>
        double vl(double x)
        {
            double v = 0.0;
            if (T.Y(x) >= P.Y)
            {
                v = Math.Sqrt(2.0 * (T.Y(x) - P.Y) / P.rho);
            }
            else if (T.Y(x) < P.Y)
            {
                v = Math.Sqrt(2.0 * (P.Y - T.Y(x)) / P.rho);
            };
            return v;
        }

        /// <summary>
        /// MBEモデルの計算を行うときに出てくる$\mu$
        /// 
        /// $$\sqrt{\rho_T / \rho_P}$$
        /// </summary>
        double mu { get { return mu_; } }

        /// <summary>
        /// MBEモデルの計算を行うときに出てくる$A$
        /// </summary>
        /// <param name="x">侵徹深さ。標的が強度の深さ依存性を持っているときを考慮するため。</param>
        /// <returns></returns>
        double A(double x)
        {
            return 2.0 * (T.Y(x) - P.Y) * (1.0 - mu * mu) / T.rho;
        }


        /// <summary>
        /// 侵徹体後端の加速度を求める。
        /// </summary>
        /// <param name="st">State</param>
        /// <returns>vdot[m/s^2]</returns>
        double calc_vdot(in State st)
        {
            double ret, lh, rh;
            if (P.Y > T.Y(st.DoP))
            {
                if (st.v < vl(st.DoP))
                {
                    //Limit vel以下
                    lh = -(T.Y(st.DoP) + 0.5 * T.rho * st.v * st.v);
                }
                else
                {
                    //Limit vel以上
                    lh = -P.Y;
                };
            }
            else
            {
                lh = -P.Y;
            };
            rh = P.rho * st.L;
            ret = lh / rh;
            return ret;
        }

        /// <summary>
        /// 侵徹体後端の速度から決まる侵徹速度を求める。
        /// </summary>
        /// <param name="st">State</param>
        /// <returns>u[m/s]</returns>
        double calc_u(in State st)
        {
            double V = st.v;
            double u;
            u = 1.0 / (1.0 - mu * mu) * (V - mu * Math.Sqrt(V * V + A(st.DoP)));
            
            if (P.Y > T.Y(st.DoP))
            {
                if (st.v < vl(st.DoP))
                {
                    u = st.v;
                };
            }
            else
            {
                if (Double.IsNaN(u))
                {
                    u = 0.0;
                }
                else if (u < 0)
                {
                    u = 0.0;
                };
            };
            return u;
        }

        //double func_p(double const u);
        //double func_pprime(double const u);
        //double calc_initu();
        /// <summary>
        /// 各時間における侵徹体の状態を計算する。
        /// 詳細はTate-Alekseebskiiモデル参照
        /// </summary>
        /// <param name="dt">時間刻み[s]</param>
        /// <param name="st0">State</param>
        /// <returns>次の時間でのState</returns>
        protected override State cycle(in double dt, in State st0)
        {
            double _vdot, _Ldot;
            double _u, _v, _L;
            State st = new State();


            _vdot = calc_vdot(st0);
            _Ldot = st0.u - st0.v;

            _u = calc_u(st0);
            _v = st0.v + _vdot * dt;
            _L = st0.L + _Ldot * dt;

            st.vdot = _vdot;
            st.Ldot = _Ldot;

            st.u = _u;
            st.v = _v;
            st.L = _L;
            st.Le = (P.L - _L) / P.L;

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
            st0.v = V0_;
            st0.u = calc_u(st0);

            st0.L = P.L;
            st0.Le = 0.0;
            st0.vdot = 0.0;
            st0.Ldot = st0.u - st0.v;
            st0.DoP = 0.0;

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
            bool ret = (st.u > 0.0) && (st.L > 0.0) && (st.v > st.u);
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
            return Y;
        }
    };

}
