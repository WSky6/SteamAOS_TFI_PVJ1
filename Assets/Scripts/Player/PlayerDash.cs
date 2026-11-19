using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
	[RequireComponent(typeof(FirstPersonController))]
	[RequireComponent(typeof(StarterAssetsInputs))]
	public class PlayerDash : MonoBehaviour
	{
		[Header("Dash")]
		[Tooltip("Velocidad durante el dash en m/s")]
		public float DashSpeed = 20f;
		[Tooltip("Duración del dash en segundos (distancia = DashSpeed * DashDuration)")]
		public float DashDuration = 0.4f;
		[Tooltip("Tiempo de espera antes de poder volver a usar el dash")]
		public float DashCooldown = 2f;
		[Tooltip("Si es false, solo se puede hacer dash estando en el suelo")]
		public bool AllowAirDash = true;
		[Tooltip("Cantidad de dashes disponibles en el aire antes de tocar el suelo")]
		public int MaxAirDashes = 1;

		[Header("Input")]
		public InputAction DashAction;

		public bool IsDashing { get; private set; }

		private CharacterController _controller;
		private FirstPersonController _fpc;
		private StarterAssetsInputs _input;

		private float _nextDashTime;
		private int _airDashesLeft;

		private void Reset()
		{
			DashAction = CreateDefaultAction();
		}

		private static InputAction CreateDefaultAction()
		{
			var action = new InputAction("Dash", InputActionType.Button);
			action.AddBinding("<Keyboard>/leftCtrl");
			action.AddBinding("<Gamepad>/buttonEast");
			return action;
		}

		private void Awake()
		{
			if (DashAction == null || DashAction.bindings.Count == 0)
				DashAction = CreateDefaultAction();

			_controller = GetComponent<CharacterController>();
			_fpc = GetComponent<FirstPersonController>();
			_input = GetComponent<StarterAssetsInputs>();
			_airDashesLeft = MaxAirDashes;
		}

		private void OnEnable()
		{
			DashAction.Enable();
			DashAction.performed += OnDashPerformed;
		}

		private void OnDisable()
		{
			DashAction.performed -= OnDashPerformed;
			DashAction.Disable();

			if (IsDashing)
			{
				StopAllCoroutines();
				_fpc.SetDashing(false);
				IsDashing = false;
			}
		}

		private void Update()
		{
			if (_fpc.Grounded && !IsDashing)
			{
				_airDashesLeft = MaxAirDashes;
			}
		}

		private void OnDashPerformed(InputAction.CallbackContext ctx)
		{
			TryDash();
		}

		private void TryDash()
		{
			if (IsDashing || Time.time < _nextDashTime) return;

			if (!_fpc.Grounded)
			{
				if (!AllowAirDash || _airDashesLeft <= 0) return;
				_airDashesLeft--;
			}

			Vector3 dir = transform.right * _input.move.x + transform.forward * _input.move.y;
			if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
			dir.Normalize();

			StartCoroutine(DashRoutine(dir));
		}

		private IEnumerator DashRoutine(Vector3 direction)
		{
			IsDashing = true;
			_fpc.SetDashing(true);

			float elapsed = 0f;
			while (elapsed < DashDuration)
			{
				_controller.Move(direction * (DashSpeed * Time.deltaTime));
				elapsed += Time.deltaTime;
				yield return null;
			}

			_fpc.SetDashing(false);
			IsDashing = false;
			_nextDashTime = Time.time + DashCooldown;
		}
	}
}
