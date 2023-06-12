import re
from Db import Db


class Model:

    TELEGRAM_BOT_SETTINGS_KEY = "telegram_bot_settings"

    def __init__(self, db: Db):

        self.db = db

    def store_contact(self, user_id, phone_number):

        clean_phone=re.sub(r'[^\d]','', phone_number)

        conn = self.db.connection()
        cur = conn.cursor()
        cmd = """
           UPDATE app_users AS app
           SET telegram_user_id = {}
           FROM aspnet_users AS asp 
           WHERE app.id = asp.id AND asp.phone_number ~ '{}$'
        """.format(user_id, clean_phone)
        cur.execute(cmd)
        conn.commit()

    def get_user_by_phone(self, phone_number):

        clean_phone=re.sub(r'[^\d]','', phone_number)
        cur = self.db.cursor() 
        cmd = """
            SELECT app.id, asp.user_name, asp.phone_number, app.telegram_user_id
            FROM app_users app 
            INNER JOIN aspnet_users asp 
            ON app.id = asp.id 
            WHERE phone_number ~ '{}$'
            LIMIT 1
        """.format(clean_phone)
        cur.execute(cmd)
        result = cur.fetchone()

        return result

    def get_user_by_telegram_user_id(self, user_id):

        cur = self.db.cursor()
        cmd = self.db.cursor() 
        cmd = """
            SELECT app.id, asp.user_name, asp.phone_number, app.telegram_user_id
            FROM app_users app 
            INNER JOIN aspnet_users asp 
            ON app.id = asp.id 
            WHERE app.telegram_user_id = {}
            LIMIT 1
        """.format(user_id)
        cur.execute(cmd)
        result = cur.fetchone()

        return result

    def set_messages_sending(self, amount):

        conn = self.db.connection()
        cur = conn.cursor()

        cur.execute("""
            UPDATE telegram_messages 
            SET status = %s 
            WHERE status = %s AND id IN (SELECT id FROM telegram_messages ORDER BY id ASC LIMIT %s)
        """, ('sending', 'pending', amount))
        conn.commit()

    def get_messages_sending(self, limit, sending_period):

        cur = self.db.cursor()
        cmd = """
            SELECT u.telegram_user_id, t.*, a.phone_number, a.user_name 
            FROM telegram_messages AS t 
            INNER JOIN app_users AS u ON (u.legacy_id = t.user_id)
            LEFT JOIN aspnet_users AS a ON (u.id = a.id)
            WHERE status='sending' 
            """
        if (sending_period > 0): # skip if sending_period <= 0
            cmd = cmd + " AND t.create_date > now() - interval '{} hours'".format(sending_period)

        cmd = cmd + " ORDER BY t.id ASC LIMIT {}".format(limit)
        cur.execute(cmd)
        result = cur.fetchall()

        return result

    def delete_old_messages(self, ageHours):
        
        if (ageHours <= 0 ): # disable deleting if ageHours < 0
            return 0

        conn = self.db.connection()
        cur = conn.cursor()
        cmd = """
            WITH deleted AS
            (
                DELETE FROM telegram_messages 
                WHERE create_date < now() - interval '{} hour'
                RETURNING *
            )
            SELECT count(*) FROM deleted
        """.format(ageHours)
        cur.execute(cmd)
        result = cur.fetchone()
        conn.commit()
        return result[0]


    def set_message_ok(self, msg_id):

        conn = self.db.connection()
        cur = conn.cursor()

        cur.execute("UPDATE telegram_messages SET status = %s, additional = %s WHERE id = %s", ('ok', None, msg_id))
        conn.commit()

    def set_message_error(self, msg_id, reason=None):

        conn = self.db.connection()
        cur = conn.cursor()

        cur.execute("UPDATE telegram_messages SET status = %s, additional = %s WHERE id = %s", ('error', reason, msg_id))
        conn.commit()

    def get_telegram_bot_settings(self):
        
        conn = self.db.connection()
        cur = conn.cursor()
        cur.execute("SELECT value FROM settings WHERE key = %s LIMIT 1", (self.TELEGRAM_BOT_SETTINGS_KEY,) )
        result = cur.fetchone()
        return result


