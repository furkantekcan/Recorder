using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CharatcerController : MonoBehaviour
{

    public UnityEngine.UI.Dropdown dropdown;
    public UnityEngine.UI.Button playButton;
    public UnityEngine.UI.Button stopButton;
    public UnityEngine.UI.Button recordButton;
    public UnityEngine.UI.Button backButton;
    public UnityEngine.UI.Button replayButton;

    public Material recordMaterial, playMaterial, defaultMaterial;
    public MeshRenderer sphere;

    public bool isRecording = false;
    public bool isReplaying = false;
    public bool isBackward = false;
    public bool isForward = false;  

    private Animator animator;
    
    private List<FrameData> recordedFrames = new List<FrameData>();
    private int currentFrame = 0;
    private List<List<FrameData>> records = new List<List<FrameData>>();
    private int selectedIndex;
    private List<FrameData> currentRecord = new List<FrameData>();

    private void Start()
    {
        playButton.onClick.AddListener(StartMotion);
        stopButton.onClick.AddListener(StopMotion);
        recordButton.onClick.AddListener(StartRecording);
        backButton.onClick.AddListener(delegate { isBackward = !isBackward; });
        
        replayButton.onClick.AddListener(Replay);

        animator = GetComponent<Animator>();    
        animator.enabled = false;
    }

    private void Update()
    {
        if (isRecording)
        {
            RecordFrame();
        }
        if (isReplaying)
        {
            
            PlayFrame();
        }
        if( !isBackward )
        {
            RecordAll();
        }
        if (isBackward)
        {
            PlayBackward();
        }
    }

    private void RecordAll()
    {
        FrameData frame = new FrameData();

        foreach (Transform bone in animator.GetComponentsInChildren<Transform>())
        {
            frame.boneTransforms.Add(new BoneData(bone.localPosition, bone.localRotation));
        }

        currentRecord.Add(frame);
    }

    private void RecordFrame()
    {

        FrameData frame = new FrameData();

        foreach (Transform bone in animator.GetComponentsInChildren<Transform>())
        {
            frame.boneTransforms.Add(new BoneData(bone.localPosition, bone.localRotation));
        }

        recordedFrames.Add(frame);
    }
    private void PlayBackward()
    {
        animator.enabled = false;

        if (currentFrame == 0 && isBackward)
        {
            currentFrame = currentRecord.Count - 1;
        }

        FrameData frame = currentRecord[currentFrame];
        int i = 0;
        foreach (Transform bone in animator.GetComponentsInChildren<Transform>())
        {
            bone.localPosition = frame.boneTransforms[i].position;
            bone.localRotation = frame.boneTransforms[i].rotation;
            i++;
        }
        currentFrame--;
        if (currentFrame == 0)
        {
            isBackward = false;
        }
    }

    private void Replay()
    {
        isReplaying = true;
        isRecording = false;

        selectedIndex = dropdown.value - 1;
        sphere.material = playMaterial;
        animator.enabled = false;
    }

    public void StartRecording()
    {
        if (isRecording == false)
        {
            isRecording = true;
            recordedFrames.Clear();
            sphere.material = recordMaterial;
        }
        else
        {
            
            isRecording = false;
            sphere.material = defaultMaterial;
            records.Add(recordedFrames.ToList());
            var data = new Dropdown.OptionData();
            data.text = $"Record{records.Count}: {gameObject.name}, ({DateTime.Now.Date})";
            dropdown.options.Add(data);
            recordedFrames.Clear();
        }
    }

    public void StopRecording()
    {
        isRecording = false;
    }

    private void PlayFrame()
    {
        if (selectedIndex < 0)
        {
            return;
        }

        if (records[selectedIndex] == null)
        {
            Debug.Log(records[selectedIndex]);
            return;
        }
        if (records[selectedIndex].Count == 0)
        {
            Debug.Log(records[selectedIndex].Count);
            return;
        }

        if (currentFrame >= records[selectedIndex].Count)
        {
            StopMotion();
            currentFrame = 0;
            isReplaying = false;
            return;
        }

        FrameData frame = records[selectedIndex][currentFrame];
        int i = 0;
        foreach (Transform bone in animator.GetComponentsInChildren<Transform>())
        {
            bone.localPosition = frame.boneTransforms[i].position;
            bone.localRotation = frame.boneTransforms[i].rotation;
            i++;
        }

        currentFrame++;
    }

    private void StartMotion()
    {
        isBackward = false;
        isForward = false;
        isRecording = false;

        animator.enabled = true;
    }

    private void StopMotion()
    {
        animator.enabled = false;
    }

    [System.Serializable]
    public class FrameData
    {
        public List<BoneData> boneTransforms = new List<BoneData>();
    }

    [System.Serializable]
    public class BoneData
    {
        public Vector3 position;
        public Quaternion rotation;

        public BoneData(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
        }
    }
}
