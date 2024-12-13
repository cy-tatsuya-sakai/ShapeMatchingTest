using UnityEngine;

namespace Samples.Sample002.Physics
{
    /// <summary>
    /// 拘束処理関連の定数
    /// </summary>
    public static class PBDDefine
    {
        /// <summary>
        /// コンプライアンス値
        /// </summary>
        public enum ComplianceType
        {
            Concrete,
            Wood,
            Leather,
            Tendon,
            Rubber,
            Muscle,
            Fat,
        }

        /// <summary>
        /// Miles Macklin's blog (http://blog.mmacklin.com/2016/10/12/xpbd-slides-and-stiffness/)
        /// </summary>
        private static readonly double[] COMPLIANCE = new double[]
        {
            0.00000000004, // 0.04 x 10^(-9) (M^2/N) Concrete
            0.00000000016, // 0.16 x 10^(-9) (M^2/N) Wood
            0.000000001,   // 1.0  x 10^(-8) (M^2/N) Leather
            0.000000002,   // 0.2  x 10^(-7) (M^2/N) Tendon
            0.0000001,     // 1.0  x 10^(-6) (M^2/N) Rubber
            0.00002,       // 0.2  x 10^(-3) (M^2/N) Muscle
            0.0001,        // 1.0  x 10^(-3) (M^2/N) Fat
        };

        /// <summary>
        /// コンプライアンス値
        /// </summary>
        public static double ComplianceValue(ComplianceType type) => COMPLIANCE[(int)type];
    }
}
