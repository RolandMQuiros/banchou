using System;
using System.Collections;

namespace Banchou {
    public abstract class Substate<T> where T : Substate<T> {
        public event Action<T> Changed;
        protected abstract bool Consume(IList actions);
        public virtual void Process(IList actions) {
            if (Consume(actions) && Changed != null) {
                Changed((T)this);
            }
        }
    }
}