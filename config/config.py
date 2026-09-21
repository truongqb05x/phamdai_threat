# config.py

# --- PROFILE CONFIG ---
# Thư mục chứa profile của Chrome. 
PROFILE_DIR = "profiles"

# --- DRIVER & RESOURCE CONFIG ---
# Trỏ thủ công đến file chromedriver.exe nếu bản tự động tải về bị lỗi [WinError 193] trên VPS
CHROMEDRIVER_PATH = "resources/chromedriver.exe" 

# Ẩn các cảnh báo về thiếu file proxy/useragent nếu đặt là False
RESOURCE_LOGGING = False

# Tải hình ảnh khi chạy trình duyệt
LOAD_IMAGES = False