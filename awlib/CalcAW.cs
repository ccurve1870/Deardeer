using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Runtime.CompilerServices;

namespace awcsc
{

    /// <summary>
    /// 高速度AWモデルを扱うためのモデル
    /// 
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
    public class CalcAW : Calc
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
                Rc_ = Crater_radius(V0, fit_param_);
            }
        }

        /// <summary>
        /// クレーター直径。2*Rc
        /// </summary>
        public double Dc { get { return 2d * Rc_; } }

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

        /// <summary>
        /// CalcAWを初期化。
        /// </summary>
        /// <param name="P0">Penetrator</param>
        /// <param name="T0">Target</param>
        /// <param name="V00">衝突速度[m/s]</param>
        /// <param name="fit_param0">クレーター径の衝突速度依存性。
        /// 
        /// わからなかったらこの引数は省略可能。その場合は[0.000287, 1.48e-07]が用いられる</param>
        public CalcAW(in Penetrator P0, in Target T0, double V00,
               double[] fit_param0=null)
        {
            if (fit_param0 == null)
            {
                fit_param0 = new double[] { 0.000287, 1.48e-07 };
            }
            P_ = new Penetrator(P0);
            T_ = new awcsc.Target(T0);
            fit_param_.AddRange(fit_param0);
            V0 = V00;
        }



        /// <summary>
        /// alphaの解を得る。
        /// dynamic cavity expansion analysisの結果
        ///  (1d + rhou2 * Yinv) * (1d + rhou2 * Yinv) * (K_t - rhou2 * alpha2) - (1d + rhou2 * alpha2 * 0.5 * T.Ginv) * (1d + rhou2 * alpha2 * 0.5 * T.Ginv) * (K_t - rhou2)=0
        ///  をalpha2について解き、それの平方根を返す。
        /// </summary>
        /// <param name="rhou2">rho*u^2[kg/m/s^2]</param>
        /// <param name="K_t">uにおけるK[Pa]</param>
        /// <param name="Yinv">1/Y(x)</param>
        /// <returns>f(alhpa)→０にしたい</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        double func_alpha(in double rhou2, in double K_t, in double Yinv)
        {
            double res = 0d;

            res = 2.0 * (-T.Ginv * K_t + T.Ginv
            * rhou2 - (Yinv * Yinv) * (rhou2 * rhou2) - 2.0 * Yinv
            * rhou2 + 1.4142135623730951 * (Yinv
            * rhou2 + 1.0) * Math.Sqrt(0.5 * (T.Ginv * T.Ginv) * (K_t * K_t) - 0.5 * (T.Ginv * T.Ginv) * K_t * rhou2 + T.Ginv
            * K_t - T.Ginv
            * rhou2 + 0.5 * (Yinv * Yinv) * (rhou2 * rhou2) + Yinv
            * rhou2 + 0.5) - 1.0) / ((T.Ginv * T.Ginv) * rhou2 * (K_t - rhou2));


            return Math.Sqrt(res);
        }

        /// <summary>
        /// 粒子速度uにおける標的の体積弾性率
        /// $U=c0+ku=\sqrt{K0/rho}+ku$ 
        /// $U=\sqrt{K_t/rho}$ 
        /// 
        /// </summary>
        /// <param name="u">粒子速度[m/s]</param>
        /// <returns>粒子速度uのときの標的のK[GPa]</returns>
        double Kt(in double u)
        {
            double K;
            K = T.K0 * ((1d + T.k * u *T.c0inv) * (1d + T.k * u *T.c0inv));
            return K;
        }


        /// <summary>
        /// alphaを直接求める。
        /// u<1で値が飛ぶのでu<1でu=1に固定(計算に与える影響は小さい
        /// </summary>
        /// <param name="u">粒子速度[m/s]</param>
        /// <param name="st">State</param>
        /// <returns>alpha</returns>
        protected double calc_alpha(in double u, in State st)
        {
            double K_t, rhou2;
            double u2;
            double Yinv = T.Yinv(st.DoP);

            K_t = Kt(u);
            if (u > 1)
            {
                u2 = u * u;
            }
            else
            {
                u2 = 1d;
            }
            rhou2 = T.rho * u2;
            double alpha = func_alpha(rhou2, K_t, Yinv);
            //System.Console.WriteLine($"{i}, {u}, {eps}, {xl - xu}, {st.L}");
            return alpha;
        }

        /// <summary>
        /// 衝突時のクレーター径を求める
        /// </summary>
        /// <param name="v">衝突速度[m/s]</param>
        /// <param name="fit">fit_params</param>
        /// <returns>Rc[m]</returns>
        double Crater_radius(in double v, in List<double> fit)
        {
            double ret;

            ret = P.R * (1d + fit[0] * v + fit[1] * v * v);
            return ret;
        }


        /// <summary>
        /// 侵徹体先端の加速度を求める。
        /// </summary>
        /// <param name="st">State</param>
        /// <returns>udot[m/s^2]</returns>
        double calc_udot(in State st)
        {
            double ret, lh, rh, denom;
            lh = P.rho * st.vdot * (st.L - st.s) + 0.5 * P.rho * st.vu_sdot * (st.s * st.s) + T.rho * st.alphadot * 2.0 * Rc * st.u / ((st.alpha + 1.0) * (st.alpha + 1.0));

            rh = 0.5 * P.rho * ((st.v - st.u) * (st.v - st.u)) -
                (0.5 * T.rho * (st.u * st.u) + 7.0 / 3.0 * T.Y(st.DoP) * Math.Log(st.alpha));
            denom = (P.rho * st.s + T.rho * Rc * (st.alpha - 1.0) / (st.alpha + 1.0));
            ret = (rh - lh) / denom;
            return ret;
        }

        /// <summary>
        /// 侵徹体後端の加速度を求める。
        /// </summary>
        /// <param name="st">State</param>
        /// <returns>vdot[m/s^2]</returns>
        double calc_vdot(in State st)
        {
            double ret = -P.Y / P.rho / (st.L - st.s) *
        (1d + (st.v - st.u) / P.c + st.sdot / P.c);
            return ret;
        }

        /// <summary>
        /// 侵徹体先端の塑性変形領域を求める
        /// </summary>
        /// <param name="st">State</param>
        /// <returns>s[m]</returns>
        double calc_s(in State st)
        {
            double ret;
            ret = Rc * 0.5 * (st.v / st.u - 1.0) * (1.0 - 1.0 / (st.alpha * st.alpha));
            return ret;
        }

        /// <summary>
        /// 衝突直後の先端速度を圧力のマッチングから求めるための圧力計算用の式。
        /// </summary>
        /// <param name="u">粒子速度[m/s]</param>
        /// <returns>圧力[Pa]</returns>
        double func_p(in double u)
        {

            double p_pen, p_tar;
            double v = V0;
            double res;
            p_pen = (v - u) * (P.c0 + P.k * (v - u)) * P.rho;
            p_tar = u * (T.c0 + T.k * u) * T.rho;

            res = p_pen - p_tar;
            return res;
        }

        /// <summary>
        /// <see cref="func_p"/>の微分。
        /// </summary>
        /// <param name="u">粒子速度[m/s]</param>
        /// <returns>圧力[Pa]</returns>
        double func_pprime(in double u)
        {

            double dfda;
            dfda = (func_p(u + 1e-3) - func_p(u)) * 1e3;

            return dfda;
        }

        /// <summary>
        /// func_pを最小化して初期の侵徹速度を求める
        /// </summary>
        /// <returns></returns>
        double calc_initu()
        {
            double ub;
            double xold, xnew;
            double f_p;
            double eps = 1e6;
            int i = 0;

            //標的と侵徹体の間に生じる圧力が平衡する侵徹速度をニュートン法で求める
            //func_pとfunc_pprime

            ub = V0 - 1.0;

            xnew = ub;

            while ((eps > 1.0))
            {
                xold = xnew;
                f_p = func_p(xold);
                xnew = xold - f_p / func_pprime(xold);

                i++;
                f_p = func_p(xnew);
                eps = Math.Abs(f_p);

                if (i > 100)
                {
                    Console.WriteLine(f_p);
                }
            }

            return xnew;
        }

        /// <summary>
        /// 諦め
        /// </summary>
        /// <param name="st0"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        State getTimeDerivative(State st0, double dt)
        {
            //いつかの実装用
            //なんか無理そう
            double dt_init = 0.1 * dt;
            double dtinv = 1d / dt_init;
            State dst = new State();
            State st1 = cycle(dt_init, st0);
            dst.udot = (st1.udot - st0.udot) * dtinv;
            dst.vdot = (st1.vdot - st0.vdot) * dtinv;
            dst.Ldot = st0.udot - st0.vdot;
            dst.L = st0.Ldot;
            dst.u = st0.udot;
            dst.v = st0.vdot;
            dst.DoP = st0.u;
            dst.alpha = st0.alphadot;
            dst.alphadot = (st1.alphadot - st0.alphadot) * dtinv;
            dst.s = st0.sdot;
            dst.sdot = (st1.sdot - st0.sdot) * dtinv;
            dst.vu_sdot = (st1.vu_sdot - st0.vu_sdot) * dtinv;
            dst.Le = st0.Ldot / P.L;
            dst.t = dt;
            return dst;
        }

        /// <summary>
        /// 書く時間における侵徹体の状態を計算する。
        /// 詳細はAnderson, Walker参照
        /// </summary>
        /// <param name="dt">時間刻み[s]</param>
        /// <param name="st0">State</param>
        /// <returns>次の時間でのState</returns>
        protected override State cycle(in double dt, in State st0)
        {
            double sn, alphan, _udot, _vdot, _Ldot;
            double _alpha, _s, _u, _v, _L;
            double _vu_sdot, _alphadot, _sdot;
            State st = new State();
            /*やりたい形
             * dst = time(st);
             * stnew = stold + dt*dst;
             * 実際には?
             * Lddot = udot-vdot
             */
            sn = st0.s;
            alphan = st0.alpha;

            _udot = 0.5 * (st0.udot + calc_udot(st0));
            _vdot = 0.5 * (st0.vdot + calc_vdot(st0));
            _Ldot = st0.u - st0.v;

            _alpha = calc_alpha(st0.u, st0);
            _s = calc_s(st0);

            _u = st0.u + _udot * dt;
            _v = st0.v + _vdot * dt;
            _L = st0.L + _Ldot * dt;

            //_vu_sdotは本来
            //4. / ((st0.alpha * st0.alpha) - 1.) / Rc * ((st0.alpha * st0.alpha) * st0.udot * 0.5 - st0.u * st0.alpha * st0.alphadot / ((st0.alpha * st0.alpha) - 1.));
            //であるべきだけど，どうもうまくいかないのでこれで保留中
            //Calc::init_Stateの条件をうまくすり合わせたらまともになるかもしれないけどかなり粘ってだめだったので諦め中
            //計算結果はたぶんあんまり大きくは変わらない→うまくいった
            _vu_sdot = 4.0 / ((st0.alpha * st0.alpha) - 1.0) / Rc * ((st0.alpha * st0.alpha) * st0.udot * 0.5 - st0.u * st0.alpha * st0.alphadot / ((st0.alpha * st0.alpha) - 1.0));
            _vu_sdot = 0.5 * (st0.vu_sdot + _vu_sdot);
            _alphadot = 0.5 * (st0.alphadot + (_alpha - alphan) / dt);

            _sdot = 0.5 * (st0.sdot + (_s - sn) / dt);

            st.udot = _udot;
            st.vdot = _vdot;
            st.Ldot = _Ldot;
            st.sdot = _sdot;
            st.alphadot = _alphadot;
            st.vu_sdot = _vu_sdot;

            st.u = _u;
            st.v = _v;
            st.L = _L;
            st.Le = (P.L - _L) / P.L;
            st.s = _s;
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
            st0.u = calc_initu();
            st0.v = V0;
            st0.s = 0.0;
            st0.L = P.L;
            st0.Le = 0.0;
            st0.Ldot = st0.u - st0.v;
            st0.alpha = calc_alpha(st0.u, st0);

            st0.s = Rc * 0.5 * (st0.v / st0.u - 1.0) * (1.0 - 1.0 / (st0.alpha * st0.alpha));
            st0.sdot = 0.0;

            st0.vdot = calc_vdot(st0);

            st0.alphadot = 0.0;
            st0.udot = 0.0;
            st0.vu_sdot = 0.0;

            st0.DoP = 0.0;
            st0.udot = calc_udot(st0);
            st0.vu_sdot = 4.0 / ((st0.alpha * st0.alpha) - 1.0) / Rc * ((st0.alpha * st0.alpha) * st0.udot * 0.5 - st0.u * st0.alpha * st0.alphadot / ((st0.alpha * st0.alpha) - 1.0));


            st0.t = 0.0;
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
            u_cond = u_cond || (Math.Abs(st.udot - st0.udot) < 5e4);
            //u_cond = true;


            //u_cond = u_cond || (std::abs(st.udot - st0.udot) < 1e4);
            //u_condの適切な条件は結構シビア
            // st.udot < 1.3*std::abs(st0.udot)は低速度域での振動を押さえる
            // std::abs(st.udot-st0.udot) < 1e4は定常域近くでの不意の停止を押さえる。
            //定常域ではudotは0に近くなるが、この時st0.udot/st.udotは大きな値を取りやすい。しかし、
            //差の絶対値はわずかであり、真の停止条件とはいえない。
            //これを補足するために2行目のu_condを入れている
            //duの侵徹の時に効く

            bool ret = ((st.u > 0) && (Double.IsNaN(st.u) == false) && u_cond) && (st.L >= 0);
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
    };
}
