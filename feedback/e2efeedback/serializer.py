from django.forms import widgets
from rest_framework import serializers
from models import e2eFeedback,nlpFeedback
from djangotoolbox.fields import ListField,DictField
import yaml
import drf_compound_fields.fields as drf
from datetime import datetime

	
class FeedBackSerializer(serializers.Serializer):
	pk = serializers.Field()  # Note: `Field` is an untyped read-only field.
	actualInput=serializers.CharField(required=False)
	tellmedaveOutput= drf.ListField(drf.ListField(serializers.CharField()),required=True)
	# planitInput= drf.ListField(serializers.CharField(),required=True)
	# tellmedaveOutput= serializers.CharField(required=False)
	# planitInput= serializers.CharField(required=False)
	videoPath=serializers.CharField(required=False)
	tellmedaveFeedback = drf.ListField(drf.ListField(serializers.CharField()),required=False)
	planitFeedback = drf.ListField(drf.DictField(serializers.CharField()),required=False)
	
	tellmedaveFeedbackText = drf.ListField(serializers.CharField(),required=False)
	planitFeedbackText = drf.ListField(serializers.CharField(),required=False)

	created_at = serializers.DateTimeField(required=False)
	feedId = serializers.CharField(required=False)
	upvotes = serializers.IntegerField(required=False)  
	downvotes = serializers.IntegerField(required=False)  


	def restore_object(self, validated_data, instance=None):
		"""
		Create or update a new snippet instance, given a dictionary
		of deserialized field values.

		Note that if we don't define this method, then deserializing
		data will simply return a dictionary of items.
		"""
		if instance:
			return instance

		validated_data['created_at']=datetime.now()
		return e2eFeedback(**validated_data)

class nlpFeedbackSerializer(serializers.Serializer):
	pk = serializers.Field()
	envNumber= serializers.CharField(required=True)
	NLPInstruction = serializers.CharField(required=True)

	def restore_object(self, validated_data, instance=None):
		if instance:
				return instance

		validated_data['created_at']=datetime.now()
		return nlpFeedback(**validated_data)

			