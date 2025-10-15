# ApluxSQL
#시작 시 아래 두 개를 입력해서 가동
이 저장소는 '길 건너 고라니' 프로젝트의 **중앙 데이터베이스 서버(API)** 코드를 담고 있습니다.

---

## 1. 서버 가동 및 접속 (Codespaces 터미널)

Codespaces를 실행한 후, 다음 명령어를 순서대로 입력하여 서버를 가동합니다.
# MariaDB 시작
sudo service mariadb start

# Apache2 시작
sudo service apache2 start
# 데이터베이스 접근
sudo mysql

# 서버 상태 확인 (정상 작동 시 "is running" 또는 "active" 확인)
sudo service mariadb status
sudo service apache2 status

# API 파일 수정 후에는 반드시 Apache를 재시작해야 적용됩니다.
sudo service apache2 restart


# DB 사용자 계정으로 접속 및 데이터 확인
mysql -u db_user -p

# (비밀번호: APlux7 입력)

# 데이터베이스 선택 및 데이터 조회
USE gora_db;
SELECT * FROM play_log;

# 셸 종료: EXIT;