from django.conf.urls import patterns, url
from e2efeedback import views

urlpatterns = patterns('',
    url(r'insertFeedback/', views.feedbackSys, name='feedbackSys'),
    url(r'upVote/', views.countUpvotes, name='countUpvotes'),
    url(r'recordFeedback/', views.recordFeedback, name='recordFeedback'),
    url(r'getTopFeeds/', views.returnTopFeeds, name='return_top_feeds'),
    url(r'getMoreFeeds/', views.addMoreFeeds, name='addMoreFeeds'),
    url(r'nlpfeedback/', views.getNLPFeedback, name='getNLPFeedback'),
    url(r'returnById/', views.returnById, name='returnById'), 
    url(r'tellmedaveFeedback/', views.tellmedaveFeedback, name='tellmedaveFeedback'), 
    url(r'planitFeedback/', views.planitFeedbackSys, name='planitFeedbackSys'), 
    
)
