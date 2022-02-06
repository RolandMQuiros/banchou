using System.Linq;
using Banchou.Player;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Banchou.Board.Part {
    public class PawnSpawner : MonoBehaviour {
        [SerializeField, AssetReferenceUILabelRestriction("Pawns")]
        private AssetReferenceGameObject _pawnAsset;
        [SerializeField, AssetReferenceUILabelRestriction("Players")]
        private AssetReferenceGameObject _playerAsset;
        public AssetReferenceGameObject PawnAsset => _pawnAsset;
        public AssetReferenceGameObject PlayerAsset => _playerAsset;
        
        private GameState _state;

        public void Construct(GameState state) {
            _state = state;
        }

        private void Start() {
            if (Application.isPlaying) {
                PlayerState player = null;
                if (!string.IsNullOrEmpty(_playerAsset?.AssetGUID)) {
                    _state.AddPlayer(out player, prefabKey: _playerAsset.AssetGUID);
                }

                if (!string.IsNullOrEmpty(_pawnAsset?.AssetGUID)) {
                    var xform = transform;
                    _state.AddPawn(
                        prefabKey: _pawnAsset.AssetGUID,
                        playerId: player?.PlayerId ?? 0,
                        position: xform.position,
                        forward: xform.forward
                    );
                }
            }
        }
        
#if UNITY_EDITOR
        private (Transform, Mesh, Material[])[] _previewMeshes;
        
        private (Transform, Mesh, Material[])[] LoadPawnMesh(string assetGuid) {
            var path = AssetDatabase.GUIDToAssetPath(assetGuid);
            var pawnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (pawnPrefab != null) {
                return pawnPrefab
                    .GetComponentsInChildren<MeshFilter>()
                    .Select(meshFilter => (
                        meshFilter.transform,
                        meshFilter.sharedMesh,
                        meshFilter.GetComponent<Renderer>().sharedMaterials
                    ))
                    .Concat(
                        pawnPrefab
                            .GetComponentsInChildren<SkinnedMeshRenderer>()
                            .Select(smr => (smr.transform, smr.sharedMesh, smr.sharedMaterials))
                    )
                    .ToArray();
            }
            return null;
        }
        
        public void DrawGizmos() {
            var guid = PawnAsset?.AssetGUID;
            if (!string.IsNullOrEmpty(guid)) {
                if (_previewMeshes != null) {
                    var xform = transform;
                    for (int i = 0; i < _previewMeshes.Length; i++) {
                        var (meshXform, mesh, materials) = _previewMeshes[i];
                        if (Application.isPlaying) {
                            var position = xform.TransformPoint(meshXform.position);
                            var rotation = transform.rotation * meshXform.rotation;
                            var scale = Vector3.Scale(xform.localScale, meshXform.localScale);
                            
                            Gizmos.color = Color.grey;
                            for (int subMesh = 0; subMesh < materials.Length; subMesh++) {
                                Gizmos.DrawWireMesh(mesh, subMesh, position, rotation, scale);
                            }
                        } else {
                            var matrix = xform.localToWorldMatrix * meshXform.localToWorldMatrix;
                            for (int subMesh = 0; subMesh < materials.Length; subMesh++) {
                                materials[subMesh].SetPass(0);
                                Graphics.DrawMeshNow(mesh, matrix, subMesh);
                            }
                        }
                    }
                } else {
                    _previewMeshes = LoadPawnMesh(guid);
                }
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        private static void DrawGizmo(PawnSpawner spawner, GizmoType gizmoType) {
            spawner.DrawGizmos();
        }
#endif
    }
}