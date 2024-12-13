using System.Collections.Generic;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using Samples.Sample002.Physics;
using Samples.Sample002.Physics.Constraints;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace Samples.Sample002
{
    /// <summary>
    /// プレイヤー処理
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        private const int POINT_NUM = 48;

        [SerializeField] private MeshFilter     _meshFilter;
        [SerializeField] private PlayerInput    _input;

        private List<PointMass> _pointList = new(POINT_NUM);
        private double2[] _shapeCirc;
        private double2[] _shapeTri;
        private double2[] _shapeRect;
        private Mesh  _mesh;
        private int   _rotate;

        private ShapeConstraint          _shapeConstraint;
        private List<DistanceConstraint> _ropeConstraintList = new();

        void Start()
        {
            _shapeCirc = ShapeEditor.CreateCircle(POINT_NUM, 1);
            _shapeTri  = ShapeEditor.CreateTriangle(POINT_NUM, 1);
            _shapeRect = ShapeEditor.CreateRectangle(POINT_NUM, 1.5f);

            for(int i = 0; i < POINT_NUM; i++)
            {
                var v = _shapeRect[i];
                _pointList.Add(PBDSimurator.AddPointMass(new Vector3((float)v.x, (float)v.y), transform));
            }

            var compliance = 0.001f;    // 適当な柔らかさ
            _shapeConstraint = PBDSimurator.AddShapeConstraint(_shapeRect, _pointList, compliance);         // シェイプマッチング
            _ropeConstraintList = PBDSimurator.AddDistanceConstraint_Rope(_pointList, compliance, true);    // 輪郭をバネで包む
            PBDSimurator.IgnoreCollision(_pointList);

            _mesh = CreateMesh(_shapeCirc);
            _meshFilter.mesh = _mesh;
            UpdateMesh();
        }

        void Update()
        {
            void UpdateShape(double2[] shape)
            {
                _shapeConstraint.UpdateShape(shape);

                var len = math.length(shape[0] - shape[1]);
                foreach(var c in _ropeConstraintList)
                {
                    c.SetLength(len);
                }
            }

            var info = _input.GetInputInfo();

            // 形状
            if(info.changeRectangle)
            {
                UpdateShape(_shapeRect);
            }
            else if(info.changeTriangle)
            {
                UpdateShape(_shapeTri);
            }
            else if(info.changeCircle)
            {
                UpdateShape(_shapeCirc);
            }

            // 回転
            _rotate = info.roll;
        }

        void FixedUpdate()
        {
            // 回転
            if(_rotate != 0)
            {
                var center = Vector2.zero;
                foreach(var point in _pointList)
                {
                    center += point.body.position;
                }
                center /= _pointList.Count;

                foreach(var point in _pointList)
                {
                    var v = point.body.position - center;
                    v = new Vector2(-v.y, v.x) * (-_rotate);
                    v.Normalize();
                    point.body.AddForce(v * Time.fixedDeltaTime * 20, ForceMode2D.Impulse);
                }
            }
        }

        void LateUpdate()
        {
            UpdateMesh();
        }

        /// <summary>
        /// メッシュ生成
        /// </summary>
        private static Mesh CreateMesh(double2[] points)
        {
            // 頂点
            var vertices = points.Select(v => new Vector3((float)v.x, (float)v.y)).ToList();
            vertices.Add(Vector3.zero);

            // インデックス
            int num = points.Length;
            var indices = new List<int>();
            for(int i = 0; i < num; i++)
            {
                indices.Add(i);
                indices.Add((i + 1) % num);
                indices.Add(num);
            }

            // UV
            var uvs = vertices.Select(v => new Vector2(v.x * 0.5f + 0.5f, v.y * 0.5f + 0.5f)).ToList();

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            
            return mesh;
        }

        /// <summary>
        /// メッシュを更新
        /// </summary>
        private void UpdateMesh()
        {
            // 頂点
            var vertices = _pointList.Select(p => p.body.transform.position).ToList();
            var num      = vertices.Count;
            var center   = Vector3.zero;
            foreach(var v in vertices)
            {
                center += v;
            }
            center /= num;

            for(int i = 0; i < num; i++)
            {
                vertices[i] = vertices[i] - center;
            }
            vertices.Add(Vector3.zero);

            // 頂点を丸める
            var tmp = new Vector3[num];
            for(int i = 0; i < num; i++)
            {
                Vector3 p1 = vertices[i];
                Vector3 p2 = vertices[(i + 1) % num];
                Vector3 p3 = vertices[(i + num - 1) % num];
                tmp[i] = (p1 * 4 + p2 * 3 + p3 * 3) * 0.1f;
            }
            for(int i = 0; i < num; i++)
            {
                vertices[i] = tmp[i] + tmp[i].normalized * 0.5f;
            }

            _mesh.SetVertices(vertices);
            _mesh.RecalculateBounds();
            _meshFilter.transform.position = center;
        }
    }
}
