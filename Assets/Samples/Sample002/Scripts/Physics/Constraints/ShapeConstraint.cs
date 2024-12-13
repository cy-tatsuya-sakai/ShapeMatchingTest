using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Samples.Sample002.Physics.Constraints
{
    /// <summary>
    /// シェイプマッチング
    /// </summary>
    public class ShapeConstraint : IConstraint
    {
        private PointMass[] _points;
        private double2[]   _shape;
        private double[]    _lambda;
        private double      _compliance;

        public ShapeConstraint(double2[] shape, IEnumerable<PointMass> points, double compliance)
        {
            _points = points.ToArray();
            _shape  = shape;
            _lambda = new double[shape.Length];
            _compliance = compliance;
        }

        public bool UpdateShape(double2[] shape)
        {
            if(shape.Length != _shape.Length) { return false; }
            _shape = shape;
            return true;
        }

        public void Prepare()
        {
            for(int i = 0; i < _lambda.Length; i++)
            {
                _lambda[i] = 0.0;
            }
        }

        public void SolvePosition(double dt)
        {
            int num = _shape.Length;

            // 重心を計算
            var center = double2.zero;
            foreach(var point in _points)
            {
                center += point.position;
            }
            center /= num;

            // 回転行列を計算
            var mtx = double2x2.zero;
            for(int i = 0; i < num; i++)
            {
                var p = _points[i].position - center;
                var q = _shape[i];

                var v = new double2(
                    p.x * q.x + p.y * q.y,  // 内積 = cos
                    p.x * q.y - p.y * q.x   // 外積 = sin
                );
                mtx.c0 += new double2(v.x, -v.y);   // X軸
                mtx.c1 += new double2(v.y,  v.x);   // Y軸
            }
            {
                // 軸ベクトルを正規化する
                var v = mtx.c0;
                var u = math.sqrt(v.x * v.x + v.y * v.y);
                mtx = (u > 0.0f) ? mtx * (1.0f / u) : float2x2.identity;
            }

            // 目標に向かって点群を移動する
            var compliance = _compliance / (dt * dt);
            for(int i = 0; i < num; i++)
            {
                var point = _points[i];
                var pos = point.position;
                var tgt = math.mul(mtx, _shape[i]) + center;    // 目標。初期位置を回転、平行移動する

                var v = tgt - pos;
                var d = math.length(v);
                if(d <= 0.0) { continue; }

                // 目標との距離を縮める
                var dLambda = (d - compliance * _lambda[i]) / (point.invMass + compliance);
                v = v / d * dLambda;

                _lambda[i] += dLambda;
                point.position += v * point.invMass;
            }
        }
    }
}
