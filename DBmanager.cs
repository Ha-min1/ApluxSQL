using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class DBManager : MonoBehaviour
{
    // 로컬 PHP API 주소
    private const string API_URL = "https://polliwog-fast-vaguely.ngrok-free.app/api/";

    // Unity에서 보낼 데이터 구조체 [수정됨]
    [System.Serializable]
    public class LogData
    {
        public int score;               // [새로 추가] 최종 점수
        public string accidentDetails;  // [새로 추가] 사고 내역 (예: "Wall Crash", "No Accident")
    }

    // DB에 데이터를 전송하는 함수 [수정됨]
    public void SavePlayLog(int finalScore, string details)
    {
        LogData log = new LogData
        {
            score = finalScore,
            accidentDetails = details
        };

        // 데이터를 JSON으로 직렬화
        string jsonBody = JsonUtility.ToJson(log);
        StartCoroutine(SendPostRequest(jsonBody));
    }

    private IEnumerator SendPostRequest(string jsonBody)
    {
        // UnityWebRequest 객체 생성
        UnityWebRequest www = new UnityWebRequest(API_URL, "POST");

        // JSON 데이터를 바이트로 변환하여 요청 본문에 첨부
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        // 헤더 설정 (JSON임을 명시)
        www.SetRequestHeader("Content-Type", "application/json");

        // 요청 전송
        yield return www.SendWebRequest();

        // 응답 처리
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("DB API Error: " + www.error);
        }
        else
        {
            // PHP 스크립트에서 반환한 JSON 응답 확인
            Debug.Log("DB Response: " + www.downloadHandler.text);
        }
    }

    // 테스트용 코루틴 [수정됨]
    private IEnumerator RunTestWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // [테스트 1] 게임 종료 (사고 없음, 1500점)
        Debug.Log("Sending Test Log 1 (Game End, No Crash)...");
        // SavePlayLog 함수를 호출하며 테스트 데이터(점수, 사고내역)를 전달합니다.
        SavePlayLog(1500, "No Accident");

        yield return new WaitForSeconds(2f);

        // [테스트 2] 게임 종료 (사고 발생, 300점)
        Debug.Log("Sending Test Log 2 (Game End, Crash)...");
        SavePlayLog(300, "Crashed into wall at Zone 1");
    }

    void Start()
    {
        Debug.Log("DB Manager Initialized");
        StartCoroutine(RunTestWithDelay(1f));
    }
}
