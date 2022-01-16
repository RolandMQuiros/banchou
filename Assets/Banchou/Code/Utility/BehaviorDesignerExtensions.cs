using System;
using BehaviorDesigner.Runtime.Tasks;
using UniRx;

namespace Banchou {
    public static class BehaviorDesignerExtensions {
        /// <summary>Dispose self on target Task has been destroyed. Return value is self disposable.</summary>
        public static T AddTo<T>(this T disposable, Task task) where T : IDisposable {
            if (task == null || task.Owner == null || task.Owner.gameObject == null) {
                disposable.Dispose();
                return disposable;
            }
            return disposable.AddTo(task.Owner.gameObject);
        }
    }
}