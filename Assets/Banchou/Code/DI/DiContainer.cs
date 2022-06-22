﻿using System;
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

        private static readonly List<(Type Key, object Value)> _applicableBindings = new();
        private static readonly List<object> _injections = new();
        private static readonly Dictionary<MethodBase, ParameterInfo[]> _constructParameters = new();

        public T Inject<T>(T target) {
            if (target is DiContainer) {
                throw new Exception("Cannot inject into a DiContainer");
            }

            _applicableBindings.Clear();
            _applicableBindings.AddRange(
                _bindings
                    .Where(pair => pair.Value.Condition == null || pair.Value.Condition(target.GetType()))
                    .Select(pair => (pair.Key, Value: pair.Value.Instance))
            );

            if (_applicableBindings.Count == 0) {
                return target;
            }

            var targetType = target.GetType();

            var injectInfo = targetType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(inject => inject.Name == "Construct")
                .Cast<MethodBase>();

            foreach (var inject in injectInfo) {
                if (!_constructParameters.TryGetValue(inject, out var parameters)) {
                    parameters = inject.GetParameters();
                    _constructParameters[inject] = parameters;
                }
                
                var injectionPairs = parameters
                    // Left join against the parameter list
                    .GroupJoin(
                        inner: _applicableBindings,
                        outerKeySelector: parameter => parameter.ParameterType,
                        innerKeySelector: pair => pair.Key,
                        resultSelector: (parameter, containerPairs) => (
                            Parameter: parameter, Values: containerPairs.Select(pair => pair.Value).ToList()
                        )
                    )
                    .Select(pair => {
                        var (parameter, values) = pair;
                        // If the Construct method has a default value, and there's no binding available, use the default value
                        if (parameter.HasDefaultValue && !values.Any()) {
                            return (Parameter: parameter, Value: parameter.DefaultValue);
                        }
                        // Otherwise, grab the first value in the grouping. There should only be one, since the container is a Dictionary.
                        else {
                            return (Parameter: parameter, Value: values.FirstOrDefault());
                        }
                    });
                
                _injections.Clear();
                _injections.AddRange(
                    injectionPairs
                        // Filter out nulls, unless defined as a parameter's default value
                        .Where(pair => pair.Parameter.HasDefaultValue || pair.Value != null)
                        // Sift out the parameters
                        .Select(pair => pair.Value)
                );

                if (_injections.Count == parameters.Length) {
                    inject.Invoke(target, _injections.ToArray());
                } else {
                    var missingTypes = parameters
                        .Select(p => p.ParameterType)
                        .Where(m => !_injections
                            .Any(i => i.GetType().IsAssignableFrom(m))
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
