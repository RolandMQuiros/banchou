using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Banchou.DependencyInjection {
    public sealed class DiContainer {
        private class Binding {
            public object Instance;
            public Func<Type, bool> Condition;
            public string Binder;
        }

        private readonly Dictionary<Type, Binding> _bindings = new Dictionary<Type, Binding>();

        public DiContainer(params object[] bindings) {
            foreach (var binding in bindings) {
                _bindings[binding.GetType()] = new Binding {
                    Instance = binding
                };
            }
            
            Bind(this);
            Bind<Instantiator>(Instantiate);
        }

        public DiContainer(in DiContainer prev, params object[] bindings) {
            _bindings = new Dictionary<Type, Binding>(prev._bindings);
            foreach (var binding in bindings) {
                _bindings[binding.GetType()] = new Binding {
                    Instance = binding
                };
            }

            Bind(this);
            Bind<Instantiator>(Instantiate);
        }

        public DiContainer Bind<T>(T instance, [CallerFilePath] string binder = null) {
            _bindings[typeof(T)] = new Binding { Instance = instance, Binder = binder };
            return this;
        }

        public DiContainer Bind<T>(T instance, Func<Type, bool> where, [CallerFilePath] string binder = null) {
            _bindings[typeof(T)] = new Binding {
                Instance = instance,
                Condition = where,
                Binder = binder
            };
            return this;
        }

        private static readonly Dictionary<Type, object> _applicableBindings = new();
        private static readonly Dictionary<Type, List<MethodBase>> _injectableMethods = new();
        private static readonly List<object> _injections = new();
        private static readonly Dictionary<MethodBase, ParameterInfo[]> _constructParameters = new();

        public T Inject<T>(T target) {
            if (target is DiContainer) {
                throw new Exception("Cannot inject into a DiContainer");
            }
            var targetType = target.GetType();

            _applicableBindings.Clear();
            foreach (var binding in _bindings) {
                if (binding.Value.Condition == null || binding.Value.Condition.Invoke(targetType)) {
                    _applicableBindings.Add(binding.Key, binding.Value.Instance);
                }
            }

            if (_applicableBindings.Count == 0) {
                return target;
            }

            if (!_injectableMethods.TryGetValue(targetType, out var injectInfo)) {
                injectInfo = targetType
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(inject => inject.Name == "Construct")
                    .Cast<MethodBase>()
                    .ToList();
                _injectableMethods[targetType] = injectInfo;
            }

            for (int i = 0; i < injectInfo.Count; i++) {
                var inject = injectInfo[i];
                
                if (!_constructParameters.TryGetValue(inject, out var parameters)) {
                    parameters = inject.GetParameters();
                    _constructParameters[inject] = parameters;
                }

                _injections.Clear();
                for (int j = 0; j < parameters.Length; j++) {
                    var parameter = parameters[j];
                    if (_applicableBindings.TryGetValue(parameter.ParameterType, out var bind) && bind != null) {
                        _injections.Add(bind);
                    }
                    // If the Construct method has a default value, and there's no binding available, use the
                    // default value
                    else if (parameter.HasDefaultValue) {
                        _injections.Add(parameter.DefaultValue);
                    }
                }

                if (_injections.Count == parameters.Length) {
                    inject.Invoke(target, _injections.ToArray());
                } else {
                    var missingTypes = parameters
                        .Select(p => p.ParameterType)
                        .Where(p => !_injections
                            .Any(injection => injection.GetType().IsAssignableFrom(p))
                        );
                    Debug.LogWarning(
                        $"Failed to satisfy the dependencies for {inject.DeclaringType}:{inject}\n" +
                        $"Missing bindings:\n\t{string.Join("\n\t", missingTypes)}"
                    );
                }
            }

            return target;
        }

        private GameObject Instantiate(GameObject original, Vector3 position, Quaternion rotation, Transform parent, params object[] additionalBindings) {
            var instance = Object.Instantiate(original, position, rotation, parent);
            instance.transform.ApplyBindings(this, additionalBindings);
            return instance;
        }
    }

    public delegate GameObject Instantiator(
        GameObject original,
        Vector3 position = new Vector3(),
        Quaternion rotation = new Quaternion(),
        Transform parent = null,
        params object[] additionalBindings
    );
}
