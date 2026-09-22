# -*- coding: utf-8 -*-
import sys
from utils.account_utils import read_all_accounts_data
from features.threads.connect import connect_threads, open_chrome_only
from features.post_threads.post_threads import post_to_threads

# Fix encoding issue on Windows
sys.stdout.reconfigure(encoding='utf-8')

def main():
    if len(sys.argv) > 1:
        if sys.argv[1] == "open_chrome" and len(sys.argv) > 2:
            target_uid = sys.argv[2]
            accounts = read_all_accounts_data()
            if not accounts:
                return
            
            for uid, cookies, proxy_str, ua_str in accounts:
                if uid == target_uid:
                    open_chrome_only(uid, proxy_str, ua_str)
                    return
            return
        elif sys.argv[1] == "connect_thread" and len(sys.argv) > 2:
            target_uid = sys.argv[2]
            accounts = read_all_accounts_data()
            if not accounts:
                return
            
            for uid, cookies, proxy_str, ua_str in accounts:
                if uid == target_uid:
                    connect_threads(uid, cookies, proxy_str, ua_str)
                    return
            return
        elif sys.argv[1] == "post_thread" and len(sys.argv) > 2:
            target_uid = sys.argv[2]
            content_source = sys.argv[3] if len(sys.argv) > 3 else "1"
            api_prompt = sys.argv[4] if len(sys.argv) > 4 else ""
            hashtag_text = sys.argv[5] if len(sys.argv) > 5 else ""
            media_paths_str = sys.argv[6] if len(sys.argv) > 6 else ""
            media_paths = [p for p in media_paths_str.split("|") if p.strip()] if media_paths_str else []
            
            accounts = read_all_accounts_data()
            if not accounts:
                return
            
            for idx, (uid, cookies, proxy_str, ua_str) in enumerate(accounts):
                if uid == target_uid:
                    post_to_threads(uid, cookies, proxy_str, ua_str, idx, content_source, api_prompt, hashtag_text, media_paths)
                    return
            return

    while True:
        print("\n" + "="*40)
        print("          MENU CHỨC NĂNG")
        print("="*40)
        print("1. Kết nối với threads")
        print("2. Đăng bài lên Threads")
        print("0. Thoát")
        print("="*40)
        choice = input("👉 Nhập lựa chọn của bạn: ").strip()
        
        if choice == "1":
            accounts = read_all_accounts_data()
            if not accounts:
                print("⚠️ Lỗi: Không thể tải dữ liệu tài khoản, vui lòng kiểm tra resources/account.txt")
            else:
                for uid, cookies, proxy_str, ua_str in accounts:
                    success = connect_threads(uid, cookies, proxy_str, ua_str)
                    if not success:
                        print(f"⏭️ Bỏ qua tài khoản {uid}, chuyển sang tài khoản tiếp theo...")
                print("\n✅ Đã duyệt xong toàn bộ tài khoản trong file!")
            break
        elif choice == "2":
            accounts = read_all_accounts_data()
            if not accounts:
                print("⚠️ Lỗi: Không thể tải dữ liệu tài khoản, vui lòng kiểm tra resources/account.txt")
            else:
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
                
                for idx, (uid, cookies, proxy_str, ua_str) in enumerate(accounts):
                    success = post_to_threads(uid, cookies, proxy_str, ua_str, idx, content_source, api_prompt, hashtag_text, [])
                    if not success:
                        print(f"⏭️ Bỏ qua tài khoản {uid}, chuyển sang tài khoản tiếp theo...")
                print("\n✅ Đã duyệt xong toàn bộ tài khoản trong file!")
            break
        elif choice == "0":
            print("👋 Đã thoát chương trình.")
            break
        else:
            print("⚠️ Lựa chọn không hợp lệ. Vui lòng thử lại!")

if __name__ == "__main__":
    main()