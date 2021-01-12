using System;

namespace Banchou {
    public interface IStore<T> : IObservable<T>, IDisposable {
        void Dispatch(object action);
    }
}