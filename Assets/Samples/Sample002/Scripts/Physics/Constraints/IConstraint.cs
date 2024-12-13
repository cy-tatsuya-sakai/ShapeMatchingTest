using UnityEngine;

namespace Samples.Sample002.Physics.Constraints
{
    /// <summary>
    /// 制約のインターフェース
    /// </summary>
    public interface IConstraint
    {
        public void Prepare();
        public void SolvePosition(double dt);
    }
}
