using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IController 
{
	// Identifications.
	void SetKeyMapping();

	// Get keys/axis.
	Vector2 GetDPad();
	Vector2 GetLeftJoystick();
	Vector2 GetRightJoystick();
	float GetL2();
	float GetR2();
	Vector2 GetMotion();
	bool GetButton(BaseController.eButtonId buttonId);
	bool GetButtonDown(BaseController.eButtonId buttonId);
	bool GetButtonUp(BaseController.eButtonId buttonId);
	Vector2 GetAxis(BaseController.eAxisId horizontalAxisId, BaseController.eAxisId verticalAxisId, float defaultValue = 0.0f);
	float GetAxis(BaseController.eAxisId axisId, float defaultValue = 0.0f);
}
