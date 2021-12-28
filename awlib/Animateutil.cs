using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace awcsc
{
    /// <summary>
    /// 侵徹体などの輪郭を記録するクラス
    /// </summary>
    public class PointArray
    {
        /// <summary>
        /// x座標とy座標があります。
        /// </summary>
        public List<double> x, y;
        /// <summary>
        /// 分割数分配列サイズを確保
        /// </summary>
        /// <param name="size"></param>
        public PointArray(int size)
        {
            x = new List<double>() { Capacity = size };
            y = new List<double>() { Capacity = size };
        }
        /// <summary>
        /// x,yに追加
        /// </summary>
        /// <param name="a">x座標</param>
        /// <param name="b">y座標</param>
        public void Add(double a, double b)
        {
            x.Add(a);
            y.Add(b);
        }
    }

    /// <summary>
    /// 計算結果を入れると侵徹挙動をアニメーションで描画するのに必要な情報作成してくれるクラス。
    /// <example>
    /// <code>
    /// Calc C = Calc(P,T,V0);
    /// AnimateUtil A = new AnimateUtil(C);
    /// </code>
    /// </example>
    /// pythonでアニメーションを書きたいときは
    ///  <example>
    /// <code>
    /// key = list(A.plot[0].Keys)
    /// fig, ax = plt.subplots()
    /// ax.set_xlim(A.xmin, A.xmax)
    /// ax.set_ylim(A.ymin, A.ymax)
    /// for p in A.plot:
    ///     for k in key:
    ///         ax.plot(p[k].x, p[k].y)
    ///  </code>
    ///  </example>
    ///  的にかけば動きます(アニメーションにする関数は別途)
    /// </summary>
    public class AnimateUtil
    {
        /// <summary>
        /// なんだっけこれ。
        /// あると作るときに便利らしい
        /// </summary>
        protected struct Point
        {
            /// <summary>
            /// x,y
            /// </summary>
            public double x, y;
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="x0">x</param>
            /// <param name="y0">y</param>
            public Point(double x0, double y0)
            {
                x = x0; y = y0;
            }
        }

        /// <summary>
        /// 座標をplotするときの、ymin,ymax,xmax,xminを提供する
        /// </summary>
        public double ymin, ymax, xmax, xmin;

        /// <summary>
        /// 計算結果を保持
        /// </summary>
        public Dictionary<string, List<double>> result { get; }
        int numsplit;
        awcsc.Calc C;

        
        public List<Dictionary<string, PointArray>> plot = new List<Dictionary<string, PointArray>>();

        /// <summary>
        /// アニメーションを作るのに必要な辞書のリストを自動的に生成します。
        /// 具体的には、Target界面、Penetrator界面、s界面、alpha界面をkeyに持つ辞書を書く時間ごとに保持したリストをplotとして持ちます。
        /// pythonでアニメーションを書きたいときは
        /// key = list(A.plot[0].Keys)
        /// fig, ax = plt.subplots()
        /// ax.set_xlim(A.xmin, A.xmax)
        /// ax.set_ylim(A.ymin, A.ymax)
        /// for p in A.plot:
        ///     for k in key:
        ///         ax.plot(p[k].x, p[k].y)
        ///  とでもすれば動くと思います。
        /// </summary>
        /// <param name="C">Calc</param>
        /// <param name="dt">dt[s] デフォルトは1e-7</param>
        /// <param name="dt_log">dt_log[s] デフォルトは1e-5。</param>
        /// <param name="numsplit">座標分割数</param>
        public AnimateUtil(Calc C, double dt=1e-7, double dt_log=1e-5, int numsplit=100)
        {

            result = C.calc(dt, dt_log);
            this.C = C;
            //Console.WriteLine("init_C");

            this.numsplit = numsplit;
            //Console.WriteLine("init_numsplit");

            if (result["alpha"][0] != 0)
            {
                double alphamax = result["alpha"].Max() * 1.2;
                ymin = -C.Rc * alphamax;
                ymax = C.Rc * alphamax;
            }

            else
            {
                ymax = C.Rc * 1.2;
                ymin = -C.Rc * 1.2;

            }
            //Console.WriteLine("init_ymax");

            xmin = -result["L"][0] * 1.1;
            xmax = (result["alpha"].Max() * C.Rc + result["DoP"].Max()) * 1.1;
            //Console.WriteLine("init_xmax");

            for (int i=0; i < result["t"].Count; i++)
            {
                plot.Add(getEachTime(i));
            }
        }

        /// <summary>
        /// 各時間における座標を取得。
        /// </summary>
        /// <param name="iteration">座標を取得するiteration</param>
        /// <returns>Point arrayの辞書</returns>
        Dictionary<string, PointArray> getEachTime(int iteration)
        {
            double dop = result["DoP"][iteration];
            double s = result["s"][iteration];
            double alpha = result["alpha"][iteration];
            double L = result["L"][iteration];
            Dictionary<string, PointArray> plotAtEachTime = new Dictionary<string, PointArray>();
            plotAtEachTime["Target"] = getTargetIF(dop);
            //Console.WriteLine("init_TargetIF");

            plotAtEachTime["Penetrator"] = getPenetratorIF(dop, L);
            //Console.WriteLine("init_PenetratorIF");

            plotAtEachTime["s"] = getsIF(dop, s);
            //Console.WriteLine("init_sIF");

            plotAtEachTime["alpha"] = getalphaIF(dop, alpha);
            //Console.WriteLine("init_alphaIF");

            return plotAtEachTime;
         }
        /// <summary>
        /// 標的側の座標を取得。
        /// </summary>
        /// <param name="dop">侵徹深さ</param>
        /// <returns>Pointarray</returns>
        PointArray getTargetIF(double dop)
        {

            //var op = new Point[numsplit + 4];
            double theta = 1.57079632679;
            var opa = new PointArray(numsplit+4);
            //op[0] = new Point(0, xmax);//装甲面上端
            //op[1] = new Point(0, Rc);//侵徹体クレーター径まで

            opa.Add(0d, xmax);
            opa.Add(0, C.Rc);
            if (dop < C.Rc)
            {
                //op[2] = new Point(0, Rc);
                theta = Math.Atan(C.Rc / (C.Rc - dop));
            }
            else
            {
                //op[2] = new Point(dop - C.Rc, C.Rc);
                theta = 1.57079632679;
            }
            for (int i = 0; i < numsplit; i++)
            {
                double ttheta = ((double)numsplit - 2d * (double)i) / ((double)numsplit) * theta;
                double x = dop + C.Rc * (Math.Cos(ttheta) - 1d);
                if (x < 0)
                {
                    x = 0d;
                }
                //op[2 + i] = new Point(x, C.Rc * Math.Sin(ttheta));
                opa.Add(x, C.Rc * Math.Sin(ttheta));
                //MessageBox.Show((dop + C.Rc * (Math.Cos(ttheta)-1d)).ToString());

            }
            //op[2 + numsplit] = new Point(0, -C.Rc);
            //op[2 + numsplit + 1] = new Point(0, ymin);

            opa.Add(0, -C.Rc);
            opa.Add(0, ymin);
            return opa;
        }
        /// <summary>
        /// 侵徹体側の座標を取得する
        /// </summary>
        /// <param name="dop">侵徹深さ</param>
        /// <param name="L">侵徹体長さ。なぜP.Lを使ってるのかは知らない</param>
        /// <returns>PointArray</returns>
        PointArray getPenetratorIF(double dop, double L)
        {
            
            var op = new Point[numsplit + 3];
            var opa = new PointArray(numsplit + 3);
            double theta = 1.57079632679;
            //op[0] = new Point(dop - L, R);//侵徹体左上端

            opa.Add(dop - L, C.P.R);
            //if (dop < C.P.R)
            //{
             //   theta = Math.Atan(C.P.R / (C.P.R - dop));
                //op[1] = new Point(dop + R * (Math.Cos(theta) - 1d), R * Math.Sin(theta));//侵徹体阪急領域前径まで
            //}
            //else
            //{
                theta = 1.57079632679;
                //op[1] = new Point(dop- R , R * Math.Sin(theta));//侵徹体阪急領域前径まで
            //}
            for (int i = 0; i < numsplit; i++)
            {
                double ttheta = ((double)numsplit - 2d * (double)i) / ((double)numsplit) * theta;
                //double ttheta = (numsplit - 2 * i) / numsplit * theta;
                double x = dop + C.P.R * (Math.Cos(ttheta) - 1d);
                double y = C.P.R * Math.Sin(ttheta);


                //op[1 + i] = new Point(x, y);
                opa.Add(x, y);
            }
            //op[1 + numsplit] = new Point(dop - L, -R);
            //op[1 + numsplit + 1] = new Point(dop - L, R);

            opa.Add(dop - L, -C.P.R);
            opa.Add(dop - L, C.P.R);
            return opa;
        }
        /// <summary>
        /// 標的の塑性領域を取得
        /// </summary>
        /// <param name="dop">侵徹深さ</param>
        /// <param name="alpha">Alpha</param>
        /// <returns>PointArray</returns>
        PointArray getalphaIF( double dop,  double alpha)
        {
            //var op = new Point[numsplit + 1];
            var opa = new PointArray(numsplit + 1);
            double theta = 1.57079632679;
            double R = C.Rc * alpha;
            if (dop < C.Rc)
            {
                theta = Math.Atan(R / (C.Rc - dop));
                //op[1] = new Point(dop + R * (Math.Cos(theta) - 1d), R * Math.Sin(theta));//侵徹体阪急領域前径まで
            }
            else
            {
                theta = 1.57079632679;
                //op[1] = new Point(dop- R , R * Math.Sin(theta));//侵徹体阪急領域前径まで
            }
            for (int i = 0; i <= numsplit; i++)
            {
                //double ttheta = (numsplit - 2 * i) / numsplit * theta;
                double ttheta = ((double)numsplit - 2d * (double)i) / ((double)numsplit) * theta;
                double x = dop + R * Math.Cos(ttheta) - C.Rc;
                if (x < 0d)
                {
                    x = 0d;
                }
                //op[i] = new Point(x, R * Math.Sin(ttheta));
                opa.Add(x, R * Math.Sin(ttheta));
            }
            //MessageBox.Show($"{dop} + {R} -{Rc};");
            return opa;
        }
        /// <summary>
        /// 侵徹体の塑性変形領域を取得。
        /// </summary>
        /// <param name="dop">侵徹深さ</param>
        /// <param name="s">侵徹体塑性変形領域</param>
        /// <returns>PointArray</returns>
        PointArray getsIF(double dop, double s)
        {
            //var op = new Point[2];
            var opa = new PointArray(2);
            //op[0] = new Point(dop - s, R);//侵徹体左上端
            //op[1] = new Point(dop - s, -R);//侵徹体左上端

            opa.Add(dop - s, C.P.R);
            opa.Add(dop - s, -C.P.R);
            return opa;
        }
    }
}
