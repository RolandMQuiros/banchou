using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace Banchou.DependencyInjection {
    public sealed class DiContainer {
        private class Binding {
            public object Instance = null;
            public Func<Type, bool> Condition = null;
        }

        private Dictionary<Type, Binding> _bindings = new Dictionary<Type, Binding>();

        public DiContainer(params object[] bindings) {
            Bind(this);
            Bind<Instantiator>(Instantiate);

            foreach (var binding in bindings) {
                _bindings[binding.GetType()] = new Binding {
                    Instance = binding
                };
            }
        }

        public DiContainer(in DiContainer prev, params object[] bindings) {
            var prevBindings = prev._bindings.Values.Concat(bindings);

            foreach (var binding in prevBindings) {
                _bindings[binding.GetType()] = new Binding {
                    Instance = binding
                };
            }

            Bind(this);
            Bind<Instantiator>(Instantiate);
        }

        public DiContainer Bind<T>(T instance) {
            _bindings[typeof(T)] = new Binding { Instance = instance };
            return this;
        }

        public DiContainer Bind<T>(T instance, Func<Type, bool> where) {
            _bindings[typeof(T)] = new Binding {
                Instance = instance,
                Condition = where
            };
            return this;
        }

        public T Inject<T>(T target) {
            if (target.GetType() == typeof(DiContainer)) {
                throw new Exception("Cannot inject into a DiContainer");
            }

            var applicableBindings = _bindings
                .Where(pair => pair.Value.Condition == null || pair.Value.Condition(target.GetType()))
                .Select(pair => (Key: pair.Key, Value: pair.Value.Instance));

            if (!applicableBindings.Any()) {
                return target;
            }

            var targetType = target.GetType();

            var injectInfo = targetType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(inject => inject.Name == "Construct")
                .Cast<MethodBase>();

            foreach (var inject in injectInfo) {
                var parameters = inject.GetParameters();
                var injectionPairs = parameters
                    // Left join against the parameter list
                    .GroupJoin(
                        inner: applicableBindings,
                        outerKeySelector: parameter => parameter.ParameterType,
                        innerKeySelector: pair => pair.Key,
                        resultSelector: (parameter, containerPairs) => (
                            Parameter: parameter, Values: containerPairs.Select(pair => pair.Value)
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

                var injections = injectionPairs
                    // Filter out nulls, unless defined as a parameter's default value
                    .Where(pair => pair.Parameter.HasDefaultValue || pair.Value != null)
                    // Sift out the parameters
                    .Select(pair => pair.Value)
                    .ToArray();

                if (injections.Length == parameters.Length) {
                    inject.Invoke(target, injections);
                } else {
                    var missingTypes = parameters
                        .Select(p => p.ParameterType)
                        .Where(m => !injections
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
            var instance = GameObject.Instantiate(original, position, rotation, parent);
            instance.transform.ApplyBindings(additionalBindings);
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

    public static class InjectionExtensions {
        /// <summary>
        /// Injects dependencies into every <see cref="Component"/> in this <see cref="Transform"/>'s hierarchy
        /// </summary>
        /// <param name="transform">The root Transform of the subtree</param>
        /// <param name="additionalBindings">Bindings not necessarily included in the hierarchy contexts</param>
        public static void ApplyBindings(this Transform transform, params object[] additionalBindings) {
            foreach (var xform in transform.BreadthFirstTraversal()) {
                var contexts = xform.FindContexts();
                var components = xform.gameObject
                    .GetComponents<Component>()
                    .SelectMany(c => Expand(c))
                    // Handle contexts first
                    .OrderBy(c => c is IContext ? 0 : 1);

                foreach (var component in components) {
                    try {
                        contexts.TakeWhile(context => context != component)
                            .ToDiContainer(additionalBindings)
                            .Inject(component);
                    } catch (Exception error) {
                        Debug.LogException(error);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the <see cref="IContext"/>s available to the current <see cref="Transform"/>, based on its position in
        /// the scene hierarchy.
        /// </summary>
        /// <param name="transform">The target <see cref="Transform"/></param>
        /// <returns>A collection of <see cref="IContext"/>s in order of closest in the scene tree, to the root</returns>
        public static IList<IContext> FindContexts(this Transform transform) {
            var climb = transform;
            var stack = new Stack<IContext>();
            while (climb != null) {
                // Traverse the hierarchy from bottom to top, while traversing each gameObject's contexts from top to bottom
                // This lets multiple contexts on a single gameObject depend on each other in a predictable way
                var contexts = climb.GetComponents<IContext>().Reverse();
                var allcomponents = climb.GetComponents<MonoBehaviour>();
                foreach (var context in contexts) {
                    stack.Push(context);
                }
                climb = climb.parent;
            }
            return stack.ToList();
        }

        /// <summary>
        /// Creates a new <see cref="DiContainer"/> from an ordered collection of <see cref="IContexts"/>
        /// </summary>
        /// <param name="contexts">An ordered collection of <see cref="IContext"/>s</param>
        /// <returns>A <see cref="DiContainer"/> build from contexts</returns>
        public static DiContainer ToDiContainer(this IEnumerable<IContext> contexts, params object[] additionalBindings) {
            var container = new DiContainer(additionalBindings);
            foreach (var context in contexts) {
                container = context.InstallBindings(container);
            }
            return container;
        }

        /// <summary>
        /// Expands a <see cref="Component"/> into subobjects that can receive dependency injections
        /// </summary>
        /// <param name="component">The target <see cref="Component"/></param>
        /// <returns>An unordered collection of subobjects</returns>
        public static IEnumerable<object> Expand(this Component component) {
            yield return component;

            var animator = component as Animator;
            if (animator != null) {
                foreach (var behaviour in animator.GetBehaviours<StateMachineBehaviour>()) {
                    yield return behaviour;
                }
            }
        }
    }
}
