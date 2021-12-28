using System;
using System.Collections.Generic;
namespace awcsc
{
    /// <summary>
    /// 侵徹体や標的の材質をまとめて扱うためのクラス。
    /// 
    /// 材料の特性を設定する。
    /// </summary>
    public class Material
    {
        private double Y_, rho_, E_, K0_, k_;

        /// <summary>
        /// 降伏強度[Pa]
        /// </summary>
        public double Y
        {
            get
            {
                return Y_;
            }
            set
            {
                Y_ = value * 1e9;
            }
        }

        /// <summary>
        /// 密度[kg/m^3]
        /// </summary>
        public double rho
        {
            get { return rho_; }
            set { rho_ = value; }
        }
        /// <summary>
        /// ヤング率[Pa]
        /// </summary>
        public double E
        {
            get
            {
                return E_;
            }
            set
            {
                E_ = value * 1e9;
            }
        }
        /// <summary>
        /// 剛性率[Pa]
        /// </summary>
        public double G
        {
            get
            {
                return 3e0 * K0 * E / (9e0 * K0 - E);
            }
        }
        /// <summary>
        /// 静的な体積弾性率[Pa]
        /// </summary>
        public double K0
        {
            get
            {
                return K0_;
            }
            set
            {
                K0_ = value * 1e9;
            }
        }
        /// <summary>
        /// 衝撃波速度の粒子速度依存性[Pa]
        /// </summary>
        public double k
        {
            get { return k_; }
            set { k_ = value; }
        }
        /// <summary>
        /// 静的な体積波速度
        /// </summary>
        public double c0 { get { return Math.Sqrt(K0 / rho); } }
        /// <summary>
        /// 縦波速度。
        /// </summary>
        public double c { get { return Math.Sqrt(this.E / rho); } }

        /// <summary>
        /// 通常使用するコンストラクタ
        /// </summary>
        /// <param name="rho">密度[kg/m^3]</param>
        /// <param name="YS">降伏強度[GPa]</param>
        /// <param name="E">ヤング率[GPa]</param>
        /// <param name="K0">静的な体積弾性率[GPa]</param>
        /// <param name="k">衝撃波速度の粒子速度依存性[-]</param>
        public Material(double rho, double YS, double E, double K0, double k)
        {

            Y = YS;
            this.rho = rho;
            this.E = E;
            this.K0 = K0;
            this.k = k;
        }
        /// <summary>
        /// Material複製用コンストラクタ
        /// </summary>
        /// <param name="M">Material</param>
        public Material(in Material M)
        {
            Y_ = M.Y_;
            rho_ = M.rho_;
            E_ = M.E_;
            K0_ = M.K0_;
            k_ = M.k_;
        }


    };

    /// <summary>
    /// 標的特性のクラス。
    /// Materialと大体同じだけど、表面硬化などを考慮できるように少し変更を加えている。
    /// </summary>
    public class Target
    {
        private double Ys_, Y_, ts_, th_, tt_;
        private double Ginv_, c0inv_;
        private double rho_, E_, G_, K0_, k_, c0_, c_;

        /// <summary>
        /// 均質な部分の降伏強度[Pa]
        /// </summary>
        public double Y0
        {
            get { return Y_; }
            set { Y_ = value * 1e9; }
        }
        /// <summary>
        /// 表面の降伏強度[Pa]
        /// </summary>
        public double Ys
        {
            get
            {
                return Ys_;
            }
            set
            {
                Ys_ = value * 1e9;
            }
        }

        /// <summary>
        /// 完全に焼きが入り、表面硬さが変化しない領域の厚み[m]
        /// 常に0 &lt; ts&lt;=thであるべき。
        /// </summary>
        public double ts
        {
            get { return ts_; }
            set
            {
                if (value > th)
                {
                    Console.WriteLine("ts should be < th");
                }
                else if (value < 0)
                {
                    Console.WriteLine("ts should be >= 0");
                }
                else
                {
                    ts_ = value;
                    tt_ = th_ - ts_;
                }
            }

        }
        /// <summary>
        /// 表面硬化層全体の厚み[m]
        /// 常に0 &lt; ts &lt;= thであるべき。
        /// </summary>
        public double th
        {
            get { return th_; }
            set
            {
                if (value < ts)
                {
                    Console.WriteLine("th should be >=ts");
                }
                else
                {
                    th_ = value;
                    tt_ = th_ - ts_;
                }
            }
        }

        /// <summary>
        /// 不完全な焼きが入り、表面硬さが均質部の硬さに向かって減少している領域の厚み[m]
        /// </summary>
        public double tt { get { return tt_; } }

        /// <summary>
        /// 均質な標的のコンストラクタ
        /// </summary>
        /// <param name="Mater">Material</param>
        public Target(in Material Mater)
        {

            rho_ = Mater.rho;
            Y_ = Mater.Y;
            G_ = Mater.G;
            E_ = Mater.E;
            K0_ = Mater.K0;
            G_ = Mater.G;
            k_ = Mater.k;
            c_ = Mater.c;
            c0_ = Mater.c0;
            Ginv_ = 1d / G_;
            c0inv_ = 1d / c0_;
            Ys_ = Y_;
            ts_ = 0.0;
            th_ = 0.0;
            tt_ = th_ - ts_;
        }

        /// <summary>
        /// 表面硬化された標的のコンストラクタ。引数を覚えるのが面倒。
        /// </summary>
        /// <param name="Mater">Material</param>
        /// <param name="Y_surface">表面強度。硬さから換算するのが楽[GPa]</param>
        /// <param name="thickness_surface">完全に焼きが入ってる領域の厚み$t_s$[m]</param>
        /// <param name="thickness_hardend">表面硬化全体の厚み$t_h$[m]</param>
        /// <remarks>常に$t_s \lt t_h$になる。</remarks>
        public Target(in Material Mater, double Y_surface, double thickness_surface, double thickness_hardend)
        {

            rho_ = Mater.rho;
            Y_ = Mater.Y;
            G_ = Mater.G;
            E_ = Mater.E;
            K0_ = Mater.K0;
            G_ = Mater.G;
            k_ = Mater.k;
            c_ = Mater.c;
            c0_ = Mater.c0;
            Ginv_ = 1d / G_;
            c0inv_ = 1d / c0_;
            Ys_ = Y_surface * 1e9;
            ts_ = thickness_surface;
            th_ = thickness_hardend;
            tt_ = th_ - ts_;
        }
        /// <summary>
        /// Target複製用のコンストラクタ。
        /// </summary>
        /// <param name="Tar">Target</param>
        public Target(in Target Tar)
        {

            rho_ = Tar.rho_;
            Y_ = Tar.Y_;
            G_ = Tar.G_;
            E_ = Tar.E_;
            K0_ = Tar.K0_;
            k_ = Tar.k_;
            c_ = Tar.c_;
            c0_ = Tar.c0_;
            Ginv_ = 1d / G_;
            c0inv_ = 1d / c0_;
            Ys_ = Tar.Ys_;
            ts_ = Tar.ts;
            th_ = Tar.th;
            tt_ = th_ - ts_;
        }
        //Target set_Y(double const &YS);

        /// <summary>
        /// 深さxにおける標的の強度
        /// </summary>
        /// <param name="x">深さ[m]</param>
        /// <returns>標的強度[Pa]</returns>
        public double Y(double x)
        {
            return calc_Y(x);
        }

        /// <summary>
        /// 深さxにおける標的の強度の逆数
        /// </summary>
        /// <param name="x">深さ[m]</param>
        /// <returns>1/標的強度[Pa^-1]</returns>
        public double Yinv(double x)
        {
            return 1d / Y(x);
        }

        /// <summary>
        /// 標的の剛性率の逆数
        /// </summary>
        public double Ginv { get { return Ginv_; } }
        /// <summary>
        /// 標的の体積波速度の逆数
        /// </summary>
        public double c0inv { get { return c0inv_; } }

        /// <summary>
        /// 密度[kg/m3]
        /// </summary>
        public double rho
        {
            get { return rho_; }
            set
            {
                rho_ = value;
                c_ = Math.Sqrt(E_ / rho_);
                c0_ = Math.Sqrt(K0_ / rho_);
                c0inv_ = 1d / c0;
            }
        }

        /// <summary>
        /// ヤング率[Pa]
        /// </summary>
        public double E
        {
            get { return E_; }
            set
            {
                E_ = value * 1e9;
                G_ = 3e0 * K0 * E / (9e0 * K0 - E);
                Ginv_ = 1d / G;
                c_ = Math.Sqrt(E_ / rho_);
            }

        }
        /// <summary>
        /// 剛性率[Pa]
        /// </summary>
        public double G { get { return G_; } }
        /// <summary>
        /// 静的な体積弾性率[Pa]
        /// </summary>
        public double K0 {
            get { return K0_; }
            set
            {
                K0_ = value * 1e9;
                G_ = 3e0 * K0 * E / (9e0 * K0 - E);
                Ginv_ = 1d / G;
                c0_ = Math.Sqrt(K0_ / rho_);
                c0inv_ = 1d / c0_;
            }
        }
        /// <summary>
        /// 衝撃波速度の粒子速度依存性[-]
        /// </summary>
        public double k
        {
            get { return k_; }
            set { k_ = value; }
        }

        /// <summary>
        /// 体積波速度[m/s]
        /// </summary>
        public double c0 { get { return c0_; } }
        /// <summary>
        /// 縦波速度[m/s]
        /// </summary>
        public double c { get { return c_; } }


        /// <summary>
        /// 均質な部分の降伏強度Y0のセッター
        /// </summary>
        /// <param name="Y_matrix">均質な部分の降伏強度[GPa]</param>
        /// <returns>Target</returns>
        public Target set_Y(double Y_matrix)
        {
            Y_ = Y_matrix * 1e9;
            return this;

        }


        /// <summary>
        /// 表面硬化層の強度、厚み設定。
        /// </summary>
        /// <param name="Y_surface">表面強度[GPa]</param>
        /// <param name="thickness_surface">完全に焼きが入った領域の厚み[m]</param>
        /// <param name="thickness_hardend">表面硬化層全体の厚み[m]</param>
        /// <returns>Target</returns>
        public Target set_Y(double Y_surface, double thickness_surface, double thickness_hardend)
        {
            Ys_ = Y_surface * 1e9;
            ts_ = thickness_surface;
            th_ = thickness_hardend;
            tt_ = th - ts;
            return this;

        }

        /// <summary>
        /// 標的強度全体を再設定
        /// </summary>
        /// <param name="Y_surface">表面強度[GPa]</param>
        /// <param name="thickness_surface">完全に焼きが入った領域の厚み[m]</param>
        /// <param name="thickness_hardend">表面硬化層全体の厚み[m]</param>
        /// <param name="Y_matrix">均質な部分の降伏強度[GPa]</param>
        /// <returns>Target</returns>
        public Target set_Y(double Y_surface, double thickness_surface, double thickness_hardend, double Y_matrix)
        {
            Ys_ = Y_surface * 1e9;
            ts_ = thickness_surface;
            th_ = thickness_hardend;
            tt_ = th - ts;
            Y_ = Y_matrix * 1e9;
            return this;
        }

        /// <summary>
        /// 深さxにおける標的の強度を計算するための関数[Pa]
        /// </summary>
        /// <param name="x">深さ[m]</param>
        /// <returns>強度[Pa]</returns>
        public double calc_Y(double x)
        {
            double ret = 0;
            if ((x >= th) || (Y_==Ys_)|| (ts ==0 && th ==0))
            {
                ret = Y_;
            }
            else if ((x >= ts) && (x < th))
            {
                ret = -(Ys_ - Y_) / tt * x + (Ys_ * (th) - Y_ * ts) / tt;
            }
            else
            {
                ret = Ys_;

            }
            return ret;
        }


    }

    /// <summary>
    /// 侵徹体の材料特性、寸法などを設定。
    /// </summary>
    public class Penetrator
    {


        double L_, D_;
        double Y_, rho_, E_, G_, K0_, k_, c0_, c_;
        double c0inv_, cinv_;
        double l_, cv_, Crh_, theta0_;
        double m_;
        /// <summary>
        /// 侵徹体長さ[m]
        /// </summary>
        public double L
        {
            get
            {
                return L_;
            }
            set
            {
                L_ = value;
                m_ = calc_m();
            }
        }
        /// <summary>
        /// 侵徹体直径[m]
        /// </summary>
        public double D
        {
            get
            {
                return D_;
            }
            set
            {
                D_ = value;
                l_ = calc_l();
                m_ = calc_m();
            }
        }
        /// <summary>
        /// 侵徹体半径[m]
        /// </summary>
        public double R { get { return 0.5 * D_; } }

        /// <summary>
        /// 侵徹体L/D比。
        /// </summary>
        public double LD { get { return L / D; } }
        /// <summary>
        /// 先端が半球状のPenetratorのコンストラクタ。
        /// CalcAW,CalcAWLV,CalcMBE用。
        /// CRHが0.5として設定される。
        /// CalcForrLVではCRHが0.5として計算されるだけで問題はない。
        /// </summary>
        /// <param name="Mater">Material</param>
        /// <param name="L">侵徹体長さ[m]</param>
        /// <param name="D">侵徹体径[m]</param>
        public Penetrator(in Material Mater, double L, double D)
        {
            rho_ = Mater.rho;
            Y_ = Mater.Y;
            G_ = Mater.G;
            E_ = Mater.E;
            K0_ = Mater.K0;
            k_ = Mater.k;
            c_ = Mater.c;
            c0_ = Mater.c0;
            this.L = L;
            this.D = D;

            c0inv_ = 1d / Mater.c0;
            cinv_ = 1d / c_;
            Crh_ = 0.5;
            theta0_ = calc_theta0();
            cv_ = calc_cv();
            l_ = calc_l();
            m_ = calc_m();
        }

        /// <summary>
        /// 侵徹体先端形状を含めたPenetratorのコンストラクタ．
        /// CRHはCalcForrLVでのみ考慮される．
        /// </summary>
        /// <param name="Mater">Material</param>
        /// <param name="L">侵徹体長さ[m]</param>
        /// <param name="D">侵徹体径[m]</param>
        /// <param name="Crh">侵徹体先端形状CRH[-]</param>
        public Penetrator(in Material Mater, double L, double D, double Crh) //Forresltal用コンストラクタ
        {
            rho_ = Mater.rho;
            Y_ = Mater.Y;
            G_ = Mater.G;
            E_ = Mater.E;
            K0_ = Mater.K0;
            G_ = Mater.G;
            k_ = Mater.k;
            c_ = Mater.c;
            c0_ = Mater.c0;
            this.L = L;
            this.D = D;

            c0inv_ = 1d / Mater.c0;
            cinv_ = 1d / c_;
            Crh_ = Crh;
            theta0_ = calc_theta0();
            cv_ = calc_cv();
            l_ = calc_l();
            m_ = calc_m();
        }

        /// <summary>
        /// Penetratorのコピー用コンストラクタ。
        /// </summary>
        /// <param name="Pen">Penetrator</param>
        public Penetrator(in Penetrator Pen)
        {
            rho_ = Pen.rho;
            Y_ = Pen.Y;
            G_ = Pen.G;
            E_ = Pen.E;
            K0_ = Pen.K0;
            k_ = Pen.k;
            c_ = Pen.c;
            c0_ = Pen.c0;
            L = Pen.L;
            D = Pen.D;

            c0inv_ = 1d / c0_;
            cinv_ = 1d / c_;

            Crh_ = Pen.Crh_;
            theta0_ = calc_theta0();
            cv_ = calc_cv();
            l_ = calc_l();
            m_ = calc_m();
        }
        //    Penetrator(Penetrator& P0);
        // Penetrator set_Y(double const &YS);



        /// <summary>
        /// 侵徹体強度[Pa]
        /// </summary>
        public double Y
        {
            get { return Y_; }
            set
            {
                Y_ = value * 1e9;
            }
        }

        /// <summary>
        /// 侵徹体密度[kg/m^3]
        /// 
        /// </summary>
        public double rho
        {
            get { return rho_; }
            set
            {
                rho_ = value;
                m_ = calc_m();
                c_ = Math.Sqrt(E_ / rho_);
                c0_ = Math.Sqrt(K0_ / rho_);
                cinv_ = 1d / c_;
                c0inv_ = 1d / c0_;
            }
        }

        /// <summary>
        /// ヤング率[Pa]
        /// </summary>
        public double E
        {
            get { return E_; }
            set
            {
                E_ = value * 1e9;
                G_ = 3e0 * K0 * E / (9e0 * K0 - E);

                c_ = Math.Sqrt(E_ / rho_);
                cinv_ = 1d / c_;
            }
        }
        /// <summary>
        /// 剛性率[Pa]
        /// </summary>
        public double G { get { return G_; } }

        /// <summary>
        /// 静的な体積弾性率[Pa]
        /// </summary>
        public double K0
        {
            get { return K0_; }
            set
            {
                K0_ = value * 1e9;
                G_ = 3e0 * K0 * E / (9e0 * K0 - E);
                
                c0_ = Math.Sqrt(K0_ / rho_);
                c0inv_ = 1d / c_;
            }
        }

        /// <summary>
        /// 衝撃波速度の粒子速度依存性
        /// </summary>
        public double k
        {
            get { return k_; }
            set { k_ = value; }
        }

        /// <summary>
        /// 静的な体積波速度[m/s]
        /// </summary>
        public double c0 { get { return c0_; } }
        /// <summary>
        /// 縦波速度[m/s]
        /// </summary>
        public double c { get { return c_; } }

        /// <summary>
        /// 1/c0[s/m]
        /// </summary>
        public double c0inv { get { return c0inv_; } }
        /// <summary>
        /// 1/c[s/m]
        /// </summary>
        public double cinv { get { return cinv_; } }

        /// <summary>
        /// 侵徹体先端形状CRH[-]
        /// </summary>
        public double Crh
        {
            get { return Crh_; }
            set
            {
                Crh_ = value;
                theta0_ = calc_theta0();
                cv_ = calc_cv();
                l_ = calc_l();

                m_ = calc_m();
            }
        }
        /// <summary>
        /// 侵徹体先端部の半球状領域の長さ。
        /// </summary>
        public double l { get { return l_; } }
        /// <summary>
        /// 侵徹体先端部の半球状領域の重さ$m$を与える係数
        /// $$m = (L-l+cv\times R)\times \rho_P\timesR^2$$
        /// </summary>
        public double cv { get { return cv_; } }

        /// <summary>
        /// 侵徹体の先端部を半球状に近似したときの侵徹先端部の開き角
        /// </summary>
        public double theta0 { get { return theta0_; } }

        /// <summary>
        /// 侵徹体の質量[kg] 
        /// $$m = (L-l+cv\times R)\times \rho_P \times R^2$$
        /// </summary>
        public double m { get { return m_; } }

        private double calc_m()
        {
            return (L - l + cv * R) * rho * R * R * Math.PI;
        }
        private double calc_l()
        {
            return R * Math.Sqrt(Crh_ * 4d - 1d);
        }
        private double calc_cv()
        {
            return (4d * Crh_ * Crh_ - 4d * Crh_ / 3d + 1d / 3d) * Math.Sqrt(4d * Crh_ - 1) - 4d * Crh_ * Crh_ * (2d * Crh_ - 1d) * Math.Asin(Math.Sqrt(4d * Crh_ - 1d) / Crh_ * 0.5);
        }

        private double calc_theta0()
        {
            return Math.Asin((2d * Crh_ - 1d) / Crh_ * 0.5);
        }
    };




}
