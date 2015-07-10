from django.db import models
from djangotoolbox.fields import ListField,DictField
from datetime import datetime
from django.db.models.signals import post_save

class e2eFeedback(models.Model):
	envName=models.TextField()
	actualInput=models.TextField()
	tellmedaveOutput= ListField(ListField())
	videoPath=models.TextField()
	tellmedaveFeedback = ListField(ListField())
	planitFeedback = ListField(DictField())
	tellmedaveFeedbackText = ListField()
	planitFeedbackText = ListField()
	created_at = models.DateTimeField(default=datetime.now())
	feedId = models.TextField(db_index=True)
	meta = {'indexes':['feedId']}
	upvotes = models.IntegerField(default=0)
	downvotes = models.IntegerField(default=0)
	
	def to_json(self):
		return {"_id":self.id,
			"envName":self.envName,
			"actualInput" : self.actualInput,
			"tellmedaveOutput" : self.tellmedaveOutput[-1],
			"videoPath" : self.videoPath,
			"feedId":self.feedId,
			"upvotes":self.upvotes,
			"downvotes":self.downvotes,
			}

	class Meta:
		db_table = 'e2eFeedback'
		get_latest_by = 'created_at'

class nlpFeedback(models.Model):
	envNumber=models.TextField()
	NLPInstruction= models.TextField()
	created_at = models.DateTimeField(default=datetime.now())
	
	def to_json(self):
		return {"_id":self.id,
			"envNumber" : self.envNumber,
			"NLPInstruction" : self.NLPInstruction,
			}

	class Meta:
		db_table = 'nlpFeedback'
		get_latest_by = 'created_at'

# class trajectoryDatabase(models.Model):
# 	envNumber=models.TextField()
# 	objectFrom=models.TextField()
# 	objectTo=models.TextField()
# 	trajectory = ListField()
# 	created_at = models.DateTimeField(default=datetime.now())
# 	meta = {'indexes':['envNumber']}
	
# 	def to_json(self):
# 		return {"_id":self.id,
# 			"envNumber" : self.envNumber,
# 			"objectFrom" : self.objectFrom,
# 			"objectTo" : self.objectTo,
# 			"trajectory":self.trajectory,
# 			}

# 	class Meta:
# 		db_table = 'trajectoryDatabase'
# 		get_latest_by = 'created_at'

# class bestTrajectoryDatabase(models.Model):
# 	envNumber=models.TextField()
# 	objectFrom=models.TextField()
# 	objectTo=models.TextField()
# 	bestTrajectory = models.TextField()
# 	created_at = models.DateTimeField(default=datetime.now())
# 	meta = {'indexes':['envNumber']}
	
# 	def to_json(self):
# 		return {"_id":self.id,
# 			"envNumber" : self.envNumber,
# 			"objectFrom" : self.objectFrom,
# 			"objectTo" : self.objectTo,
# 			"bestTrajectory":self.bestTrajectory,
# 			}

# 	class Meta:
# 		db_table = 'bestTrajectoryDatabase'
# 		get_latest_by = 'created_at'


# class objectDatabase(models.Model):
# 	envNumber=models.TextField()
# 	objects=ListField()
# 	created_at = models.DateTimeField(default=datetime.now())
# 	meta = {'indexes':['envNumber']}
	
# 	def to_json(self):
# 		return {"_id":self.id,
# 			"envNumber" : self.envNumber,
# 			"objects" : self.objects,
# 			}

# 	class Meta:
# 		db_table = 'objectDatabase'
# 		get_latest_by = 'created_at'

# class planitLog(models.Model):
# 	initialWeight=DictField()
# 	feedback=DictField()
# 	finalWeight=DictField()
# 	created_at = models.DateTimeField(default=datetime.now())
# 	meta = {'indexes':['created_at']}
	
# 	def to_json(self):
# 		return {"_id":self.id,
# 			"initialWeight" : self.initialWeight,
# 			"feedback" : self.feedback,
# 			"finalWeight": self.finalWeight,
# 			"created_at": self.created_at
# 			}

# 	class Meta:
# 		db_table = 'planitLog'
# 		get_latest_by = 'created_at'


