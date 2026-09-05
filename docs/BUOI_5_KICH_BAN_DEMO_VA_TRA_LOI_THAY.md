# 🐧 BUỔI 5: KỊCH BẢN DEMO THỰC HÀNH LINUX (WSL) & BỘ LỜI GIẢI THÍCH CHO GIẢNG VIÊN

> **Dự án thực tế:** `WebLibrary` - Web API .NET 10 (3-Tier Layered Architecture, SQLite `library.db`, `appsettings.json`, Quản lý thư viện & SOLID Principles).  
> **Mục tiêu buổi thực hành:** Làm chủ môi trường Linux/WSL, quản lý người dùng & nhóm, thiết lập phân quyền file/thư mục theo nguyên tắc an toàn thông tin **Least Privilege** cho server backend thực tế.

---

## 📋 MỤC LỤC
1. [Hướng dẫn chuẩn bị môi trường WSL 2 trên máy](#1-hướng-dẫn-chuẩn-bị-môi-trường-wsl-2-trên-máy)
2. [Kịch bản Demo từng bước từ A - Z (Kèm câu lệnh & Lời giải thích trực tiếp)](#2-kịch-bản-demo-từng-bước-từ-a---z)
   - [Bước 1: Giới thiệu môi trường & các lệnh điều hướng cơ bản](#bước-1-giới-thiệu-môi-trường--các-lệnh-điều-hướng-cơ-bản)
   - [Bước 2: Cài đặt công cụ & Runtime bằng `apt`](#bước-2-cài-đặt-công-cụ--runtime-bằng-apt)
   - [Bước 3: Tạo User & Group riêng cho Backend (Nguyên tắc Least Privilege)](#bước-3-tạo-user--group-riêng-cho-backend)
   - [Bước 4: Chuẩn bị thư mục mã nguồn WebLibrary & Thiết lập Phân quyền (`chmod`, `chown`)](#bước-4-chuẩn-bị-thư-mục-mã-nguồn--thiết-lập-phân-quyền)
   - [Bước 5: Thao tác cấu hình file `appsettings.json` bằng `nano`](#bước-5-thao-tác-cấu-hình-file-appsettingsjson-bằng-nano)
   - [Bước 6: Khởi chạy WebLibrary với User thường & Kiểm thử API bằng `curl`](#bước-6-khởi-chạy-weblibrary-với-user-thường--kiểm-thử-api-bằng-curl)
   - [Bước 7: Kịch bản thực nghiệm đối chứng: User thường vs `root` (`sudo`)](#bước-7-kịch-bản-thực-nghiệm-đối-chứng-user-thường-vs-root-sudo)
3. [Bộ 10 câu hỏi "bẫy" Giảng viên hay hỏi nhất & Câu trả lời ghi điểm tuyệt đối](#3-bộ-10-câu-hỏi-bẫy-giảng-viên-hay-hỏi-nhất--câu-trả-lời)

---

## 1. Hướng dẫn chuẩn bị môi trường WSL 2 trên máy

Nếu máy Windows của bạn gặp thông báo `Class not registered (Error code: Wsl/CallMsi/Install/REGDB_E_CLASSNOTREG)` khi gõ `wsl`:
- **Nguyên nhân:** Gói dịch vụ COM của WSL trên Windows chưa được kích hoạt hoặc bản cài đặt MSI bị lỗi liên kết.
- **Cách khắc phục nhanh nhất (Chạy 1 lệnh duy nhất trong PowerShell với quyền Administrator):**
  ```powershell
  winget install Microsoft.WSL --accept-package-agreements --accept-source-agreements
  winget install Canonical.Ubuntu.2404 --accept-package-agreements --accept-source-agreements
  ```
  *Sau khi cài xong, mở Menu Start gõ **Ubuntu 24.04 LTS** để khởi tạo username/password đầu tiên cho Linux.*

---

## 2. Kịch bản Demo từng bước từ A - Z

### Bước 1: Giới thiệu môi trường & các lệnh điều hướng cơ bản

#### 🎯 Mục tiêu:
Chứng minh với thầy rằng nhóm đã quen thuộc với Terminal Linux, biết kiểm tra thông tin hệ điều hành và vị trí thư mục hiện tại.

#### ⌨️ Các lệnh thực hiện:
```bash
# 1. Xem thông tin hệ điều hành Ubuntu / Linux kernel
uname -a
lsb_release -a

# 2. Xem đường dẫn thư mục hiện tại
pwd

# 3. Xem danh sách tiến trình đang chạy và người dùng hiện tại
whoami
id
```

#### 🗣️ Lời giải thích cho Thầy nghe:
> *"Thưa thầy, trước tiên nhóm em sử dụng terminal của Ubuntu chạy trên nền WSL 2. Lệnh `uname -a` và `lsb_release -a` cho thấy môi trường đang là Linux Kernel và bản phân phối Ubuntu LTS.  
> Hiện tại tài khoản đăng nhập ban đầu là tài khoản cá nhân có quyền dùng `sudo`, nhưng theo chuẩn bảo mật vận hành server, tài khoản này sẽ không dùng để chạy ứng dụng backend."*

---

### Bước 2: Cài đặt công cụ & Runtime bằng `apt`

#### 🎯 Mục tiêu:
Minh họa cơ chế quản lý gói `apt` (Advanced Package Tool), hiểu rõ khi nào bắt buộc dùng `sudo` (vì ghi vào thư mục hệ thống `/usr`, `/etc`).

#### ⌨️ Các lệnh thực hiện:
```bash
# 1. Cập nhật danh sách gói từ repository chính thức
sudo apt update

# 2. Cài đặt các công cụ mạng, biên tập và xử lý JSON cần thiết
sudo apt install -y curl wget nano iputils-ping jq lsof

# 3. Kiểm tra kết nối mạng bằng ping
ping -c 3 google.com

# 4. Cài đặt .NET 10 SDK / ASP.NET Core Runtime (để build và chạy WebLibrary)
# Ubuntu 24.04 đã có sẵn gói dotnet trên kho chính thức của Microsoft/Ubuntu:
sudo apt install -y dotnet-sdk-10.0 || sudo apt install -y dotnet-sdk-9.0 || sudo apt install -y dotnet-sdk-8.0

# Kiểm tra phiên bản dotnet đã cài thành công
dotnet --version
```

#### 🗣️ Lời giải thích cho Thầy nghe:
> *"Thưa thầy, `apt update` không phải là nâng cấp phần mềm mà là tải lại danh sách phiên bản gói mới nhất (package metadata) từ các mirror repository.  
> Vì thao tác cài đặt phần mềm can thiệp vào các thư mục hệ thống như `/usr/bin`, `/etc/`, nên bắt buộc ta phải dùng tiền tố `sudo` để mượn quyền quản trị viên `root`.  
> Nhóm em cài đặt các công cụ:
> - `curl`: Dùng để gửi HTTP Request kiểm thử API trực tiếp từ console.
> - `jq`: Định dạng và parse JSON response từ Web API cho dễ đọc.
> - `ping`: Kiểm tra kết nối mạng (ICMP protocol).
> - `nano`: Trình soạn thảo văn bản trực quan ngay trong terminal.
> - `dotnet-sdk`: Runtime và compiler để biên dịch, vận hành ứng dụng WebLibrary của nhóm."*

---

### Bước 3: Tạo User & Group riêng cho Backend

#### 🎯 Mục tiêu:
Áp dụng **Nguyên tắc Đặc quyền tối thiểu (Principle of Least Privilege)**. Tuyệt đối không chạy Web API bằng user `root`.

#### ⌨️ Các lệnh thực hiện:
```bash
# 1. Tạo một nhóm người dùng chuyên dụng cho ứng dụng (appgroup)
sudo groupadd appgroup

# 2. Tạo một user hệ thống chuyên dụng tên là weblib_svc thuộc nhóm appgroup
# -r: system user (không có hạn dùng, UID hệ thống < 1000)
# -s /bin/bash: shell làm việc
# -m: tạo thư mục home /home/weblib_svc
sudo useradd -r -g appgroup -s /bin/bash -m weblib_svc

# 3. Đặt mật khẩu cho user (nếu cần switch user)
echo "weblib_svc:Password123@" | sudo chpasswd

# 4. Kiểm tra user vừa tạo bằng lệnh id
id weblib_svc
```

#### 🖥️ Output mong đợi:
```
uid=998(weblib_svc) gid=1001(appgroup) groups=1001(appgroup)
```

#### 🗣️ Lời giải thích cho Thầy nghe:
> *"Thưa thầy, trong thực tế production, nếu ta chạy ứng dụng bằng user `root`, khi ứng dụng có lỗ hổng bảo mật (ví dụ: lỗi File Upload, Deserialization, hoặc Remote Code Execution - RCE), kẻ tấn công sẽ lập tức có toàn quyền `root` để xóa sạch ổ đĩa, cài mã độc, đánh cắp mật khẩu hệ thống.  
> Vì vậy, nhóm em tạo một user chuyên dụng `weblib_svc` và group `appgroup`. User này là user dịch vụ, hoàn toàn **KHÔNG CÓ QUYỀN SUDO**, không nằm trong group `sudo` hay `wheel`, do đó phạm vi ảnh hưởng chỉ bị cô lập trong thư mục của ứng dụng."*

---

### Bước 4: Chuẩn bị thư mục mã nguồn & Thiết lập Phân quyền (`chmod`, `chown`)

#### 🎯 Mục tiêu:
Demo quy trình tổ chức thư mục chuẩn Linux tại `/var/www/weblibrary` hoặc `/opt/weblibrary`, minh họa chi tiết ma trận phân quyền theo số Bát phân (Octal).

#### ⌨️ Các lệnh thực hiện:
```bash
# 1. Tạo thư mục chứa ứng dụng chuẩn Linux trong /opt
sudo mkdir -p /opt/weblibrary/logs

# 2. Sao chép toàn bộ source code / mã nguồn WebLibrary vào /opt/weblibrary
# (Lấy trực tiếp từ ổ C Windows thông qua mount point /mnt/c/)
sudo cp -r /mnt/c/Users/dotru/source/repos/WebLibrary/* /opt/weblibrary/

# 3. Chuyển quyền sở hữu (chown) toàn bộ thư mục cho weblib_svc và nhóm appgroup
sudo chown -R weblib_svc:appgroup /opt/weblibrary

# 4. Phân quyền thư mục (chmod 750):
# - Owner (weblib_svc): rwx (7) -> Đọc, ghi, truy cập thư mục
# - Group (appgroup):   r-x (5) -> Đọc và truy cập
# - Others (Người lạ):  --- (0) -> CẤM HOÀN TOÀN
sudo chmod 750 /opt/weblibrary
sudo chmod 750 /opt/weblibrary/logs

# 5. Phân quyền file cấu hình nhạy cảm appsettings.json (chmod 640):
# - Owner: rw- (6) -> Đọc và ghi cấu hình
# - Group: r-- (4) -> Chỉ đọc
# - Others: --- (0) -> CẤM ĐỌC (bảo vệ connection string, secret key)
sudo chmod 640 /opt/weblibrary/Library.Presentation/appsettings.json
sudo chmod 640 /opt/weblibrary/Library.Presentation/appsettings.Development.json

# 6. Kiểm tra lại kết quả phân quyền bằng ls -la
ls -la /opt/weblibrary
ls -la /opt/weblibrary/Library.Presentation/appsettings.json
```

#### 🖥️ Output mong đợi:
```
drwxr-x--- 6 weblib_svc appgroup 4096 Sep  3 07:00 /opt/weblibrary
-rw-r----- 1 weblib_svc appgroup  151 Sep  3 07:00 /opt/weblibrary/Library.Presentation/appsettings.json
```

#### 🗣️ Lời giải thích cho Thầy nghe:
> *"Thưa thầy, tại sao nhóm em sử dụng số `750` và `640` mà không dùng `777`?  
> - `chmod 777` là một sai lầm nghiêm trọng trong bảo mật vì bất kỳ user nào trong hệ thống cũng có thể sửa hoặc xóa file.  
> - Đối với thư mục ứng dụng `/opt/weblibrary`, nhóm dùng **`750`** (`rwxr-x---`): Chỉ định user chạy app `weblib_svc` có quyền toàn diện (7), các thành viên nhóm bảo trì `appgroup` có quyền vào xem (5), còn tất cả những user khác trong hệ điều hành bị cấm tiệt (0).  
> - Đối với file cấu hình `appsettings.json`, nhóm đặt **`640`** (`rw-r-----`): File này chứa chuỗi kết nối Database và các cấu hình nhạy cảm. Do đó chỉ duy nhất user `weblib_svc` được sửa, group được đọc để kiểm tra, còn `others` là 0 - hoàn toàn không thể xem trộm cấu hình hệ thống."*

---

### Bước 5: Thao tác cấu hình file `appsettings.json` bằng `nano`

#### 🎯 Mục tiêu:
Thành thạo trình soạn thảo `nano` để chỉnh sửa file cấu hình hoặc biến môi trường trực tiếp trên server không cần GUI.

#### ⌨️ Các lệnh thực hiện:
```bash
# Mở file cấu hình appsettings.json bằng nano
nano /opt/weblibrary/Library.Presentation/appsettings.json
```

*Trong giao diện `nano`, chỉnh sửa nội dung bổ sung phần `Kestrel` để quy định cổng chạy (Port 5000) và cấu hình Log:*

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      }
    }
  }
}
```

*Thao tác phím tắt cần demo:*
1. **Lưu file:** Nhấn tổ hợp phím `Ctrl + O`, sau đó nhấn `Enter`.
2. **Thoát nano:** Nhấn tổ hợp phím `Ctrl + X`.
3. **Thao tác nhanh:** `Ctrl + K` (Cắt 1 dòng), `Ctrl + U` (Dán dòng vừa cắt).

#### 🗣️ Lời giải thích cho Thầy nghe:
> *"Thưa thầy, trên các máy chủ Linux thực tế đa phần là phiên bản Server Headless (không có giao diện đồ họa Desktop GUI). Mọi thao tác cấu hình đều thực hiện qua terminal bằng `nano` hoặc `vim`.  
> Ở đây nhóm em đã dùng `nano` để cấu hình cổng lắng nghe `http://0.0.0.0:5000` cho Kestrel Web Server của .NET và lưu lại bằng phím tắt `Ctrl+O` rồi thoát bằng `Ctrl+X`."*

---

### Bước 6: Khởi chạy WebLibrary với User thường & Kiểm thử API bằng `curl`

#### 🎯 Mục tiêu:
Chứng minh ứng dụng backend .NET 10 chạy hoàn toàn ổn định với quyền của user dịch vụ `weblib_svc`, không cần quyền `root`. Kiểm thử API trả về dữ liệu chuẩn JSON bằng `curl`.

#### ⌨️ Các lệnh thực hiện:

```bash
# 1. Chuyển sang phiên làm việc của user weblib_svc
sudo -u weblib_svc -i

# 2. Kiểm tra lại danh tính user hiện tại
whoami
# Kết quả hiển thị: weblib_svc (KHÔNG PHẢI ROOT!)

# 3. Di chuyển vào thư mục Presentation và khởi chạy ứng dụng
cd /opt/weblibrary/Library.Presentation
dotnet run --urls "http://0.0.0.0:5000"
```

*(Mở một cửa sổ terminal WSL thứ 2 để kiểm thử API trong lúc server đang chạy):*

```bash
# 1. Kiểm tra tiến trình dotnet và cổng 5000 đang lắng nghe
lsof -i :5000 || ss -tulpn | grep 5000

# 2. Gọi thử API lấy danh sách Sách (GET /api/Books) bằng curl
curl -i http://localhost:5000/api/Books

# 3. Dùng curl kết hợp jq để parse JSON chuyên nghiệp
curl -s http://localhost:5000/api/Books | jq .

# 4. Gọi API tính phí phạt mượn sách trễ hạn (Minh họa nguyên lý SOLID OCP)
curl -s "http://localhost:5000/api/Books/1/fee-preview?daysLate=5&memberType=VIP" | jq .

# 5. Kiểm tra file database SQLite và log tự động sinh ra đúng quyền sở hữu của weblib_svc
ls -la /opt/weblibrary/Library.Presentation/library.db*
```

#### 🖥️ Output JSON thực tế nhận được từ curl:
```json
[
  {
    "id": 1,
    "title": "Clean Code",
    "author": "Robert C. Martin",
    "category": "Technology",
    "isbn": "978-0132350884",
    "baseLateFeePerDay": 5000,
    "isAvailable": true
  }
]
```

#### 🗣️ Lời giải thích cho Thầy nghe:
> *"Thưa thầy, như thầy thấy trên màn hình:  
> - Ứng dụng WebLibrary đang được vận hành bởi tiến trình của user `weblib_svc`.  
> - Lệnh `curl -i http://localhost:5000/api/Books` trả về HTTP Status Code `200 OK`, cùng toàn bộ danh sách dữ liệu sách từ cơ sở dữ liệu SQLite `library.db`.  
> - Đồng thời, lệnh `curl` gọi API tính phí phạt trễ hạn với độc giả VIP `fee-preview` chạy thành công thuật toán OCP Strategy Pattern của nhóm.  
> - File CSDL `library.db` được sinh ra tự động và gán quyền sở hữu chính xác cho `weblib_svc`. Điều này chứng minh ứng dụng hoạt động hoàn hảo với đặc quyền bị hạn chế."*

---

### Bước 7: Kịch bản thực nghiệm đối chứng: User thường vs `root` (`sudo`)

#### 🎯 Mục tiêu:
Thực hiện yêu cầu bắt buộc của môn học: **"Minh họa sự khác biệt khi thao tác (đọc/ghi file, cài đặt gói, khởi động service) với quyền user thường và quyền root (sudo), và khi nào bắt buộc phải dùng sudo."**

#### ⌨️ Các lệnh thực nghiệm:

#### Thí nghiệm 1: Cài đặt gói bằng user thường không có sudo
```bash
# Đang đứng ở user weblib_svc, gõ thử lệnh cài đặt:
apt install htop
```
* **Kết quả terminal:**  
  `E: Could not open lock file /var/lib/dpkg/lock-frontend - open (13: Permission denied)`  
  `E: Unable to acquire the dpkg frontend lock, are you root?`
* **Giải thích cho Thầy:** Quản lý gói phần mềm sửa đổi thư viện dùng chung của toàn hệ thống, user thường không có quyền can thiệp.

#### Thí nghiệm 2: Cố tình đọc/sửa file mật khẩu hệ thống `/etc/shadow`
```bash
cat /etc/shadow
```
* **Kết quả terminal:**  
  `cat: /etc/shadow: Permission denied`
* **Giải thích cho Thầy:** `/etc/shadow` chứa hash mật khẩu của tất cả user trên Linux, được phân quyền `chmod 640` hoặc `chmod 000` thuộc về `root:shadow`. User thường bị từ chối truy cập ngay lập tức, ngăn chặn rò rỉ hash mật khẩu.

#### Thí nghiệm 3: Dùng `sudo` để thực thi tác vụ quản trị
```bash
# Thoát khỏi weblib_svc trở về tài khoản quản trị
exit

# Đọc file bằng quyền root qua sudo
sudo head -n 3 /etc/shadow
```
* **Kết quả:** Hiển thị thông tin thành công vì `sudo` nâng quyền tương đương quyền `root`.

#### 🗣️ Tổng kết câu nói cho Thầy:
> *"Thưa thầy, qua các thí nghiệm đối chứng trên, nhóm em đúc kết 3 nguyên tắc bất di bất dịch khi vận hành server:  
> 1. **Chỉ dùng `sudo` khi cấu hình hạ tầng:** Cài đặt gói (`apt`), cấu hình firewall, tạo user, quản lý system service (`systemctl`).  
> 2. **Chạy ứng dụng bằng User dịch vụ không có sudo:** Để ngăn ngừa nguy cơ Remote Code Execution leo thang đặc quyền.  
> 3. **Phân quyền tối thiểu (`chmod 750`, `640`):** Không bao giờ dùng `chmod 777` trên môi trường máy chủ."*

---

## 3. Bộ 10 câu hỏi "bẫy" Giảng viên hay hỏi nhất & Câu trả lời

### ❓ Câu 1: Tại sao không chạy luôn ứng dụng bằng tài khoản `root` cho tiện, đỡ phải phân quyền phức tạp?
* **Trả lời:**
  > *"Dạ thưa thầy, chạy ứng dụng bằng `root` vi phạm nghiêm trọng nguyên tắc bảo mật **Least Privilege**. Trong thực tế, nếu ứng dụng Web API của mình bị một lỗ hổng bảo mật (ví dụ như lỗ hổng upload file shell, Command Injection, hoặc lỗi thư viện thứ ba), hacker sẽ thực thi mã độc với quyền của user đang chạy tiến trình đó.  
  > Nếu chạy bằng `root`, hacker sẽ chiếm toàn quyền máy chủ, có thể đọc trộm file `/etc/shadow`, cài đặt rootkit, mã hóa toàn bộ dữ liệu máy chủ.  
  > Còn nếu ta chạy bằng user giới hạn `weblib_svc` (không có sudo), hacker chỉ bị giam lỏng trong phạm vi thư mục app và không thể can thiệp vào nhân hệ điều hành."*

### ❓ Câu 2: Các con số `750` và `640` trong lệnh `chmod` được tính như thế nào?
* **Trả lời:**
  > *"Dạ thưa thầy, quyền trong Linux biểu diễn bằng hệ Bát phân (Octal) với 3 quyền cơ bản:  
  > - **Read (r)** = 4  
  > - **Write (w)** = 2  
  > - **Execute (x)** = 1  
  > Tổng của 3 quyền là 7 (4+2+1 = đầy đủ rwx).  
  > Cấu trúc 3 chữ số đại diện cho 3 đối tượng: **User (Chủ sở hữu) - Group (Nhóm) - Others (Người ngoài)**.  
  > - Với `chmod 750`:  
  >   + Số `7` = 4+2+1 (`rwx`) cho User `weblib_svc`.  
  >   + Số `5` = 4+0+1 (`r-x`) cho Group `appgroup` (chỉ đọc và cd vào thư mục, không được sửa/xóa).  
  >   + Số `0` = 0+0+0 (`---`) cho Others (cấm hoàn toàn).  
  > - Với `chmod 640`:  
  >   + Số `6` = 4+2+0 (`rw-`) cho User (đọc và sửa cấu hình).  
  >   + Số `4` = 4+0+0 (`r--`) cho Group (chỉ đọc).  
  >   + Số `0` = (`---`) cho Others (cấm đọc mã nguồn/file cấu hình)."*

### ❓ Câu 3: Khác biệt cốt lõi giữa `chmod` và `chown` là gì?
* **Trả lời:**
  > *"Dạ thưa thầy:  
  > - `chown` (Change Owner) dùng để **thay đổi chủ sở hữu và nhóm sở hữu** của file hoặc thư mục (cú pháp: `chown user:group target`).  
  > - `chmod` (Change Mode) dùng để **thay đổi quyền truy cập** (đọc, ghi, thực thi) của các đối tượng (Owner, Group, Others) trên file/thư mục đó."*

### ❓ Câu 4: Lệnh `usermod -aG appgroup username` có cờ `-a` để làm gì? Nếu quên không gõ cờ `-a` thì chuyện gì xảy ra?
* **Trả lời:**
  > *"Dạ thưa thầy, cờ `-a` viết tắt của **Append** (thêm vào).  
  > Đi kèm với `-G` (Secondary Group), lệnh `usermod -aG` sẽ thêm nhóm mới vào danh sách nhóm hiện có của user.  
  > **Nếu quên cờ `-a` (chỉ gõ `usermod -G`)**, hệ điều hành sẽ xóa sạch user đó ra khỏi tất cả các nhóm phụ trước hành (bao gồm cả nhóm `sudo` nếu có) và chỉ giữ lại duy nhất nhóm mới. Điều này có thể khiến tài khoản admin bị mất quyền `sudo` ngay lập tức."*

### ❓ Câu 5: Lệnh `curl` khác gì so với dùng trình duyệt Web hay Postman? Tại sao trên server Linux ta bắt buộc phải thành thạo `curl`?
* **Trả lời:**
  > *"Dạ thưa thầy:  
  > 1. Trình duyệt và Postman yêu cầu giao diện đồ họa (GUI). Nhưng hầu hết server production Linux đều là bản Minimal / Server Core, chỉ có màn hình dòng lệnh (CLI), không thể cài Postman hay Chrome.  
  > 2. `curl` là công cụ dòng lệnh nhẹ, linh hoạt, hỗ trợ đầy đủ các phương thức HTTP (GET, POST, PUT, DELETE, Headers, Auth), có thể chạy trong bash script tự động kiểm tra sức khỏe hệ thống (Health Check, CI/CD pipeline).  
  > 3. Cờ `-i` trong `curl -i` giúp xem cả HTTP Response Headers, cờ `-s` kết hợp với pipe `| jq` giúp format JSON trả về cực kỳ nhanh chóng."*

### ❓ Câu 6: Trong lệnh `useradd`, cờ `-r`, `-m`, `-s` có ý nghĩa gì?
* **Trả lời:**
  > *"Dạ thưa thầy:  
  > - `-r` (System account): Khai báo đây là tài khoản dịch vụ hệ thống (thường có UID nhỏ hơn 1000), không hiển thị trên màn hình đăng nhập người dùng thông thường.  
  > - `-m` (Create home): Tự động tạo thư mục người dùng tại `/home/username`.  
  > - `-s /bin/bash`: Chỉ định shell mặc định khi user đăng nhập. Nếu là tài khoản hoàn toàn không được login, ta có thể đặt `-s /usr/sbin/nologin`."*

### ❓ Câu 7: Vì sao file SQLite `library.db` của WebLibrary lại cần quyền ghi (`write`) cho user chạy ứng dụng?
* **Trả lời:**
  > *"Dạ thưa thầy, SQLite là cơ sở dữ liệu dạng file (file-based database). Không giống như SQL Server hay PostgreSQL chạy dưới dạng dịch vụ riêng biệt qua cổng mạng, SQLite được nạp trực tiếp vào bộ nhớ của tiến trình `WebLibrary`.  
  > Khi ứng dụng khởi chạy và thực thi `InitializeDatabaseAsync()`, nó phải ghi cấu trúc bảng và seed dữ liệu vào file `library.db`. Nếu user `weblib_svc` không có quyền Write vào file này hoặc thư mục chứa nó, SQLite sẽ báo lỗi `SQLite Error 14: unable to open database file` và ứng dụng sập ngay."*

### ❓ Câu 8: Sự khác biệt giữa `apt update` và `apt upgrade` là gì?
* **Trả lời:**
  > *"Dạ thưa thầy:  
  > - `apt update`: Chỉ cập nhật danh sách và metadata các gói từ máy chủ kho phần mềm về máy tính (không thay đổi, không tải mới mã phần mềm nào).  
  > - `apt upgrade`: Dựa vào danh sách metadata vừa cập nhật, tải các bản vá và nâng cấp các phần mềm đã cài đặt trên máy lên phiên bản mới."*

### ❓ Câu 9: Tại sao khi biên tập file bằng `nano` đôi khi bị thông báo `[ Error writing ...: Permission denied ]`?
* **Trả lời:**
  > *"Dạ thưa thầy, đó là do tài khoản hiện tại không có quyền ghi (`Write`) vào file đó hoặc vào thư mục cha chứa nó. Ví dụ khi mở file cấu hình `/etc/nginx/nginx.conf` bằng user thường thì chỉ mở ở chế độ chỉ đọc (Read-only). Để sửa được, ta bắt buộc phải mở bằng `sudo nano /etc/nginx/nginx.conf`."*

### ❓ Câu 10: Nếu muốn ứng dụng WebLibrary tự động chạy nền và tự khởi động lại khi server restart thì trong Linux ta dùng cơ chế gì?
* **Trả lời:**
  > *"Dạ thưa thầy, ta sẽ tạo một **Systemd Service** tại đường dẫn `/etc/systemd/system/weblibrary.service`.  
  > Trong file service đó, ta khai báo rõ `User=weblib_svc`, `Group=appgroup`, `WorkingDirectory=/opt/weblibrary/Library.Presentation`, `ExecStart=/usr/bin/dotnet ...`.  
  > Sau đó dùng lệnh `sudo systemctl enable --now weblibrary.service` để Linux tự quản lý tiến trình dưới dạng daemon chạy ngầm."*

---

## 4. Tóm tắt các lệnh "Cứu cánh" khi đang đứng trước Thầy

| Thao tác cần làm | Câu lệnh chuẩn |
| :--- | :--- |
| Xem user hiện tại | `whoami && id` |
| Xem quyền thư mục dạng danh sách chi tiết | `ls -la` |
| Chuyển sang user app để chạy demo | `sudo -u weblib_svc -i` |
| Xem cổng 5000 có app nào đang chạy | `lsof -i :5000` hoặc `ss -tulpn \| grep 5000` |
| Kiểm tra RAM & CPU ứng dụng đang chiếm | `top` hoặc `htop` |
| Gọi API test nhanh bằng curl | `curl -s http://localhost:5000/api/Books \| jq .` |
| Kill tiến trình nếu app bị treo cổng | `fuser -k 5000/tcp` |

---
*Tài liệu được thiết kế bám sát 100% dự án `WebLibrary` phục vụ buổi thực hành số 5.*
