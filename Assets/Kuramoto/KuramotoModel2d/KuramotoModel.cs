using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class KuramotoModel : MonoBehaviour {

    public const string PROP_PARTICLE_MODEL_MATRIX = "_ParticleModelMatrices";
    public const string PROP_PHASES = "_Phases";
    public const string PROP_COLOR_HIGHLIGHT = "_Highlight";
    public const string PROP_COLOR_SHADOW = "_Shadow";
    public const string PROP_COH_PHI = "_CohPhi";

    public const float CIRCLE_IN_RADIAN = 2f * Mathf.PI;
    public const float RADIAN_TO_NORMALIZED = 1f / CIRCLE_IN_RADIAN;

    [SerializeField] protected Material mat;
    [SerializeField] protected int nOnLine = 100;
    [Range(0.01f, 10f)]
    [SerializeField] protected float sizeGain = 10f;

    [Range(0f, 5f)]
    [SerializeField] protected float coupling = 0f;
    [Range(1, 10)]
    [SerializeField] protected int couplingRange = 1;
    [Range(0f, 1f)]
    [SerializeField] protected float speed = 1f;
    [Range(0f, 1f)]
    [SerializeField]
    protected float speedVariation = 0.1f;
    [Range(0f, 10f)]
    [SerializeField] protected float noise = 0.1f;

    [SerializeField] protected Vector3 shadowColorHsvOffset;

    bool invalid;
    Rect windowRect;

    Matrix4x4[] particleModelMatrices;
    ComputeBuffer particleModelMatricesBuffer;

    float[] phases;
    ComputeBuffer phasesBuffer;
    
    float[] speeds;
    float[] coherencePhis;
    float[] coherenceRadiuses;
    ComputeBuffer coherencePhaseBuffer;

    #region Unity
    private void OnEnable() {
        invalid = true;
    }
    private void Update() {
        //reset if values change
        if (invalid) {
            invalid = false;
            Reset();
        }
        // call main coherence function
        Coherence();
        // get the time between this frame and the last
        var dt = Time.deltaTime;
        //loop over patricle phases
        for (var i = 0; i < phases.Length; i++) {
            //get the phase
            var p = phases[i];
            // get the its current angle to positive x axis
            var cphi = coherencePhis[i];
            // Get its distance to 0
            var crad = coherenceRadiuses[i];
            // get the noise
            float thisNoise = noise * Noise();
            // not sure but im guessing the main distance to phase function sin((angleDistToX-phase) * (2*Pi))
            float calc = Mathf.Sin((cphi - p) * CIRCLE_IN_RADIAN);
            // second phase (scaler * distTo0 * lastClac)
            calc = coupling * crad * calc;
            // add the speed noise and calc together and times by delta time, and add to P
            p += dt * (speeds[i]+ thisNoise + calc);
            // subtract its intiger to leave it as a 0. someting
            p -= (int)p;
            // set the new phase value
            phases[i] = p;
        }
        // set the phase data computebuffer
        phasesBuffer.SetData(phases);
    }
    // Gui Listner
    private void OnGUI() {
        windowRect = GUILayout.Window(GetInstanceID(), windowRect, WindowFunc, name,
            GUILayout.MinWidth(200f));
    }
    // when rendered
    private void OnRenderObject() {
        // connect up the compute buffers and set values
        mat.SetBuffer(PROP_PARTICLE_MODEL_MATRIX, particleModelMatricesBuffer);
        mat.SetBuffer(PROP_PHASES, phasesBuffer);
        mat.SetBuffer(PROP_COH_PHI, coherencePhaseBuffer);
        // set the render layer it is on to 0
        mat.SetPass(0);
        // get the num particles
        var n = nOnLine * nOnLine;
        // add procedural mesh to draw the shader on
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, n);
    }
    private void OnValidate() {
        invalid = true;
    }
    private void OnDisable() {
        Release();
    }
#endregion
    // setup GUI
    protected void WindowFunc(int id) {
        GUILayout.BeginVertical();

        GUILayout.Label(string.Format("Coherence : {0}", coupling));
        GUILayout.BeginHorizontal();
        coupling = GUILayout.HorizontalSlider(coupling, 0f, 10f);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUI.DragWindow();
    }
    // random value between -1 to 1
    protected float Noise() {
        return 2f * Random.value - 1f;
    }
    // Disposes GPU Computes buffers
    protected void Release() {
        if (particleModelMatricesBuffer != null) {
            particleModelMatricesBuffer.Dispose();
            particleModelMatricesBuffer = null;
        }
        if (phasesBuffer != null) {
            phasesBuffer.Dispose();
            phasesBuffer = null;
        }
        if (coherencePhaseBuffer != null) {
            coherencePhaseBuffer.Dispose();
            coherencePhaseBuffer = null;
        }
    }
    // Resets the simulation
    protected void Reset() {
        // release the last shader buffer data
        Release();
        // workout the size of the array
        var n = nOnLine * nOnLine;
        // set the size of each particle dynamicly
        var size = sizeGain / nOnLine;
        //  set to a new matrix
        particleModelMatrices = new Matrix4x4[n];
        // set to a new compute buffer for the matrix n long, each size of the matrix
        particleModelMatricesBuffer = new ComputeBuffer(n, Marshal.SizeOf(typeof(Matrix4x4)));
        // set to a new float array n long
        phases = new float[n];
        // create a new compute buffer n long, each size of float
        phasesBuffer = new ComputeBuffer(n, Marshal.SizeOf(typeof(float)));

        // set to a new float array n long
        speeds = new float[n];
        // set to a new float array n long
        coherencePhis = new float[n];
        // set to a new float array n long
        coherenceRadiuses = new float[n];
        // create a new compute buffer n long, each size of float
        coherencePhaseBuffer = new ComputeBuffer(n, Marshal.SizeOf(typeof(float)));

        // create a vec3 called offset 
        var offset = new Vector3(-0.5f * nOnLine * size, -0.5f * nOnLine * size, 0f);
        // double loop over the list
        for (var y = 0; y < nOnLine; y++) {
            for (var x = 0; x < nOnLine; x++) {
                // get index in the list
                var i = x + y * nOnLine;
                // set the position of the particle 
                var pos = new Vector3(x * size, y * size, 0f) + offset;
                // create transfor Rotattion and Scaling matrix from ( pos, no rotation, vec3 size)
                var m = Matrix4x4.TRS(pos, Quaternion.identity, size * Vector3.one);
                // set the particles TRS matrix in the list
                particleModelMatrices[i] = m;
            }
        }
        // push the data to the shader buffer
        particleModelMatricesBuffer.SetData(particleModelMatrices);

        // loop over and randomise the particles phases
        for (var i = 0; i < phases.Length; i++) {
            // randomise phase
            phases[i] = Random.value;
            // randomise speed negative to positive
            speeds[i] = speed * Random.Range(1f - speedVariation, 1f + speedVariation);
        }
        // set compute buffer from phases list
        phasesBuffer.SetData(phases);
    }
    // main comparing function 
    protected void Coherence() {
        //itterate of texturx,y
        for (var y = 0; y < nOnLine; y++) {
            for (var x = 0; x < nOnLine; x++) {
                // find its index
                var i = x + y * nOnLine;

                // variables to hold oscilation totals
                var sumx = 0f;
                var sumy = 0f;
                //patch over (loop in a square) the neighbours
                for (var h = -couplingRange; h <= couplingRange; h++) {
                    for (var w = -couplingRange; w <= couplingRange; w++) {
                        // find neighbour pos
                        var y1 = y + h;
                        var x1 = x + w;
                        //if this pos or outside the grid skip
                        if ((x1 == x && y1 == y) 
                            || x1 < 0 || nOnLine <= x1 
                            || y1 < 0 || nOnLine <= y1)
                            continue;
                        // get index on texture
                        var j = (x + w) + (y + h) * nOnLine;
                        // times the points value by 2*Pi
                        var theta = phases[j] * CIRCLE_IN_RADIAN;
                        // add to oscilation values to sums (total) 
                        sumx += Mathf.Cos(theta);
                        sumy += Mathf.Sin(theta);
                    }
                }
                // get the number of values taken
                var bandwidth = 2 * couplingRange + 1;
                var counter = bandwidth * bandwidth - 1;
                // average the values over total
                sumx /= counter;
                sumy /= counter;

                // angle to x,y pos to positive x axis
                coherencePhis[i] = Mathf.Atan2(sumy, sumx) * RADIAN_TO_NORMALIZED;
                // distance to 0
                coherenceRadiuses[i] = Mathf.Sqrt(sumx * sumx + sumy * sumy);
            }
        }
        //set shader buffer with the angle to positive x
        coherencePhaseBuffer.SetData(coherencePhis);
    }

}

