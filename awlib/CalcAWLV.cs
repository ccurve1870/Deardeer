using System;
using System.Collections.Generic;
using System.Text;

namespace awcsc
{

    /// <summary>
    /// 低速度AWモデルを扱うためのモデル
    /// 
    /// 基本的な使い方は<see cref="Material"/>により材料定数を設定し、
    /// <see cref="Target"/>,<see cref="Penetrator"/>で標的、侵徹体の特徴を決める。
    /// 最も簡単な例を以下に示す。
    /// <example>
    ///   <code>
    ///   Material M = new Material(rho, YS, E, K0, k);
    ///   Target T = new Target(M);
    ///   Penetrator P = new Penetrator(M, L,D);
    ///   CalcAWLV C = new CalcAWLV(P,T, V0, cfp);
    ///   CalcAWLV C = new CalcAWLV(P,T, V0); //cfpは省略可能
    ///   var  res = C.calc(dt_log, dt);
    /// </code>
    /// </example>
    /// これは衝突速度V0での侵徹挙動を計算する。
    /// <c>var res = C.calc_Vdependent(V_list);</c>
    /// とすることで、速度が変化したときの侵徹終了時の各種値を返す。
    /// </summary>
    public class CalcAWLV : CalcAW
    {


        //double V0_;
        //private double Rc_;
        //public double Rc { get { return Rc_; } }
        /*
        public override double V0
        {
            get
            {
                return V0_;
            }
            set
            {
                V0_ = value;
                Rc_ = Crater_radius(V0, fit_param_);
            }
        }
        //public double Dc { get { return 2d * Rc_; } }
        //Penetrator P_;
        //Target T_;
        public override Penetrator P
        {
            get { return P_; }
            set
            {
                P_ = new Penetrator(value);
                Rc_ = Crater_radius(V0, fit_param_);
            }
        }
        public override Target T
        {
            get { return T_; }
            set
            {
                T_ = new Target(value);
            }
        }
        */

        /// <summary>
        /// CalcAWを初期化。
        /// </summary>
        /// <param name="P0">Penetrator</param>
        /// <param name="T0">Target</param>
        /// <param name="V00">衝突速度[m/s]</param>
        /// <param name="fit_param0">クレーター径の衝突速度依存性。
        /// 
        /// わからなかったらこの引数は省略可能。その場合は[0.000287, 1.48e-07]が用いられる</param>

        public CalcAWLV(in Penetrator P0, in Target T0, double V00,
               double[] fit_param0=null) : base(P0, T0, V00, fit_param0) { }



        /// <summary>
        /// 侵徹体先端の加速度を求める。
        /// </summary>
        /// <param name="st">State</param>
        /// <returns>udot[m/s^2]</returns>
        double calc_udot(in State st)
        {
            double ret, lh, rh, denom;
            lh = T.rho * st.alphadot * 2d * Rc * st.u / ((st.alpha + 1d) * (st.alpha + 1d));
            rh = -(0.5 * T.rho * st.u * st.u + 7d / 3d * T.Y(st.DoP) * Math.Log(st.alpha));
            denom = P.rho * P.L + T.rho * Rc * (st.alpha - 1d) / (st.alpha + 1d);
            ret = (rh - lh) / denom;
            return ret;
        }
        double calc_vdot(in State st)
        {
            double ret = calc_udot(st);
            return ret;
        }
        //double func_p(double const u);
        //double func_pprime(double const u);
        //double calc_initu();
        /// <summary>
        /// 各時間における侵徹体の状態を計算する。
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

            _alpha = calc_alpha(st0.u, st0);

            _u = st0.u + _udot * dt;
            _v = st0.v + _vdot * dt;

            _alphadot = 0.5 * (st0.alphadot + (_alpha - alphan) / dt);


            st.udot = _udot;
            st.vdot = _vdot;
            st.alphadot = _alphadot;
            st.L = P.L;

            st.u = _u;
            st.v = _v;
            st.alpha = _alpha;

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
            st0.alpha = calc_alpha(st0.u, st0);



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
            return Y * 7.0 / 3.0 * Math.Log(stold.alpha);
        }
        //CalcAWLVの継承元をCalcからCalcAWにしたので以下が不要に
        //List<double> fit_param_;
        /*
        public CalcAWLV(in Penetrator P0, in Target T0, double V00,
               double[] fit_param0)
        {
            P_ = new Penetrator(P0);
            T_ = new awcsc.Target(T0);
            fit_param_.AddRange(fit_param0);
            V0 = V00;

        }
        public CalcAWLV(in Penetrator P0, in Target T0, double V00,
       List<double> fit_param0)
        {
            P_ = new Penetrator(P0);
            T_ = new awcsc.Target(T0);
            fit_param_.AddRange(fit_param0);
            V0 = V00;

        }
        public CalcAWLV(in Penetrator P0, in Target T0, double V00)
        {
            P_ = new Penetrator(P0);
            T_ = new awcsc.Target(T0);
            var fit_param0 = new double[] {0.000287, 1.48e-07};
            fit_param_.AddRange(fit_param0);
            V0 = V00;

        }
        */
        /*
        double func_alpha(in double alpha, in double rhou2, in double K_t, in double Yinv)
        {
            double res = 0d;
            res = (1d + rhou2 * Yinv) * (1d + rhou2 * Yinv) * (K_t - rhou2 * alpha) - (1d + rhou2 * alpha * 0.5 * T.Ginv) * (1d + rhou2 * alpha * 0.5 * T.Ginv) * (K_t - rhou2);


            return res;
        }
        double func_alphaprime(in double alpha, in double rhou2, in double K_t, in double Yinv)
        {

            double dfda;
            dfda = (func_alpha(alpha + 1e-7, rhou2, K_t, Yinv) - func_alpha(alpha, rhou2, K_t, Yinv)) * 1e7;

            return dfda;
        }
        double Kt(in double u)
        {
            double K;
            K = T.K0 * ((1d + T.k * u / T.c0) * (1d + T.k * u / T.c0));
            return K;
        }
 double calc_alpha(in double u, in State st)
        {
            double K_t, rhou2, ub;
            double xl, xu, xc;
            double fl, fc, fu;
            double xl2, xu2, xc2;

            double Yinv = T.Yinv(st.DoP);

            double eps = 1e14;
            double eps_fin = 10d;
            int i = 0;
            K_t = Kt(u);
            rhou2 = T.rho * u * u;

            ub = Math.Sqrt(K_t / rhou2);
            if (st.alpha > 2)
            {
                xl = st.alpha - 1d;
            }
            else
            {
                xl = 1d;
            }
            xu = ub - 1e-5;
            xc = 0.5 * (xu + xl);
            fc = 1e14;
            xl2 = xl * xl;
            xu2 = xu * xu;
            fl = func_alpha(xl2, rhou2, K_t, Yinv);
            fu = func_alpha(xu2, rhou2, K_t, Yinv);
            while ((eps > eps_fin)&&(Math.Abs(xl-xu )>1e-8)) 
            {



                xc2 = xc * xc;

                fc = func_alpha(xc2, rhou2, K_t, Yinv);


                if (fl * fc > 0d)
                {
                    xl = xc;
                }
                else if (fu * fc > 0d)
                {
                    xu = xc;
                }
                eps = Math.Abs(fc);
                xc = 0.5 * (xu + xl);
                i++;
                if (i > 50)
                {
                    eps_fin = 1.1 * eps_fin;
                    
                }
            }
            //System.Console.WriteLine($"{i}, {u}, {eps}, {xl - xu}, {st.L}");
            return xc;
        }
        double Crater_radius(in double v, in List<double> fit)
        {
            double ret;
            ret = P.R * (1d + fit[0] * v + fit[1] * v * v);
            return ret;
        }
        */

    };

}
