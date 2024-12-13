using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Samples.Sample002.Physics.Constraints;
using Unity.Mathematics;
using UnityEngine;

namespace Samples.Sample002.Physics
{
    /// <summary>
    /// 物理演算の後処理で位置ベース物理を差し込む
    /// </summary>
    public class PBDSimurator : SingletonMonoBehaviour<PBDSimurator>
    {
        private const int STEP_NUM = 8;

        [SerializeField] private PointMass _pointMassPrefab;

        private List<PointMass>         _pointMassList  = new();
        private List<IConstraint>       _constraintList = new();
        private List<ContactConstraint> _contactList    = new();    // コリジョンは別管理にしておく。頻繁に追加・削除されそうなので
        private int                     _contactNum     = 0;        // コリジョン数。毎フレームリセットする

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(Simurate());
        }

        /// <summary>
        /// 拘束を追加
        /// </summary>
        private static void AddConstraint(IConstraint constraint)
        {
            Instance._constraintList.Add(constraint);
        }

        /// <summary>
        /// 質点を生成
        /// </summary>
        public static PointMass AddPointMass(Vector3 pos, Transform parent)
        {
            var inst = Instantiate(Instance._pointMassPrefab, parent);
            inst.transform.position = pos;
            Instance._pointMassList.Add(inst);
            return inst;
        }

        /// <summary>
        /// シェイプマッチング拘束を追加
        /// </summary>
        public static ShapeConstraint AddShapeConstraint(double2[] shape, IEnumerable<PointMass> points, double compliance)
        {
            var inst = new ShapeConstraint(shape, points, compliance);
            AddConstraint(inst);
            return inst;
        }

        /// <summary>
        /// 距離拘束を追加
        /// </summary>
        public static DistanceConstraint AddDistanceConstraint(PointMass a, PointMass b, double length, double compliance)
        {
            var inst = new DistanceConstraint(a, b, length, compliance);
            AddConstraint(inst);
            return inst;
        }

        /// <summary>
        /// ひも状の拘束を追加
        /// </summary>
        public static List<DistanceConstraint> AddDistanceConstraint_Rope(IList<PointMass> points, double compliance, bool loop)
        {
            int num = points.Count;
            if(loop == false) { num--; }
            if(num <= 0)
            {
                return new List<DistanceConstraint>();
            }

            var ret = new List<DistanceConstraint>(num);
            for(int i = 0; i < num; i++)
            {
                var a = points[i];
                var b = points[(i + 1) % points.Count];
                var len = math.length(a.body.transform.position - b.body.transform.position);
                var inst = AddDistanceConstraint(a, b, len, compliance);
                ret.Add(inst);
            }

            return ret;
        }

        /// <summary>
        /// コリジョンの押し出しを追加
        /// </summary>
        public static void AddContactConstraint(PointMass p, double2 contactPoint, double2 normal)
        {
            ContactConstraint contact = null;
            if(Instance._contactNum < Instance._contactList.Count)
            {
                // 何となく要素使い回してみる…
                contact = Instance._contactList[Instance._contactNum++];
            }
            else
            {
                contact = new ContactConstraint();
                Instance._contactList.Add(contact);
                Instance._contactNum++;
            }

            contact.Init(p, contactPoint, normal);
        }

        /// <summary>
        /// 質点同士のコリジョンを無視する。連結した質点の衝突を止める目的
        /// </summary>
        public static void IgnoreCollision(IList<PointMass> points)
        {
            var num = points.Count;
            for(int i = 0; i < num; i++)
            for(int j = i; j < num; j++)
            {
                Physics2D.IgnoreCollision(points[i].collider, points[j].collider);
            }
        }

        /// <summary>
        /// シミュレーション
        /// </summary>
        private IEnumerator Simurate()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while(true)
            {
                yield return waitForFixedUpdate;

                // 前処理
                foreach(var point in _pointMassList)
                {
                    point.Prepare();
                }
                foreach(var constraint in _constraintList)
                {
                    constraint.Prepare();
                }
                for(int i = 0; i < _contactNum; i++)
                {
                    _contactList[i].Prepare();
                }

                // 拘束計算
                double dt = Time.fixedDeltaTime;
                Simurate(dt, STEP_NUM);

                // Rigidbody2Dに速度を書き戻す
                foreach(var point in _pointMassList)
                {
                    point.UpdateBody();
                }

                _contactNum = 0;    // コリジョンをリセット
            }
        }

        /// <summary>
        /// シミュレーション
        /// </summary>
        private void Simurate(double dt, int step)
        {
            // 位置を更新
            // foreach(var point in _pointMassList)
            // {
            //     point.UpdatePosition(dt);
            // }

            // 拘束計算
            for(int itr = 0; itr < step; itr++)
            {
                foreach(var constraint in _constraintList)
                {
                    constraint.SolvePosition(dt);
                }
                for(int i = 0; i < _contactNum; i++)
                {
                    _contactList[i].SolvePosition(dt);
                }
            }

            // 速度を更新
            foreach(var point in _pointMassList)
            {
                point.UpdateVelocity(dt);
            }
        }
    }
}
