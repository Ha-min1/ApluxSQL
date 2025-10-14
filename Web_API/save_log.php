<?php
// 1. DB 접속 정보 설정 (Codespaces의 MariaDB 설정과 동일)
$servername = "localhost";
$username = "db_user";     
$password = "db_password"; 
$dbname = "gora_db";       

// 2. Unity에서 POST로 전송된 데이터 받기 (JSON 형식 가정)
$json_data = file_get_contents('php://input');
$data = json_decode($json_data, true);

// 데이터 추출 (Unity C# 스크립트의 LogData 구조체와 변수명이 일치해야 함)
$steering_angle = isset($data['angle']) ? $data['angle'] : 0.0;
$load_pressure = isset($data['pressure']) ? $data['pressure'] : 0.0;
$is_crash = isset($data['crash']) ? 1 : 0; // boolean은 1 또는 0으로 저장

// 3. MySQL 연결
$conn = new mysqli($servername, $username, $password, $dbname);

// 연결 오류 확인
if ($conn->connect_error) {
    // 500 Internal Server Error 발생 방지를 위해 JSON 형식으로 출력
    http_response_code(500);
    die(json_encode(array("status" => "error", "message" => "DB Connection failed: " . $conn->connect_error)));
}

// 4. SQL 쿼리 작성 및 데이터 삽입
$sql = "INSERT INTO play_log (timestamp, steering_angle, load_cell_pressure, is_crash)
        VALUES (NOW(), ?, ?, ?)";

$stmt = $conn->prepare($sql);
$stmt->bind_param("ddi", $steering_angle, $load_pressure, $is_crash); // d: double, i: integer

if ($stmt->execute()) {
    echo json_encode(array("status" => "success", "message" => "Log saved successfully."));
} else {
    echo json_encode(array("status" => "error", "message" => "Error executing query: " . $conn->error));
}

$stmt->close();
$conn->close();
?>
