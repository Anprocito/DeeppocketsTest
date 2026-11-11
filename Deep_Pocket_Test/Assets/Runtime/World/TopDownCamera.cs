using UnityEngine;

namespace DeepPocket.Runtime
{
    public class TopDownCamera : MonoBehaviour
    {
        public Transform target;
        [SerializeField] Vector3 offset = new Vector3(0f, 16f, -9f);
        [SerializeField] float smoothTime = 0.12f;

        Vector3 velocity;

        void Start() => transform.rotation = Quaternion.LookRotation(-offset, Vector3.up);

        void LateUpdate()
        {
            if (target == null)
            {
                PlayerHealth p = FindFirstObjectByType<PlayerHealth>();
                if (p == null) return;
                target = p.transform;
            }
            transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref velocity, smoothTime);
        }
    }
}
