using System;
using System.Collections;

namespace Banchou {
    public abstract class Substate {
        public event Action Changed;
        protected abstract bool Consume(IList actions);
        public virtual void Process(IList actions) {
            if (Consume(actions) && Changed != null) {
                Changed();
            }
        }
    }
}