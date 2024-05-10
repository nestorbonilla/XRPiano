using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NAudio.Midi;

public class MidiManager: MonoBehaviour
{
    private int midiIndex;
    private MidiIn midiInput;
    public GameObject anchorPrefab;
    private GameObject _leftPrefab;
    private int _leftLimitStep = 0;
    private GameObject _rightPrefab;
    private int _rightLimitStep = 0;
    [SerializeField] private OVRHand _leftHand;
    [SerializeField] private OVRHand _rightHand;
    [SerializeField] private GameObject _pianoObject;
    [SerializeField] private GameObject _linePrefab;
    public PianoKeyRenderer pianoKeyRenderer;
    private ConcurrentQueue<MidiInMessageEventArgs> _midiEventsQueue = new ConcurrentQueue<MidiInMessageEventArgs>();
    private Dictionary<string, (LineRenderer, Coroutine)> activeAnimations = new Dictionary<string, (LineRenderer, Coroutine)>();
    float easeInOut(float t) {
        if (t < 0.5f) {
            return Mathf.Pow(2 * t, 2);
        } else {
            return 1 - Mathf.Pow(2 * (1 - t), 2);
        }
    }
    void Start()
    {
        Debug.Log("Starting MIDI Manager...");
        InitializeMIDI();
    }

    private void InitializeMIDI()
    {
        midiIndex = GetMidiIndex();
        midiInput = new MidiIn(midiIndex);
        midiInput.Start();
        midiInput.MessageReceived += OnMidiMessageReceived;
    }

    int GetMidiIndex() {
        for (int i = 0; i < MidiIn.NumberOfDevices; i++) {
            var deviceInfo = MidiIn.DeviceInfo(i);
            Debug.Log("Dispositivo MIDI encontrado: " + deviceInfo.ProductName + "");
            if (deviceInfo.ProductName.Contains("KONTROL S61 MK3")) {
                Debug.Log("ID de dispositivo MIDI: " + i);
                return i;
            }
        }
        return -1;
    }
    void Update()
    {
        while (_midiEventsQueue.TryDequeue(out var midiEvent))
        {
            ProcessMidiEvent(midiEvent);
        }

        if (_leftLimitStep == 1)
        {
            Vector3 leftPinkyOffset = new Vector3(0.15f, 0.06f, -0.045f);
            Vector3 leftPinkyPosition = _leftHand.transform.position + _leftHand.transform.rotation * leftPinkyOffset;
            _leftPrefab = Instantiate(anchorPrefab, leftPinkyPosition, Quaternion.identity);
            _leftPrefab.AddComponent<OVRSpatialAnchor>();
            _leftPrefab.transform.rotation = Quaternion.Euler(0, _leftHand.transform.eulerAngles.y, 0);
            _leftLimitStep++;
        }
        if (_rightLimitStep == 1)
        {
            Vector3 rightPinkyOffset = new Vector3(-0.15f, -0.06f, 0.045f);
            Vector3 rightPinkyPosition = _rightHand.transform.position + _rightHand.transform.rotation * rightPinkyOffset;
            _rightPrefab = Instantiate(anchorPrefab, rightPinkyPosition, Quaternion.identity);
            _rightPrefab.AddComponent<OVRSpatialAnchor>();
            _rightPrefab.transform.rotation = Quaternion.Euler(0, _rightHand.transform.eulerAngles.y, 0);
            _rightLimitStep++;
        }
        
        if (_leftLimitStep == 2 && _rightLimitStep == 2)
        {
            
            if (_pianoObject != null)
            {
                if (_pianoObject.GetComponent<PianoKeyRenderer>() == null)
                {
                    pianoKeyRenderer = _pianoObject.AddComponent<PianoKeyRenderer>();
                }
                else
                {
                    pianoKeyRenderer = _pianoObject.GetComponent<PianoKeyRenderer>();
                }
        
                pianoKeyRenderer.CreatePianoKeys(_leftPrefab.transform, _rightPrefab.transform);
        
                _leftLimitStep++;
                _rightLimitStep++;
            }
            else
            {
                Debug.LogError("Error: _pianoObject is null!");
            }
        }
    }

    void OnMidiMessageReceived(object sender, MidiInMessageEventArgs e)
    {
        _midiEventsQueue.Enqueue(e);
    }

    void ProcessMidiEvent(MidiInMessageEventArgs e)
    {
        if (_leftLimitStep == 0 && e.MidiEvent.ToString().Contains("C3")) _leftLimitStep++;
        if (_rightLimitStep == 0 && e.MidiEvent.ToString().Contains("C8")) _rightLimitStep++;
        // Detect key presses and trigger color change on piano keys
        if (_leftLimitStep == 3 || _rightLimitStep  == 3)
        {
            string[] midiEventParts = e.MidiEvent.ToString().Split(' ');
            string midiNoteAction = midiEventParts[1];
            if (midiNoteAction == "NoteOn")
            {
                string midiNoteValue = midiEventParts[4];
                foreach (GameObject keyObject in pianoKeyRenderer.pianoKeys)
                {
                    MeshRenderer keyRenderer = keyObject.GetComponent<MeshRenderer>();

                    if (keyObject.name == midiNoteValue)
                    {
                        keyRenderer.material.color = Color.gray;
                        
                        GameObject lineObject = new GameObject("LineObject");
                        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
                        lineRenderer.startWidth = 0.01f;
                        lineRenderer.endWidth = 0.01f;

                        // Set the initial position of the LineRenderer to the position of the key that was pressed
                        lineRenderer.SetPosition(0, keyObject.transform.position);

                        // Assign the Unlit/Color shader to the LineRenderer's material
                        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
                        lineRenderer.material.color = Color.yellow;

                        // Start the animation coroutine
                        Coroutine coroutine = StartCoroutine(AnimateLineRenderer(lineRenderer));
                        activeAnimations[midiNoteValue] = (lineRenderer, coroutine);

                    }
                }
            }
            if (midiNoteAction == "NoteOff")
            {
                string midiNoteValue = midiEventParts[4];
                foreach (GameObject keyObject in pianoKeyRenderer.pianoKeys)
                {
                    MeshRenderer keyRenderer = keyObject.GetComponent<MeshRenderer>();

                    if (keyObject.name == midiNoteValue)
                    {
                        keyRenderer.material.color = Color.white;
                        if (activeAnimations.TryGetValue(midiNoteValue, out var animation))
                        {
                            StopCoroutine(animation.Item2);
                            Destroy(animation.Item1.gameObject);
                            activeAnimations.Remove(midiNoteValue);
                        }
                    }
                }
            }
        }
    }
    
    IEnumerator AnimateLineRenderer(LineRenderer lineRenderer)
    {
        float timeElapsed = 0f;

        // Continue the animation indefinitely
        while (true)
        {
            timeElapsed += Time.deltaTime;
            float distance = Mathf.Lerp(0, 10, easeInOut(timeElapsed));
            Vector3 position = lineRenderer.GetPosition(0) + lineRenderer.transform.forward * distance;
            lineRenderer.SetPosition(1, position);

            yield return null;
        }
    }
}