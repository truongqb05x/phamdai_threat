import os

def read_all_accounts_data(file_path="resources/account.txt"):
    """
    Reads the account file and extracts UID and cookies for all accounts.
    Format: uid|password|cookie|token...
    Returns: A list of tuples [(uid, cookies), (uid, cookies), ...]
    """
    accounts = []
    
    print("🍪 Đang đọc toàn bộ tài khoản và cookie...")
    try:
        if not os.path.exists(file_path):
            print(f"⚠️ Không tìm thấy file {file_path}")
            return accounts
            
        with open(file_path, "r", encoding="utf-8") as f:
            lines = f.readlines()
            for i, line in enumerate(lines):
                line = line.strip()
                if not line:
                    continue
                parts = line.split("|")
                if len(parts) >= 3:
                    uid = parts[0]
                    import time
                    expiry_time = int(time.time()) + 365 * 24 * 3600 # 1 năm
                    
                    cookie_str = parts[2]
                    cookies = []
                    for c in cookie_str.split(";"):
                        c = c.strip()
                        if "=" in c:
                            k, v = c.split("=", 1)
                            cookies.append({
                                "name": k.strip(), 
                                "value": v.strip(), 
                                "domain": ".instagram.com", 
                                "path": "/",
                                "expiry": expiry_time
                            })
                    
                    proxy_str = parts[7].strip() if len(parts) > 7 else None
                    ua_str = parts[8].strip() if len(parts) > 8 else None
                    
                    accounts.append((uid, cookies, proxy_str, ua_str))
                else:
                    print(f"⚠️ Dòng {i+1} trong file tài khoản không đúng định dạng.")
                    
        print(f"✅ Đã tìm thấy {len(accounts)} tài khoản hợp lệ.")
    except Exception as e:
        print(f"⚠️ Lỗi khi đọc dữ liệu tài khoản: {e}")
        
    return accounts
