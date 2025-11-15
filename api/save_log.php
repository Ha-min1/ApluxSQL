<?php
// (1) 응답 헤더를 JSON으로 설정
header('Content-Type: application/json');
header("Access-Control-Allow-Origin: *");
header("Access-Control-Allow-Methods: GET, POST, OPTIONS");
header("Access-Control-Allow-Headers: Content-Type");

// (2) DB 연결 정보 (올바른 비밀번호 사용!)
$servername = "localhost";
$username = "db_user";
$password = "APlux7";
$dbname = "gora_db";

// (3) DB 연결
$conn = new mysqli($servername, $username, $password, $dbname);

// (4) DB 연결 실패 시 에러 응답
if ($conn->connect_error) {
    echo json_encode([
        'status' => 'error',
        'message' => 'DB Connection failed: ' . $conn->connect_error,
        'inserted_id' => 0
    ]);
    exit();
}

// GET 요청 처리 - 로그 조회
if ($_SERVER['REQUEST_METHOD'] === 'GET') {
    // (5) ID 파라미터 확인
    if (!isset($_GET['id']) || empty($_GET['id'])) {
        echo json_encode([
            'status' => 'error',
            'message' => 'ID parameter is required',
            'log' => null
        ]);
        exit();
    }
    
    $logId = intval($_GET['id']);
    
    // (6) 로그 조회 쿼리
    $stmt = $conn->prepare("SELECT id, score, accidentDetails, log_time FROM play_log WHERE id = ?");
    $stmt->bind_param("i", $logId);
    $stmt->execute();
    $result = $stmt->get_result();
    
    if ($result->num_rows > 0) {
        $log = $result->fetch_assoc();
        echo json_encode([
            'status' => 'success',
            'message' => 'Log retrieved successfully',
            'log' => [
                'id' => $log['id'],
                'score' => $log['score'],
                'accidentDetails' => $log['accidentDetails'],
                'created_at' => $log['log_time']  // log_time을 created_at으로 매핑
            ]
        ]);
    } else {
        echo json_encode([
            'status' => 'error', 
            'message' => 'Log not found',
            'log' => null
        ]);
    }
    
    $stmt->close();
}
// POST 요청 처리 - 로그 저장 (기존 코드)
else if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    // (7) Unity(C#)에서 보낸 raw JSON 데이터 읽기
    $json_input = file_get_contents('php://input');
    $data = json_decode($json_input, true);

    // (8) JSON 데이터가 비어있거나, C#에서 보낸 'score'가 없으면 에러 응답
    if ($data === null || !isset($data['score']) || !isset($data['accidentDetails'])) {
        echo json_encode([
            'status' => 'error',
            'message' => 'Invalid JSON input or missing data (score/accidentDetails).',
            'inserted_id' => 0
        ]);
        exit();
    }

    // (9) SQL 인젝션 방지 (새 테이블 구조에 맞게 수정됨)
    $stmt = $conn->prepare("INSERT INTO play_log (score, accidentDetails) VALUES (?, ?)");
    // 'is'는 (i)nteger, (s)tring 타입을 의미
    $stmt->bind_param("is", $data['score'], $data['accidentDetails']);

    // (10) 쿼리 실행 및 결과 응답
    if ($stmt->execute()) {
        // (11) C# 스크립트가 기대하는 성공 응답
        echo json_encode([
            'status' => 'success',
            'message' => 'Log saved successfully.',
            'inserted_id' => $conn->insert_id
        ]);
    } else {
        // 쿼리 실행 실패 시 에러 응답
        echo json_encode([
            'status' => 'error',
            'message' => 'Query failed: ' . $stmt->error,
            'inserted_id' => 0
        ]);
    }

    // (12) 연결 종료
    $stmt->close();
}
else {
    echo json_encode([
        'status' => 'error',
        'message' => 'Invalid request method. Use GET or POST.',
        'log' => null
    ]);
}

$conn->close();
?>
