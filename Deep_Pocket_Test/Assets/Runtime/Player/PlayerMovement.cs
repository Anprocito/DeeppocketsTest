using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepPocket.Runtime
{
    using DeepPocket.Data;

    // Rigidbody + Input System nuevo. Sin gravedad (Y congelada). La velocidad sale de PlayerStats.
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [Tooltip("Se usa solo si no hay PlayerStats.")]
        [SerializeField] float fallbackSpeed = 5f;

        Rigidbody rb;
        PlayerStats stats;
        InputAction moveAction;
        Vector2 moveInput;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            moveAction.AddBinding("<Gamepad>/leftStick");
        }

        void Start() => stats = GetComponent<PlayerStats>();

        void OnEnable() => moveAction.Enable();
        void OnDisable() => moveAction.Disable();
        void OnDestroy() => moveAction.Dispose();

        void Update() => moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

        void FixedUpdate()
        {
            float speed = stats != null ? stats.Get(StatType.MoveSpeed) : fallbackSpeed;
            Vector3 velocity = new Vector3(moveInput.x, 0f, moveInput.y) * speed;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = velocity;
#else
            rb.velocity = velocity;
#endif
        }
    }
}
