using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Banchou.DependencyInjection {
    public static class DiExtensions {
        /// <summary>
        /// Injects dependencies into every <see cref="Component"/> in this <see cref="Transform"/>'s hierarchy
        /// </summary>
        /// <param name="transform">The root Transform of the subtree</param>
        /// <param name="additionalBindings">Bindings not necessarily included in the hierarchy contexts</param>
        public static void ApplyBindings(this Transform transform, params object[] additionalBindings) {
            foreach (var xform in transform.BreadthFirstTraversal()) {
                var contexts = xform.FindContexts();
                var components = xform.GetInjectees();
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
        /// Injects dependencies into every <see cref="Component"/> in this <see cref="Transform"/>'s hierarchy
        /// </summary>
        /// <param name="transform">The root Transform of the subtree</param>
        /// <param name="baseContainer">The base <see cref="DiContainer"/> to include in all injections</param>
        /// <param name="additionalBindings">Bindings not necessarily included in the hierarchy contexts</param>
        public static void ApplyBindings(this Transform transform, DiContainer baseContainer, params object[] additionalBindings) {
            foreach (var xform in transform.BreadthFirstTraversal()) {
                var contexts = xform.FindContexts();
                var components = xform.GetInjectees();
                foreach (var component in components) {
                    try {
                        contexts.TakeWhile(context => context != component)
                            .ToDiContainer(baseContainer, additionalBindings)
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
                foreach (var context in contexts) {
                    stack.Push(context);
                }
                climb = climb.parent;
            }
            return stack.ToList();
        }
        
        

        /// <summary>
        /// Creates a new <see cref="DiContainer"/> from an ordered collection of <see cref="IContext"/>s
        /// </summary>
        /// <param name="contexts">An ordered collection of <see cref="IContext"/>s</param>
        /// <param name="additionalBindings">
        /// Additional objects to bind to the resulting <see cref="DiContainer"/>
        /// </param>
        /// <returns>A <see cref="DiContainer"/> built from contexts</returns>
        public static DiContainer ToDiContainer(
            this IEnumerable<IContext> contexts,
            params object[] additionalBindings
        ) {
            var container = new DiContainer(additionalBindings);
            foreach (var context in contexts) {
                container = context.InstallBindings(container);
            }
            return container;
        }
        
        /// <summary>
        /// Creates a new <see cref="DiContainer"/> from an ordered collection of <see cref="IContext"/>s
        /// </summary>
        /// <param name="contexts">An ordered collection of <see cref="IContext"/>s</param>
        /// <param name="baseContainer">Another <see cref="DiContainer"/> to base the resulting <c>DiContainer</c> from </param>
        /// <param name="additionalBindings">
        /// Additional objects to bind to the resulting <see cref="DiContainer"/>
        /// </param>
        /// <returns>A <see cref="DiContainer"/> built from contexts</returns>
        public static DiContainer ToDiContainer(
            this IEnumerable<IContext> contexts,
            DiContainer baseContainer,
            params object[] additionalBindings
        ) {
            var container = new DiContainer(baseContainer, additionalBindings);
            foreach (var context in contexts) {
                container = context.InstallBindings(container);
            }
            return container;
        }

        /// <summary>
        /// Retrieves an ordered collection of <see cref="Transform"/> descendants that can receive dependency injection.
        /// Prioritizes implementations of <see cref="IContext"/> first.
        /// </summary>
        /// <param name="transform">
        /// The root transform of the <see cref="GameObject"/> tree to search for injectees
        /// </param>
        /// <returns>An ordered collection of eligible injectee objects</returns>
        private static IEnumerable<object> GetInjectees(this Transform transform) {
            return transform.gameObject
                .GetComponents<Component>()
                .Select((component, index) => (component, index))
                // Handle contexts first
                .OrderBy(args => args.component is IContext ? 0 : 1)
                    // Maintain component order on the GameObject
                    .ThenBy(args => args.index)
                .Select(args => args.component);
        }
    }
}