import argparse
import os, logging, logging.handlers
from Db import Db
from Model import Model
from NotificationBot import NotificationBot
import json
import requests

SCRIPT_DIR, SCRIPT_FILE = os.path.split(os.path.abspath(__file__))

parser = argparse.ArgumentParser(description='Dozor Telegram Bot')
parser.add_argument('--service', action='store_true',
  help='run as service (log to file)')
args = parser.parse_args()


DB_CONFIG_FILE = 'db_config.json'

def load_config():

    with open(DB_CONFIG_FILE, 'r') as f:
        bot_config = json.load(f)
    return bot_config

def initLogger(serviceMode, config):
    if serviceMode:
        LOG_FILENAME = config['logFile']
        LOG_SIZE = config['logSizeMb'] * 1024 * 1024
        LOG_ROTATECOUNT = config['logRotateCount']
    
        logging.basicConfig(filename=LOG_FILENAME,
            format='%(asctime)s - %(name)s - %(levelname)s - %(message)s', 
            level=logging.INFO)
        logger = logging.getLogger(__name__)
        logger.setLevel(logging.INFO)

        logRotationHandler = logging.handlers.RotatingFileHandler(
            LOG_FILENAME, 
            maxBytes=LOG_SIZE, 
            backupCount=LOG_ROTATECOUNT)
        logger.addHandler(logRotationHandler)
        print("SERVICE MODE!!! LOG TO " + LOG_FILENAME)

    else:
        logging.basicConfig(#filename=LOG_FILENAME,
            format='%(asctime)s - %(name)s - %(levelname)s - %(message)s', 
            level=logging.INFO)
        logger = logging.getLogger(__name__)
        logger.setLevel(logging.INFO)
        print("CONSOLE MODE!!!\nPress Ctrl+C for stop")
    return logger

config = load_config()
logger = initLogger(args.service, config)

error_message = """Telegram Bot Settings not found found in DB
Table "settings"
Record key "telegram_bot_settings"
Record value example:
{
    "token": "token_id:token_id_hash",
    "msg_batch_limit": 30,
    "msg_length_limit": 4096,
    "use_proxy": false,
    "proxy_settings": {
        "proxy_url": "http://username:password@proxy:port"
    }
}"""

def check(token):

    url = 'https://api.telegram.org/bot' + token + '/getMe'
    response = requests.get(url)
    if response.status_code != 200:
        raise Exception("Telegram CHECK error Code " + str(response.status_code) + " reason " + response.reason)
    result = response.json()


def main(config, logger):
    
    
    db = Db(config)
    model = Model(db)

    #with open(TG_CONFIG_FILE, 'r') as f:
    #    bot_config = json.load(f)
    
    config_str = model.get_telegram_bot_settings()
    if not config_str:
        logger.error(error_message)
        exit(1)

    bot_config = json.loads(config_str[0])

    # token = Token
    if "Token" in bot_config:
        bot_config["token"] = bot_config["Token"]
    if "token" not in bot_config:
        raise Exception("No Token in config")

    if "sending_period" not in bot_config:
        bot_config["sending_period"] = 24 # 24 hours by default if absent

    if "store_messages_period" not in bot_config:
        bot_config["store_messages_period"] = 24*30 # 30 days by default if absent

    check(bot_config["token"])

    # Set default config values if absent
    bot_config["msg_batch_limit"] = bot_config.get("msg_batch_limit", 30)
    bot_config["msg_length_limit"] = bot_config.get("msg_length_limit", 4096)
    bot_config["use_proxy"] = bot_config.get("use_proxy", False)

    bot = NotificationBot(bot_config, model, logger)
    logger.info('NotificationBot started')
    bot.idle()


if __name__ == '__main__':
    try:
        main(config, logger)
    except Exception as e:
        logger.error(e)
