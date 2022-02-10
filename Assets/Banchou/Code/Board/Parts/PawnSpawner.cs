using System.Linq;
using Banchou.Player;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Banchou.Board.Part {
    public abstract class PawnSpawner : MonoBehaviour {
        public abstract AssetReferenceGameObject PlayerAsset { get; protected set; }
        public abstract AssetReferenceGameObject PawnAsset { get; protected set; }

        protected GameState State;
        protected int PlayerId;
        protected int PawnId;

        public void Construct(GameState state) {
            State = state;
        }
        
        protected virtual void Start() {
            if (Application.isPlaying) {
                PlayerState player = null;
                var playerGuid = PlayerAsset?.AssetGUID;
                if (!string.IsNullOrEmpty(playerGuid)) {
                    State.AddPlayer(out player, prefabKey: playerGuid);
                    PlayerId = player.PlayerId;
                }

                var pawnGuid = PawnAsset?.AssetGUID;
                if (!string.IsNullOrEmpty(pawnGuid)) {
                    var xform = transform;
                    State.AddPawn(
                        out var pawn,
                        prefabKey: pawnGuid,
                        playerId: player?.PlayerId ?? 0,
                        position: xform.position,
                        forward: xform.forward
                    );
                    PawnId = pawn.PawnId;
                }
            }
        }
        
        protected virtual void OnDestroy() {
            if (PlayerId != default) {
                State.RemovePlayer(PlayerId);
            }

            if (PawnId != default) {
                State.RemovePawn(PawnId);
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
        
        public void OnDrawGizmos() {
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
#endif
    }
}