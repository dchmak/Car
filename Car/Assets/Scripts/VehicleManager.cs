/*
* Created by Daniel Mak
*/

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleManager : MonoBehaviour {

    [Header("Movement")]
    public float sensitivityDeadZone;
    public float groundDrag;
    public float airDrag;
    public float maxSpeed;
    public float hoverForce;
    public float gravityForce;
    public float hoverHeight;
    public float forwardAcceleration;
    public float backwardAcceleration;
    public float torque;
    public float thrustAirLimit;
    public float turnAirLimit;
    public float jumpForce;
    public GameObject[] hoverPoints;

    [Header("Camera")]
    public Transform cam;
    [Range(0f, 1f)] public float smoothSpeed;
    public Vector3 offset;

    private int layerMask;
    private float thrust, turn;
    private Rigidbody rb;

    private void Start () {
        rb = GetComponent<Rigidbody>(); // Get RigidBody component
        rb.centerOfMass = Vector3.down; // Lower the center of mass

        // Ignore Vehicle layer
        layerMask = 1 << LayerMask.NameToLayer("Vehicle");
        layerMask = ~layerMask;
	}
	
	private void Update () {
        // Main thrust
        thrust = 0f;
        float accel = Input.GetAxis("Vertical");
        if (accel > sensitivityDeadZone) thrust = accel * forwardAcceleration;
        else if (accel < -sensitivityDeadZone) thrust = accel * backwardAcceleration;

        // Turn
        turn = 0f;
        float turnAxis = Input.GetAxis("Horizontal");
        if (Mathf.Abs(turnAxis) > sensitivityDeadZone) turn = turnAxis * torque;
    }

    private void FixedUpdate() {
        RaycastHit hit;
        bool grounded = false;
        foreach (GameObject hoverPoint in hoverPoints) {
            Vector3 hoverPos = hoverPoint.transform.position;

            // Cast a downward ray from hover point to see if the vehicle is on ground.
            if (Physics.Raycast(hoverPos, Vector3.down, out hit, hoverHeight, layerMask)) {
                rb.AddForceAtPosition(Vector3.up * hoverForce * (1f - (hit.distance / hoverHeight)), hoverPos);

                grounded = true;
            } else { // if not on ground...
                // self-balance
                if (transform.position.y > hoverPos.y) {
                    rb.AddForceAtPosition(hoverPoint.transform.up * hoverForce, hoverPos);
                } else {
                    rb.AddForceAtPosition(-hoverPoint.transform.up * gravityForce, hoverPos);
                }
            }

            if (grounded) {
                rb.drag = groundDrag;
            } else {
                rb.drag = airDrag;
                thrust *= thrustAirLimit;
                turn *= turnAirLimit;
            }
        }

        // Jump
        if (Input.GetKeyUp(KeyCode.Space) && grounded) rb.velocity += transform.up * jumpForce;

        rb.AddForce(transform.forward * thrust);
        rb.AddRelativeTorque(Vector3.up * turn);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

        // Update camera position and rotation
        cam.localPosition = Vector3.Lerp(cam.position, offset, smoothSpeed);
        Quaternion rota = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        cam.rotation = Quaternion.Slerp(cam.rotation, rota, smoothSpeed);
    }
}