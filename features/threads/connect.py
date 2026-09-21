import time
import os
from utils.driver_utils import create_driver

def connect_threads(uid, cookies):
    """
    Khởi tạo trình duyệt, nạp cookie Instagram và điều hướng sang Threads.
    Returns:
        bool: True nếu login thành công, False nếu thất bại (cookie chết).
    """
    print(f"\n🚀 Đang xử lý tài khoản: {uid}")

    user_data_dir = None
    if uid:
        user_data_dir = os.path.join(os.getcwd(), "profiles", uid)
        print(f"📁 Profile: {user_data_dir}")

    driver = None
    try:
        driver, wait, proxy_config = create_driver(user_data_dir=user_data_dir)
        
        if proxy_config:
            print(f"✅ Proxy: {proxy_config.get('host')}:{proxy_config.get('port')}")
        else:
            print("⚠️ Dùng IP trực tiếp.")

        print("🌍 Mở Instagram để kiểm tra phiên đăng nhập...")
        driver.get("https://www.instagram.com/")
        time.sleep(3)
        
        # Kiểm tra xem profile đã có sẵn cookie đăng nhập chưa (sessionid)
        is_logged_in = False
        if driver.get_cookie("sessionid") or "login" not in driver.current_url.lower():
            # Có thể đã đăng nhập, thử lấy cookie sessionid để chắc chắn
            if driver.get_cookie("sessionid"):
                is_logged_in = True
                print("⚡ Profile đã lưu phiên đăng nhập, bỏ qua bước nạp Cookie mới!")
        
        if not is_logged_in:
            if cookies:
                print(f"🔄 Profile chưa đăng nhập. Đang nạp {len(cookies)} cookies mới...")
                for cookie in cookies:
                    try:
                        driver.add_cookie(cookie)
                    except Exception:
                        pass
                
                print("♻️ Làm mới trang Instagram...")
                driver.refresh()
                time.sleep(5) # Đợi trang load xong để kiểm tra trạng thái
                
                # Kiểm tra lại trạng thái đăng nhập
                if "login" in driver.current_url.lower() or not driver.get_cookie("sessionid"):
                    print("❌ Cookie đã chết (bị đá ra trang Login).")
                    return False
                
                print("✅ Đăng nhập Instagram bằng Cookie thành công!")
            else:
                print("❌ Không có cookie để đăng nhập.")
                return False
        
        print("🌐 Đang chuyển hướng sang trang Threads...")
        driver.get("https://www.threads.net/")

        try:
            print("⏳ Đợi hộp thoại 'Join with Instagram' xuất hiện...")
            from selenium.webdriver.common.by import By
            from selenium.webdriver.support import expected_conditions as EC
            
            # Dùng XPath tối giản: tìm thẻ div đóng vai trò button, bên trong có chứa icon Instagram
            xpath = "//div[@role='button' and .//i[@aria-label='Instagram']]"
            
            # Đợi phần tử có mặt trong DOM (không dùng clickable vì đôi khi thẻ div bị thẻ span che khuất)
            login_btn = wait.until(EC.presence_of_element_located((By.XPATH, xpath)))
            print("👆 Đã tìm thấy nút đăng nhập, đang tiến hành click...")
            
            # Dùng Javascript click để ép click xuyên qua các lớp phủ (overlays) hoặc element bị che
            driver.execute_script("arguments[0].scrollIntoView({block: 'center'});", login_btn)
            time.sleep(1)
            driver.execute_script("arguments[0].click();", login_btn)
            
            time.sleep(5)
            print("✅ Đã bấm đăng nhập Threads bằng Instagram thành công!")
        except Exception as e:
            print(f"⚠️ Không tìm thấy nút đăng nhập Threads tự động hoặc có lỗi: {e}")

        import random
        wait_time = random.randint(1, 10)
        print(f"🎉 Hoàn tất kết nối Threads. Tự động chuyển sang tài khoản tiếp theo sau {wait_time} giây...")
        time.sleep(wait_time)
                
        return True
            
    except Exception as e:
        print(f"❌ Lỗi khi xử lý tài khoản {uid}: {e}")
        return False
    finally:
        if driver:
            try:
                driver.quit()
            except:
                pass
