import os
import random
import time
from utils.driver_utils import create_driver

def test_random_profile():
    print("🧪 Bắt đầu chạy test mở Chrome với một Profile ngẫu nhiên...")
    
    profiles_dir = os.path.join(os.getcwd(), "profiles")
    
    # Kiểm tra xem thư mục profiles có tồn tại không
    if not os.path.exists(profiles_dir):
        print("⚠️ Thư mục 'profiles' chưa tồn tại. Vui lòng chạy tính năng 1 trước để tạo profile.")
        return
        
    # Lấy danh sách các thư mục con trong profiles (chính là các UID)
    available_profiles = [d for d in os.listdir(profiles_dir) if os.path.isdir(os.path.join(profiles_dir, d))]
    
    if not available_profiles:
        print("⚠️ Không có Profile nào bên trong thư mục 'profiles'.")
        return
        
    # Chọn ngẫu nhiên 1 UID
    random_uid = random.choice(available_profiles)
    user_data_dir = os.path.join(profiles_dir, random_uid)
    
    print(f"🎲 Đã chọn ngẫu nhiên Profile: {random_uid}")
    print(f"📁 Đường dẫn Profile: {user_data_dir}")
    
    driver = None
    try:
        # Mở Chrome với profile ngẫu nhiên này
        driver, wait, proxy_config = create_driver(user_data_dir=user_data_dir)
        
        if proxy_config:
            print(f"✅ Đã cấu hình proxy: {proxy_config.get('host')}:{proxy_config.get('port')}")
        else:
            print("⚠️ Không có proxy nào được cấu hình, đang dùng IP trực tiếp.")
            
        print("🌐 Đang chuyển hướng sang trang Threads...")
        driver.get("https://www.threads.net/")
        
        # Thêm bước click để mở modal Đăng bài (Compose Post)
        try:
            print("⏳ Đợi trang Threads ổn định để mở modal Đăng bài...")
            time.sleep(5) # Đợi trang load thêm
            from selenium.webdriver.common.by import By
            from selenium.webdriver.support import expected_conditions as EC
            
            # Phương án 1: Nút "What's new?"
            xpath1 = "//div[@role='button' and @aria-label='Empty text field. Type to compose a new post.']"
            # Phương án 2: Nút icon "Create"
            xpath2 = "//div[@role='button' and .//svg[@aria-label='Create']]"
            
            compose_btn = None
            try:
                compose_btn = wait.until(EC.presence_of_element_located((By.XPATH, xpath1)))
                print("👆 Tìm thấy nút 'What's new?' (Phương án 1).")
            except:
                print("⚠️ Không thấy nút 'What's new?', thử tìm nút icon 'Create' (Phương án 2)...")
                compose_btn = wait.until(EC.presence_of_element_located((By.XPATH, xpath2)))
                print("👆 Tìm thấy nút icon 'Create' (Phương án 2).")
                
            if compose_btn:
                driver.execute_script("arguments[0].scrollIntoView({block: 'center'});", compose_btn)
                time.sleep(1)
                driver.execute_script("arguments[0].click();", compose_btn)
                print("✅ Đã bấm mở modal Đăng bài thành công!")
                
                time.sleep(3) # Đợi modal xuất hiện
                # Đọc nội dung từ resources/stt.txt
                stt_text = ""
                try:
                    with open("resources/stt.txt", "r", encoding="utf-8") as f:
                        stt_text = f.read()
                except Exception as e:
                    print(f"⚠️ Lỗi đọc stt.txt: {e}")
                
                if stt_text:
                    print("📝 Đang nhập nội dung bài viết...")
                    textbox_xpath = "//div[@role='textbox' and @contenteditable='true']"
                    textbox = wait.until(EC.element_to_be_clickable((By.XPATH, textbox_xpath)))
                    
                    # Bấm vào textbox trước khi gõ
                    driver.execute_script("arguments[0].click();", textbox)
                    time.sleep(1)
                    
                    # Giả lập gõ từng chữ như người thật để web nhận diện được nội dung và mở khóa nút Post
                    for char in stt_text:
                        textbox.send_keys(char)
                        time.sleep(random.uniform(0.02, 0.08))
                        
                    time.sleep(2)
                    
                    print("🚀 Đang tìm và bấm nút Post...")
                    # Dùng XPath chặt chẽ: Nút Post phải nằm trong hộp thoại (dialog) để không click nhầm nút tạo bài mới ở ngoài
                    post_btn_xpath = "//div[@role='dialog']//div[@role='button' and (text()='Post' or .//div[text()='Post'] or .//span[text()='Post'])]"
                    
                    # Đợi cho đến khi ít nhất một nút Post xuất hiện
                    wait.until(EC.presence_of_element_located((By.XPATH, post_btn_xpath)))
                    
                    # Quét toàn bộ nút Post trong DOM, chọn cái nào ĐANG HIỂN THỊ
                    buttons = driver.find_elements(By.XPATH, post_btn_xpath)
                    clicked = False
                    for btn in buttons:
                        if btn.is_displayed():
                            driver.execute_script("arguments[0].scrollIntoView({block: 'center'});", btn)
                            time.sleep(1)
                            driver.execute_script("arguments[0].click();", btn)
                            clicked = True
                            print("✅ Đã bấm nút Đăng bài, đang chờ hệ thống xử lý...")
                            
                            # Đợi thông báo 'Posted' xuất hiện góc dưới màn hình
                            try:
                                success_xpath = "//div[text()='Posted'] | //span[text()='Posted']"
                                wait.until(EC.presence_of_element_located((By.XPATH, success_xpath)))
                                print("🎉 THÀNH CÔNG: Đã xác nhận bài viết được đăng lên Threads!")
                            except Exception:
                                print("⚠️ Đã bấm đăng nhưng không nhận được thông báo 'Posted' (Có thể mạng chậm hoặc lỗi).")
                            
                            break
                            
                    if not clicked:
                        print("⚠️ Không tìm thấy nút Post nào đang hiển thị trên màn hình!")
        except Exception as e:
            print(f"⚠️ Không thể mở modal đăng bài: {e}")
        
        print("🎉 Hoàn tất. Vui lòng đóng trình duyệt bằng tay để kết thúc test...")
        # Treo trình duyệt đợi user đóng
        while True:
            try:
                _ = driver.window_handles
                time.sleep(2)
            except Exception:
                print("👋 Đã nhận diện trình duyệt đóng, kết thúc test.")
                break
                
    except Exception as e:
        print(f"❌ Lỗi khi test mở trình duyệt: {e}")
    finally:
        if driver:
            try:
                driver.quit()
            except:
                pass

if __name__ == "__main__":
    test_random_profile()
