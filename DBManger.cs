using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class DBManager : MonoBehaviour
{
    // API 엔드포인트 - 실제 PHP 파일명으로 수정
   private const string API_URL = "https://polliwog-fast-vaguely.ngrok-free.app/api/save_log.php";


    private static DBManager _instance;
    public static DBManager Instance => _instance;

    [System.Serializable]
    public class LogData
    {
        public int score;
        public string accidentDetails;
    }

    [System.Serializable]
    public class ApiResponse
    {
        public string status;
        public string message;
        public int inserted_id;
    }

    void Awake()
    {
        // 싱글톤 패턴
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("DB Manager Initialized");
    }

 /*   //Test를 위한 임시 메서드 추가함

 테스트가 정상적으로 종료되었으므로 주석처리함
*/



    // 외부에서 호출할 정적 메서드
    public static void SaveGameLog(int finalScore, string accidentDetails)
    {
        if (_instance != null)
        {
            _instance.SavePlayLog(finalScore, accidentDetails);
        }
        else
        {
            Debug.LogWarning("DBManager instance not found! Make sure DBManager is in the scene.");
        }
    }

    public static void SaveGameLog(
    int finalScore, 
    string accidentDetails, 
    System.Action<int, string> callback)
{
    if (_instance != null)
    {
        _instance.SavePlayLog(finalScore, accidentDetails, callback);
    }
    else
    {
        Debug.LogWarning("DBManager instance not found!");
        callback?.Invoke(-1, "DBManager instance not found");
    }
}

    // 실제 데이터 저장 처리
    private void SavePlayLog(
    int finalScore, 
    string accidentDetails,
    System.Action<int, string> callback)
{
    LogData log = new LogData
    {
        score = finalScore,
        accidentDetails = accidentDetails
    };

    string jsonBody = JsonUtility.ToJson(log);
    StartCoroutine(SendPostRequest(jsonBody, callback));
}

    private IEnumerator SendPostRequest(string jsonBody, System.Action<int, string> callback)
{
    using (UnityWebRequest www = new UnityWebRequest(API_URL, "POST"))
    {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");

        www.timeout = 10;

        yield return www.SendWebRequest();

        HandleResponse(www, callback);
    }
}


    private void ProcessResponse(UnityWebRequest www)
    {
        switch (www.result)
        {
            case UnityWebRequest.Result.Success:
                HandleSuccessResponse(www);
                break;
            case UnityWebRequest.Result.ConnectionError:
                Debug.LogError($"Connection Error: {www.error}");
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError($"HTTP Error: {www.error}");
                Debug.LogError($"Status Code: {www.responseCode}");
                if (!string.IsNullOrEmpty(www.downloadHandler.text))
                {
                    Debug.LogError($"Error Response: {www.downloadHandler.text}");
                }
                break;
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError($"Data Processing Error: {www.error}");
                break;
        }
    }

    private void HandleResponse(UnityWebRequest www, System.Action<int, string> callback)
{
    if (www.result != UnityWebRequest.Result.Success)
    {
        callback?.Invoke(-1, www.error);
        return;
    }

    string responseText = www.downloadHandler.text;

    try
    {
        ApiResponse response = JsonUtility.FromJson<ApiResponse>(responseText);

        if (response != null && response.status == "success")
        {
            callback?.Invoke(response.inserted_id, response.message);
        }
        else
        {
            callback?.Invoke(-1, response?.message ?? "Unknown server error");
        }
    }
    catch (System.Exception e)
    {
        callback?.Invoke(-1, "JSON parse error: " + e.Message);
    }
}

}