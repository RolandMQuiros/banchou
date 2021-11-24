using UnityEngine;

namespace Banchou.Utility {
    [RequireComponent(typeof(Animation))]
    public class PlayAnimation : MonoBehaviour {
        private Animation _animation;
        
        private void Awake() {
            _animation = GetComponent<Animation>();
        }

        public void PlayAdditional(object eventObject) {
            if (eventObject is AnimationClip clip) {
                _animation.clip = clip;
                _animation.Play();
            }
        }

        public void Play(string clipName) {
            _animation.Play(clipName);
        }
    }
}