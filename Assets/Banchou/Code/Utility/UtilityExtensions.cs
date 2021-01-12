using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Banchou {
    public static class UtilityExtensions {
        public static IEnumerable<Transform> BreadthFirstTraversal(this Transform root) {
            var queue = new Queue<Transform>();
            queue.Enqueue(root);

            while (queue.Count > 0) {
                var xform = queue.Dequeue();
                yield return xform;

                foreach (var child in xform) {
                    queue.Enqueue((Transform)child);
                }
            }
        }

        public static Vector3 Centroid(this IEnumerable<Vector3> positions) {
            return positions
                .Aggregate(Vector3.zero, (sum, position) => sum + position) / positions.Count();
        }

        public static IObservable<T> CatchIgnoreLog<T>(this IObservable<T> stream) {
            return stream.CatchIgnore((Exception error) => { Debug.LogException(error); });
        }

        public static IObservable<Unit> EnabledAsObservable(this MonoBehaviour component) {
            return component.OnEnableAsObservable()
                .Merge(component.OnDisableAsObservable())
                .Where(_ => component.isActiveAndEnabled);
        }

        public static IObservable<T> WhenBehaviourActive<T>(this IObservable<T> source, MonoBehaviour component) {
            return component.EnabledAsObservable()
                .Merge(component.OnDisableAsObservable())
                .Where(_ => component.isActiveAndEnabled)
                .SelectMany(_ => source);
        }

        public static byte[] ToByteArray<T>(this T obj) where T : struct {
            var size = Marshal.SizeOf(obj);
            var arr = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public static T ToObject<T>(this byte[] arr, ref T obj) where T : struct {
            var size = Marshal.SizeOf(obj);
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(arr, 0, ptr, size);
            obj = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);
            return obj;
        }
    }
}