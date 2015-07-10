from django.forms import widgets
from rest_framework import serializers
from models import *
from djangotoolbox.fields import ListField,DictField
import yaml
import drf_compound_fields.fields as drf
from datetime import datetime

class FeedBackSerializer(serializers.Serializer):
	pk = serializers.Field()  # Note: `Field` is an untyped read-only field.
	envName=serializers.CharField(required=True)
	actualInput=serializers.CharField(required=True)
	tellmedaveOutput= drf.ListField(drf.ListField(serializers.CharField()),required=True)
	videoPath=serializers.CharField(required=False)
	tellmedaveFeedback = drf.ListField(drf.ListField(serializers.CharField()),required=False)
	planitFeedback = drf.ListField(drf.DictField(serializers.CharField()),required=False)
	tellmedaveFeedbackText = drf.ListField(serializers.CharField(),required=False)
	planitFeedbackText = drf.ListField(serializers.CharField(),required=False)
	created_at = serializers.DateTimeField(required=False)
	feedId = serializers.CharField(required=True)
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
	envName= serializers.CharField(required=True)
	NLPInstruction = serializers.CharField(required=True)

	def restore_object(self, validated_data, instance=None):
		if instance:
				return instance

		validated_data['created_at']=datetime.now()
		return nlpFeedback(**validated_data)

class trajectorySerializer(serializers.Serializer):
	pk = serializers.Field()  # Note: `Field` is an untyped read-only field.
	envName=serializers.CharField(required=True)
	objectFrom=serializers.CharField(required=True)
	objectTo=serializers.CharField(required=True)
	trajectory = drf.ListField(serializers.CharField(),required=True)
	created_at = serializers.DateTimeField(required=False)

	def restore_object(self, validated_data, instance=None):
		if instance:
			return instance

		validated_data['created_at']=datetime.now()
		return trajectoryDatabase(**validated_data)


class bestTrajectorySerializer(serializers.Serializer):
	pk = serializers.Field()  # Note: `Field` is an untyped read-only field.
	envName=serializers.CharField(required=True)
	objectFrom=serializers.CharField(required=True)
	objectTo=serializers.CharField(required=True)
	bestTrajectory = serializers.CharField(required=True)
	created_at = serializers.DateTimeField(required=False)

	def restore_object(self, validated_data, instance=None):
		
		if instance:
			return instance

		validated_data['created_at']=datetime.now()
		return bestTrajectoryDatabase(**validated_data)


class objectSerializer(serializers.Serializer):
	pk = serializers.Field()  # Note: `Field` is an untyped read-only field.
	envName=serializers.CharField(required=True)
	objectsList=drf.ListField(serializers.CharField(),required=True)
	created_at = serializers.DateTimeField(required=False)
	def restore_object(self, validated_data, instance=None):
		
		if instance:
			return instance

		validated_data['created_at']=datetime.now()
		return objectDatabase(**validated_data)

class objectPositionSerializer(serializers.Serializer):
	pk = serializers.Field()  # Note: `Field` is an untyped read-only field.
	envName=serializers.CharField(required=True)
	objectName=serializers.CharField(required=True)
	objectPosition=serializers.CharField(required=True)
	created_at = serializers.DateTimeField(required=False)
	def restore_object(self, validated_data, instance=None):
		
		if instance:
			return instance

		validated_data['created_at']=datetime.now()
		return objectPositionDatabase(**validated_data)

class planitLogSerializer(serializers.Serializer):
	initialWeight=drf.DictField(serializers.CharField(),required=True)
	feedback=drf.DictField(serializers.CharField(),required=True)
	finalWeight=drf.DictField(serializers.CharField(),required=True)
	created_at = serializers.DateTimeField(required=False)

	def restore_object(self, validated_data, instance=None):
		
		if instance:
			return instance

		validated_data['created_at']=datetime.now()
		return planitLog(**validated_data)



			