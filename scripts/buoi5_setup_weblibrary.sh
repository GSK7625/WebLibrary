#!/usr/bin/env bash
# ==============================================================================
# Script tự động chuẩn bị môi trường Linux & Phân quyền cho WebLibrary (.NET 10)
# Dùng cho Buổi 5: Thực hành Linux (WSL) - User, Group & Permissions
# ==============================================================================

set -e

GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${BLUE}======================================================================${NC}"
echo -e "${GREEN}   BẮT ĐẦU THIẾT LẬP MÔI TRƯỜNG LINUX CHO DỰ ÁN WEBLIBRARY (.NET 10)  ${NC}"
echo -e "${BLUE}======================================================================${NC}"

# 1. Kiểm tra quyền root / sudo
if [ "$EUID" -ne 0 ]; then
  echo -e "${RED}[LỖI] Vui lòng chạy script này với quyền sudo: sudo bash $0${NC}"
  exit 1
fi

# 2. Cài đặt các công cụ cơ bản
echo -e "\n${YELLOW}[1/6] Cập nhật apt và cài đặt các công cụ cần thiết (curl, jq, lsof, nano)...${NC}"
apt update -y
apt install -y curl wget nano iputils-ping jq lsof

# 3. Tạo Group 'appgroup'
echo -e "\n${YELLOW}[2/6] Quản lý Nhóm: Tạo group 'appgroup'...${NC}"
if getent group appgroup > /dev/null 2>&1; then
    echo -e "Group 'appgroup' đã tồn tại."
else
    groupadd appgroup
    echo -e "${GREEN}✓ Đã tạo group 'appgroup' thành công.${NC}"
fi

# 4. Tạo User 'weblib_svc' thuộc 'appgroup' (Không có sudo - Least Privilege)
echo -e "\n${YELLOW}[3/6] Quản lý Người dùng: Tạo system user 'weblib_svc' (Least Privilege)...${NC}"
if id -u weblib_svc > /dev/null 2>&1; then
    echo -e "User 'weblib_svc' đã tồn tại."
else
    useradd -r -g appgroup -s /bin/bash -m weblib_svc
    echo "weblib_svc:Password123@" | chpasswd
    echo -e "${GREEN}✓ Đã tạo user 'weblib_svc' thuộc group 'appgroup'.${NC}"
fi
id weblib_svc

# 5. Tổ chức thư mục triển khai /opt/weblibrary
echo -e "\n${YELLOW}[4/6] Triển khai mã nguồn vào /opt/weblibrary...${NC}"
mkdir -p /opt/weblibrary/logs

SOURCE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
echo -e "Đang sao chép mã nguồn từ: ${SOURCE_DIR} sang /opt/weblibrary/..."
cp -ru "${SOURCE_DIR}"/* /opt/weblibrary/ 2>/dev/null || true

# 6. Thiết lập quyền sở hữu và phân quyền truy cập (chown & chmod)
echo -e "\n${YELLOW}[5/6] Thiết lập Phân quyền Chặt chẽ (Least Privilege)...${NC}"

# Đổi chủ sở hữu toàn bộ thư mục cho weblib_svc:appgroup
chown -R weblib_svc:appgroup /opt/weblibrary

# Quyền thư mục: 750 (User có toàn quyền, Group chỉ đọc và duyệt, Others bị cấm)
chmod 750 /opt/weblibrary
chmod 750 /opt/weblibrary/logs

# Quyền file cấu hình nhạy cảm appsettings.json: 640 (Owner rw-, Group r--, Others cấm)
if [ -f "/opt/weblibrary/Library.Presentation/appsettings.json" ]; then
    chmod 640 /opt/weblibrary/Library.Presentation/appsettings.json
fi
if [ -f "/opt/weblibrary/Library.Presentation/appsettings.Development.json" ]; then
    chmod 640 /opt/weblibrary/Library.Presentation/appsettings.Development.json
fi

echo -e "\n${YELLOW}[6/6] Kiểm tra kết quả phân quyền (ls -la):${NC}"
ls -ld /opt/weblibrary
ls -l /opt/weblibrary/Library.Presentation/appsettings*.json

echo -e "\n${BLUE}======================================================================${NC}"
echo -e "${GREEN}   THIẾT LẬP HOÀN TẤT! CÁC BƯỚC TIẾP THEO ĐỂ DEMO VỚI THẦY:          ${NC}"
echo -e "${BLUE}======================================================================${NC}"
echo -e "1. Chuyển sang user dịch vụ:      ${YELLOW}sudo -u weblib_svc -i${NC}"
echo -e "2. Di chuyển vào thư mục code:    ${YELLOW}cd /opt/weblibrary/Library.Presentation${NC}"
echo -e "3. Khởi chạy Web API:             ${YELLOW}dotnet run --urls 'http://0.0.0.0:5000'${NC}"
echo -e "4. Mở terminal khác và test API:  ${YELLOW}bash /opt/weblibrary/scripts/test_api.sh${NC}"
echo -e "${BLUE}======================================================================${NC}"
