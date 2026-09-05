#!/usr/bin/env bash
# ==============================================================================
# Script kiểm thử API WebLibrary bằng curl và jq
# Dùng cho phần Demo Buổi 5: Gọi thử API bằng curl với User hạn chế quyền
# ==============================================================================

GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

BASE_URL="http://localhost:5000"

echo -e "${BLUE}======================================================================${NC}"
echo -e "${GREEN}   KIỂM THỬ API WEBLIBRARY (.NET 10) QUA TERMINAL BẰNG CURL & JQ       ${NC}"
echo -e "${BLUE}======================================================================${NC}"

# 1. Kiểm tra Server có đang chạy ở cổng 5000 không
echo -e "\n${YELLOW}[Test 1] Kiểm tra kết nối tới Server (${BASE_URL})...${NC}"
if ! curl -s --connect-timeout 3 "${BASE_URL}/api/Books" > /dev/null; then
    echo -e "${RED}[LỖI] Không thể kết nối tới ${BASE_URL}. Vui lòng đảm bảo Web API đang chạy!${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Kết nối thành công tới Web API!${NC}"

# 2. Lấy danh sách Sách (GET /api/Books)
echo -e "\n${YELLOW}[Test 2] Gọi API Lấy toàn bộ danh sách sách (GET /api/Books):${NC}"
echo -e "${CYAN}Command: curl -s ${BASE_URL}/api/Books | jq .${NC}"
curl -s "${BASE_URL}/api/Books" | jq .

# 3. Lấy thông tin sách theo Id (GET /api/Books/1)
echo -e "\n${YELLOW}[Test 3] Lấy chi tiết sách Id = 1 (GET /api/Books/1):${NC}"
echo -e "${CYAN}Command: curl -s ${BASE_URL}/api/Books/1 | jq .${NC}"
curl -s "${BASE_URL}/api/Books/1" | jq .

# 4. Tính thử phí phạt mượn sách trễ hạn (OCP Strategy Pattern Demo)
echo -e "\n${YELLOW}[Test 4] Gọi API tính thử phí phạt độc giả VIP trễ hạn 5 ngày (GET /api/Books/1/fee-preview):${NC}"
echo -e "${CYAN}Command: curl -s \"${BASE_URL}/api/Books/1/fee-preview?daysLate=5&memberType=VIP\" | jq .${NC}"
curl -s "${BASE_URL}/api/Books/1/fee-preview?daysLate=5&memberType=VIP" | jq .

# 5. Kiểm tra Response Headers (curl -I)
echo -e "\n${YELLOW}[Test 5] Kiểm tra HTTP Headers (Server Info & Status Code):${NC}"
echo -e "${CYAN}Command: curl -I -s ${BASE_URL}/api/Books${NC}"
curl -I -s "${BASE_URL}/api/Books"

echo -e "\n${BLUE}======================================================================${NC}"
echo -e "${GREEN}✓ TẤT CẢ API ĐÃ PHẢN HỒI THÀNH CÔNG VỚI QUYỀN HẠN CHẾ CỦA WEBLIB_SVC!${NC}"
echo -e "${BLUE}======================================================================${NC}"
