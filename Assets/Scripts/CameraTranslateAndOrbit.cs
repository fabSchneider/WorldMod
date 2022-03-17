using UnityEngine;
using UnityEngine.InputSystem;

namespace Fab.Geo
{
    /// <summary>
    /// A simple camera orbit script that works with Unity's new Input System.
    /// </summary>
    public class CameraTranslateAndOrbit : MonoBehaviour
    {
        [SerializeField]
        private PlayerInput playerInput;

        [Header("Orbit")]
        [SerializeField]
        private float orbitSpeed = 0.4f;
        [SerializeField]
        private float orbitLag = 4f;

        [Header("Move")]
        [SerializeField]
        private float moveSpeed = 0.01f;
        [SerializeField]
        private float moveLag = 4f;
        [SerializeField]
        Bounds moveBounds;

        [Header("Zoom")]
        [SerializeField]
        public float startZoom = 1.5f;
        [SerializeField]
        private float minZoom = 1f;
        [SerializeField]
        private float maxZoom = 20f;
        [SerializeField]
        private float zoomSpeed = 1f;
        [SerializeField]
        private float zoomLag = 4f;

        private float targetZoom;

        private InputActionAsset actions;

        private InputAction deltaAction;
        private InputAction panAction;

        private InputAction zoomAction;
        private InputAction orbitAction;

        private Vector3 focusPoint;
        private float zoomCurrent;

        private Vector3 targetMove;
        private Vector3 positionOffsetCurrent;

        private Quaternion targetRot;

        private bool pan;
        private bool orbit;

        private void Start()
        {
            actions = playerInput.actions;

            deltaAction = actions.FindAction("Player/Delta");
            panAction = actions.FindAction("Player/Pan");
            zoomAction = actions.FindAction("Player/Zoom");
            orbitAction = actions.FindAction("Player/Orbit");

            BindActions();

            targetZoom = startZoom;

            focusPoint = transform.forward * -targetZoom;
            positionOffsetCurrent = targetMove;
            zoomCurrent = targetZoom;
            targetRot = transform.rotation;
        }

        private void OnEnable()
        {
            if (actions)
                BindActions();
        }

        private void OnDisable()
        {
            if (actions)
                UnbindActions();
        }

        private void BindActions()
        {
            deltaAction.performed += OnDelta;
            panAction.performed += OnPan;
            zoomAction.performed += OnZoom;
            orbitAction.performed += OnOrbit;
        }

        private void UnbindActions()
        {
            deltaAction.performed -= OnDelta;
            panAction.performed -= OnPan;
            zoomAction.performed -= OnZoom;
            orbitAction.performed -= OnOrbit;
        }

        private void OnDelta(InputAction.CallbackContext ctx)
        {
            if (orbit)
            {
                Vector2 delta = ctx.ReadValue<Vector2>() * Time.deltaTime;
                Move(delta);
            }
            else if (pan)
            {
                Vector2 delta = ctx.ReadValue<Vector2>() * Time.deltaTime;
                Orbit(delta);
            }
        }

        public void Move(Vector2 delta)
        {

            targetMove += transform.up.normalized * (delta.y * -(targetZoom * moveSpeed));
            targetMove += transform.right.normalized * (delta.x * -(targetZoom * moveSpeed));

            if (!moveBounds.Contains(targetMove))
            {
                targetMove = moveBounds.ClosestPoint(targetMove);
                Orbit(delta);
            }
        }

        public void Orbit(Vector2 delta)
        {
            targetRot = Quaternion.AngleAxis(delta.x * orbitSpeed, Vector3.up) * targetRot;
            targetRot = Quaternion.AngleAxis(delta.y * -orbitSpeed, targetRot * Vector3.right) * targetRot;
        }

        public void Zoom(float delta)
        {
            targetZoom += delta * zoomSpeed * -targetZoom * Time.deltaTime;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        private void Update()
        {
            if (!playerInput.inputIsActive)
            {
                orbit = false;
                pan = false;
            }
            UpdateTransform();
        }

        private void OnOrbit(InputAction.CallbackContext ctx)
        {
            orbit = ctx.ReadValueAsButton();
        }

        private void OnPan(InputAction.CallbackContext ctx)
        {
            pan = ctx.ReadValueAsButton();
        }

        private void OnZoom(InputAction.CallbackContext ctx)
        {
            float scroll = ctx.ReadValue<Vector2>().y;
            Zoom(scroll);
        }

        private void UpdateTransform()
        {
            Quaternion rot = transform.rotation;
            rot = Quaternion.Slerp(rot, targetRot, Time.deltaTime / orbitLag);
            zoomCurrent = Mathf.Lerp(zoomCurrent, targetZoom, Time.deltaTime / zoomLag);

            focusPoint = rot * Vector3.forward * -zoomCurrent;
            positionOffsetCurrent = Vector3.Lerp(positionOffsetCurrent, targetMove, Time.deltaTime / moveLag);
            Vector3 focusWorldPoint = transform.position - focusPoint;
            Vector3 pos = focusPoint + positionOffsetCurrent;

            transform.SetPositionAndRotation(pos, rot);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(transform.position - focusPoint, 0.5f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetMove, 0.5f);
            Gizmos.DrawWireCube(moveBounds.center, moveBounds.size);
        }
    }
}
