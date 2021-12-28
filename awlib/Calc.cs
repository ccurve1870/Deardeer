using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace awcsc
{
    /// <summary>
    /// 侵徹計算の各種モデルを統一的に扱うためのAbstract class
    /// 具体的な実装は
    /// <see cref="CalcAW"/>,<see cref="CalcAWLV"/>,<see cref="CalcAWHVLV"/>,<see cref="CalcForrLV"/>,<see cref="CalcMBE"/>
    /// を参照のこと。
    /// 
    /// 基本的な使い方は<see cref="Material"/>により材料定数を設定し、
    /// <see cref="Target"/>,<see cref="Penetrator"/>で標的、侵徹体の特徴を決める。
    /// 最も簡単な例を以下に示す。
    /// <example>
    ///   <code>
    ///   Material M = new Material(rho, YS, E, K0, k);
    ///   Target T = new Target(M);
    ///   Penetrator P = new Penetrator(M, L,D);
    ///   Calc C = new Calc(P,T, V0, cfp);
    ///   var  res = C.calc(dt_log, dt);
    /// </code>
    /// </example>
    /// これは衝突速度V0での侵徹挙動を計算する。
    /// <c>var res = C.calc_Vdependent(V_list);</c>
    /// とすることで、速度が変化したときの侵徹終了時の各種値を返す。
    /// </summary>
    public abstract class Calc
    {
        /// <summary>
        /// 衝突速度[m/s]
        /// </summary>
        protected double V0_;
        /// <summary>
        /// 衝突時に形成されるクレーター径[m]
        /// </summary>
        /// <seealso cref="CalcAW.Crater_radius"/>
        /// <seealso cref="fit_param"/>
        protected double Rc_;
        /// <summary>
        /// 衝突時に形成されるクレーター径[m]
        /// </summary>
        /// <seealso cref="CalcAW.Crater_radius"/>
        /// <seealso cref="fit_param"/>
        public double Rc { get { return Rc_; } }

        /// <summary>
        /// 衝突速度[m/s]
        /// </summary>
        public abstract double V0 { get; set; }

        /// <summary>
        /// Calcが計算に使用するPenetrator
        /// </summary>
        /// <seealso cref="Penetrator"/>
        protected Penetrator P_;
        /// <summary>
        /// Calcが計算に使用するTarget
        /// </summary>
        /// <seealso cref="Target"/>
        protected Target T_;
        /// <summary>
        /// Calcが計算に使用するPenetrator
        /// </summary>
        /// <seealso cref="Penetrator"/>
        public abstract Penetrator P { get; set; }
        /// <summary>
        /// Calcが計算に使用するTarget
        /// </summary>
        /// <seealso cref="Target"/>
        public abstract Target T { get; set; } //Calcが計算に使用するTarget

        /// <summary>
        ///  衝突時に形成されるCrater径を求める際の速度依存性。わからなければ触れないこと
        /// </summary>
        /// <seealso cref="CalcAW.Crater_radius"/>
        /// <seealso cref="Rc"/>
        protected List<double> fit_param_ = new List<double>();
        /// <summary>
        ///  衝突時に形成されるCrater径を求める際の速度依存性。わからなければ触れないこと
        /// </summary>
        /// <seealso cref="CalcAW.Crater_radius"/>
        /// <seealso cref="Rc"/>
        public List<double> fit_param { get { return fit_param_; } }

        /// <summary>
        /// 速度V0で衝突する侵徹挙動を計算を実行する関数。
        /// 各種モデルについて統一的にこの関数を用いて計算を行い、各パラメータが格納された辞書を返す。
        /// SoAを返さないのはpythonとの相互作用を考慮。 
        /// 
        /// と書いていたけどよく考えると普通にSoAで返したほうが幸せかもしれない(検討
        /// 
        /// 計算はdtごとに行い、時間がdt_log経過するごとにその時の計算結果を保存するという形式。
        /// 結果を格納した辞書の中身は<see cref="State"/>参照
        /// </summary>
        /// <param name="dt0">計算時間ステップ[s]</param>
        /// <param name="dt_log0">計算結果取得ステップ[s]</param>
        /// <returns>結果を格納した辞書。引数の詳細は<see cref="State"/>参照</returns>
        /// <seealso cref="State"/>
        public virtual Dictionary<string, List<double>> calc(in double dt0, in double dt_log0)
        {
            double time_log = 0d;
            int size = 500;
            bool endcond = true;
            var result = new Dictionary<string, List<double>>();
            double dt = dt0;
            double dt_log = dt_log0;

            State stold;
            var stnew = new State();

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

            stold = init_State(dt);
            stnew.Copy(stold);

            int i = 0;
            while (endcond)
            {
                while (endcond && (stnew.t < time_log))
                {

                    Swap(ref stnew, ref stold);
                    
                    stnew = cycle(dt, stold);
                   
                    endcond = cond_endcalc(stnew, stold); //将来の変数からtmaxをへらすようの変更のための前準備
                };

                double Y = T_.calc_Y(stold.DoP) * 1e-9;
                result["u"].Add(stold.u);
                result["v"].Add(stold.v);
                result["s"].Add(stold.s);
                result["L"].Add(stold.L);
                result["alpha"].Add(stold.alpha);
                result["Le"].Add(stold.Le);
                result["vdot"].Add(stold.vdot);
                result["udot"].Add(stold.udot);
                result["Ldot"].Add(stold.Ldot);
                result["sdot"].Add(stold.sdot);
                result["vu_sdot"].Add(stold.vu_sdot);
                result["alphadot"].Add(stold.alphadot);
                result["DoP"].Add(stold.DoP);
                result["t"].Add(stold.t * 1e3);
                result["Y"].Add(Y);
                result["Rt"].Add(getRt(Y*1e9, stold) * 1e-9);

                time_log += dt_log;
                //stnewだと1サイクル余分に進んでいるためstoldで
            };
            return result;
        }
        
        /// <summary>
        /// 速度V0で衝突する侵徹挙動を計算を実行する関数。
        /// 各種モデルについて統一的にこの関数を用いて計算を行い、各パラメータが格納された辞書を返す。
        /// 
        /// <see cref="calc(in double, in double)"/>でListをpythonに返したときにpythonnetが行う
        /// Implicit List conversionがあまりにも遅いので生の配列を返すようにした。
        /// 
        /// 
        /// 計算はdtごとに行い、時間がdt_log経過するごとにその時の計算結果を保存するという形式。
        /// 結果を格納した辞書の中身は<see cref="State"/>参照
        /// </summary>
        /// <param name="dt0">計算時間ステップ[s]</param>
        /// <param name="dt_log0">計算結果取得ステップ[s]</param>
        /// <returns>結果を格納した辞書。引数の詳細は<see cref="State"/>参照</returns>
        /// <seealso cref="State"/>
        /// <seealso cref="calc(in double, in double)"/>
        public virtual Dictionary<string, double[]> calcPyInterop(in double dt0, in double dt_log0)
        {
            Dictionary<string, List<double>> Listres = calc(dt0, dt_log0);
            Dictionary<string, double[]> result = new Dictionary<string, double[]>();

            foreach(var key in Listres.Keys)
            {
                result[key] = Listres[key].ToArray();
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
        public virtual Dictionary<string, List<double>> calc_Vdependent(in double[] V0_list)
        {
            List<double> arr = new List<double>();
            arr.AddRange(V0_list);
            return calc_Vdependent(arr);
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
        public virtual Dictionary<string, List<double>> calc_Vdependent(in List<double> V0_list)
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
                    result[key].Add(r[key][1]);
                }

            }
            result["V0"] = V0_list;
            this.V0 = V0_or;

            return result;
        }

        /// <summary>
        /// 衝突速度V0を変化させながら、衝突終了後の値を取得する関数。
        /// 各種モデルについて統一的にこの関数を用いて計算を行い、各パラメータが格納された辞書を返す。
        /// 
        /// <see cref="calc_Vdependent(in double[])"/>でListをpythonに返したときにpythonnetが行う
        /// Implicit List conversionがあまりにも遅いので生の配列を返すようにした。
        /// 
        /// 結果を格納した辞書の中身は<see cref="State"/>参照。
        /// また、この関数では衝突速度V0も格納した辞書が返される
        /// </summary>
        /// <seealso cref="State"/>
        /// <seealso cref="calc_Vdependent(in double[])"/>
        /// <param name="V0_list">V0のリスト[m/s]。10000 m/sくらいから結果が不安定になる(この辺は割と難しくて放置)。</param>
        /// <returns>結果を格納した辞書。引数の詳細は<see cref="State"/>参照。この関数ではV0のリストも格納されている</returns>
        public virtual Dictionary<string, double[]> calc_VdependentPyInterop(in double[] V0_list)
        {
            Dictionary<string, List<double>> Listres = calc_Vdependent(V0_list);
            Dictionary<string, double[]> result = new Dictionary<string, double[]>();

            foreach (var key in Listres.Keys)
            {
                result[key] = Listres[key].ToArray();
            }
            return result;
        }
        /// <summary>
        /// 標的強度項Rtは常に変化するので、それを取得して記録することを矯正するための関数。
        /// </summary>
        /// <param name="Y">Material.Y[GPa]</param>
        /// <param name="stold">State</param>
        /// <returns></returns>
        protected abstract double getRt(double Y, in State stold);

        /// <summary>
        ///計算モデルのt=0sにおける状態を取得するための関数
        /// </summary>
        /// <param name="dt">dt[s]。まあ気にしなくていいと思います</param>
        /// <returns>初期化されたStateを返します</returns>
        protected abstract State init_State(in double dt);

        /// <summary>
        /// 実際に時間を進めるときの、1サイクルを記述した関数。気合だ。
        /// </summary>
        /// <param name="dt">時間刻み[s]</param>
        /// <param name="st0">時刻tにおける状態</param>
        /// <returns></returns>
        protected abstract State cycle(in double dt, in State st0);

        /// <summary>
        /// 計算を終了するための判定基準。
        /// がんばろう。
        /// </summary>
        /// <param name="st">Statenew</param>
        /// <param name="st0">Stateold</param>
        /// <returns></returns>
        protected abstract bool cond_endcalc(in State st, in State st0);
        /// <summary>
        /// cycleごとにstnewとstoldをスワップしたいので作った。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        protected static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
}

