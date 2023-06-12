--
-- PostgreSQL database dump
--

-- Dumped from database version 9.6.5
-- Dumped by pg_dump version 9.6.5

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

SET search_path = public, pg_catalog;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- Name: auth_user; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE auth_user (
    id integer NOT NULL,
    date_joined timestamp without time zone NOT NULL,
    email character varying(255) NOT NULL,
    is_active boolean NOT NULL,
    is_superuser boolean NOT NULL,
    last_login timestamp without time zone,
    password character varying(255) NOT NULL,
    username character varying(255) NOT NULL,
    is_admin boolean DEFAULT true NOT NULL,
    is_pwd_valid boolean DEFAULT true NOT NULL,
    name character varying(255),
    phone character varying(64),
    photo character varying(2048),
    create_date timestamp without time zone DEFAULT now() NOT NULL,
    update_date timestamp without time zone,
    telegram_user_id integer,
    enable_telegram_sending boolean NOT NULL
);


ALTER TABLE auth_user OWNER TO django;

--
-- Name: auth_user_id_seq; Type: SEQUENCE; Schema: public; Owner: django
--

CREATE SEQUENCE auth_user_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE auth_user_id_seq OWNER TO django;

--
-- Name: auth_user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: django
--

ALTER SEQUENCE auth_user_id_seq OWNED BY auth_user.id;


--
-- Name: auth_user id; Type: DEFAULT; Schema: public; Owner: django
--

ALTER TABLE ONLY auth_user ALTER COLUMN id SET DEFAULT nextval('auth_user_id_seq'::regclass);


--
-- Data for Name: auth_user; Type: TABLE DATA; Schema: public; Owner: django
--

COPY auth_user (id, date_joined, email, is_active, is_superuser, last_login, password, username, is_admin, is_pwd_valid, name, phone, photo, create_date, update_date, telegram_user_id, enable_telegram_sending) FROM stdin;
2	2018-02-06 13:07:50.112817	admin@loca.host	t	f	\N	4b83f8bdaf4e4f14fafb8e18bf6f2935bc859320	spherebot	f	t	spherebot	\N	\N	2018-02-06 13:07:50.112817	\N	\N	f
1	2018-02-06 13:07:47.660103	admin@loca.host	t	t	\N	d033e22ae348aeb5660fc2140aec35850c4da997	admin	t	t	\N	79856851840	\N	2018-02-06 13:07:48.523	\N	200736025	t
\.


--
-- Name: auth_user_id_seq; Type: SEQUENCE SET; Schema: public; Owner: django
--

SELECT pg_catalog.setval('auth_user_id_seq', 2, true);


--
-- Name: auth_user auth_user_pkey; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY auth_user
    ADD CONSTRAINT auth_user_pkey PRIMARY KEY (id);


--
-- Name: auth_user auth_user_username_uidx; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY auth_user
    ADD CONSTRAINT auth_user_username_uidx UNIQUE (username);


--
-- PostgreSQL database dump complete
--
