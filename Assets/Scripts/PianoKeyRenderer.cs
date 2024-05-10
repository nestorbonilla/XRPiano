using System.Collections.Generic;
using UnityEngine;

public class PianoKeyRenderer : MonoBehaviour
{
    private float _keyWidth;
    private float _keyDistance = 0.001f;
    public List<GameObject> pianoKeys = new List<GameObject>();
    Mesh CubeMesh()
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh mesh = cube.GetComponent<MeshFilter>().mesh;
        Destroy(cube);
        return mesh;
    }
 
    public void CreatePianoKeys(Transform leftLimitPosition, Transform rightLimitPosition)
    {
        Debug.Log("Piano keys starting!");
        _keyWidth = (rightLimitPosition.transform.position.x - leftLimitPosition.transform.position.x - _keyDistance * 35) / 36f;

        for (int i = 0; i < 36; i++)
        {
            string noteName = GetNoteName(i);
            GameObject keyObject = new GameObject(noteName);
            float t = i / 35f;
            Vector3 keyPosition = Vector3.Lerp(leftLimitPosition.transform.position, rightLimitPosition.transform.position, t);

            MeshFilter keyMeshFilter = keyObject.AddComponent<MeshFilter>();
            keyMeshFilter.mesh = CubeMesh();

            MeshRenderer keyRenderer = keyObject.AddComponent<MeshRenderer>();
            keyRenderer.material = new Material(Shader.Find("Unlit/Color"));
            Color randomColor = Color.white;
            keyRenderer.material.color = randomColor;
            keyRenderer.transform.position = keyPosition;

            keyRenderer.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            keyRenderer.transform.localScale = new Vector3(_keyWidth, 0.01f, 0.1f);

            BoxCollider keyCollider = keyObject.AddComponent<BoxCollider>();
            keyCollider.isTrigger = true;
            
            pianoKeys.Add(keyObject);
            
            // Check for adjacent black keys
            // if (i % 12 == 1 || i % 12 == 3 || i % 12 == 6 || i % 12 == 8 || i % 12 == 10)
            // {
            //     int blackKeyIndex = GetBlackKeyIndex(i);
            //     GameObject blackKeyObject = new GameObject(GetNoteName(blackKeyIndex));
            //     blackKeyObject.transform.SetParent(_pianoObject.transform);
            //     blackKeyObject.transform.position = keyPosition;
            //     float offset = GetBlackKeyOffset(i);
            //     blackKeyObject.transform.position += new Vector3(offset, 0f, 0f);
            //
            //     MeshFilter blackKeyMeshFilter = blackKeyObject.AddComponent<MeshFilter>();
            //     blackKeyMeshFilter.mesh = CubeMesh();
            //
            //     MeshRenderer blackKeyRenderer = blackKeyObject.AddComponent<MeshRenderer>();
            //     blackKeyRenderer.material = new Material(Shader.Find("Unlit/Color"));
            //     Color randomBlackColor = Color.black;
            //     blackKeyRenderer.material.color = randomBlackColor;
            //
            //     BoxCollider blackKeyCollider = blackKeyObject.AddComponent<BoxCollider>();
            //     blackKeyCollider.isTrigger = true;
            //     
            // }
        }
        Debug.Log("Piano keys created!");
    }
    
    // private int GetBlackKeyIndex(int whiteKeyIndex)
    // {
    //     if (whiteKeyIndex % 12 == 1) return 0; // A# (black key)
    //     if (whiteKeyIndex % 12 == 3) return 2; // C# (black key)
    //     if (whiteKeyIndex % 12 == 6) return 4; // F# (black key)
    //     if (whiteKeyIndex % 12 == 8) return 9; // A (black key)
    //     if (whiteKeyIndex % 12 == 10) return 11; // C (black key)
    //
    //     return -1;
    // }
    //
    // private float GetBlackKeyOffset(int blackKeyIndex)
    // {
    //     switch (blackKeyIndex % 7)
    //     {
    //         case 0: return 0.05f; // A# (black key)
    //         case 2: return -0.03f; // C# (black key)
    //         case 4: return 0.02f; // F# (black key)
    //         case 9: return -0.01f; // A (black key)
    //         case 11: return 0.04f; // C (black key)
    //
    //         default:
    //             return 0f;
    //     }
    // }
    
    private string GetNoteName(int index)
    {
        int noteIndex = index % 7;
        int octaveNumber = (index / 7) + 3; // Lowest octave is 3, highest is 8
    
        switch (noteIndex)
        {
            case 0: return "C" + octaveNumber.ToString();
            case 1: return "D" + octaveNumber.ToString();
            case 2: return "E" + octaveNumber.ToString();
            case 3: return "F" + octaveNumber.ToString();
            case 4: return "G" + octaveNumber.ToString();
            case 5: return "A" + octaveNumber.ToString();
            case 6: return "B" + octaveNumber.ToString();
            default: return "";
        }
    }
}