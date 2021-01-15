using System;
using MessagePack;
using UniRx;

namespace Banchou {
    public abstract class Substate<T> where T : Substate<T> {
        [IgnoreMember] public bool Notified { get; private set; }
        [IgnoreMember] public ulong ProcessCount { get; private set; }
        public event Action<T> Changed;
        public virtual IObservable<T> Observe() {
            return Observable.FromEvent<T>(
                h => Changed += h,
                h => Changed -= h
            ).StartWith((T)this);
        }

        // Call the Process methods of child states
        protected virtual void OnProcess() {  }

        public void Process() {
            ProcessCount++;
            OnProcess();
            if (Notified) {
                Changed?.Invoke((T)this);
                Notified = false;
            }
        }

        protected void Notify() {
            Notified = true;
        }
    }
}