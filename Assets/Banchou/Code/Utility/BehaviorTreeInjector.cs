using Banchou.DependencyInjection;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace Banchou.Utility {
    [RequireComponent(typeof(Behavior))]
    public class BehaviorTreeInjector : MonoBehaviour {
        public void Construct(DiContainer container) {
            GetComponent<Behavior>()
                .FindTasks<Task>()
                .ForEach(task => {
                    container.Inject(task);
                });
        }
    }
}