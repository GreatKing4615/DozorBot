import logging
import os, signal, threading
import json
from time import sleep
from telegram import (Bot, Update, ReplyKeyboardMarkup, KeyboardButton)
from telegram.ext import (Job, Updater, CommandHandler, ConversationHandler, MessageHandler, Filters)


class NotificationBot:
    IDLE, NEED_CONTACT = range(2)

    def __init__(self, config, model, logger):

        if config['use_proxy']:
            REQUEST_KWARGS = config['proxy_settings']
        else:
            REQUEST_KWARGS = {}

        self.token = config['token']
        self.msg_batch_limit = config['msg_batch_limit']
        self.msg_length_limit = config['msg_length_limit']
        self.store_messages_period = config['store_messages_period']
        self.sending_period = config['sending_period']
        self.model = model
        self.logger = logger

        self.updater = Updater(self.token, workers=1, request_kwargs=REQUEST_KWARGS)
        #self.updater = Updater(self.token, workers=1)

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

        # disable apscheduler useless logging
        logging.getLogger('apscheduler.executors.default').setLevel(logging.WARNING)
        logging.getLogger('apscheduler.scheduler').setLevel(logging.WARNING)
        self.logger.info('Bot started')
        self.updater.start_polling()
        self.updater.job_queue.run_once(self.__cb_notificate, 5.0)

    def idle(self):
        return self.updater.idle()

    #def __cb_notificate(self, bot: Bot, job: Job):
    def __cb_notificate(self, bot: Bot):
         
        # check token settings and restart when changes
        config_str = self.model.get_telegram_bot_settings()
        if config_str :
            if len(config_str) == 1 :
                if self.token not in config_str[0] :
                    self.logger.info('Exit due to token change')
                    os.kill(os.getpid(), signal.SIGINT)
                    return
    
        deletedCount = self.model.delete_old_messages(self.store_messages_period)
        if deletedCount > 0:
            self.logger.info('Store period expired for {} message(s). Deleted'.format(deletedCount))

        msgs = self.model.get_messages_sending(self.msg_batch_limit, self.sending_period)
        for m in msgs:

            failmsg = None

            if m['telegram_user_id'] is not None:
                #if m['enable_telegram_sending']:
                if True :
                    try:
                        if len(m['text']) > self.msg_length_limit:
                            parts = [m['text'][i:i + self.msg_length_limit] for i in
                                     range(0, len(m['text']), self.msg_length_limit)]
                            sleep(1)
                            for part in parts:
                                bot.bot.send_message(m['telegram_user_id'], part)
                                sleep(0.1)
                        else:
                            bot.bot.send_message(m['telegram_user_id'], m['text'])
                        self.model.set_message_ok(m['id'])
                        self.logger.info('Message id={0} sent for user={1} phone={2} text="{3}"'.format(m['id'], m['user_name'], m['phone_number'], m['text']))
                    except Exception as e:
                        failmsg = str(e)
                else:
                    failmsg = 'sending disabled for this user'
            else:
                failmsg = 'telegram_user_id is empty'

            if failmsg:
                self.model.set_message_error(m['id'], failmsg)
                self.logger.error('Failed to send message id={0} for user={1} phone={2} text="{3}" reason="{4}"'.format(m['id'],m['user_name'], m['phone_number'], m['text'], failmsg))

        msgs_left = len(msgs) - self.msg_batch_limit if len(msgs) - self.msg_batch_limit >= 0 else 0
        self.model.set_messages_sending(self.msg_batch_limit - msgs_left)

        self.updater.job_queue.run_once(self.__cb_notificate, 2.0)

    def __error(self, bot: Bot, update: Update, error):
        self.logger.warning('Update "%s" caused error "%s"' % (update, error))

    def __start(self, bot: Bot, update: Update):
        return self.__idle(bot, update)

    def __idle(self, bot: Bot, update: Update):

        user = self.model.get_user_by_telegram_user_id(bot.message.chat_id)
        if user:
            #if user['enable_telegram_sending']:
            if True :
                bot.message.reply_text('Your contact info was stored, just wait for notifications!')
                self.logger.info('Contact already exists for user ' + str(user['user_name']))
            else:
                bot.message.reply_text('Your phone notifications disabled!')
                self.logger.info('Notification disabled for user ' + str(user['user_name']))
            return self.IDLE
        else:
            self.logger.info('Detected unknown telegram_id ' + str(bot.message.chat_id))
            return self.__need_contact(bot, update)

    def __need_contact(self, bot: Bot, update: Update):

        if bot.message.contact:
            if bot.message.contact.phone_number:
                if bot.message.contact.user_id == bot.message.chat_id:
                    user = self.model.get_user_by_phone(bot.message.contact.phone_number)
                    if user:
                        self.model.store_contact(bot.message.contact.user_id, bot.message.contact.phone_number)
                        self.logger.info('Store telegram_id '+ str(bot.message.contact.user_id) +' for user ' + str(user['user_name']))
                        return self.__idle(bot, update)
                    else:
                        failmsg = 'You are not permitted to use this bot!'
                        self.logger.info('Reject for unknown phone ' + str (bot.message.contact.phone_number))
                else:
                    failmsg = 'It is not your contact!'
                    self.logger.info('It is not your contact! for ' + str(bot.message.contact.phone_number))
            else:
                failmsg = 'There is no phone number!'
                self.logger.info('There is no phone number! for  ' +str(bot.message.contact.user_id))
        else:
            failmsg = None

        buttons = [KeyboardButton('My phone number', request_contact=True), 'Cancel']
        text = 'Bot needs your contact info to operate.'
        fulltext = '{0}\r\n{1}'.format(failmsg, text) if failmsg else text

        bot.message.reply_text(
            fulltext,
            reply_markup=ReplyKeyboardMarkup([buttons], one_time_keyboard=True)
        )
        return self.NEED_CONTACT
