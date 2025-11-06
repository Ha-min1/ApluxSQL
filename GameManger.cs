public class GameManager : MonoBehaviour
{
    private int playerScore = 0;
    private bool gameEnded = false;

    void Update()
    {
        // 게임 로직...
    }

    // 게임 종료 시 호출 (사고 없이 클리어)
    public void EndGameSuccess(int finalScore)
    {
        if (gameEnded) return;

        gameEnded = true;
        Debug.Log($"Game Completed! Score: {finalScore}");

        // DB에 성공 기록 저장
        DBManager.SaveGameLog(finalScore, "No Accident");

        // 결과 화면 표시 등...
    }

    // 사고 발생 시 호출
    public void ReportAccident(string accidentType, int currentScore)
    {
        Debug.Log($"Accident occurred: {accidentType}");

        // DB에 사고 기록 저장 (게임 중간에도 가능)
        DBManager.SaveGameLog(currentScore, accidentType);
    }

    // 벽 충돌 시
    public void OnWallCollision(Vector3 collisionPoint)
    {
        string accidentDetails = $"Wall Crash at {collisionPoint}";
        ReportAccident(accidentDetails, playerScore);
    }

    // 장애물 충돌 시
    public void OnObstacleCollision(string obstacleName)
    {
        string accidentDetails = $"Obstacle Crash: {obstacleName}";
        ReportAccident(accidentDetails, playerScore);
    }
}
