

INSERT INTO PUBLIC.ASPNET_USERS(ID,
	USER_NAME,
	NORMALIZED_USER_NAME,
	EMAIL,
	NORMALIZED_EMAIL,
	EMAIL_CONFIRMED,
	PHONE_NUMBER,
	PHONE_NUMBER_CONFIRMED,
	LOCKOUT_ENABLED,
	LOCKOUT_END_UNIX_TIME_MILLISECONDS,
	PASSWORD_HASH,
	ACCESS_FAILED_COUNT,
	SECURITY_STAMP,
	TWO_FACTOR_ENABLED,
	CONCURRENCY_STAMP)
VALUES (2,
		'vitas',
		'VITAS',
		'test@mail.ru', 
		'testtest@MAIL.RU',
		TRUE,
		'+80932321212',
		TRUE, 
		FALSE,
		0,
		'test', 
		0, 
		now():: timestamp,
		FALSE,
		now():: timestamp
	   );

	INSERT INTO PUBLIC.APP_USERS(ID,

																				NAME,
																				CREATE_DATE,
																				UPDATE_DATE,
																				TELEGRAM_USER_ID,
																				DOMAIN,
																				DOMAIN_UID,
																				LEGACY_ID,
																				IS_DELETED,
																				IS_BLOCKED,
																				IS_MANUAL_ROLE_SET,
																				IS_AUTOCREATED,
																				GUID)
VALUES ('2', 'vitas', now()::date, now()::date, 754284879, 2, '2','2',FALSE, FALSE, FALSE, FALSE, '85520084-a78c-4724-9e78-17a589d60c12');



	   INSERT INTO public.telegram_messages(
	id, user_id, text, status, additional, create_date)
	VALUES (2, 2,'test2', 'sending', 'test', now()::date);
