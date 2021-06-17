using System.Collections.Generic;
using UnityEngine;

namespace Banchou.Utility {
    [RequireComponent(typeof(MeshFilter))]
    public class UVWarp : MonoBehaviour {
        private enum AxisMapping { X, Y, Z }
        [SerializeField] private Transform _reference = null;
        [SerializeField] private AxisMapping _axisU = AxisMapping.X;
        [SerializeField] private AxisMapping _axisV = AxisMapping.Y;
        [SerializeField] private int _UVChannel = 0;

        private MeshFilter _meshFilter;
        private List<Vector2> _baseUVs;
        private List<Vector2> _offsetUVs;

        private void Awake() {
            _meshFilter = GetComponent<MeshFilter>();
            _meshFilter.mesh.GetUVs(_UVChannel, _baseUVs);
            _offsetUVs = new List<Vector2>(_baseUVs);
        }

        private void Update() {
            if (_reference?.localPosition != Vector3.zero) {
                for (int i = 0; i < _baseUVs.Count; i++) {
                    var offset = Vector2.zero;

                    switch (_axisU) {
                        case AxisMapping.X:
                            offset.x = _reference.localPosition.x;
                            break;
                        case AxisMapping.Y:
                            offset.x = _reference.localPosition.y;
                            break;
                        case AxisMapping.Z:
                            offset.y = _reference.localPosition.z;
                            break;
                    }

                    switch (_axisV) {
                        case AxisMapping.X:
                            offset.y = _reference.localPosition.x;
                            break;
                        case AxisMapping.Y:
                            offset.y = _reference.localPosition.y;
                            break;
                        case AxisMapping.Z:
                            offset.y = _reference.localPosition.z;
                            break;
                    }

                    _offsetUVs[i] = _baseUVs[i] + offset;
                }

                _meshFilter.mesh.SetUVs(_UVChannel, _offsetUVs);
            }
        }
    }
}