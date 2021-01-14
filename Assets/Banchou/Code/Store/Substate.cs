using System;
using System.Collections;
using UniRx;

namespace Banchou {
    public abstract class Substate<T> where T : Substate<T> {
        public event Action<T> Changed;
        public virtual IObservable<T> Observe() {
            return Observable.FromEvent<T>(
                h => Changed += h,
                h => Changed -= h
            ).StartWith((T)this);
        }

        protected abstract bool Consume(IList actions);
        public virtual void Process(IList actions) {
            if (Consume(actions) && Changed != null) {
                Changed((T)this);
            }
        }
    }
}