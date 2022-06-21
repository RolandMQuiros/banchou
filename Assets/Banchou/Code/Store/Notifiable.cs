using System;
using System.Runtime.CompilerServices;
using MessagePack;
using UniRx;
using UnityEngine;

namespace Banchou {
    public record Notifiable<TNotifier> : IDisposable where TNotifier : Notifiable<TNotifier> {
        public event Action<TNotifier> Changed;
        [field:SerializeField, IgnoreMember] public string LastCaller { get; private set; }

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

        protected virtual TNotifier Notify([CallerMemberName]string caller = null) {
            LastCaller = caller;
            Changed?.Invoke((TNotifier)this);
            return (TNotifier)this;
        }

        public virtual void Dispose() {
            Changed = null;
        }
    }

    public abstract record NotifiableWithHistory<TNotifier> : Notifiable<TNotifier>, IRecordable<TNotifier>
        where TNotifier : Notifiable<TNotifier>, IRecordable<TNotifier> {

        protected readonly History<TNotifier> History;

        protected NotifiableWithHistory(int historyBufferSize) {
            History = new History<TNotifier>(historyBufferSize);
        }

        public abstract void Set(TNotifier other);

        public virtual TNotifier Rewind(float to) {
            if (History != null) {
                History.Rewind(to, out var frame);
                Set(frame);
                return Notify();
            }
            return this as TNotifier;
        }

        protected virtual TNotifier Notify(float when, [CallerMemberName] string caller = null) {
            if (History != null) {
                History.PushFrame(this as TNotifier, when);
            }
            return Notify(caller);
        }
    }
}