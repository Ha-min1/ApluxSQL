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
void Start()
    {
        // 씬이 완전히 로드되고 2초 후에 테스트를 실행합니다.
        StartCoroutine(RunQuickTest());
    }

    private IEnumerator RunQuickTest()
    {
        // 2초 대기 (Awake가 확실히 끝난 후 실행)
        yield return new WaitForSeconds(2f);

        Debug.Log("--- Starting DB Test ---");
        // 스크립트의 static 함수를 호출하여 테스트 로그 전송
        DBManager.SaveGameLog(999, "This is a quick test log.");
    }
    //이 주속 및 위의 주석은 건드리지 말것
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

    // 실제 데이터 저장 처리
    private void SavePlayLog(int finalScore, string accidentDetails)
    {
        LogData log = new LogData
        {
            score = finalScore,
            accidentDetails = accidentDetails
        };

        string jsonBody = JsonUtility.ToJson(log);
        StartCoroutine(SendPostRequest(jsonBody));
    }

    private IEnumerator SendPostRequest(string jsonBody)
    {
        using (UnityWebRequest www = new UnityWebRequest(API_URL, "POST"))
        {
            // 요청 본문 설정
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            // 헤더 설정
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");

            // 타임아웃 설정 (10초)
            www.timeout = 10;

            Debug.Log($"Sending request to: {API_URL}");
            Debug.Log($"Request data: {jsonBody}");

            // 요청 전송 및 대기
            yield return www.SendWebRequest();

            // 응답 처리
            ProcessResponse(www);
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

    private void HandleSuccessResponse(UnityWebRequest www)
    {
        string responseText = www.downloadHandler.text;
        Debug.Log($"Raw Response: {responseText}");

        try
        {
            // PHP 응답 파싱 시도
            ApiResponse response = JsonUtility.FromJson<ApiResponse>(responseText);

            if (response != null)
            {
                if (response.status == "success")
                {
                    Debug.Log($"✅ Data saved successfully! ID: {response.inserted_id}");
                }
                else
                {
                    Debug.LogWarning($"⚠ Server returned error: {response.message}");
                }
            }
            else
            {
                Debug.LogWarning("⚠ Failed to parse response JSON");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠ Response parsing error: {e.Message}");
            Debug.Log($"Raw response was: {responseText}");
        }
    }
}
:w