import json
import os
from pip._vendor import requests

TELEGRAM_TOKEN = os.environ["TELEGRAM_TOKEN"]
SEND_MESSAGE_URL = f"https://api.telegram.org/bot{TELEGRAM_TOKEN}/sendMessage"
SEND_VIDEO_URL = f"https://api.telegram.org/bot{TELEGRAM_TOKEN}/sendVideo"

def lambda_handler(event, context):
    body = json.loads(event.get("body", "{}"))
    message = body.get("message", {})
    chat_id = message.get("chat", {}).get("id")
    text = message.get("text", "")

    if "tiktok.com" not in text:
        Send_message(chat_id, "Это не ссылка")
    else:
        video_url = Get_tiktok_video_url(text)
        
        if video_url:
            Send_video(chat_id, video_url)
        else:
            Send_message(chat_id, "Не удалось скачать видео. Попробуйте другую ссылку.")

    return {"statusCode": 200, "body": json.dumps("OK")}

def Send_message(chat_id, text):
    response = requests.post(SEND_MESSAGE_URL, json={"chat_id": chat_id, "text": text})
    print(response.json()) 
    return response.json()

def Send_video(chat_id, video_url):
    requests.post(SEND_VIDEO_URL, json={"chat_id": chat_id, "video": video_url})

def Get_tiktok_video_url(url):
    try:
        api_url = f"https://www.tikwm.com/api/?url={url}"
        response = requests.get(api_url).json()

        return response.get("data", {}).get("play")
    except Exception as e:
        print(f"Ошибка: {e}")
        return None
