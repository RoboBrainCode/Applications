from django.conf.urls import patterns, url
from gui import views

urlpatterns = patterns('',
    url(r'playTraj/', views.playTraj, name='playTraj'),
    url(r'saveSeq/', views.saveSeq, name='playTraj'),
    url(r'initApp/', views.initApp, name='initApp'),
    url(r'resumeTraj/', views.resumeTraj, name='resumeTraj'),
    url(r'stopTraj/', views.stopTraj, name='stopTraj'),
    url(r'capTraj/', views.capTraj, name='capTraj'),
    url(r'capNextTraj/', views.capNextTraj, name='capNextTraj'),
)
