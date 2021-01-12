using System;
using System.Collections;

namespace Banchou {
    public abstract class Substate {
        public event Action Changed;
        private bool _didChange = false;

        protected abstract bool Consume(IEnumerable actions);
        public virtual void Process(IEnumerable actions) {
            if (Consume(actions) && Changed != null) {
                Changed();
            }
        }
    }
}