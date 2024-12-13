using UnityEngine;
using Unity.Mathematics;
using System.Linq;

namespace Samples.Sample001
{
    /// <summary>
    /// シェイプマッチングのベタ書きサンプル
    /// </summary>
    public class ShapeMatchingTest : MonoBehaviour
    {
        private const int   POINT_NUM   = 6;
        private const float GRAVITY     = 9.8f;
        private const float WALL_X      = 10;
        private const float WALL_Y      = 5;

        /// <summary>
        /// 点
        /// </summary>
        public class Point
        {
            public float2 position;     // 座標
            public float2 prevPosition; // 前フレームの座標
            public float2 velocity;     // 速度

            public Point(float2 pos)
            {
                position = prevPosition = pos;
                velocity = float2.zero;
            }

            /// <summary>
            /// 座標を更新
            /// </summary>
            public void UpdatePosition(float dt)
            {
                prevPosition = position;
                position += velocity * dt;
            }

            /// <summary>
            /// 速度を更新
            /// </summary>
            public void UpdateVelocity(float dt)
            {
                velocity = (position - prevPosition) * (1.0f / dt);
            }
        }

        private Point[]     _points;        // 点群
        private float2[]    _shape;         // 目標にする形状の初期位置

        private float2      _pickPosition;  // マウス操作用
        private bool        _isPick;        //
        private int         _pickIdx;       //
        private float2[]    _debugGoal;     // デバッグ表示用
        private float2x2    _debugMtx;      //

        void Start()
        {
            _shape  = new float2[POINT_NUM];
            _points = new Point[POINT_NUM];
            _debugGoal   = new float2[POINT_NUM];

            // 適当な形状を生成。とりあえず円形
            float ang = math.radians(360) / POINT_NUM;
            for(int i = 0; i < POINT_NUM; i++)
            {
                float r = ang * i;
                _shape[i]  = new float2(math.cos(r), math.sin(r));
                _points[i] = new Point(_shape[i]);
            }
        }

        void Update()
        {
            _isPick = Input.GetMouseButton(0);

            if(_isPick)
            {
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                _pickPosition = new float2(pos.x, pos.y);
            }

            if(Input.GetMouseButtonDown(0))
            {
                float dst = 10000000;
                int idx = 0;
                for(int i = 0; i < POINT_NUM; i++)
                {
                    var len = math.lengthsq(_pickPosition - _points[i].position);
                    if(len < dst)
                    {
                        dst = len;
                        idx = i;
                    }
                }
                _pickIdx = idx;
            }
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            // 座標を更新
            foreach(var point in _points)
            {
                point.UpdatePosition(dt);
            }

            // 適当な処理
            {
                // マウスの引っ張り
                if(_isPick)
                {
                    _points[_pickIdx].position = _pickPosition;
                }
            }

            // シェイプマッチング処理
            UpdateShape(dt);

            // 速度を更新
            foreach(var point in _points)
            {
                point.UpdateVelocity(dt);
            }

            // 適当な処理
            {
                // 重力
                float g = GRAVITY * dt;
                foreach(var point in _points)
                {
                    point.velocity.y -= g;
                }

                // 壁
                float wx = WALL_X;
                float wy = WALL_Y;
                foreach(var point in _points)
                {
                    if(point.position.x < -wx)
                    {
                        point.position.x = -wx;
                        point.velocity.x = math.max(point.velocity.x, 0.0f);
                        point.velocity.y = 0.0f;
                    }
                    else if(point.position.x > wx)
                    {
                        point.position.x = wx;
                        point.velocity.x = math.min(point.velocity.x, 0.0f);
                        point.velocity.y = 0.0f;
                    }

                    if(point.position.y < -wy)
                    {
                        point.position.y = -wy;
                        point.velocity.x = 0.0f;
                        point.velocity.y = math.max(point.velocity.y, 0.0f);
                    }
                }
            }
        }

        /// <summary>
        /// シェイプマッチング処理
        /// </summary>
        private void UpdateShape(float dt)
        {
            // 重心を計算
            var center = float2.zero;
            foreach(var point in _points)
            {
                center += point.position;
            }
            center *= (1.0f / POINT_NUM);

            // 回転行列を計算
            var mtx = float2x2.zero;
            for(int i = 0; i < POINT_NUM; i++)
            {
                var p = _points[i].position - center;
                var q = _shape[i];

                var v = new float2(
                    p.x * q.x + p.y * q.y,  // 内積 = cos
                    p.y * q.x - p.x * q.y   // 外積 = sin
                );
                mtx.c0 += new float2( v.x, v.y);    // X軸
                mtx.c1 += new float2(-v.y, v.x);    // Y軸
            }
            {
                // 軸ベクトルを正規化する
                var v = mtx.c0;
                var u = math.sqrt(v.x * v.x + v.y * v.y);
                mtx = (u > 0.0f) ? mtx * (1.0f / u) : float2x2.identity;
                _debugMtx = mtx;
            }

            // 目標に向かって点群を移動する
            for(int i = 0; i < POINT_NUM; i++)
            {
                var pos = _points[i].position;
                var tgt = math.mul(mtx, _shape[i]) + center;    // 目標。初期位置を回転、平行移動する
                _debugGoal[i] = tgt;

                // 適当に目標との距離を縮める
                _points[i].position += (tgt - pos) * 0.1f;
            }
        }

        void OnDrawGizmos()
        {
            if(Application.isPlaying == false) { return; }

            // 壁
            Gizmos.color = Color.green;
            Gizmos.DrawLineStrip(new Vector3[]
            {
                new(-WALL_X, 1000),
                new(-WALL_X, -WALL_Y),
                new( WALL_X, -WALL_Y),
                new( WALL_X, 1000),
            }, false);

            // 目標
            {
                Gizmos.color = Color.blue;
                var list = _debugGoal.Select(p => new Vector3(p.x, p.y)).ToArray();
                Gizmos.DrawLineStrip(list, true);
            }
            // 実際の頂点
            {
                Gizmos.color = Color.white;
                var list = _points.Select(p => new Vector3(p.position.x, p.position.y)).ToArray();
                Gizmos.DrawLineStrip(list, true);
            }

            // シェイプマッチングの回転行列
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero, new(_debugMtx.c0.x, _debugMtx.c0.y));
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero, new(_debugMtx.c1.x, _debugMtx.c1.y));
        }
    }
}
