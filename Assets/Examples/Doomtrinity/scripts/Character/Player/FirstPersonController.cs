using UnityEngine;
using UnityStandardAssets.Utility;
using System.Collections;
using DoomtrinityFPSPrototype.Character;
using DoomtrinityFPSPrototype.Audio;
using DoomtrinityFPSPrototype.Maps;
using DoomtrinityFPSPrototype.Utils;
using DoomtrinityFPSPrototype.InputManagement;
using Random = UnityEngine.Random;

// Script from Unity Standard Assets.
// Heavily modified, now supports ladder climbing and crouching.
namespace UnityStandardAssets.Characters.FirstPerson
{
	[RequireComponent(typeof (CharacterController))]
	[RequireComponent(typeof (AudioSource))]
	public class FirstPersonController : UpdateableMonoBehaviour
	{
		// ========================================================================================================================
		// Instance variables

		[SerializeField] private bool m_IsWalking;
		[SerializeField] private float m_WalkSpeed;
		[SerializeField] private float m_RunSpeed;
		[SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
		[SerializeField] private float m_JumpSpeed;
		[SerializeField] private float m_StickToGroundForce;
		[SerializeField] private float m_GravityMultiplier;
		[SerializeField] private MouseLook m_MouseLook;
		[SerializeField] private bool m_UseFovKick;
		[SerializeField] private FOVKick m_FovKick = new FOVKick();
		[SerializeField] private bool m_UseHeadBob;
		[SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
		[SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
		[SerializeField] private float m_StepInterval;
		[SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
		[SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
		[SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

		private Vector2 m_Input;
		private Vector3 m_MoveDir = Vector3.zero;
		private CharacterController m_CharacterController;
		private CollisionFlags m_CollisionFlags;
		private bool m_PreviouslyGrounded;
		private float m_StepCycle;
		private float m_NextStep;
		private AudioSource m_AudioSource;

		// _DT -->

		public bool rotateView { get; private set;}
		private bool isLevelEnd = false;

		[SerializeField] [Range(0f, 1.5f)] private float crouchHeight = 0.8f; // Do not change at runtime.
		private crouch_s crouch_param = new crouch_s (); // height, camera offset, etc. Used for crouch stuff.
		private camera_s fpcamera = new camera_s (); // main camera, local offset, transitions timer, etc. camera.

		private float character_previous_velocity = 0;

		// ladder stuff
		[SerializeField] private AudioClip[] ladderSounds;    // an array of ladder climb sounds that will be randomly selected from.
		private Transform currentLadderTransform = null;
		private bool lock_move = false;
		// player states
		private Action currentAction;
		private bool noclip = false;
		// _DT <--

		// ========================================================================================================================
		// Start

		protected override void Begin()
		{
			DevConsole.AddCvar (new Cvar<bool> ("noclip", Noclip, false, false, "Enable/Disable player clipping"));

			rotateView = true;
			Camera _camera =  Camera.main;
			m_CharacterController = GetComponent<CharacterController>();
			m_FovKick.Setup(_camera);
			m_HeadBob.Setup(_camera, m_StepInterval);
			m_StepCycle = 0f;
			m_NextStep = m_StepCycle/2f;
			m_AudioSource = GetComponent<AudioSource>();

			float m_mouseSensitivity = SaveLoadSettings.Instance.gameSettings.inputCfg.mouseSensitivity;
			m_MouseLook.Init(transform , _camera.transform, m_mouseSensitivity);

			// _DT -->

			m_AudioSource.volume = AudioManager.Instance.GetSfxVolume ();

			MessagingSystem.Instance.AttachListener (typeof(EscapeMessage), this.ProcessEscapeMessage);
			MessagingSystem.Instance.AttachListener (typeof(LevelEndMessage), this.ProcessLevelEndMessage);
			MessagingSystem.Instance.AttachListener (typeof(SettingsSavedMessage), this.ProcessSettingsSavedMessage);

			fpcamera.SetParam(_camera);
			InitCrouchParam();
			currentAction = Action.NONE;
			// _DT <--
		}

		// ========================================================================================================================
		// Update

		public override void Think (float dt) {

			Move ();

			if(!isLevelEnd) {
				if (rotateView) { RotateView(); } // doomtrinity - added toggle for pause screen.

				// Important! Any check of current vs previous value must be done after calling the controller 'Move'.
				if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
				{
					StartCoroutine(m_JumpBob.DoBobCycle());
					PlayLandingSound();
					m_MoveDir.y = 0f;
				}

				// fall down when go over the edge
				if(!noclip){
					if (!m_CharacterController.isGrounded && (currentAction != Action.JUMPING) && m_PreviouslyGrounded)
					{
						m_MoveDir.y = 0f;
					}
				}


				m_PreviouslyGrounded = m_CharacterController.isGrounded;
			}
		}

		// ========================================================================================================================
		// Move

		private void Move()
		{
			float speed = 0;
			Vector3 desiredMove = Vector3.zero;

			if (!isLevelEnd) 
			{
				GetInput(out speed);
			}

			if (lock_move) // locked
			{ 
				m_MoveDir = Vector3.zero;
			} 
			else if ( currentAction == Action.CLIMBING ) 
			{		
				desiredMove = transform.up * m_Input.y + transform.right * m_Input.x * 0.3f;

				if (m_CharacterController.isGrounded) 
				{
					// Detach from the ladder if moving backward.
					desiredMove += transform.forward * m_Input.y;
				} 

				m_MoveDir = desiredMove * speed;
				// bool up = m_Camera.transform.localRotation.x < 0;
			} 
			else
			{
				if (noclip) {
					Vector3 direction = new Vector3(m_Input.x,0.0f,m_Input.y);
					direction = direction * Time.deltaTime * speed;
					transform.Translate (direction,fpcamera.camera_obj.transform);
				} else {
					// always move along the camera forward as it is the direction that it being aimed at
					desiredMove = transform.forward*m_Input.y + transform.right*m_Input.x;
					// get a normal for the surface that is being touched to move along it
					RaycastHit hitInfo;
					Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
						m_CharacterController.height/2f, ~0, QueryTriggerInteraction.Ignore);

					desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

					m_MoveDir.x = desiredMove.x*speed;
					m_MoveDir.z = desiredMove.z*speed;

					if (m_CharacterController.isGrounded) {
						m_MoveDir.y = -m_StickToGroundForce;

						if (currentAction == Action.JUMPING) {
							m_MoveDir.y = m_JumpSpeed;
							PlayJumpSound ();
						}
					}
					else
					{
						m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.deltaTime;
					}

				}

			}
			if (!noclip) {
				m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.deltaTime);
			}

			ProgressStepCycle(speed);
			UpdateCameraPosition(speed);
			m_MouseLook.UpdateCursorLock();
		}

		// ========================================================================================================================
		// GetInput

		private void GetInput(out float speed)
		{
			// Quite redundant FSM.
			// Actions should never be set outside the switch statement ( except initialization ),
			// otherwise you will likely break the logic. 
			// If you follow this rule, adding new actions should be relatively simple.
			// I will probably move to a object-oriented ( State ) pattern, if needed.
			// Some actions will run only for the current frame ( e.g. jump ).
			switch (currentAction) {

			case Action.JUMPING: 
				currentAction = Action.NONE;

				break;
			case Action.CROUCHING: 

				if (!InputManager.GetKey (ActionCode.Crouch) && CanRise() ) { 
					currentAction = Action.UNCROUCHING;

				} else if ( CrouchToggle (true) ) { // timer end
					currentAction = Action.NONE;
				}

				break;
			case Action.UNCROUCHING: 

				if (InputManager.GetKey (ActionCode.Crouch)) {
					currentAction = Action.CROUCHING;

				} else if ( CrouchToggle (false) ) { // timer end
					currentAction = Action.NONE;
				}

				break;
			case Action.CLIMBING: 
				if ( !TowardLadder() ) {
					currentAction = Action.NONE;

				} else if ( InputManager.GetKeyDown(ActionCode.Jump) ) { // jump check
					currentAction = Action.JUMPING;
				}

				break;
			case Action.NONE: 
				if(!noclip){
					if ( InputManager.GetKeyDown(ActionCode.Jump) && !crouch_param.isCrouched ) { // jump check
						currentAction = Action.JUMPING;

					} else if ( InputManager.GetKey (ActionCode.Crouch) && !crouch_param.isCrouched ) { // crouch check
						currentAction = Action.CROUCHING;

					} else if ( !InputManager.GetKey (ActionCode.Crouch) && crouch_param.isCrouched && CanRise() ) { // crouch check
						currentAction = Action.UNCROUCHING;

					} else if ( TowardLadder() && !crouch_param.isCrouched ) { // ladder check
						currentAction = Action.CLIMBING;
					}
				}


				break;

			default : break;
			}

			// On standalone builds, walk/run speed is modified by a key press.
			// keep track of whether or not the character is walking or running
			m_IsWalking = !InputManager.GetKey(ActionCode.Run) || (crouch_param.isCrouched && m_CharacterController.isGrounded) || (currentAction == Action.CLIMBING);

			// set the desired speed to be walking or running
			speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
			m_Input = GetInputAxis ();

			// normalize input if it exceeds 1 in combined length:
			if (m_Input.sqrMagnitude > 1)
			{
				m_Input.Normalize();
			}
		}

		// ========================================================================================================================
		// GetInputAxis

		private Vector2 GetInputAxis() {
			// Read input
			float right = InputManager.GetKey(ActionCode.MoveRight)? 1:0;
			float left = InputManager.GetKey(ActionCode.MoveLeft)? 1:0;
			float forward= InputManager.GetKey(ActionCode.MoveForward)? 1:0;
			float backward = InputManager.GetKey(ActionCode.MoveBackward)? 1:0;
			return new Vector2 (right-left,forward-backward);
		}

		// ========================================================================================================================
		// CrouchToggle

		// This method is called each frame, when a crouch/uncrouch request comes in, until the state is reached.
		// You can think of it as a coroutine.
		// Take a look to the image in the __docs folder, which shows how crouch works.
		private bool CrouchToggle(bool isCrouching) {

			bool completed = false;

			float dist_to_crouch = crouch_param.height_default - crouch_param.height_crouch; // The distance the camera must travel between stand and crouch.
			float crouch_time = 0.1f; // Hardcoded, how much time to complete the translation.
			int dir = isCrouching ? -1 : 1;
			float speed = dist_to_crouch / crouch_time;
			float offset_to_ground = 0;
			float camera_offset_new = 0;

			// Check if the fp controller is changing direction - up to down or viceversa.
			// This is needed if we press/release the crouch button while the transition is not completed yet.
			if (crouch_param.camera_transition_completed > 0 && (crouch_param.wasCrouching != isCrouching) ) {
				crouch_param.camera_transition_completed = dist_to_crouch - crouch_param.camera_transition_completed;
			}

			Vector3 camera_pos = Vector3.zero; // The current position of the camera.
			Vector3 camera_pos_inc = Vector3.zero; // The movement step for this frame.

			// The code in the 'if' only moves the camera up/down, the collider capsule is still unchanged.
			// This is being excecuted until the camera reaches the desired crouched/uncrouched position.
			// For each frame, the camera performs a little step up/down.
			// You can think of it as the "coroutine" part of this method.
			if (crouch_param.camera_transition_completed < dist_to_crouch) 
			{
				crouch_param.camera_transition_completed += Time.deltaTime * speed; // The completed percentage of the translation.
				float pos_y_inc = Time.deltaTime * speed * dir; // The y movement step for this frame.
				camera_pos = fpcamera.tr.localPosition;
				camera_pos_inc = new Vector3 (camera_pos.x, camera_pos.y + pos_y_inc, camera_pos.z);
				fpcamera.tr.localPosition = camera_pos_inc; // Translate the camera.

			}
			// This is the tricky part.
			// If the camera has reached the desired position, the capsule collider of the controller must be set accordingly.
			// This involves 3 steps that must happen in the same frame: resize the collider, stick it to ground, restore the desired camera position.
			// 1 - Resize the collider: the collider needs to scale according to the desired camera position. It will no longer be sticked to ground;
			// 2 - Stick it to ground: move the collider down/up ( crouch/uncrouch) so its lower point touches the ground.
			// 3 - Restore camera: the previous operations compromise the desired position of the camera, so we must restore it.
			else 
			{
				crouch_param.camera_transition_completed = 0;
				completed = true;

				if (isCrouching) {

					crouch_param.isCrouched = true;

					// Need to check if the current height is the desired target height, 
					// since we may also fall here if we release the crouch button while the transition is not completed.
					// This would mean that the controller collider must not change, so we don't have to do anything  
					// ( the offset won't be calculated, so it is 0 ).
					// 'offset_to_ground' is the distance between the ground and the lower point of the collider.
					if (m_CharacterController.height == crouch_param.height_default) {
						offset_to_ground = ((crouch_param.height_default - crouch_param.height_crouch) / 2) * dir; // Related to part 2.
					}
					camera_offset_new = crouch_param.camera_offset_crouch; // Related to part 3.
					m_CharacterController.height = crouch_param.height_crouch; // Part 1.
					m_CharacterController.radius = crouch_param.radius_crouch; // Part 1.

				} else {

					crouch_param.isCrouched = false;

					// See above comment, this is the uncrouch code but the logic is the same.
					if (m_CharacterController.height == crouch_param.height_crouch) {
						offset_to_ground = ((crouch_param.height_default - crouch_param.height_crouch) / 2) * dir;
					}
					camera_offset_new = crouch_param.camera_offset_default;
					m_CharacterController.height = crouch_param.height_default;
					m_CharacterController.radius = crouch_param.radius_default;
				}

				// These two instructions need to be called in tandem. This compensates the changes we've made to 
				// camera offset and controller height and radius, "hacking" the camera so it keeps its position in world space.
				transform.position = new Vector3(transform.position.x, transform.position.y + offset_to_ground, transform.position.z); // Part 2.
				fpcamera.tr.localPosition = m_HeadBob.currentHeadBob(camera_offset_new); // Part 3.

				// Reset the camera position if the player is still now ( he could be moving during the transition ).
				// Needed to avoid possible camera "stuttering" when reaching the desired position.
				// This has nothing to do with crouch stuff. This reset is about the offset of the head bob.
				Vector2 vel = new Vector2 (m_CharacterController.velocity.x, m_CharacterController.velocity.z);
				if ( vel.sqrMagnitude == 0 ) {
					fpcamera.reset_camera = true;
				}
			}

			crouch_param.wasCrouching = isCrouching;
			return completed; // Return true if the translation is 100% completed ( reached the desired state ).
		}

		// ========================================================================================================================
		// TowardLadder

		// Requirements for ladder trigger:
		// - an empty gameobject with a box collider component, isTrigger selected;
		// - 'Ladder' tag, 'IgnoreRaycast' layer;
		// - must be perfectly vertical;
		// - its "horizontal" width should be the same of player width ( for example: player radius 0.5f --> trigger x and z should be 1.0f );
		// - its z axis must point toward the ladder;
		// - its height should be the same of the ladder model, but shifted a little bit up, let's say 0.2 units.
		// - it should be very close to the ladder model ( the mesh );
		// Take a look to test scene to see how triggers have been placed.
		private bool TowardLadder() {

			if( currentLadderTransform == null ) 
			{
				return false;
			}
			float maxDistanceFromLadder = 0.15f;
			Vector2 ladder_pos = new Vector2(currentLadderTransform.position.x,currentLadderTransform.position.z);
			Vector2 player_pos = new Vector2(transform.position.x,transform.position.z);
			float distanceFromLadder = Vector2.Distance (player_pos, ladder_pos);

			float player_orientation = transform.rotation.eulerAngles.y;
			float ladder_orientation = currentLadderTransform.rotation.eulerAngles.y;
			float player_delta_angle = Mathf.Abs( Mathf.DeltaAngle(player_orientation,ladder_orientation));

			if (player_delta_angle < 90 ) { // Can climb only if facing the ladder, otherwise fall down.
				if (distanceFromLadder <= maxDistanceFromLadder) {
					return true;
				}
			}
			return false;
		}

		// ========================================================================================================================
		// LadderInitClimb

		private IEnumerator LadderInitClimb() {

			float time_required = 0.2f;
			float current_time = 0;
			Vector3 pos1 = currentLadderTransform.TransformPoint(Vector3.back*0.3f); 
			pos1.y = transform.position.y;

			Vector3 ray_start_pos = transform.TransformPoint(Vector3.back*0.5f);
			bool isDownstairs = Physics.Raycast (ray_start_pos, Vector3.down, 1.0f);

			if (isDownstairs) 
			{
				yield break; // Continue only if the player is about to climb-down from the top of the ladder, or jumping on it.
			} 
			else 
			{
				lock_move = true;
			}
			while (current_time < time_required) {
				current_time += Time.deltaTime;
				float lerp = current_time / time_required;
				transform.position = Vector3.Lerp (transform.position, pos1, lerp); // Go backward a bit, so the player get detached from the current floor.
				yield return null;
			}

			float maxDistanceFromLadder = 0.15f;
			current_time = 0;
			Vector2 ladder_pos = new Vector2(currentLadderTransform.position.x,currentLadderTransform.position.z);
			Vector2 player_pos = new Vector2(transform.position.x,transform.position.z);
			float distanceFromLadder = Vector2.Distance (player_pos, ladder_pos);

			while (distanceFromLadder > maxDistanceFromLadder) {
				current_time += Time.deltaTime;
				if (current_time > 0.3f) { 
					// Safety check, not really care about translate through lerp, if the elapsed time is pretty high, then something went wrong.
					// This should happen quicky, if not, the player could no longer be attached to the ladder ( landed ), so this must be stopped.
					// A timed check in while loop would be better though...
					lock_move = false;
					yield break;
				}					
				player_pos = new Vector2(transform.position.x,transform.position.z);
				distanceFromLadder = Vector2.Distance (player_pos, ladder_pos);
				Vector3 tr = (Vector3.forward + Vector3.down*2).normalized;
				transform.Translate (tr*Time.deltaTime*2, currentLadderTransform); // Move the player toward the ladder so he can start climbing.
				yield return null;
			}
			lock_move = false;
			yield return null;
		}

		// ========================================================================================================================
		// UpdateCameraPosition

		// This method is needed if the player is using head bob.
		private void UpdateCameraPosition(float speed)
		{
			if ( fpcamera.reset_camera || fpcamera.isResetting) {
				fpcamera.reset_camera = false;
				ResetCamera ();
			}

			if (!m_UseHeadBob || currentAction != Action.NONE )
			{
				return;
			}

			Vector3 newCameraPosition;
			Vector2 vel;

			vel = new Vector2 (m_CharacterController.velocity.x, m_CharacterController.velocity.z);
			if (character_previous_velocity != vel.sqrMagnitude && vel.sqrMagnitude == 0 && m_JumpBob.Offset() == 0 ) {
				fpcamera.reset_camera = true;
				character_previous_velocity = vel.sqrMagnitude;
				return;
			}

			if (vel.sqrMagnitude > 0 && m_CharacterController.isGrounded) {

				float headbob = m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten));
				float offset = crouch_param.isCrouched ? crouch_param.camera_offset_crouch : crouch_param.camera_offset_default;

				fpcamera.tr.localPosition = m_HeadBob.DoHeadBob (headbob, offset);
				newCameraPosition = fpcamera.tr.localPosition;
				newCameraPosition.y = fpcamera.tr.localPosition.y - m_JumpBob.Offset ();
			} else {

				newCameraPosition = fpcamera.tr.localPosition;
				fpcamera.original_pos.y = crouch_param.isCrouched ? crouch_param.camera_offset_crouch : crouch_param.camera_offset_default;
				newCameraPosition.y = fpcamera.original_pos.y - m_JumpBob.Offset();
			} 
			fpcamera.tr.localPosition = newCameraPosition;
			character_previous_velocity = vel.sqrMagnitude;
		}

		// ========================================================================================================================
		// InitCrouchParam

		private void InitCrouchParam() {
			float crouch_radius = 0;
			float crouch_camera_offset = 0;

			// The character controller is a capsule. Its height cannot be less than radius*2 ( sphere ).
			// If you try to set its height lower than that from Inspector, you will see that its height won't change,
			// although 'm_CharacterController.height' stores that value - which is misleading...
			// If 'crouchHeight' ( the desired height in crouch pose ) doesn't fulfill this condition, then set the correct radius.
			if (crouchHeight / 2 < m_CharacterController.radius) {
				crouch_radius = crouchHeight / 2;
			} else {
				crouch_radius = m_CharacterController.radius;
			}

			// Set camera offset in crouch mode.
			crouch_camera_offset = fpcamera.tr.localPosition.y - ((m_CharacterController.height - crouchHeight) / 2);

			crouch_param.SetParam (m_CharacterController.height, crouchHeight, m_CharacterController.radius,
				crouch_radius, fpcamera.tr.localPosition.y, crouch_camera_offset);
		}

		// ========================================================================================================================
		// CanRise

		private bool CanRise() {
			RaycastHit hitInfo;
			float radius = crouch_param.radius_default;
			Vector3 origin = transform.position;
			origin.y = transform.position.y - m_CharacterController.height/2; // Lower point of the capsule, not taking into account skin width.
			// Note that Physics.SphereCast maxdistance refers to the center of the sphere being casted!
			// This means that the real maxdistance ( the higher point of the spherecast ) includes the radius of the sphere being casted.
			// To match the player's collider height ( the higher point of its capsule ), that radius must be subtracted.
			float max_distance = crouch_param.height_default - radius;
			bool canRise = !(Physics.SphereCast(origin, radius, Vector3.up, out hitInfo, max_distance, ~0, QueryTriggerInteraction.Ignore));

			return canRise;
		}

		// ========================================================================================================================
		// OnTriggerEnter

		private void OnTriggerEnter(Collider other) {
			if(other.CompareTag("Ladder")) 
			{
				currentLadderTransform = other.transform;
				StartCoroutine (LadderInitClimb ());
			}
		}

		// ========================================================================================================================
		// OnTriggerExit

		private void OnTriggerExit(Collider other) {
			if(other.CompareTag("Ladder")) 
			{				
				currentLadderTransform = null;
			}
		}

		// ========================================================================================================================
		// ResetCamera

		// This method will reset the camera to default position, from the current head bob position.
		// Like the 'CrouchToggle' method, you can think of it as a coroutine, so it will be called continuously for several frames.
		private void ResetCamera() {

			Vector3 default_pos = Vector3.zero;

			default_pos.y += crouch_param.isCrouched ? crouch_param.camera_offset_crouch : crouch_param.camera_offset_default;
			float speed = 10; // Hardcoded, meters per second. How fast move the camera to default position, from bob position.

			// Stop camera reset if changing pose ( stand --> crouch or vice-versa ).
			if (currentAction != Action.NONE) {
				fpcamera.reset_percent = 0;
				return;
			}

			if (fpcamera.reset_percent == 0) {
				fpcamera.camera_pos_old = fpcamera.tr.localPosition;
			}

			fpcamera.isResetting = true;

			if (fpcamera.reset_percent <= 1) {

				fpcamera.reset_percent += Time.deltaTime * speed;
				fpcamera.tr.localPosition = Vector3.Lerp (fpcamera.camera_pos_old, default_pos, fpcamera.reset_percent);
			} else {
				m_HeadBob.ResetCyclePos ();
				fpcamera.reset_percent = 0;
				fpcamera.isResetting = false;

			}
		}

		// ========================================================================================================================
		// RotateView

		private void RotateView()
		{
			m_MouseLook.LookRotation (transform, fpcamera.tr);
		}

		// ========================================================================================================================
		// ProcessEscapeMessage

		private bool ProcessEscapeMessage(BaseMessage msg) {
			OnRotateViewToggle ();
			return false;
		}

		// ========================================================================================================================
		// OnRotateViewToggle

		private void OnRotateViewToggle() {
			rotateView = !rotateView;
		}

		// ========================================================================================================================
		// PlayJumpSound

		private void PlayJumpSound()
		{
			m_AudioSource.clip = m_JumpSound;
			m_AudioSource.Play();
		}

		// ========================================================================================================================
		// ProgressStepCycle

		private void ProgressStepCycle(float speed)
		{
			float step_interval = ( currentAction == Action.CLIMBING ) ? 3.0f : m_StepInterval;

			if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
			{
				m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
					Time.deltaTime; // _DT - was FixedDeltaTime
			}

			if (!(m_StepCycle > m_NextStep))
			{
				return;
			}

			m_NextStep = m_StepCycle + step_interval;

			PlayFootStepAudio();
		}

		// ========================================================================================================================
		// PlayFootStepAudio

		private void PlayFootStepAudio()
		{
			AudioClip[] foot_sounds = null;
			if (!m_CharacterController.isGrounded && (currentAction != Action.CLIMBING))
			{
				return;
			}
			foot_sounds = (currentAction != Action.CLIMBING) ? m_FootstepSounds : ladderSounds;
			// pick & play a random footstep sound from the array,
			// excluding sound at index 0
			int n = Random.Range(1, foot_sounds.Length);
			m_AudioSource.clip = foot_sounds[n];
			m_AudioSource.PlayOneShot(m_AudioSource.clip);
			// move picked sound to index 0 so it's not picked next time
			foot_sounds[n] = foot_sounds[0];
			foot_sounds[0] = m_AudioSource.clip;
		}

		// ========================================================================================================================
		// PlayLandingSound

		private void PlayLandingSound() {
			m_AudioSource.clip = m_LandSound;
			m_AudioSource.Play();
			m_NextStep = m_StepCycle + .5f;			            
		}

		// ========================================================================================================================
		// OnControllerColliderHit

		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			Rigidbody body = hit.collider.attachedRigidbody;
			//dont move the rigidbody if the character is on top of it
			if (m_CollisionFlags == CollisionFlags.Below)
			{
				return;
			}

			if (body == null || body.isKinematic)
			{
				return;
			}
			body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
		}

		// ========================================================================================================================
		// Noclip

		private void Noclip(bool _noclip){
			noclip = _noclip;
			m_CharacterController.enabled = !_noclip;
		}

		// ========================================================================================================================
		// ProcessLevelEndMessage

		private bool ProcessLevelEndMessage(BaseMessage msg) {
			StartCoroutine(LockMovement());
			return false;
		}

		// ========================================================================================================================
		// ProcessSettingsSavedMessage

		private bool ProcessSettingsSavedMessage(BaseMessage msg) {
			m_AudioSource.volume = AudioManager.Instance.GetSfxVolume ();
			return false;
		}


		// ========================================================================================================================
		// LockMovement

		private IEnumerator LockMovement() {
			yield return new WaitForSeconds (2);
			isLevelEnd = true;
		}

		// ========================================================================================================================
		// OnDisable

		protected override void OnKill() {
			m_MouseLook.OnControllerKill ();
			if (MessagingSystem.IsAlive) {
				MessagingSystem.Instance.DetachListener (typeof(EscapeMessage), this.ProcessEscapeMessage);
				MessagingSystem.Instance.DetachListener (typeof(LevelEndMessage), this.ProcessLevelEndMessage);
				MessagingSystem.Instance.DetachListener (typeof(SettingsSavedMessage), this.ProcessSettingsSavedMessage);
			}
		}

		// ========================================================================================================================
		// OnDestroy

		private void OnDestroy() {
			DevConsole.RemoveCvar ("noclip");
		}

		// ========================================================================================================================

		public enum Action {
			NONE,
			JUMPING,
			CROUCHING,
			UNCROUCHING,
			CLIMBING
		}

		// ========================================================================================================================

		public struct camera_s {

			public Camera camera_obj;
			public Transform tr;
			public bool isResetting;
			public bool reset_camera;
			public float reset_percent;
			public Vector3 camera_pos_old;
			public Vector3 original_pos;

			public void SetParam(Camera _camera) {
				camera_obj = _camera;
				tr = camera_obj.transform;
				original_pos = tr.localPosition;
				isResetting = false;
				reset_camera = false;
				reset_percent = 0;
				camera_pos_old = Vector3.zero;
			}
		}

		// ========================================================================================================================

		public struct crouch_s {

			// "Static" params
			public float height_default;
			public float height_crouch;
			public float radius_default;
			public float radius_crouch;
			public float camera_offset_default;
			public float camera_offset_crouch;

			// "Dynamic" params
			public float camera_transition_completed;
			public bool wasCrouching;
			public bool isCrouched;

			public void SetParam(float height_def,float height_cr,float radius_def,float radius_cr,float camera_offset_def,float camera_offset_cr) {
				height_default = height_def;
				height_crouch = height_cr;
				radius_default = radius_def;
				radius_crouch = radius_cr;
				camera_offset_default = camera_offset_def;
				camera_offset_crouch = camera_offset_cr;

				camera_transition_completed = 0;
				wasCrouching = false;
				isCrouched = false;
			}
		}

		// ========================================================================================================================
	}
}
