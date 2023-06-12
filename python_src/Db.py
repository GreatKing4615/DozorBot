import psycopg2, psycopg2.extras


class Db:

    def __init__(self, config):

        self.addr = config['addr']
        self.port = config['port']
        self.user = config['user']
        self.pasw = config['pasw']
        self.name = config['name']

        self.conn = None

    def __connect(self):
        self.conn = psycopg2.connect(
            host=self.addr, port=self.port, user=self.user, password=self.pasw, dbname=self.name
        )

    def __sure_connected(self):

        if self.conn and not self.conn.closed:
            try:
                self.conn.isolation_level
            except psycopg2.OperationalError:
                self.__connect()
        else:
            self.__connect()

    def connection(self):
        self.__sure_connected()
        return self.conn

    def cursor(self):
        return self.connection().cursor(cursor_factory=psycopg2.extras.DictCursor)
