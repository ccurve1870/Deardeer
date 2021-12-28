using System;
using System.Collections.Generic;
using System.Text;

namespace awcsc
{
    /// <summary>
    /// 侵徹時の状態を保持するクラス。
    /// </summary>
    public class State
    {
        /// <summary>
        /// 侵徹体先端速度[m/s]
        /// </summary>
        public double u;
        /// <summary>
        /// 侵徹体後端速度[m/s]
        /// </summary>
        public double v;
        /// <summary>
        /// 侵徹体先端塑性領域[m]
        /// </summary>
        public double s;
        /// <summary>
        /// 侵徹体長さ
        /// </summary>
        public double L;
        /// <summary>
        /// 標的塑性領域
        /// </summary>
        public double alpha;

        /// <summary>
        /// 侵徹体消耗率[-]
        /// $$Le =  \frac{L0-L}{L0}$$
        /// </summary>
        public double Le;
        /// <summary>
        /// 侵徹体後端加速度[m/s^2]
        /// </summary>
        public double vdot;
        /// <summary>
        /// 侵徹体先端加速度[m/s^2]
        /// </summary>
        public double udot;
        /// <summary>
        /// 侵徹体消耗速度[m/s]
        /// </summary>
        public double Ldot;
        /// <summary>
        /// 侵徹体塑性領域移動速度
        /// </summary>
        public double sdot;
        /// <summary>
        /// 説明が難しいけどvu_sdot
        /// </summary>
        public double vu_sdot;
        /// <summary>
        /// 標的塑性領域速度[1/s]
        /// </summary>
        public double alphadot;
        /// <summary>
        /// 侵徹深さ[m]
        /// </summary>
        public double DoP;
        /// <summary>
        /// 時間[s]
        /// </summary>
        public double t;


        /// <summary>
        /// コピー用の関数だけどなんでこんなことをしているのかよくわからない
        /// </summary>
        /// <param name="st">State</param>
        public void Copy(in State st)
        {
            u = st.u;
            v = st.v;
            s = st.s;
            L = st.L;
            Le = st.Le;
            Ldot = st.Ldot;

            alpha = st.alpha;
            vdot = st.vdot;
            udot = st.udot;
            sdot = st.sdot;
            alphadot = st.alphadot;
            vu_sdot = st.vu_sdot;
            DoP = st.DoP;
            t = st.t;

        }
        /// <summary>
        /// コンストラクタだけど、Calcのinit_Stateでやるから適当。
        /// </summary>
        public State()
        {
            u = 0d;
            v = 0d;
            s = 0d;
            L = 0d;
            Le = 0d;
            Ldot = 0d;

            alpha = 0d;
            vdot = 0d;
            udot = 0d;
            sdot = 0d;
            alphadot = 0d;
            vu_sdot = 0d;
            DoP = 0d;
            t = 0d;
        }

        /// <summary>
        /// 使わない
        /// </summary>
        /// <param name="st"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        public static State operator +(in State st, in State dst)
        {
            State stret = new State();
            stret.u = st.u + dst.u;
            stret.v = st.v + dst.v;
            stret.s = st.s + dst.s;
            stret.L = st.L + dst.L;
            stret.Le = st.Le + dst.Le;
            stret.Ldot = st.Ldot + dst.Ldot;

            stret.alpha = st.alpha + dst.alpha;
            stret.vdot = st.vdot + dst.vdot;
            stret.udot = st.udot + dst.udot;
            stret.sdot = st.sdot + dst.sdot;
            stret.alphadot = st.alphadot + dst.alphadot;
            stret.vu_sdot = st.vu_sdot + dst.vu_sdot;
            stret.DoP = st.DoP + dst.DoP;
            stret.t = st.t + dst.t;
            return stret;
        }

        /// <summary>
        /// 使わない
        /// </summary>
        /// <param name="st"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static State operator *(in State st, in double dt)
        {
            State stret = new State();
            stret.u = st.u * dt;
            stret.v = st.v * dt;
            stret.s = st.s * dt;
            stret.L = st.L * dt;
            stret.Le = st.Le * dt;
            stret.Ldot = st.Ldot * dt;

            stret.alpha = st.alpha * dt;
            stret.vdot = st.vdot * dt;
            stret.udot = st.udot * dt;
            stret.sdot = st.sdot * dt;
            stret.alphadot = st.alphadot * dt;
            stret.vu_sdot = st.vu_sdot * dt;
            stret.DoP = st.DoP * dt;
            stret.t = st.t * dt;
            return stret;
        }
    };
}
