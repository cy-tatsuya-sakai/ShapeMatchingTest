using Samples.Sample002.Physics.Constraints;
using Unity.Mathematics;
using UnityEngine;

namespace Samples.Sample002.Physics.Constraints
{
    /// <summary>
    /// コリジョンの押し出し
    /// 効果あるかあんまり分かってない…
    /// </summary>
    public class ContactConstraint : IConstraint
    {
        private PointMass _point;
        private double2   _contactPoint;
        private double2   _normal;
        private double    _lambda;

        public void Init(PointMass point, double2 contactPoint, double2 normal)
        {
            _point = point;
            _contactPoint = contactPoint;
            _normal = normal;
        }

        public void Prepare()
        {
            _lambda = 0.0;
        }

        public void SolvePosition(double dt)
        {
            var v = _point.position - _contactPoint;
            var d = math.dot(v, _normal);
            if(d >= 0.0) { return; }

            var compliance = 0.00001 / (dt * dt); // 適当に押し出す
            var dLambda = (d - compliance * _lambda) / (_point.invMass + compliance);

            _lambda += dLambda;
            _point.position -= _normal * dLambda * _point.invMass;
        }
    }
}
