using System;
using UniRx;

namespace Banchou {
    public abstract class Notifiable<T> where T : Notifiable<T> {
        public event Action<T> Changed;

        public virtual IObservable<T> Observe() {
            return Observable.FromEvent<T>(
                h => Changed += h,
                h => Changed -= h
            ).StartWith((T)this);
        }

        protected void Notify() {
            Changed?.Invoke((T)this);
        }
    }
}