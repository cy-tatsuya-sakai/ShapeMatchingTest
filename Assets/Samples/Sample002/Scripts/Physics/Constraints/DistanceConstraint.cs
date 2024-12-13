using Unity.Mathematics;
using UnityEngine;

namespace Samples.Sample002.Physics.Constraints
{
    /// <summary>
    /// 距離拘束
    /// </summary>
    public class DistanceConstraint : IConstraint
    {
        private PointMass   _a, _b;
        private double      _length;
        private double      _lambda;
        private double      _compliance;

        public void SetLength(double length)
        {
            _length = length;
        }

        public DistanceConstraint(PointMass a, PointMass b, double length, double compliance)
        {
            _a = a;
            _b = b;
            _length = length;
            _compliance = compliance;
        }

        public void Prepare()
        {
            _lambda = 0.0;
        }

        public void SolvePosition(double dt)
        {
            var sumMass = _a.invMass + _b.invMass;
            if(sumMass <= 0.0) { return; }

            var v = _b.position - _a.position;
            var d = math.length(v);
            if(d <= 0.0) { return; }

            var constraint = d - _length;
            var compliance = _compliance / (dt * dt);
            var dLambda    = (constraint - compliance * _lambda) / (sumMass + compliance);
            v = v / d * dLambda;

            _lambda += dLambda;
            _a.position += v * _a.invMass;
            _b.position -= v * _b.invMass;
        }
    }
}
