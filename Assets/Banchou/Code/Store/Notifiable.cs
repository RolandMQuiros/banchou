using System;
using UniRx;

namespace Banchou {
    public abstract class Notifiable<TNotifier> where TNotifier : Notifiable<TNotifier> {
        public event Action<TNotifier> Changed;

        public virtual IObservable<TNotifier> Observe() {
            return Observable.FromEvent<TNotifier>(
                h => Changed += h,
                h => Changed -= h
            ).StartWith((TNotifier)this);
        }

        public virtual IObservable<T> OnChange<T>(T emit) {
            return Observe()
                .Select(_ => emit)
                .StartWith(emit);
        }

        protected void Notify() {
            Changed?.Invoke((TNotifier)this);
        }
    }
}