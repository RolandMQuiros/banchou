using System;
using UniRx;

namespace Banchou {
    public class Notifiable<TNotifier> where TNotifier : Notifiable<TNotifier> {
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

        protected virtual TNotifier Notify() {
            Changed?.Invoke((TNotifier)this);
            return (TNotifier)this;
        }
    }

    public abstract class NotifiableWithHistory<TNotifier> : Notifiable<TNotifier>, IRecordable<TNotifier>
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

        protected virtual TNotifier Notify(float when) {
            if (History != null) {
                History.PushFrame(this as TNotifier, when);
            }
            return Notify();
        }
    }
    
    public record NotifiableRecord<TNotifier> where TNotifier : NotifiableRecord<TNotifier> {
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

        protected virtual TNotifier Notify() {
            Changed?.Invoke((TNotifier)this);
            return (TNotifier)this;
        }
    }
    
    public abstract record NotifiableRecordWithHistory<TNotifier> : NotifiableRecord<TNotifier>, IRecordable<TNotifier>
        where TNotifier : NotifiableRecord<TNotifier>, IRecordable<TNotifier> {

        protected readonly History<TNotifier> History;

        protected NotifiableRecordWithHistory(int historyBufferSize) {
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

        protected virtual TNotifier Notify(float when) {
            if (History != null) {
                History.PushFrame(this as TNotifier, when);
            }
            return Notify();
        }
    }
}