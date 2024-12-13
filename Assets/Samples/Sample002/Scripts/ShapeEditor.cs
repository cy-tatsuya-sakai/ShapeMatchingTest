using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Samples.Sample002
{
    /// <summary>
    /// 形状を作成
    /// </summary>
    public static class ShapeEditor
    {
        /// <summary>
        /// 円を作成
        /// </summary>
        public static double2[] CreateCircle(int num, float radius)
        {
            var ret = new double2[num];
            var ang = math.radians(360.0f) / num;
            for(int i = 0; i < num; i++)
            {
                float r = ang * i;
                ret[i] = new double2(math.sin(r), math.cos(r)) * radius; // (0, 1)から始めたいのでsin, cosを入れ替える
            }

            return ret;
        }

        /// <summary>
        /// 四角形を作成
        /// 4で割り切れる頂点数にすること
        /// </summary>
        public static double2[] CreateRectangle(int num, float radius)
        {
            // 適当に矩形に変形する
            double ConvSize(double val)
            {
                if(val < -0.01f) { return -radius; }
                if(val >  0.01f) { return  radius; }
                return 0.0f;
            }

            var points = CreateCircle(8, radius)
                .Select(v => new double2(ConvSize(v.x), ConvSize(v.y)))
                .ToArray();
            return CreateLoopLine(num, points);
        }

        /// <summary>
        /// 三角形を作成
        /// 3で割り切れる頂点数にすること
        /// </summary>
        public static double2[] CreateTriangle(int num, float radius)
        {
            return CreateLoopLine(num, CreateCircle(3, radius));
        }

        /// <summary>
        /// 線を作成
        /// </summary>
        private static double2[] CreateLoopLine(int num, double2[] points)
        {
            var ret = new double2[num];
            int cnt = 0;
            int n = num / points.Length;
            for(int i = 0; i < points.Length; i++)
            {
                var p1 = points[i];
                var p2 = points[(i + 1) % points.Length];
                for(int j = 0; j < n; j++)
                {
                    ret[cnt++] = math.lerp(p1, p2, (float)j / n);
                }
            }

            return ret;
        }
    }
}
