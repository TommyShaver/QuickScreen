using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;
using System.Collections;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public TextMeshProUGUI warningText;
    public TextMeshProUGUI linkText;
    public TMP_InputField inputField;

    [SerializeField] private string url;

    private bool leftControlKeyPressed;
    private bool insertKeyPressed;
    private bool messageSent;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        warningText.text = string.Empty;
        SaveLoadData.Load();
        CheckUrlText();
        
    }

    // Update is called once per frame
    void Update()
    {
        InputManager();
        CompareCheck();
    }


    //Input commands ---------------------------------------------
    private void InputManager()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            leftControlKeyPressed = true;
        }
        if (Input.GetKeyDown(KeyCode.Insert))
        {
            insertKeyPressed = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            leftControlKeyPressed = false;
        }
        if (Input.GetKeyUp(KeyCode.Insert))
        {
            insertKeyPressed = false;
        }
    }
    private void CompareCheck()
    {
        if (leftControlKeyPressed && insertKeyPressed)
        {
            insertKeyPressed = false;
            leftControlKeyPressed = false;
            OpenWebsite();
            WarningTextSetUp("Checking");
        }
    }

    //User interaction ---------------------------------------------------------------------------------------
    public void IncomingUserText()
    {
       
        //On input field events this scripted attached to { On End Edit (String) [defaults to keypad enter] }
        url = inputField.text;
        CheckUrlText();
        WarningTextSetUp("URL saved");
        SaveLoadData.Save();
    }

    private void CheckUrlText()
    {
        if (string.IsNullOrEmpty(url))
        {
            linkText.text = "Please enter link...";
        }
        else
        {
            linkText.text = url;
        }
    }



    //Connect to web ----------------------------------------------------------------------------------------------
    private void OpenWebsite()
    {
        if(string.IsNullOrEmpty(url))
        {
            WarningTextSetUp("URL cannot be empty.");
            return;
        }
        StartCoroutine(CheckURL());
    }

    //Timer ------------------------------------------------------------------------------------------------------
    private IEnumerator CheckURL()
    {
        using (UnityWebRequest request = UnityWebRequest.Head(url))
        {
            yield return request.SendWebRequest();
            if(request.result == UnityWebRequest.Result.Success)
            {
                Application.OpenURL(url);
            }
            else
            {
                WarningTextSetUp("Failed to reach the URL");
            }
        }
    }

    //Warning message -----------------------------------------------------------
    private void WarningTextSetUp(string s)
    {
        if (messageSent)
        {
            StopCoroutine(ClearMessage(0));
        }
        warningText.text = s;
        StartCoroutine(ClearMessage(5));
        messageSent = true;
    }

    private IEnumerator ClearMessage(int i)
    {
        yield return new WaitForSeconds(i);
        warningText.text = string.Empty;
        messageSent = false;    
        
    }

    //Save Data --------------------------------------------------------------------
   public void SaveData(ref GameManagerSaveData saveData)
    {
        saveData.linkSave = url;
    }

    public void LoadData(ref GameManagerSaveData loadData)
    {
        url = loadData.linkSave;
    }
}
[System.Serializable]
public struct GameManagerSaveData
{
    public string linkSave;
}

public class SaveLoadData
{
    private static SaveData _saveData = new SaveData();

    [System.Serializable]
    public struct SaveData
    {
        public GameManagerSaveData gameManaerSaveData;
    }

    public static string SaveFileName()
    {
        string saveFile = Application.persistentDataPath + "/save " + ".json";
        return saveFile;
    }

    public static void Save()
    {
        HandleSaveData();
        File.WriteAllText(SaveFileName(), JsonUtility.ToJson(_saveData, true));
    }
    private static void HandleSaveData()
    {
        GameManager.instance.SaveData(ref _saveData.gameManaerSaveData);
    }

    public static void Load()
    {
        string saveFileName = SaveFileName();
        if (File.Exists(saveFileName))
        {
            string saveContent = File.ReadAllText(SaveFileName());
            _saveData = JsonUtility.FromJson<SaveData>(saveContent);
            HandleLoadData();
        }
        else
        {
            Debug.LogWarning($"Save file not found: {saveFileName}. This is due to first time load.");
        }
    }

    private static void HandleLoadData()
    {
        GameManager.instance.LoadData(ref _saveData.gameManaerSaveData);
    }
}

