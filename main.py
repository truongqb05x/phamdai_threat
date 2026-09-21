# -*- coding: utf-8 -*-
import sys
from utils.account_utils import read_all_accounts_data
from features.threads.connect import connect_threads, open_chrome_only

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

    while True:
        print("\n" + "="*40)
        print("          MENU CHỨC NĂNG")
        print("="*40)
        print("1. Kết nối với threads")
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
        elif choice == "0":
            print("👋 Đã thoát chương trình.")
            break
        else:
            print("⚠️ Lựa chọn không hợp lệ. Vui lòng thử lại!")

if __name__ == "__main__":
    main()