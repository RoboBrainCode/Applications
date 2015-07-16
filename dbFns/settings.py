DATABASES = {
    'default': {
        'ENGINE': 'django_mongodb_engine', # Add 'postgresql_psycopg2', 'mysql', 'sqlite3' or 'oracle'.
        'NAME': 'planitDaveF',                      # Or path to database file if using sqlite3.
        'USER': '',                      # Not used with sqlite3.
        'PASSWORD': '',                  # Not used with sqlite3.
        'HOST': '',                      # Set to empty string for localhost. Not used with sqlite3.
        'PORT': '',                      # Set to empty string for default. Not used with sqlite3.
    }
}

INSTALLED_APPS = (
    'db',
    )

SECRET_KEY = '-srlyoddel%68@txqo0%qx#$_7gkao#k4(!gj$z4iq0yrd32bb'