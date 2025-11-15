using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class DBManager : MonoBehaviour
{
    // API 엔드포인트
    private const string API_URL = "https://polliwog-fast-vaguely.ngrok-free.app/api/save_log.php";

    private static DBManager _instance;
    public static DBManager Instance => _instance;

    // 데이터 저장용 클래스
    [System.Serializable]
    public class LogData
    {
        public int score;
        public string accidentDetails;
    }

    // 데이터 저장 응답 클래스
    [System.Serializable]
    public class ApiResponse
    {
        public string status;
        public string message;
        public int inserted_id;
    }

    // 로그 항목 클래스
    [System.Serializable]
    public class LogEntry
    {
        public int id;
        public int score;
        public string accidentDetails;
        public string created_at;
    }

    // 로그 조회 응답 클래스
    [System.Serializable]
    public class LogResponse
    {
        public string status;
        public string message;
        public LogEntry log;
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

    // ===== 데이터 저장 메서드들 =====

    // 외부에서 호출할 정적 메서드 (콜백 없음)
    public static void SaveGameLog(int finalScore, string accidentDetails)
    {
        if (_instance != null)
        {
            _instance.SavePlayLog(finalScore, accidentDetails, null);
        }
        else
        {
            Debug.LogWarning("DBManager instance not found! Make sure DBManager is in the scene.");
        }
    }

    // 외부에서 호출할 정적 메서드 (콜백 있음)
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

    // POST 요청 전송
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

            HandleSaveResponse(www, callback);
        }
    }

    // 저장 응답 처리
    private void HandleSaveResponse(UnityWebRequest www, System.Action<int, string> callback)
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

    // ===== 데이터 조회 메서드들 =====

    // 외부에서 호출할 정적 메서드 - ID로 로그 조회
    public static void GetGameLog(int logId, System.Action<LogEntry, string> callback)
    {
        if (_instance != null)
        {
            _instance.GetPlayLog(logId, callback);
        }
        else
        {
            Debug.LogWarning("DBManager instance not found!");
            callback?.Invoke(null, "DBManager instance not found");
        }
    }

    // 실제 데이터 조회 처리
    private void GetPlayLog(int logId, System.Action<LogEntry, string> callback)
    {
        StartCoroutine(SendGetRequest(logId, callback));
    }

    // GET 요청 전송
    private IEnumerator SendGetRequest(int logId, System.Action<LogEntry, string> callback)
    {
        string url = $"{API_URL}?id={logId}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Accept", "application/json");
            www.timeout = 10;

            yield return www.SendWebRequest();

            HandleGetResponse(www, callback);
        }
    }

    // 조회 응답 처리
    private void HandleGetResponse(UnityWebRequest www, System.Action<LogEntry, string> callback)
    {
        if (www.result != UnityWebRequest.Result.Success)
        {
            callback?.Invoke(null, www.error);
            return;
        }

        string responseText = www.downloadHandler.text;

        try
        {
            LogResponse response = JsonUtility.FromJson<LogResponse>(responseText);

            if (response != null && response.status == "success" && response.log != null)
            {
                callback?.Invoke(response.log, response.message);
            }
            else
            {
                callback?.Invoke(null, response?.message ?? "Log not found");
            }
        }
        catch (System.Exception e)
        {
            callback?.Invoke(null, "JSON parse error: " + e.Message);
        }
    }

    // ===== 테스트 메서드 =====
/*
    // 테스트를 위한 메서드
    public void TestSaveAndLoad()
    {
        // 테스트 데이터 저장
        SaveGameLog(95, "테스트: 장애물 충돌", (savedId, saveMessage) => 
        {
            if (savedId > 0)
            {
                Debug.Log($"저장 성공! ID: {savedId}");
                
                // 저장된 데이터 바로 조회
                GetGameLog(savedId, (log, loadMessage) => 
                {
                    if (log != null)
                    {
                        Debug.Log($"조회 성공 - ID: {log.id}, Score: {log.score}, Accident: {log.accidentDetails}, Date: {log.created_at}");
                    }
                    else
                    {
                        Debug.LogError($"조회 실패: {loadMessage}");
                    }
                });
            }
            else
            {
                Debug.LogError($"저장 실패: {saveMessage}");
            }
        });
    }
    */
}