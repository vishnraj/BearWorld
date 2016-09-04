using UnityEngine;
using System.Collections;

namespace Utility
{
    public class Rotation
    {
        // Rotation to the current vector along x-z plane around y-axis
        // Angle is from positive z-axis
        public float CalculateXZRotation(Vector3 current_XZ_vector) {
            if (current_XZ_vector.x > 0)
                return Vector3.Angle(Vector3.forward, current_XZ_vector);
            else
                return -Vector3.Angle(Vector3.forward, current_XZ_vector);
        }

        // calculate the XZ rotation based on camera rotation and the custom angle passed in
        public float CalculateXZRotation(Vector3 current_XZ_vector, Vector3 custom_forward, bool forward_facing = true) {
            bool left = false;
            Vector3 z_axis = Vector3.forward;
            float xz_vector_angle_from_z;
            float forward_angle_from_z;

            // the forward facing camera
            if (custom_forward.z < 0) {
                z_axis = -z_axis;
            }

            if (forward_facing) 
            {
                if (current_XZ_vector.x >= 0)
                    xz_vector_angle_from_z = Vector3.Angle(z_axis, current_XZ_vector);
                else
                    xz_vector_angle_from_z = -Vector3.Angle(z_axis, current_XZ_vector);

                if (custom_forward.x >= 0)
                    forward_angle_from_z = Vector3.Angle(z_axis, custom_forward);
                else
                    forward_angle_from_z = -Vector3.Angle(z_axis, custom_forward);          
            }
            else 
            {
                if (current_XZ_vector.x >= 0)
                    xz_vector_angle_from_z = -Vector3.Angle(z_axis, current_XZ_vector);
                else
                    xz_vector_angle_from_z = Vector3.Angle(z_axis, current_XZ_vector);

                if (custom_forward.x >= 0)
                    forward_angle_from_z = -Vector3.Angle(z_axis, custom_forward);
                else
                    forward_angle_from_z = Vector3.Angle(z_axis, custom_forward);
            }

            if (forward_angle_from_z > xz_vector_angle_from_z)
                left = true;

            if (!left)
                return Vector3.Angle(custom_forward, current_XZ_vector);
            else
                return -Vector3.Angle(custom_forward, current_XZ_vector);
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

