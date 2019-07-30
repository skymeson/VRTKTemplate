using System;
using UnityEngine;
using DoomtrinityFPSPrototype.Utils;

// Script from Unity Standard Assets
namespace UnityStandardAssets.Characters.FirstPerson
{
    [Serializable]
    public class MouseLook
    {
	// ========================================================================================================================
	// Instance variables

		// Sensitivity managed in menu settings.
		// public float XSensitivity = 2f;
        // public float YSensitivity = 2f;
        private float sensitivity;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth;
        public float smoothTime = 5f;
        public bool lockCursor = true;


        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private bool m_cursorIsLocked = true;


    // ========================================================================================================================
    // Init

        public void Init(Transform character, Transform camera, float _sensitivity)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
            sensitivity = _sensitivity;

			MessagingSystem.Instance.AttachListener (typeof(SettingsSavedMessage), this.ProcessSettingsSavedMessage);
        }

	// ========================================================================================================================
	// ProcessSettingsSavedMessage

		private bool ProcessSettingsSavedMessage(BaseMessage msg) {
			UpdateSensitivity ();
			return false;
		}

	// ========================================================================================================================
	// OnControllerKill

		public void OnControllerKill() {
			if (MessagingSystem.IsAlive) {
				MessagingSystem.Instance.DetachListener (typeof(SettingsSavedMessage), this.ProcessSettingsSavedMessage);
			}
		}

	// ========================================================================================================================
	// UpdateSensitivity

		private void UpdateSensitivity() {
			sensitivity = SaveLoadSettings.Instance.gameSettings.inputCfg.mouseSensitivity;
		}

    // ========================================================================================================================
    // LookRotation

        public void LookRotation(Transform character, Transform camera)
        {
            float yRot = Input.GetAxis("Mouse X") * sensitivity; // doomtrinity - sensitivity modified through input menu.
            float xRot = Input.GetAxis("Mouse Y") * sensitivity;

            m_CharacterTargetRot *= Quaternion.Euler (0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler (-xRot, 0f, 0f);

            if(clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis (m_CameraTargetRot);

            if(smooth)
            {
                character.localRotation = Quaternion.Slerp (character.localRotation, m_CharacterTargetRot,
                    smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp (camera.localRotation, m_CameraTargetRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }

            UpdateCursorLock();
        }

    // ========================================================================================================================
    // SetCursorLock

        public void SetCursorLock(bool value)
        {
            lockCursor = value;
            if(!lockCursor)
            {//we force unlock the cursor if the user disable the cursor locking helper
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

    // ========================================================================================================================
    // UpdateCursorLock

        public void UpdateCursorLock()
        {
            //if the user set "lockCursor" we check & properly lock the cursos
            if (lockCursor)
                InternalLockUpdate();
        }

    // ========================================================================================================================
    // InternalLockUpdate

        private void InternalLockUpdate()
        {
            if(Input.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
            }
            else if(Input.GetMouseButtonUp(0))
            {
                m_cursorIsLocked = true;
            }

            if (m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

    // ========================================================================================================================
    // ClampRotationAroundXAxis

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

            angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
	// ========================================================================================================================

    }
}
