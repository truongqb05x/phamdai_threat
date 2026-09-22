import os
import random
import time
from utils.driver_utils import create_driver
from utils.account_utils import read_all_accounts_data
from config import config

def test_random_profile():
    print("🧪 Bắt đầu chạy test mở Chrome với một Profile ngẫu nhiên...")
    
    print("\nChọn nguồn nội dung bài viết:")
    print("1. Lấy từ file Excel (resources/data.xlsx)")
    print("2. Lấy từ API (Gemini)")
    content_source = input("👉 Nhập lựa chọn (1/2) [Mặc định: 1]: ").strip()
    
    if content_source not in ["1", "2"]:
        content_source = "1"
        
    api_prompt = ""
    if content_source == "2":
        api_prompt = input("\n👉 Nhập yêu cầu/prompt cho AI (VD: Viết một câu nói hay về tình yêu): ").strip()
        if not api_prompt:
            api_prompt = "Viết một status thả thính vui nhộn, ngắn gọn, phù hợp với mạng xã hội Threads."
        
    hashtag_text = input("\n👉 Nhập hashtag muốn thêm (để trống nếu không thêm): ").strip()
        
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
    
    # Lấy proxy và user_agent từ file account.txt
    accounts = read_all_accounts_data()
    proxy_str = None
    ua_str = None
    account_index = -1
    if accounts:
        for idx, (u, cookies, p_str, ua) in enumerate(accounts):
            if u == random_uid:
                proxy_str = p_str
                ua_str = ua
                account_index = idx
                break
                
    proxy_config = None
    proxy_type = getattr(config, 'PROXY_TYPE', 0)
    
    if proxy_type != 0 and proxy_str:
        proxy_parts = proxy_str.split(":")
        if len(proxy_parts) >= 4:
            proxy_config = {
                "host": proxy_parts[0].strip(),
                "port": proxy_parts[1].strip(),
                "user": proxy_parts[2].strip(),
                "pass": proxy_parts[3].strip()
            }
        elif len(proxy_parts) >= 2:
            proxy_config = {
                "host": proxy_parts[0].strip(),
                "port": proxy_parts[1].strip(),
                "user": "",
                "pass": ""
            }
    elif proxy_type == 0:
        print("⚠️ Cấu hình đang chọn 'Không dùng Proxy'.")
    
    driver = None
    try:
        # Mở Chrome với profile ngẫu nhiên, gắn proxy và user_agent
        driver, wait, proxy_config = create_driver(user_data_dir=user_data_dir, proxy_config=proxy_config, user_agent=ua_str)

        
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
                # Chuẩn bị nội dung
                stt_text = ""
                if content_source == "1":
                    try:
                        import pandas as pd
                        data_path = os.path.join(os.getcwd(), "resources", "data.xlsx")
                        if os.path.exists(data_path):
                            df = pd.read_excel(data_path)
                            if account_index != -1 and account_index < len(df):
                                # Lấy nội dung tương ứng với thứ tự tài khoản
                                stt_text = str(df.iloc[account_index]['CONTENT'])
                                print(f"📝 Đã lấy nội dung từ data.xlsx cho tài khoản thứ {account_index + 1}")
                            else:
                                print(f"⚠️ Tài khoản thứ {account_index + 1} vượt quá số dữ liệu trong data.xlsx")
                        else:
                            print(f"⚠️ Không tìm thấy file {data_path}. Vui lòng tạo file có cột STT và CONTENT.")
                    except ImportError:
                        print("⚠️ Vui lòng mở Terminal và chạy lệnh: pip install pandas openpyxl")
                    except Exception as e:
                        print(f"⚠️ Lỗi đọc data.xlsx: {e}")
                elif content_source == "2":
                    print("🔄 Đang gọi Gemini API để tạo nội dung...")
                    try:
                        from google import genai
                        API_KEY = os.environ.get("GEMINI_API_KEY", "")
                        client = genai.Client(api_key=API_KEY)
                        
                        system_rule = "Bạn là một chuyên gia tạo content mạng xã hội Threads. Hãy viết nội dung ngắn gọn, súc tích (1-3 câu), có chứa emoji phù hợp. TRẢ LỜI TRỰC TIẾP bằng nội dung bài đăng, tuyệt đối không xin chào, không diễn giải, không nói 'Đây là nội dung...'"
                        full_prompt = f"Quy tắc (tuân thủ nghiêm ngặt): {system_rule}\n\nYêu cầu của người dùng: {api_prompt}"
                        
                        response = client.models.generate_content(
                            model="gemini-2.5-flash",
                            contents=full_prompt
                        )
                        stt_text = response.text.strip()
                        print("\n✅ Gemini API đã trả về nội dung thành công!")
                        print(f"👉 Nội dung: {stt_text}\n")
                    except ImportError:
                        print("⚠️ Lỗi: Bạn chưa cài thư viện google-genai. Vui lòng mở Terminal chạy lệnh: pip install google-genai")
                        stt_text = ""
                    except Exception as e:
                        print(f"⚠️ Lỗi khi gọi Gemini API: {e}")
                        stt_text = ""
                
                if stt_text:
                    print("📝 Đang chuẩn bị textbox để nhập nội dung...")
                    textbox_xpath = "//div[@role='textbox' and @contenteditable='true']"
                    textbox = wait.until(EC.element_to_be_clickable((By.XPATH, textbox_xpath)))
                    
                    # Bấm vào textbox trước khi gõ
                    driver.execute_script("arguments[0].click();", textbox)
                    time.sleep(1)
                    
                    # Thêm Hashtag / Topic (nếu có) trước
                    if hashtag_text:
                        try:
                            print(f"✍️ Đang gõ hashtag qua ký tự '#': {hashtag_text}")
                            # Gõ dấu # đầu tiên để mở menu hashtag
                            textbox.send_keys("#")
                            time.sleep(1)
                            
                            for char in hashtag_text:
                                textbox.send_keys(char)
                                time.sleep(random.uniform(0.05, 0.1))
                                
                            time.sleep(2) # Đợi popup gợi ý hiện ra như trong ảnh
                            
                            # Nhấn phím mũi tên xuống để focus vào gợi ý và Enter để chọn
                            from selenium.webdriver.common.keys import Keys
                            textbox.send_keys(Keys.ARROW_DOWN)
                            time.sleep(0.5)
                            textbox.send_keys(Keys.ENTER)
                            
                            print("✅ Đã chọn gợi ý hashtag đầu tiên.")
                            
                            # Thêm khoảng trắng để tách hashtag và nội dung chính
                            textbox.send_keys(" ")
                            time.sleep(0.5)
                        except Exception as e:
                            print(f"⚠️ Lỗi khi nhập hashtag: {e}")

                    # Giả lập gõ từng chữ nội dung chính
                    print("📝 Đang nhập nội dung bài viết chính...")
                    for char in stt_text:
                        textbox.send_keys(char)
                        time.sleep(random.uniform(0.02, 0.08))
                        
                    time.sleep(2)

                    # Thêm file đính kèm (nếu có)
                    try:
                        print("📎 Đang tìm ô upload file...")
                        # Thẻ input type='file' bị ẩn thường được dùng để upload
                        file_input_xpath = "//input[@type='file']"
                        file_input = driver.find_element(By.XPATH, file_input_xpath)
                        
                        # Danh sách đường dẫn file muốn đính kèm (có thể 1 hoặc nhiều file)
                        media_paths = [
                            os.path.join(os.getcwd(), "resources", "img", "Threads-Logo.jpg"),
                            os.path.join(os.getcwd(), "resources", "img", "3f0012a97f804141ad8c8ffa93006991_IMG_1264.jpeg"),
                        ]
                        
                        # Lọc ra các file có tồn tại trên máy
                        valid_paths = [p for p in media_paths if os.path.exists(p)]
                        
                        if valid_paths:
                            print(f"🖼️ Đang đính kèm {len(valid_paths)} file...")
                            # Để upload nhiều file trên Selenium, nối các đường dẫn bằng dấu xuống dòng (\n)
                            file_input.send_keys("\n".join(valid_paths))
                            time.sleep(3) # Đợi load hình/video
                        else:
                            print(f"⚠️ Không tìm thấy file nào hợp lệ trong danh sách đường dẫn.")
                    except Exception as e:
                        print(f"⚠️ Không thể đính kèm file: {e}")

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
                        print("⚠️ Không tìm thấy nút Post nào đang hiển thị trên màn hình (hoặc đang tắt tạm thời)!")
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
