using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Banchou.Pawn.Part {
    [ExecuteAlways]
    public class SetMaterialProperties : MonoBehaviour {
        private readonly int _colorIndex = Shader.PropertyToID("_Color");
        private readonly int _tintIndex = Shader.PropertyToID("_TintColor");
        
        [field: SerializeField] public Color Color { get; private set; }
        [field: SerializeField] public Color Tint { get; private set; }

        [Serializable]
        private class ChildMaterial {
            [HideInInspector] public string Name;
            [HideInInspector] public Renderer Renderer;
            [HideInInspector] public int MaterialIndex;
            [HideInInspector] public Color OriginalColor;
            [HideInInspector] public Color OriginalTint;
            public bool Apply;
        }

        [SerializeField] private ChildMaterial[] _materials;
        private MaterialPropertyBlock _properties;
        private readonly List<Material> _childMaterials = new();

        private ChildMaterial[] GetChildMaterials() =>
            transform.BreadthFirstTraversal()
                .Select(child => {
                    child.TryGetComponent<Renderer>(out var childRenderer);
                    return childRenderer;
                })
                .Where(childRenderer => childRenderer != null)
                .SelectMany(childRenderer => childRenderer.sharedMaterials
                    .Select((material, index) => {
                        var childMaterial = new ChildMaterial {
                            Name = $"{material.name} ({childRenderer.name})",
                            Renderer = childRenderer,
                            MaterialIndex = index,
                        };
                        
                        if (material.HasProperty(_colorIndex)) {
                            childMaterial.OriginalColor = material.GetColor(_colorIndex);
                        }
                        
                        if (material.HasProperty(_tintIndex)) {
                            childMaterial.OriginalTint = material.GetColor(_tintIndex);
                        }
                        
                        return childMaterial;
                    })
                )
                .GroupJoin(
                    _materials?.Where(material => material.Renderer != null) ?? Enumerable.Empty<ChildMaterial>(),
                    oldMaterial => (oldMaterial.Renderer, oldMaterial.MaterialIndex),
                    newMaterial => (newMaterial.Renderer, newMaterial.MaterialIndex),
                    (material, group) => {
                        material.Apply = group.SingleOrDefault()?.Apply ?? false;
                        return material;
                    }
                )
                .OrderBy(childMaterial => childMaterial.Name)
                .ToArray();

        private void ApplyProperties() {
            _properties ??= new();
            
            for (int i = 0; i < _materials.Length; i++) {
                var childMaterial = _materials[i];
                if (childMaterial.Renderer == null) continue; // Skip renderers that were deleted
                
                childMaterial.Renderer.GetSharedMaterials(_childMaterials);
                
                var material = _childMaterials[childMaterial.MaterialIndex];
                childMaterial.Renderer.GetPropertyBlock(_properties, childMaterial.MaterialIndex);
                
                if (childMaterial.Apply) {
                    if (material.HasProperty(_colorIndex)) {
                        _properties.SetColor(_colorIndex, Color);
                    }

                    if (material.HasProperty(_tintIndex)) {
                        _properties.SetColor(_tintIndex, Tint);
                    }
                } else {
                    if (material.HasProperty(_colorIndex)) {
                        _properties.SetColor(_colorIndex, childMaterial.OriginalColor);
                    }

                    if (material.HasProperty(_tintIndex)) {
                        _properties.SetColor(_tintIndex, childMaterial.OriginalTint);
                    }
                }
                
                childMaterial.Renderer.SetPropertyBlock(_properties, childMaterial.MaterialIndex);
            }
        }

        private void OnValidate() {
            if (Application.isEditor && !Application.isPlaying) {
                _materials = GetChildMaterials();
            }
            ApplyProperties();
        }

        private void Update() {
            ApplyProperties();
        }
    }
}