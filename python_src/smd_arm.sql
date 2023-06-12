--
-- PostgreSQL database dump
--

-- Dumped from database version 10.16
-- Dumped by pg_dump version 10.16

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: timescaledb; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS timescaledb WITH SCHEMA public;


--
-- Name: EXTENSION timescaledb; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION timescaledb IS 'Enables scalable inserts and complex queries for time-series data';


--
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


--
-- Name: uuid-ossp; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS "uuid-ossp" WITH SCHEMA public;


--
-- Name: EXTENSION "uuid-ossp"; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION "uuid-ossp" IS 'generate universally unique identifiers (UUIDs)';


--
-- Name: snow_flake_id(); Type: FUNCTION; Schema: public; Owner: django
--

CREATE FUNCTION public.snow_flake_id() RETURNS bigint
    LANGUAGE sql
    AS $$

select (extract(epoch from current_timestamp) * 1000)::bigint * 1000000
  + 2 * 10000
  + nextval('public.snow_flake_id_seq') % 1000
  as snow_flake_id

$$;


ALTER FUNCTION public.snow_flake_id() OWNER TO django;

--
-- Name: FUNCTION snow_flake_id(); Type: COMMENT; Schema: public; Owner: django
--

COMMENT ON FUNCTION public.snow_flake_id() IS 'snow flake id ';


SET default_tablespace = '';

SET default_with_oids = false;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO django;

--
-- Name: user_legacy_integer_id; Type: SEQUENCE; Schema: public; Owner: django
--

CREATE SEQUENCE public.user_legacy_integer_id
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.user_legacy_integer_id OWNER TO django;

--
-- Name: app_users; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.app_users (
    id character varying(32) NOT NULL,
    name text,
    create_date timestamp with time zone DEFAULT now() NOT NULL,
    update_date timestamp with time zone,
    telegram_user_id bigint,
    domain integer,
    domain_uid character varying(80),
    legacy_id integer DEFAULT nextval('public.user_legacy_integer_id'::regclass) NOT NULL,
    is_deleted boolean DEFAULT false NOT NULL,
    is_blocked boolean DEFAULT false NOT NULL,
    is_manual_role_set boolean DEFAULT true NOT NULL,
    is_autocreated boolean DEFAULT false NOT NULL,
    guid uuid DEFAULT public.uuid_generate_v4()
);


ALTER TABLE public.app_users OWNER TO django;

--
-- Name: TABLE app_users; Type: COMMENT; Schema: public; Owner: django
--

COMMENT ON TABLE public.app_users IS 'application users table.';


--
-- Name: snow_flake_id_seq; Type: SEQUENCE; Schema: public; Owner: django
--

CREATE SEQUENCE public.snow_flake_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.snow_flake_id_seq OWNER TO django;

--
-- Name: aspnet_role_claims; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.aspnet_role_claims (
    id integer DEFAULT nextval('public.snow_flake_id_seq'::regclass) NOT NULL,
    role_id character varying(32) NOT NULL,
    claim_type character varying(1024) NOT NULL,
    claim_value character varying(1024) NOT NULL
);


ALTER TABLE public.aspnet_role_claims OWNER TO django;

--
-- Name: TABLE aspnet_role_claims; Type: COMMENT; Schema: public; Owner: django
--

COMMENT ON TABLE public.aspnet_role_claims IS 'aspnet role claims table';


--
-- Name: aspnet_roles; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.aspnet_roles (
    id character varying(32) DEFAULT (public.snow_flake_id())::character varying NOT NULL,
    name character varying(64) NOT NULL,
    normalized_name character varying(64) NOT NULL,
    concurrency_stamp character varying(36)
);


ALTER TABLE public.aspnet_roles OWNER TO django;

--
-- Name: aspnet_user_claims; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.aspnet_user_claims (
    id integer DEFAULT nextval('public.snow_flake_id_seq'::regclass) NOT NULL,
    user_id character varying(32) NOT NULL,
    claim_type character varying(1024) NOT NULL,
    claim_value character varying(1024) NOT NULL
);


ALTER TABLE public.aspnet_user_claims OWNER TO django;

--
-- Name: TABLE aspnet_user_claims; Type: COMMENT; Schema: public; Owner: django
--

COMMENT ON TABLE public.aspnet_user_claims IS 'aspnet user claims table';


--
-- Name: aspnet_user_logins; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.aspnet_user_logins (
    login_provider character varying(32) NOT NULL,
    provider_key character varying(1024) NOT NULL,
    provider_display_name character varying(32) NOT NULL,
    user_id character varying(32) NOT NULL
);


ALTER TABLE public.aspnet_user_logins OWNER TO django;

--
-- Name: TABLE aspnet_user_logins; Type: COMMENT; Schema: public; Owner: django
--

COMMENT ON TABLE public.aspnet_user_logins IS 'aspnet user logins table.';


--
-- Name: aspnet_user_roles; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.aspnet_user_roles (
    user_id character varying(32) NOT NULL,
    role_id character varying(32) NOT NULL
);


ALTER TABLE public.aspnet_user_roles OWNER TO django;

--
-- Name: TABLE aspnet_user_roles; Type: COMMENT; Schema: public; Owner: django
--

COMMENT ON TABLE public.aspnet_user_roles IS 'aspnet user roles relation table.';


--
-- Name: aspnet_user_tokens; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.aspnet_user_tokens (
    user_id character varying(32) NOT NULL,
    login_provider character varying(32) NOT NULL,
    name character varying(32) NOT NULL,
    value character varying(256)
);


ALTER TABLE public.aspnet_user_tokens OWNER TO django;

--
-- Name: TABLE aspnet_user_tokens; Type: COMMENT; Schema: public; Owner: django
--

COMMENT ON TABLE public.aspnet_user_tokens IS 'aspnet user tokens table.';


--
-- Name: aspnet_users; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.aspnet_users (
    id character varying(32) DEFAULT (public.snow_flake_id())::character varying NOT NULL,
    user_name character varying(64) NOT NULL,
    normalized_user_name character varying(64) NOT NULL,
    email character varying(256) NOT NULL,
    normalized_email character varying(256) NOT NULL,
    email_confirmed boolean NOT NULL,
    phone_number character varying(32),
    phone_number_confirmed boolean NOT NULL,
    lockout_enabled boolean NOT NULL,
    lockout_end_unix_time_milliseconds bigint,
    password_hash character varying(256),
    access_failed_count integer NOT NULL,
    security_stamp character varying(256),
    two_factor_enabled boolean NOT NULL,
    concurrency_stamp character varying(36)
);


ALTER TABLE public.aspnet_users OWNER TO django;

--
-- Name: TABLE aspnet_users; Type: COMMENT; Schema: public; Owner: django
--

COMMENT ON TABLE public.aspnet_users IS 'aspnet users table.';


--
-- Name: auth_token_id_seq; Type: SEQUENCE; Schema: public; Owner: django
--

CREATE SEQUENCE public.auth_token_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.auth_token_id_seq OWNER TO django;

--
-- Name: domain_groups; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.domain_groups (
    id integer NOT NULL,
    domain_id integer NOT NULL,
    role_id character varying(32) NOT NULL,
    group_name character varying(255) NOT NULL
);


ALTER TABLE public.domain_groups OWNER TO django;

--
-- Name: domain_types; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.domain_types (
    id integer NOT NULL,
    name character varying(50) NOT NULL,
    user_class_name_default character varying(50) NOT NULL,
    user_filter_default character varying(200),
    uniqueid_attribute_name_default character varying(50) NOT NULL,
    fullname_attribute_name_default character varying(50) NOT NULL,
    ldap_user_lookup_attribute_name_default character varying(50),
    ad_user_lookup_upn_attribute_name_default character varying(50),
    ad_user_lookup_san_attribute_name_default character varying(50)
);


ALTER TABLE public.domain_types OWNER TO django;

--
-- Name: domains; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.domains (
    id integer NOT NULL,
    name character varying(50) NOT NULL,
    fqdn character varying(50) NOT NULL,
    address character varying(50) NOT NULL,
    type integer NOT NULL,
    port integer NOT NULL,
    username character varying(50) NOT NULL,
    password character varying(50),
    basedn character varying(200),
    autocreate_user boolean DEFAULT false NOT NULL,
    description character varying(250),
    user_class_name character varying(50) NOT NULL,
    user_filter character varying(200),
    uniqueid_attribute_name character varying(50) NOT NULL,
    fullname_attribute_name character varying(50) NOT NULL,
    is_disabled boolean DEFAULT false NOT NULL,
    is_default boolean DEFAULT false NOT NULL,
    guid uuid DEFAULT public.uuid_generate_v4(),
    ldap_user_lookup_attribute_name character varying(50),
    ad_user_lookup_upn_attribute_name character varying(50),
    ad_user_lookup_san_attribute_name character varying(50)
);


ALTER TABLE public.domains OWNER TO django;

--
-- Name: hibernate_sequence; Type: SEQUENCE; Schema: public; Owner: django
--

CREATE SEQUENCE public.hibernate_sequence
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.hibernate_sequence OWNER TO django;

--
-- Name: host_data; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.host_data (
    id bigint NOT NULL,
    host_id integer NOT NULL,
    upload_date timestamp with time zone NOT NULL,
    data text,
    processed_date timestamp with time zone,
    is_processed boolean DEFAULT false NOT NULL
);


ALTER TABLE public.host_data OWNER TO django;

--
-- Name: host_data_id_seq; Type: SEQUENCE; Schema: public; Owner: django
--

CREATE SEQUENCE public.host_data_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.host_data_id_seq OWNER TO django;

--
-- Name: host_data_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: django
--

ALTER SEQUENCE public.host_data_id_seq OWNED BY public.host_data.id;


--
-- Name: host_event; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.host_event (
    id bigint NOT NULL,
    host_id integer NOT NULL,
    create_date timestamp with time zone DEFAULT now() NOT NULL,
    status integer,
    sensor character varying NOT NULL,
    message character varying,
    host_date timestamp without time zone,
    attachment_id bigint
);


ALTER TABLE public.host_event OWNER TO django;

--
-- Name: host_event_id_seq; Type: SEQUENCE; Schema: public; Owner: django
--

CREATE SEQUENCE public.host_event_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.host_event_id_seq OWNER TO django;

--
-- Name: host_event_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: django
--

ALTER SEQUENCE public.host_event_id_seq OWNED BY public.host_event.id;


--
-- Name: hosts; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.hosts (
    id integer NOT NULL,
    ip character varying(32),
    name character varying(64),
    serial_number character varying(255),
    time_diff_ms bigint,
    diff_update_time timestamp without time zone,
    is_licensed boolean DEFAULT false NOT NULL,
    guid uuid DEFAULT public.uuid_generate_v4(),
    warranty_id integer,
    state text,
    activation_status text
);


ALTER TABLE public.hosts OWNER TO django;

--
-- Name: hosts_id_seq; Type: SEQUENCE; Schema: public; Owner: django
--

CREATE SEQUENCE public.hosts_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.hosts_id_seq OWNER TO django;

--
-- Name: hosts_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: django
--

ALTER SEQUENCE public.hosts_id_seq OWNED BY public.hosts.id;


--
-- Name: obac_objecttypes; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.obac_objecttypes (
    id uuid NOT NULL,
    description text NOT NULL
);


ALTER TABLE public.obac_objecttypes OWNER TO django;

--
-- Name: obac_permissions; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.obac_permissions (
    id uuid NOT NULL,
    description text NOT NULL
);


ALTER TABLE public.obac_permissions OWNER TO django;

--
-- Name: obac_permissions_in_roles; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.obac_permissions_in_roles (
    perm_id uuid NOT NULL,
    role_id uuid NOT NULL
);


ALTER TABLE public.obac_permissions_in_roles OWNER TO django;

--
-- Name: obac_roles; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.obac_roles (
    id uuid NOT NULL,
    description text NOT NULL
);


ALTER TABLE public.obac_roles OWNER TO django;

--
-- Name: obac_userpermissions; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.obac_userpermissions (
    id uuid NOT NULL,
    userid integer NOT NULL,
    permid uuid NOT NULL,
    objtypeid uuid NOT NULL,
    objid integer
);


ALTER TABLE public.obac_userpermissions OWNER TO django;

--
-- Name: obac_users; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.obac_users (
    id integer NOT NULL,
    external_id_int integer,
    external_id_str text,
    description text NOT NULL
);


ALTER TABLE public.obac_users OWNER TO django;

--
-- Name: obac_users_id_seq; Type: SEQUENCE; Schema: public; Owner: django
--

ALTER TABLE public.obac_users ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.obac_users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: settings; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.settings (
    key character varying(255) NOT NULL,
    value character varying(512) NOT NULL
);


ALTER TABLE public.settings OWNER TO django;

--
-- Name: telegram_messages_seq; Type: SEQUENCE; Schema: public; Owner: django
--

CREATE SEQUENCE public.telegram_messages_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.telegram_messages_seq OWNER TO django;

--
-- Name: telegram_messages; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.telegram_messages (
    user_id integer NOT NULL,
    text text NOT NULL,
    status character varying(50) NOT NULL,
    additional character varying(128),
    id integer DEFAULT nextval('public.telegram_messages_seq'::regclass) NOT NULL,
    create_date timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.telegram_messages OWNER TO django;

--
-- Name: warranties_id_seq; Type: SEQUENCE; Schema: public; Owner: django
--

CREATE SEQUENCE public.warranties_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.warranties_id_seq OWNER TO django;

--
-- Name: warranties; Type: TABLE; Schema: public; Owner: django
--

CREATE TABLE public.warranties (
    id integer DEFAULT nextval('public.warranties_id_seq'::regclass) NOT NULL,
    serial_number character varying(255),
    plan character varying(255),
    start_date timestamp without time zone,
    finish_date timestamp without time zone,
    last_synchronize_date timestamp with time zone NOT NULL,
    warranty character varying NOT NULL,
    up_to_date boolean NOT NULL,
    program_id integer,
    call_home text
);


ALTER TABLE public.warranties OWNER TO django;

--
-- Name: host_data id; Type: DEFAULT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.host_data ALTER COLUMN id SET DEFAULT nextval('public.host_data_id_seq'::regclass);


--
-- Name: host_event id; Type: DEFAULT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.host_event ALTER COLUMN id SET DEFAULT nextval('public.host_event_id_seq'::regclass);


--
-- Data for Name: cache_inval_bgw_job; Type: TABLE DATA; Schema: _timescaledb_cache; Owner: postgres
--

COPY _timescaledb_cache.cache_inval_bgw_job  FROM stdin;
\.


--
-- Data for Name: cache_inval_extension; Type: TABLE DATA; Schema: _timescaledb_cache; Owner: postgres
--

COPY _timescaledb_cache.cache_inval_extension  FROM stdin;
\.


--
-- Data for Name: cache_inval_hypertable; Type: TABLE DATA; Schema: _timescaledb_cache; Owner: postgres
--

COPY _timescaledb_cache.cache_inval_hypertable  FROM stdin;
\.


--
-- Data for Name: hypertable; Type: TABLE DATA; Schema: _timescaledb_catalog; Owner: postgres
--

COPY _timescaledb_catalog.hypertable (id, schema_name, table_name, associated_schema_name, associated_table_prefix, num_dimensions, chunk_sizing_func_schema, chunk_sizing_func_name, chunk_target_size, compressed, compressed_hypertable_id) FROM stdin;
\.


--
-- Data for Name: chunk; Type: TABLE DATA; Schema: _timescaledb_catalog; Owner: postgres
--

COPY _timescaledb_catalog.chunk (id, hypertable_id, schema_name, table_name, compressed_chunk_id, dropped) FROM stdin;
\.


--
-- Data for Name: dimension; Type: TABLE DATA; Schema: _timescaledb_catalog; Owner: postgres
--

COPY _timescaledb_catalog.dimension (id, hypertable_id, column_name, column_type, aligned, num_slices, partitioning_func_schema, partitioning_func, interval_length, integer_now_func_schema, integer_now_func) FROM stdin;
\.


--
-- Data for Name: dimension_slice; Type: TABLE DATA; Schema: _timescaledb_catalog; Owner: postgres
--

COPY _timescaledb_catalog.dimension_slice (id, dimension_id, range_start, range_end) FROM stdin;
\.


--
-- Data for Name: chunk_constraint; Type: TABLE DATA; Schema: _timescaledb_catalog; Owner: postgres
--

COPY _timescaledb_catalog.chunk_constraint (chunk_id, dimension_slice_id, constraint_name, hypertable_constraint_name) FROM stdin;
\.


--
-- Data for Name: chunk_index; Type: TABLE DATA; Schema: _timescaledb_catalog; Owner: postgres
--

COPY _timescaledb_catalog.chunk_index (chunk_id, index_name, hypertable_id, hypertable_index_name) FROM stdin;
\.


--
-- Data for Name: compression_chunk_size; Type: TABLE DATA; Schema: _timescaledb_catalog; Owner: postgres
--

COPY _timescaledb_catalog.compression_chunk_size (chunk_id, compressed_chunk_id, uncompressed_heap_size, uncompressed_toast_size, uncompressed_index_size, compressed_heap_size, compressed_toast_size, compressed_index_size) FROM stdin;
\.


--
-- Data for Name: bgw_job; Type: TABLE DATA; Schema: _timescaledb_config; Owner: postgres
--

COPY _timescaledb_config.bgw_job (id, application_name, job_type, schedule_interval, max_runtime, max_retries, retry_period) FROM stdin;
\.


--
-- Data for Name: continuous_agg; Type: TABLE DATA; Schema: _timescaledb_catalog; Owner: postgres
--

COPY _timescaledb_catalog.continuous_agg (mat_hypertable_id, raw_hypertable_id, user_view_schema, user_view_name, partial_view_schema, partial_view_name, bucket_width, job_id, refresh_lag, direct_view_schema, direct_view_name, max_interval_per_job, ignore_invalidation_older_than, materialized_only) FROM stdin;
\.


--
-- Data for Name: continuous_aggs_completed_threshold; Type: TABLE DATA; Schema: _timescaledb_catalog; Owner: postgres
--

COPY _timescaledb_catalog.continuous_aggs_completed_threshold (materialization_id, watermark) FROM stdin;
\.


--
-- Data for Name: continuous_aggs_hypertable_invalidation_log; Type: TABLE DATA; Schema: _timescaledb_catalog; Owner: postgres
--

COPY _timescaledb_catalog.continuous_aggs_hypertable_invalidation_log (hypertable_id, modification_time, lowest_modified_value, greatest_modified_value) FROM stdin;
\.


--
-- Data for Name: continuous_aggs_invalidation_threshold; Type: TABLE DATA; Schema: _timescaledb_catalog; Owner: postgres
--

COPY _timescaledb_catalog.continuous_aggs_invalidation_threshold (hypertable_id, watermark) FROM stdin;
\.


--
-- Data for Name: continuous_aggs_materialization_invalidation_log; Type: TABLE DATA; Schema: _timescaledb_catalog; Owner: postgres
--

COPY _timescaledb_catalog.continuous_aggs_materialization_invalidation_log (materialization_id, modification_time, lowest_modified_value, greatest_modified_value) FROM stdin;
\.


--
-- Data for Name: hypertable_compression; Type: TABLE DATA; Schema: _timescaledb_catalog; Owner: postgres
--

COPY _timescaledb_catalog.hypertable_compression (hypertable_id, attname, compression_algorithm_id, segmentby_column_index, orderby_column_index, orderby_asc, orderby_nullsfirst) FROM stdin;
\.


--
-- Data for Name: metadata; Type: TABLE DATA; Schema: _timescaledb_catalog; Owner: postgres
--

COPY _timescaledb_catalog.metadata (key, value, include_in_telemetry) FROM stdin;
exported_uuid	2f913307-aaca-4c58-b2e5-23ce8bcc6fcb	t
\.


--
-- Data for Name: tablespace; Type: TABLE DATA; Schema: _timescaledb_catalog; Owner: postgres
--

COPY _timescaledb_catalog.tablespace (id, hypertable_id, tablespace_name) FROM stdin;
\.


--
-- Data for Name: bgw_policy_compress_chunks; Type: TABLE DATA; Schema: _timescaledb_config; Owner: postgres
--

COPY _timescaledb_config.bgw_policy_compress_chunks (job_id, hypertable_id, older_than) FROM stdin;
\.


--
-- Data for Name: bgw_policy_drop_chunks; Type: TABLE DATA; Schema: _timescaledb_config; Owner: postgres
--

COPY _timescaledb_config.bgw_policy_drop_chunks (job_id, hypertable_id, older_than, cascade, cascade_to_materializations) FROM stdin;
\.


--
-- Data for Name: bgw_policy_reorder; Type: TABLE DATA; Schema: _timescaledb_config; Owner: postgres
--

COPY _timescaledb_config.bgw_policy_reorder (job_id, hypertable_id, hypertable_index_name) FROM stdin;
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20200706102524_InitialMigrations	3.1.5
\.


--
-- Data for Name: app_users; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.app_users (id, name, create_date, update_date, telegram_user_id, domain, domain_uid, legacy_id, is_deleted, is_blocked, is_manual_role_set, is_autocreated, guid) FROM stdin;
1626241063693020004	Администратор	2017-12-15 18:09:24.69+00	2023-03-10 10:21:07.15464+00	\N	1	\N	1	f	f	f	f	85520084-a78c-4724-9e78-17a589d60c11
\.


--
-- Data for Name: aspnet_role_claims; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.aspnet_role_claims (id, role_id, claim_type, claim_value) FROM stdin;
\.


--
-- Data for Name: aspnet_roles; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.aspnet_roles (id, name, normalized_name, concurrency_stamp) FROM stdin;
1626241063541020002	ADMIN	ADMIN	3378318f-9c1c-4020-90aa-9fd9c7d09661
1626241063554020003	OPERATOR	OPERATOR	32e7fd86-2508-4a52-85c2-d330fd990b45
\.


--
-- Data for Name: aspnet_user_claims; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.aspnet_user_claims (id, user_id, claim_type, claim_value) FROM stdin;
\.


--
-- Data for Name: aspnet_user_logins; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.aspnet_user_logins (login_provider, provider_key, provider_display_name, user_id) FROM stdin;
\.


--
-- Data for Name: aspnet_user_roles; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.aspnet_user_roles (user_id, role_id) FROM stdin;
1626241063693020004	1626241063541020002
1626241063693020004	1626241063554020003
\.


--
-- Data for Name: aspnet_user_tokens; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.aspnet_user_tokens (user_id, login_provider, name, value) FROM stdin;
\.


--
-- Data for Name: aspnet_users; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.aspnet_users (id, user_name, normalized_user_name, email, normalized_email, email_confirmed, phone_number, phone_number_confirmed, lockout_enabled, lockout_end_unix_time_milliseconds, password_hash, access_failed_count, security_stamp, two_factor_enabled, concurrency_stamp) FROM stdin;
1626241063693020004	Administrator	ADMINISTRATOR	admin@localhost	ADMIN@LOCALHOST	f	79265444592	f	t	\N	AQAAAAEAACcQAAAAEGxj1Lsoz1zIthR3ALxY2gLiRR4S3J8OfDhjdSrMiKtkDVJ7YRpXdllfEJm5c6nD6w==	0	HFIFUV4YZSU4RDQG2MHTEV3IKUW7ZEKD	f	369315fdf14a4d228617543a16b0dc52
\.


--
-- Data for Name: domain_groups; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.domain_groups (id, domain_id, role_id, group_name) FROM stdin;
\.


--
-- Data for Name: domain_types; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.domain_types (id, name, user_class_name_default, user_filter_default, uniqueid_attribute_name_default, fullname_attribute_name_default, ldap_user_lookup_attribute_name_default, ad_user_lookup_upn_attribute_name_default, ad_user_lookup_san_attribute_name_default) FROM stdin;
3	Local					\N	\N	\N
2	LDAP	inetorgperson	(objectclass=inetor gperson)	entryUUID	displayName	sn	\N	\N
1	Active Directory	user	(&(objectCategory= Person) (sAMAccountName= *))	objectGUID	givenName	\N	userPrincipalName	SamAccountName
\.


--
-- Data for Name: domains; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.domains (id, name, fqdn, address, type, port, username, password, basedn, autocreate_user, description, user_class_name, user_filter, uniqueid_attribute_name, fullname_attribute_name, is_disabled, is_default, guid, ldap_user_lookup_attribute_name, ad_user_lookup_upn_attribute_name, ad_user_lookup_san_attribute_name) FROM stdin;
1	Local	local	local	3	80	admin	\N	\N	f	\N	user	*	name	name	f	t	44cdc179-9ee0-4f6a-bfaf-0b4c3b3e38ca	\N	\N	\N
\.


--
-- Data for Name: host_data; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.host_data (id, host_id, upload_date, data, processed_date, is_processed) FROM stdin;
\.


--
-- Data for Name: host_event; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.host_event (id, host_id, create_date, status, sensor, message, host_date, attachment_id) FROM stdin;
\.


--
-- Data for Name: hosts; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.hosts (id, ip, name, serial_number, time_diff_ms, diff_update_time, is_licensed, guid, warranty_id, state, activation_status) FROM stdin;
\.


--
-- Data for Name: obac_objecttypes; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.obac_objecttypes (id, description) FROM stdin;
\.


--
-- Data for Name: obac_permissions; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.obac_permissions (id, description) FROM stdin;
00000001-1000-5000-8fe3-f7ccf2100004	PermissionDeviceOperator
00000002-1000-5000-8fe3-f7ccf2100004	PermissionDeviceAdmin
\.


--
-- Data for Name: obac_permissions_in_roles; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.obac_permissions_in_roles (perm_id, role_id) FROM stdin;
\.


--
-- Data for Name: obac_roles; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.obac_roles (id, description) FROM stdin;
\.


--
-- Data for Name: obac_userpermissions; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.obac_userpermissions (id, userid, permid, objtypeid, objid) FROM stdin;
b52ff9d5-d717-482a-bac1-48eb43fafdb3	1	00000002-1000-5000-8fe3-f7ccf2100004	d35b64d0-1001-5001-8fe3-f7ccf2100004	1
\.


--
-- Data for Name: obac_users; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.obac_users (id, external_id_int, external_id_str, description) FROM stdin;
1	\N	\N	admin
\.


--
-- Data for Name: settings; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.settings (key, value) FROM stdin;
NotificationAlertSettings	{"NotificationAlertEnabled":false,"NotificationAlertToEmail":"hotline1@depo.ru"}
\.


--
-- Data for Name: telegram_messages; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.telegram_messages (user_id, text, status, additional, id, create_date) FROM stdin;
1	Test message from tests	sending	\N	1	2023-06-09 10:36:03.473402+00
\.


--
-- Data for Name: warranties; Type: TABLE DATA; Schema: public; Owner: django
--

COPY public.warranties (id, serial_number, plan, start_date, finish_date, last_synchronize_date, warranty, up_to_date, program_id, call_home) FROM stdin;
\.


--
-- Name: chunk_constraint_name; Type: SEQUENCE SET; Schema: _timescaledb_catalog; Owner: postgres
--

SELECT pg_catalog.setval('_timescaledb_catalog.chunk_constraint_name', 1, false);


--
-- Name: chunk_id_seq; Type: SEQUENCE SET; Schema: _timescaledb_catalog; Owner: postgres
--

SELECT pg_catalog.setval('_timescaledb_catalog.chunk_id_seq', 1, false);


--
-- Name: dimension_id_seq; Type: SEQUENCE SET; Schema: _timescaledb_catalog; Owner: postgres
--

SELECT pg_catalog.setval('_timescaledb_catalog.dimension_id_seq', 1, false);


--
-- Name: dimension_slice_id_seq; Type: SEQUENCE SET; Schema: _timescaledb_catalog; Owner: postgres
--

SELECT pg_catalog.setval('_timescaledb_catalog.dimension_slice_id_seq', 1, false);


--
-- Name: hypertable_id_seq; Type: SEQUENCE SET; Schema: _timescaledb_catalog; Owner: postgres
--

SELECT pg_catalog.setval('_timescaledb_catalog.hypertable_id_seq', 1, false);


--
-- Name: bgw_job_id_seq; Type: SEQUENCE SET; Schema: _timescaledb_config; Owner: postgres
--

SELECT pg_catalog.setval('_timescaledb_config.bgw_job_id_seq', 1000, false);


--
-- Name: auth_token_id_seq; Type: SEQUENCE SET; Schema: public; Owner: django
--

SELECT pg_catalog.setval('public.auth_token_id_seq', 1, false);


--
-- Name: hibernate_sequence; Type: SEQUENCE SET; Schema: public; Owner: django
--

SELECT pg_catalog.setval('public.hibernate_sequence', 1, true);


--
-- Name: host_data_id_seq; Type: SEQUENCE SET; Schema: public; Owner: django
--

SELECT pg_catalog.setval('public.host_data_id_seq', 1, false);


--
-- Name: host_event_id_seq; Type: SEQUENCE SET; Schema: public; Owner: django
--

SELECT pg_catalog.setval('public.host_event_id_seq', 1, false);


--
-- Name: hosts_id_seq; Type: SEQUENCE SET; Schema: public; Owner: django
--

SELECT pg_catalog.setval('public.hosts_id_seq', 1, false);


--
-- Name: obac_users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: django
--

SELECT pg_catalog.setval('public.obac_users_id_seq', 1, false);


--
-- Name: snow_flake_id_seq; Type: SEQUENCE SET; Schema: public; Owner: django
--

SELECT pg_catalog.setval('public.snow_flake_id_seq', 1, false);


--
-- Name: telegram_messages_seq; Type: SEQUENCE SET; Schema: public; Owner: django
--

SELECT pg_catalog.setval('public.telegram_messages_seq', 1, false);


--
-- Name: user_legacy_integer_id; Type: SEQUENCE SET; Schema: public; Owner: django
--

SELECT pg_catalog.setval('public.user_legacy_integer_id', 1, false);


--
-- Name: warranties_id_seq; Type: SEQUENCE SET; Schema: public; Owner: django
--

SELECT pg_catalog.setval('public.warranties_id_seq', 1, false);


--
-- Name: obac_objecttypes PK_obac_objecttypes; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.obac_objecttypes
    ADD CONSTRAINT "PK_obac_objecttypes" PRIMARY KEY (id);


--
-- Name: obac_permissions PK_obac_permissions; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.obac_permissions
    ADD CONSTRAINT "PK_obac_permissions" PRIMARY KEY (id);


--
-- Name: obac_permissions_in_roles PK_obac_permissions_in_roles; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.obac_permissions_in_roles
    ADD CONSTRAINT "PK_obac_permissions_in_roles" PRIMARY KEY (perm_id, role_id);


--
-- Name: obac_roles PK_obac_roles; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.obac_roles
    ADD CONSTRAINT "PK_obac_roles" PRIMARY KEY (id);


--
-- Name: obac_userpermissions PK_obac_userpermissions; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.obac_userpermissions
    ADD CONSTRAINT "PK_obac_userpermissions" PRIMARY KEY (id);


--
-- Name: obac_users PK_obac_users; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.obac_users
    ADD CONSTRAINT "PK_obac_users" PRIMARY KEY (id);


--
-- Name: app_users app_users_guid_key; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.app_users
    ADD CONSTRAINT app_users_guid_key UNIQUE (guid);


--
-- Name: domain_groups domain_groups_pkey; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.domain_groups
    ADD CONSTRAINT domain_groups_pkey PRIMARY KEY (id);


--
-- Name: domain_types domain_types_pkey; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.domain_types
    ADD CONSTRAINT domain_types_pkey PRIMARY KEY (id);


--
-- Name: domains domains_guid_key; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.domains
    ADD CONSTRAINT domains_guid_key UNIQUE (guid);


--
-- Name: domains domains_pkey; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.domains
    ADD CONSTRAINT domains_pkey PRIMARY KEY (id);


--
-- Name: app_users pk_app_users; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.app_users
    ADD CONSTRAINT pk_app_users PRIMARY KEY (id);


--
-- Name: aspnet_role_claims pk_aspnet_role_claims; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_role_claims
    ADD CONSTRAINT pk_aspnet_role_claims PRIMARY KEY (id);


--
-- Name: aspnet_roles pk_aspnet_roles; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_roles
    ADD CONSTRAINT pk_aspnet_roles PRIMARY KEY (id);


--
-- Name: aspnet_user_claims pk_aspnet_user_claims; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_user_claims
    ADD CONSTRAINT pk_aspnet_user_claims PRIMARY KEY (id);


--
-- Name: aspnet_user_logins pk_aspnet_user_logins; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_user_logins
    ADD CONSTRAINT pk_aspnet_user_logins PRIMARY KEY (login_provider, provider_key);


--
-- Name: aspnet_user_roles pk_aspnet_user_roles; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_user_roles
    ADD CONSTRAINT pk_aspnet_user_roles PRIMARY KEY (user_id, role_id);


--
-- Name: aspnet_user_tokens pk_aspnet_user_tokens; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_user_tokens
    ADD CONSTRAINT pk_aspnet_user_tokens PRIMARY KEY (user_id, login_provider, name);


--
-- Name: aspnet_users pk_aspnet_users; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_users
    ADD CONSTRAINT pk_aspnet_users PRIMARY KEY (id);


--
-- Name: host_data pk_host_data; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.host_data
    ADD CONSTRAINT pk_host_data PRIMARY KEY (id);


--
-- Name: host_event pk_host_event; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.host_event
    ADD CONSTRAINT pk_host_event PRIMARY KEY (id);


--
-- Name: aspnet_roles u_aspnet_roles_name; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_roles
    ADD CONSTRAINT u_aspnet_roles_name UNIQUE (name);


--
-- Name: aspnet_roles u_aspnet_roles_normalized_name; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_roles
    ADD CONSTRAINT u_aspnet_roles_normalized_name UNIQUE (normalized_name);


--
-- Name: aspnet_users u_aspnet_users_normalized_user_name; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_users
    ADD CONSTRAINT u_aspnet_users_normalized_user_name UNIQUE (normalized_user_name);


--
-- Name: aspnet_users u_aspnet_users_username; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_users
    ADD CONSTRAINT u_aspnet_users_username UNIQUE (user_name);


--
-- Name: app_users u_legacy_id; Type: CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.app_users
    ADD CONSTRAINT u_legacy_id UNIQUE (legacy_id);


--
-- Name: aspnet_role_claims fk_aspnet_roles_id; Type: FK CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_role_claims
    ADD CONSTRAINT fk_aspnet_roles_id FOREIGN KEY (role_id) REFERENCES public.aspnet_roles(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: aspnet_user_roles fk_aspnet_roles_id; Type: FK CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_user_roles
    ADD CONSTRAINT fk_aspnet_roles_id FOREIGN KEY (role_id) REFERENCES public.aspnet_roles(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: aspnet_user_logins fk_aspnet_user_logins_user_id; Type: FK CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_user_logins
    ADD CONSTRAINT fk_aspnet_user_logins_user_id FOREIGN KEY (user_id) REFERENCES public.aspnet_users(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: aspnet_user_claims fk_aspnet_users_id; Type: FK CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_user_claims
    ADD CONSTRAINT fk_aspnet_users_id FOREIGN KEY (user_id) REFERENCES public.aspnet_users(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: aspnet_user_tokens fk_aspnet_users_id; Type: FK CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_user_tokens
    ADD CONSTRAINT fk_aspnet_users_id FOREIGN KEY (user_id) REFERENCES public.aspnet_users(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: aspnet_user_roles fk_aspnet_users_id; Type: FK CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.aspnet_user_roles
    ADD CONSTRAINT fk_aspnet_users_id FOREIGN KEY (user_id) REFERENCES public.aspnet_users(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: app_users fk_aspnet_users_id; Type: FK CONSTRAINT; Schema: public; Owner: django
--

ALTER TABLE ONLY public.app_users
    ADD CONSTRAINT fk_aspnet_users_id FOREIGN KEY (id) REFERENCES public.aspnet_users(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

