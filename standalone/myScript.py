from django.conf import settings

settings.configure(
    DATABASE_ENGINE    = "django_mongodb_engine",
    DATABASE_NAME      = "standalonetest",
    DATABASE_USER      = "",
    DATABASE_PASSWORD  = "",
    DATABASE_HOST      = "",
    DATABASE_PORT      = "",
    INSTALLED_APPS     = ("myApp")
)

from django.db import models
from myApp.models import *