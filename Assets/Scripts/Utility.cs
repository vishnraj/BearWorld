using UnityEngine;

namespace Utility
{
    public class Rotation
    {
        // Rotation to the current vector along x-z plane around y-axis
        // Angle is from positive z-axis
        public float CalculateZXRotation(Vector3 current_ZX_vector) {
            if (current_ZX_vector.x > 0)
                return Vector3.Angle(Vector3.forward, current_ZX_vector);
            else
                return -Vector3.Angle(Vector3.forward, current_ZX_vector);
        }

		// calculate the ZX rotation based on the custom vector passed in and the camera rotation
		public float CalculateZXRotation(Vector3 current_ZX_vector, Vector3 custom_forward) {
			float forward_angle_from_z;

			if (custom_forward.x >= 0)
				forward_angle_from_z = Vector3.Angle(Vector3.forward, custom_forward);
			else
				forward_angle_from_z = -Vector3.Angle(Vector3.forward, custom_forward);

			//custom_forward = Quaternion.Euler(0, forward_angle_from_z, 0) * custom_forward;
			Vector3 rotated_current_ZX_vector = Quaternion.Euler(0, -forward_angle_from_z, 0) * current_ZX_vector;

			if (rotated_current_ZX_vector.x >= 0)
				return Vector3.Angle(custom_forward, current_ZX_vector);
			else
				return -Vector3.Angle(custom_forward, current_ZX_vector);	
		}
			
		public float CalculateXZRotation(Vector3 current_XZ_vector) {
			if (current_XZ_vector.z > 0)
				return Vector3.Angle(Vector3.right, current_XZ_vector);
			else
				return -Vector3.Angle(Vector3.right, current_XZ_vector);
		}

        // Rotation to the current vector along the x-y plane around z-axis
        // Angle is from positive x-axis
        public float CalculateXYRotation(Vector3 current_XY_vector) {
            if (current_XY_vector.y > 0)
                return Vector3.Angle(Vector3.right, current_XY_vector);
            else
                return -Vector3.Angle(Vector3.right, current_XY_vector);
        }

        // Rotation to the current vector along the y-z plane around x-axis
        // Angle is from positive z-axis
        public float CalculateYZRotation(Vector3 current_YZ_vector) {
            if (current_YZ_vector.y > 0)
                return Vector3.Angle(Vector3.forward, current_YZ_vector);
            else
                return -Vector3.Angle(Vector3.forward, current_YZ_vector);
        }
    }

    public class Searching
    {
        public GameObject FindComponentUpHierarchy<T>(Transform child) {
            if (child.GetComponent<T>() != null)
                return child.gameObject;
            else if (child.parent != null)
                return FindComponentUpHierarchy<T>(child.parent);

            return null;
        }
    }
}

