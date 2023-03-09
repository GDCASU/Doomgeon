using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapGenerator : MonoBehaviour
{
    const int ROOM_FACES = 6;

    [Header("Settings")]
    public float gridCellSize = 40;
    public bool debug = true;

    [Header("Max Rooms")]
    public int mainTrailMaxRooms = 10;
    public int augmentationTrailMaxRooms = 1;
    public int keycardTrailMaxRooms = 2;
    public int trialTrailMaxRooms = 2;
    public int bossTrailMaxRooms = 2;

    [Header("Room Spawn Chances")]
    [Range(0, 1)] public float tRoomChance = 0.4f;
    [Range(0, 1)] public float hRoomChance = 0.5f;
    [Range(0, 1)] public float bRoomChance = 1.0f;


    [Header("Room Prefabs")]
    public GameObject bluePrintPrefab;
    public List<GameObject> gPrefab;
    public List<GameObject> tPrefab;
    public List<GameObject> hPrefab;
    public List<GameObject> bPrefab;

    [Header("Trails")]
    public List<BlueprintRoom> masterTrail; // All trails combined
    public List<BlueprintRoom> mainTrail; // Trail to Boss Room
    public List<BlueprintRoom> augmentationTrail; // Trail to Augmentation Room
    public List<BlueprintRoom> keycardTrail; // Trail to Keycard Room
    public List<BlueprintRoom> trialTrail; // Trail to Trial Room
    public List<BlueprintRoom> bossTrail; // Trail to Boss Room

    int entrFlagIdx = 0;
    void Start()
    {
        masterTrail = new List<BlueprintRoom>(); // All trails combined
        mainTrail = new List<BlueprintRoom>(); // Trail to Boss Room
        augmentationTrail = new List<BlueprintRoom>(); // Trail to Augmentation Room
        keycardTrail = new List<BlueprintRoom>(); // Trail to Keycard Room
        trialTrail = new List<BlueprintRoom>(); // Trail to Trial Room
        bossTrail = new List<BlueprintRoom>(); // Trail to Boss Room

        if (!debug)
            Procedure();
    }

    void Procedure()
    {
        RandomWalker(mainTrailMaxRooms, mainTrail, null); // Main Trail to boss

        int randomIdx = UnityEngine.Random.Range(1, (mainTrail.Count - 1));
        BlueprintRoom randomStartingRoom = mainTrail[randomIdx];
        RandomWalker(augmentationTrailMaxRooms, augmentationTrail, randomStartingRoom); // Trail to Augmentation Room

        randomIdx = UnityEngine.Random.Range(1, (mainTrail.Count - 1));
        randomStartingRoom = mainTrail[randomIdx];
        RandomWalker(trialTrailMaxRooms, trialTrail, randomStartingRoom); // Trial Trail Generation

        randomIdx = UnityEngine.Random.Range(1, (mainTrail.Count - 1));
        randomStartingRoom = mainTrail[randomIdx];
        RandomWalker(keycardTrailMaxRooms, keycardTrail, randomStartingRoom); // Keycard Trail Generation
    }

    #region RandomWalker
    void RandomWalker(int maxRooms, List<BlueprintRoom> trail, BlueprintRoom startingRoom)
    {
        Vector3 curPos = Vector3.zero; // Set the position of the starting room
        BlueprintRoom curRoom = null;
        Vector3 tempPos = Vector3.zero; // new postion to be choosen

        if (startingRoom == null) // If there are not yet any rooms
        {
            BlueprintRoom newRoom = new BlueprintRoom(curPos);
            trail.Add(newRoom);
            masterTrail.Add(newRoom);
            curRoom = newRoom;
        }
        else
        {
            curPos = startingRoom.position;
            curRoom = startingRoom;
        }

        int failedAttempts = 0;
        while (trail.Count < maxRooms)
        {
            tempPos = curPos;
            switch (UnityEngine.Random.Range(1, ROOM_FACES + 1)) // Choosing position of next room from 6 possible directions
            {
                case 1: tempPos += Vector3.right * gridCellSize; // E0 (cellSize, 0, 0) * Cell Unit Size
                    entrFlagIdx = 0;
                    break;
                case 2: tempPos += Vector3.left * gridCellSize; // E1 (-cellSize, 0, 0) * Cell Unit Size
                    entrFlagIdx = 1;
                    break;
                case 3: tempPos += Vector3.forward * gridCellSize; // E2 (0, 0, cellSize) * Cell Unit Size
                    entrFlagIdx = 2;
                    break;
                case 4: tempPos += Vector3.back * gridCellSize; // E3 (0, 0, -cellSize) * Cell Unit Size
                    entrFlagIdx = 3;
                    break;
                case 5: tempPos += Vector3.up * gridCellSize;  // E4 (0, cellSize, 0) * Cell Unit Size
                    entrFlagIdx = 4;
                    break;
                case 6: tempPos += Vector3.down * gridCellSize; // E5 (0, -cellSize, 0) * Cell Unit Size
                    entrFlagIdx = 5;
                    break; 
            }

            bool inRoomList = false;
            BlueprintRoom collidedRoom = null;
            foreach(BlueprintRoom room in masterTrail) // Check master trail for colliding rooms (the temp pos is inside another designated room space)
            {
                if (Vector3.Equals(tempPos, room.position)) // Test Failed
                {
                    collidedRoom = room;
                    inRoomList = true;
                    failedAttempts++;
                    break;
                }
            }

            if (!inRoomList) // Test Passed
            {
                curPos = tempPos; // Change Current Position to new position

                BlueprintRoom newRoom = new BlueprintRoom(curPos);
                FlagDoorways(newRoom, curRoom, entrFlagIdx);
                GenerateBlueprintGizmo(curPos);

                curRoom = newRoom;
                trail.Add(newRoom);
                masterTrail.Add(newRoom);

                failedAttempts = 0;
            }

            if (failedAttempts >= ROOM_FACES) // If failed too many times backtrack (very rare)
            {
                curPos = tempPos;
                curRoom = collidedRoom;
                failedAttempts = 0;
            }
        }
    }

    void FlagDoorways(BlueprintRoom newRoom, BlueprintRoom prevRoom,  int entrFlagIdx) // Flag the entranceways to be activated in each room
    {
        if (entrFlagIdx % 2 == 0) // If choosen an even numbered side (E4) then set opposite (E3) to true
            newRoom.activeEntranceways[entrFlagIdx + 1] = true;
        else // If choosen an odd numbered side (E3) then set opposite (E4) to true
            newRoom.activeEntranceways[entrFlagIdx - 1] = true;

        prevRoom.activeEntranceways[entrFlagIdx] = true;
    }

    private void GenerateBlueprintGizmo(Vector3 roomPosition) // Generate Gizmo for Debugging purposes 
    {
        GameObject genRoom = Instantiate(bluePrintPrefab, roomPosition, Quaternion.identity) as GameObject;
        genRoom.name = $"{bluePrintPrefab.name}";
        genRoom.transform.SetParent(transform);
    }
    #endregion

    #region DebugGUI
    bool alreadyGMain, alreadyGAugmentation, alreadyGTrial,
        alreadyGKeycard;
    void OnGUI()
    {
        if (debug)
        {
            if (GUILayout.Button("Reload Scene"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload Active Scene
            }

            if (GUILayout.Button("Generate Main Trail"))
            {
                if (!alreadyGMain)
                {
                    RandomWalker(mainTrailMaxRooms, mainTrail, null); // Main Trial Generation

                    alreadyGMain = true;
                    Debug.Log("Main Trail Generated");
                }
                else
                    Debug.Log("Error: Already generated Main Trail");
            }

            if (GUILayout.Button("Generate Augmentation Trail"))
            {
                if (!alreadyGAugmentation)
                {
                    int randomIdx = UnityEngine.Random.Range(1, (mainTrail.Count - 1));
                    BlueprintRoom randomStartingRoom = mainTrail[randomIdx];

                    RandomWalker(augmentationTrailMaxRooms, augmentationTrail, randomStartingRoom); // Augmentation Trail Generation

                    alreadyGAugmentation = true;
                    Debug.Log($"Augmentation Trail Generated at room index : {randomIdx}");
                }
                else
                    Debug.Log("Error: Already generated Augmentation Trail");
            }

            if (GUILayout.Button("Generate Trial Trail"))
            {
                if (!alreadyGTrial)
                {
                    int randomIdx = UnityEngine.Random.Range(1, (mainTrail.Count - 1));
                    BlueprintRoom randomStartingRoom = mainTrail[randomIdx];

                    RandomWalker(trialTrailMaxRooms, trialTrail, randomStartingRoom); // Trial Trail Generation

                    alreadyGTrial = true;
                    Debug.Log($"Trial Trail Generated at room index : {randomIdx}");
                }
                else
                    Debug.Log("Error: Already generated Trial Trail");
            }

            if (GUILayout.Button("Generate Keycard Trail"))
            {
                if (!alreadyGKeycard)
                {
                    int randomIdx = UnityEngine.Random.Range(1, (mainTrail.Count - 1));
                    BlueprintRoom randomStartingRoom = mainTrail[randomIdx];

                    RandomWalker(keycardTrailMaxRooms, keycardTrail, randomStartingRoom); // Keycard Trail Generation

                    alreadyGKeycard = true;
                    Debug.Log($"Keycard Trail Generated at room index : {randomIdx}");
                }
                else
                    Debug.Log("Error: Already generated Keycard Trail");
            }
        }
    }
    #endregion
}