using UnityEngine;

public class GeneticMovementTarget : GeneticMovement
{
    public bool targeting = true;
    public Vector3 target;// the target to aim for
    [Tooltip("Scaler for the target speed")]
    [SerializeField]
    public float targetSpeedScl = 1.5f; // sclr for the speed
}