# -*- coding: utf-8 -*-
"""
Main Script: Simplified version to initialize Chrome with Proxy and navigate to Instagram
"""
import time
import sys
from utils.driver_utils import create_driver

# Fix encoding issue on Windows
sys.stdout.reconfigure(encoding='utf-8')

def main():
    print("🚀 Bắt đầu khởi tạo Chrome...")
    account_file = "resources/account.txt"
    uid = None
    cookies = []
    
    print("🍪 Đang đọc tài khoản và cookie...")
    try:
        with open(account_file, "r", encoding="utf-8") as f:
            lines = f.readlines()
            if lines:
                first_line = lines[0].strip()
                parts = first_line.split("|")
                if len(parts) >= 3:
                    uid = parts[0]
                    cookie_str = parts[2]
                    for c in cookie_str.split(";"):
                        c = c.strip()
                        if "=" in c:
                            k, v = c.split("=", 1)
                            cookies.append({"name": k.strip(), "value": v.strip(), "domain": ".instagram.com", "path": "/"})
                else:
                    print("⚠️ File account.txt không đúng định dạng (cần ít nhất 3 phần chia bởi '|').")
            else:
                print("⚠️ File account.txt trống.")
    except FileNotFoundError:
        print(f"⚠️ Không tìm thấy file {account_file}")
    except Exception as e:
        print(f"⚠️ Lỗi khi đọc cookie: {e}")

    # Set profile directory if uid exists
    user_data_dir = None
    if uid:
        import os
        user_data_dir = os.path.join(os.getcwd(), "profiles", uid)
        print(f"📁 Sử dụng profile tại: {user_data_dir}")

    try:
        # Khởi tạo driver với profile cụ thể
        driver, wait, proxy_config = create_driver(user_data_dir=user_data_dir)
        
        if proxy_config:
            print(f"✅ Đã cấu hình proxy: {proxy_config.get('host')}:{proxy_config.get('port')}")
        else:
            print("⚠️ Không có proxy nào được cấu hình, đang dùng IP trực tiếp.")

        print("🌍 Đang mở trang Instagram...")
        driver.get("https://www.instagram.com/")
        time.sleep(3) # Wait for page to initialize domain
        
        if cookies:
            print(f"🔄 Đang nạp {len(cookies)} cookies vào trình duyệt...")
            for cookie in cookies:
                try:
                    driver.add_cookie(cookie)
                except Exception as ce:
                    print(f"⚠️ Lỗi khi nạp cookie {cookie['name']}: {ce}")
            
            print("♻️ Tải lại trang để áp dụng cookie...")
            driver.refresh()

        print("🎉 Hoàn tất. Giữ trình duyệt mở...")
        # Giữ trình duyệt mở để người dùng thao tác
        while True:
            # Kiểm tra xem trình duyệt còn mở không
            _ = driver.window_handles
            time.sleep(5)
            
    except KeyboardInterrupt:
        print("\n⛔ Đã dừng chương trình bởi người dùng.")
    except Exception as e:
        print(f"❌ Lỗi xảy ra: {e}")
    finally:
        try:
            driver.quit()
        except:
            pass

if __name__ == "__main__":
    main()