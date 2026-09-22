import os
from google import genai

API_KEY = os.environ.get("GEMINI_API_KEY", "")

client = genai.Client(api_key=API_KEY)

print("=== Gemini Chat ===")
print("Nhập 'exit' để thoát.\n")

while True:
    question = input("Bạn: ")

    if question.lower() == "exit":
        break

    response = client.models.generate_content(
        model="gemini-2.5-flash",
        contents=question
    )

    print("Gemini:", response.text)
    print()