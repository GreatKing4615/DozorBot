from time import sleep
from telegram import (Bot, Update, ReplyKeyboardMarkup, KeyboardButton)
from telegram.ext import (Job, Updater, CommandHandler, ConversationHandler, MessageHandler, Filters)


class NotificationBot:

    IDLE, NEED_CONTACT = range(2)

    def __init__(self, config, model, logger):

        REQUEST_KWARGS={
         'proxy_url': 'http://bubnaprom:8118'
#          'urllib3_proxy_kwargs': {
#             'username': 'telega',
#             'password': 'fYXr7vAaV24tw8nX',
#          }
        }

        self.token = config['token']
        self.msg_batch_limit = config['msg_batch_limit']
        self.msg_length_limit = config['msg_length_limit']
        self.model = model
        self.logger = logger

        self.updater = Updater(self.token, workers=1, request_kwargs=REQUEST_KWARGS)

        conv_handler = ConversationHandler(
            entry_points=[CommandHandler('start', self.__start), MessageHandler(Filters.all, self.__start)],
            states={
                self.IDLE: [MessageHandler(Filters.all, self.__idle)],
                self.NEED_CONTACT: [MessageHandler(Filters.all, self.__need_contact)],
            },
            fallbacks=[],
        )

        dp = self.updater.dispatcher
        dp.add_handler(conv_handler)
        dp.add_error_handler(self.__error)

        self.updater.start_polling()
        self.updater.job_queue.run_once(self.__cb_notificate, 5.0)

    def idle(self):
        return self.updater.idle()

    def __cb_notificate(self, bot: Bot, job: Job):
        msgs = self.model.get_messages_sending(self.msg_batch_limit)
        for m in msgs:

            failmsg = None

            if m['telegram_user_id'] is not None:
                if m['enable_telegram_sending']:
                    try:
                        if len(m['text']) > self.msg_length_limit:
                            parts = [m['text'][i:i + self.msg_length_limit] for i in range(0, len(m['text']), self.msg_length_limit)]
                            sleep(1)
                            for part in parts:
                                bot.send_message(m['telegram_user_id'], part)
                                sleep(0.1)
                        else:
                            bot.send_message(m['telegram_user_id'], m['text'])
                        self.model.set_message_ok(m['id'])
                        self.logger.info('Message (id={0}) sent'.format(m['id']))
                    except Exception as e:
                        failmsg = str(e)
                else:
                    failmsg = 'sending disabled for this user'
            else:
                failmsg = 'telegram_user_id is empty'

            if failmsg:
                self.model.set_message_error(m['id'], failmsg)
                self.logger.error('Failed to send message (id={0}): {1}'.format(m['id'], failmsg))

        msgs_left = len(msgs) - self.msg_batch_limit if len(msgs) - self.msg_batch_limit >= 0 else 0
        self.model.set_messages_sending(self.msg_batch_limit - msgs_left)

        self.updater.job_queue.run_once(self.__cb_notificate, 2.0)

    def __error(self, bot: Bot, update: Update, error):
        self.logger.warning('Update "%s" caused error "%s"' % (update, error))

    def __start(self, bot: Bot, update: Update):
        return self.__idle(bot, update)

    def __idle(self, bot: Bot, update: Update):

        user = self.model.get_user_by_telegram_user_id(update.message.chat_id)
        if user:
            if user['enable_telegram_sending']:
                update.message.reply_text('Your contact info was stored, just wait for notifications!')
            else:
                update.message.reply_text('Your phone notifications disabled!')
            return self.IDLE
        else:
            return self.__need_contact(bot, update)

    def __need_contact(self, bot: Bot, update: Update):

        if update.message.contact:
            if update.message.contact.phone_number:
                if update.message.contact.user_id == update.message.chat_id:
                    user = self.model.get_user_by_phone(update.message.contact.phone_number)
                    if user:
                        self.model.store_contact(update.message.contact.user_id, update.message.contact.phone_number)
                        return self.__idle(bot, update)
                    else:
                        failmsg = 'You are not permitted to use this bot!'
                else:
                    failmsg = 'It is not your contact!'
            else:
                failmsg = 'There is no phone number!'
        else:
            failmsg = None

        buttons = [KeyboardButton('My phone number', request_contact=True), 'Cancel']
        text = 'Bot needs your contact info to operate.'
        fulltext = '{0}\r\n{1}'.format(failmsg, text) if failmsg else text

        update.message.reply_text(
            fulltext,
            reply_markup=ReplyKeyboardMarkup([buttons], one_time_keyboard=True)
        )
        return self.NEED_CONTACT
