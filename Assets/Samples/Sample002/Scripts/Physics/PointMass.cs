using Unity.Mathematics;
using UnityEngine;

namespace Samples.Sample002.Physics
{
    /// <summary>
    /// 質点。Rigidbody2Dのシミュレーション後に位置ベースの拘束差し込み用
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class PointMass : MonoBehaviour
    {
        [SerializeField] private Collider2D _collider;
        public new Collider2D collider => _collider;

        public Rigidbody2D body         { get; private set; }
        public double2     position     { get; set; }
        public double2     prevPosition { get; private set; }
        public double2     velocity     { get; set; }
        public double      invMass      { get; private set; }

        void Awake()
        {
            body = gameObject.GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// 計算前の初期化
        /// </summary>
        public void Prepare()
        {
            var pos = body.position;
            position = prevPosition = new double2(pos.x, pos.y);
            velocity = double2.zero;
            invMass  = 1.0 / math.max(body.mass, 0.0001);
        }

        /// <summary>
        /// 座標を更新
        /// </summary>
        public void UpdatePosition(double dt)
        {
            prevPosition = position;
            position += velocity * dt;
        }

        /// <summary>
        /// 速度を更新
        /// </summary>
        public void UpdateVelocity(double dt)
        {
            velocity = (position - prevPosition) * (1.0 / dt);
        }

        /// <summary>
        /// 位置ベースの計算結果をRigidbody2Dに書き戻す
        /// </summary>
        public void UpdateBody()
        {
            body.position = new Vector2((float)position.x, (float)position.y);

            var vel = new Vector2((float)velocity.x, (float)velocity.y);
            body.AddForce(vel, ForceMode2D.Impulse);
        }

        void OnCollisionStay2D(Collision2D coll)
        {
            var other = coll.gameObject.GetComponentInParent<Rigidbody2D>();
            var otherInvMass = (other != null) ? 1.0f / other.mass : 0.0f;

            foreach(var contact in coll.contacts)
            {
                var p = contact.point;
                var n = contact.normal;
                PBDSimurator.AddContactConstraint(this, new double2(p.x, p.y), new double2(n.x, n.y));
            }
        }
    }
}
