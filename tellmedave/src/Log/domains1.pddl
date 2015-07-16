(define (problem data_Nov-14-2014_1)
(:objects Robot PR2 Floor Wall_1 Wall_2 
Wall_3 Wall_4 Bed_1 Tv_1 Blackcouch_1 
Blackcouch_2 Roundtable_1 Almirah_1 Bedtable_1 Vase_1 
Wallshelf_1 Squaremirror_1 Humansit_1 Humanreach_1 Humansit_2 
Humansit_3 Floorlamp_1 Pillow_2 Pillow_4 Pillow_5 
Books_4 Books_5 Books_6 Classroomchair_1 Tv_1PowerButton 
Floorlamp_1PowerButton In On)
(:init 
(IsPlaceableOn Bed_1)
(Pressable Tv_1)
(IsPlaceableOn Blackcouch_1)
(IsPlaceableOn Blackcouch_2)
(IsPlaceableOn Roundtable_1)
(IsPlaceableOn Almirah_1)
(IsPlaceableOn Bedtable_1)
(IsGraspable Vase_1)
(IsPlaceableOn Wallshelf_1)
(IsGraspable Floorlamp_1)
(IsGraspable Pillow_2)
(IsGraspable Pillow_4)
(IsGraspable Pillow_5)
(On Vase_1 Bedtable_1)(On Floorlamp_1 Blackcouch_1)(On Pillow_2 Bed_1)(On Pillow_4 Wallshelf_1)(On Pillow_5 Bed_1)(Near Robot PR2))(:goal (and (state Tv_1 IsOn))))
